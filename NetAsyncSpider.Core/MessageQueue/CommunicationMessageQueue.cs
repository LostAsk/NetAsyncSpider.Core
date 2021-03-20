using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.MessageQueue
{
	public class CommunicationMessage : CommunicationMessage<byte[]>, ICommunicationMessage {
		public CommunicationMessage(IServiceProvider serviceProvider) : base(serviceProvider) { }
	}

	public class CommunicationMessage<T> :BaseCommunicationMessage<T>, ICommunicationMessage<T>
	{
		public CommunicationMessage(IServiceProvider serviceProvider) : base(serviceProvider) { }

	}
}
