using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.MessageQueue
{
	/// <summary>
	/// RabbitMQ配置
	/// </summary>
	public class RabbitMQOptions
	{
		public string Exchange { get; set; } = "DotnetSpider";

		public string HostName { get; set; }

		public int Port { get; set; }

		public string UserName { get; set; }

		public string Password { get; set; }

		public int RetryCount { get; set; } = 5;
	}
}
