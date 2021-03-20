using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using NetAsyncSpider.Core.MessageQueue;

namespace NetAsyncSpider.MessageQueue
{
	public class CommunicationRabbitMQMessageQueue :BaseCommunicationMessage<byte[]>, ICommunicationMessage
	{
		private readonly RabbitMQOptions _options;
		private readonly PersistentConnection _connection;
		private readonly ILogger<CommunicationRabbitMQMessageQueue> _logger;
		private readonly IModel _publishChannel;

		public CommunicationRabbitMQMessageQueue(IServiceProvider serviceProvider,IOptions<RabbitMQOptions> options, ILoggerFactory loggerFactory):base(serviceProvider)
		{
			_logger = loggerFactory.CreateLogger<CommunicationRabbitMQMessageQueue>();
			_options = options.Value;
			_connection = new PersistentConnection(CreateConnectionFactory(),
				loggerFactory.CreateLogger<PersistentConnection>(), _options.RetryCount);

			if (!_connection.IsConnected)
			{
				_connection.TryConnect();
			}

			_logger.LogTrace("正在连接 rabbmq");

			_publishChannel = _connection.CreateModel();
			_publishChannel.ExchangeDeclare(_options.Exchange, "direct", true);
		}

		private IConnectionFactory CreateConnectionFactory()
		{
			var connectionFactory = new ConnectionFactory { HostName = _options.HostName, DispatchConsumersAsync = true };
			if (_options.Port > 0)
			{
				connectionFactory.Port = _options.Port;
			}

			if (!string.IsNullOrWhiteSpace(_options.UserName))
			{
				connectionFactory.UserName = _options.UserName;
			}

			if (!string.IsNullOrWhiteSpace(_options.Password))
			{
				connectionFactory.Password = _options.Password;
			}

			return connectionFactory;
		}

		public override async Task PublishAsync(string topic, byte[] bytes)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException(nameof(bytes));
			}

			if (!_connection.IsConnected)
			{
				_connection.TryConnect();
			}

			var policy = Policy.Handle<BrokerUnreachableException>()
				.Or<SocketException>()
				.WaitAndRetry(_options.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
					(ex, time) =>
					{
						_logger.LogWarning(ex,
							"Could not publish data after {Timeout}s ({ExceptionMessage})",
							$"{time.TotalSeconds:n1}", ex.Message);
					});

			_logger.LogTrace("Declaring RabbitMQ exchange to publish event");

			policy.Execute(() =>
			{
				var properties = _publishChannel.CreateBasicProperties();
				properties.DeliveryMode = 2; // persistent
				properties.ContentEncoding = "lz4";
				_logger.LogTrace("Publishing event to RabbitMQ");

				_publishChannel.BasicPublish(_options.Exchange, topic, true, properties, bytes);
			});

			await Task.CompletedTask;
		}

		public override void CloseQueue(string queue)
		{
			using var channel = _connection.CreateModel();
			channel.QueueDelete(queue);
		}

		public override bool IsDistributed => true;

		public override Task RegisterConsumeAsync(string queue,Func<IServiceProvider,byte[],Task> func,
			CancellationToken stoppingToken)
		{
			var task = base.RegisterConsumeAsync(queue, func,stoppingToken);
			if (!_connection.IsConnected)
			{
				_connection.TryConnect();
			}

			var channel = _connection.CreateModel();
			var basicConsumer = new AsyncEventingBasicConsumer(channel);
			channel.QueueDeclare(QueueKey,
				true,
				false,
				true,
				null);
			channel.QueueBind(QueueKey, _options.Exchange, QueueKey);
			basicConsumer.Received += async (model, ea) =>
			{
				try
				{
					await base.PublishAsync(QueueKey, ea.Body.ToArray());
					
				}
				finally
				{
					channel.BasicAck(ea.DeliveryTag, false);
				}
			};
			//consumer.OnClosing += x =>
			//{
			//	channel.Close();
			//};
			//7. 启动消费者
			channel.BasicConsume(QueueKey, false, basicConsumer);

			return task;
		}

		public override void Dispose()
		{
			base.Dispose();
			_connection?.Dispose();
			_publishChannel?.Dispose();
		}
	}

	public class PersistentConnection : IDisposable
	{
		private readonly IConnectionFactory _connectionFactory;
		private readonly ILogger<PersistentConnection> _logger;
		private readonly int _retryCount;
		private IConnection _connection;
		private bool _disposed;
		private readonly object _syncLocker = new object();

		public PersistentConnection(IConnectionFactory connectionFactory,
			ILogger<PersistentConnection> logger, int retryCount = 5)
		{
			_connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_retryCount = retryCount;
		}

		public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

		public bool TryConnect()
		{
			_logger.LogInformation("RabbitMQ Client is trying to connect");

			lock (_syncLocker)
			{
				var policy = Policy.Handle<SocketException>()
					.Or<BrokerUnreachableException>()
					.WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
						(ex, time) =>
						{
							_logger.LogWarning(ex,
								"RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})",
								$"{time.TotalSeconds:n1}", ex.Message);
						}
					);

				policy.Execute(() =>
				{
					_connection = _connectionFactory
						.CreateConnection();
				});

				if (IsConnected)
				{
					_connection.ConnectionShutdown += OnConnectionShutdown;
					_connection.CallbackException += OnCallbackException;
					_connection.ConnectionBlocked += OnConnectionBlocked;

					_logger.LogInformation(
						"RabbitMQ Client acquired a persistent connection to '{HostName}' and is subscribed to failure events",
						_connection.Endpoint.HostName);

					return true;
				}
				else
				{
					_logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");

					return false;
				}
			}
		}

		public IModel CreateModel()
		{
			if (!IsConnected)
			{
				throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
			}

			return _connection.CreateModel();
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			try
			{
				_connection.Dispose();
			}
			catch (IOException ex)
			{
				_logger.LogCritical(ex.ToString());
			}
		}

		private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
		{
			if (_disposed) return;

			_logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

			TryConnect();
		}

		private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
		{
			if (_disposed) return;

			_logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

			TryConnect();
		}

		private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
		{
			if (_disposed) return;

			_logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

			TryConnect();
		}
	}
}
