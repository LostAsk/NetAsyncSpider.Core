using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
namespace NetAsyncSpider.Core.DownProvider
{
    internal class NetCoreDefaultHttpMessageHandlerBuilder : HttpMessageHandlerBuilder
    {
        public NetCoreDefaultHttpMessageHandlerBuilder(IServiceProvider services)
        {
            Services = services;
        }

        private string _name;

        public override string Name
        {
            get => _name;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _name = value;
            }
        }

        public override HttpMessageHandler PrimaryHandler { get; set; } = new HttpClientProxyHandler();

        public override IList<DelegatingHandler> AdditionalHandlers { get; } = new List<DelegatingHandler>();

        public override IServiceProvider Services { get; }

        public override HttpMessageHandler Build()
        {
            if (PrimaryHandler == null)
            {
                throw new InvalidOperationException("PrimaryHandler必须设置");
            }

            return CreateHandlerPipeline(PrimaryHandler, AdditionalHandlers);
        }

    }
}
