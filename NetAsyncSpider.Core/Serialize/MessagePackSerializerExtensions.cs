using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetAsyncSpider.Core;
using MessagePack;
using NetAsyncSpider.Core.Untils;

namespace NetAsyncSpider.Core.Serialize
{
	public static class MessagePackSerializerExtensions
	{
		private static readonly MessagePackSerializerOptions _serializerOptions =
			MessagePackSerializer.Typeless.DefaultOptions.WithCompression(MessagePackCompression.Lz4Block);

		public static byte[] Serialize(this object message)
		{
			message.NotNull(nameof(message));

				var bytes = MessagePackSerializer.Typeless.Serialize(message, _serializerOptions);
				return bytes;

			
		}

		public static object Deserialize(this byte[] bytes)
		{
			var stream = new MemoryStream(bytes);
			return  MessagePackSerializer.Typeless.Deserialize(stream, _serializerOptions);
		}

		public static T Deserialize<T>(this byte[] bytes)
			where T : class
		{
			var result = bytes.Deserialize();
			return result as T;
		}

		public static async Task<object> DeserializeAsync(this byte[] bytes,
			CancellationToken cancellationToken = default)
		{
			var stream = new MemoryStream(bytes);
			return await MessagePackSerializer.Typeless.DeserializeAsync(stream, _serializerOptions, cancellationToken);
		}

		public static async Task<T> DeserializeAsync<T>(this byte[] bytes,
			CancellationToken cancellationToken = default)
			where T : class
		{
			var result = await bytes.DeserializeAsync(cancellationToken);
			return result as T;
		}
	}
}
