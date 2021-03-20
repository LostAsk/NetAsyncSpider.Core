using NetAsyncSpider.Core.Untils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core
{
	[Serializable]
	public class ResponseHeader: Dictionary<string, dynamic>
	{
		public ResponseHeader() { }
		public string AcceptRanges
		{
			get => GetHeader(HeaderNames.AcceptRanges);
			set => this[HeaderNames.AcceptRanges] = value;
		}

		public string ETag
		{
			get => GetHeader(HeaderNames.ETag);
			set => this[HeaderNames.ETag] = value;
		}

		public string Location
		{
			get => GetHeader(HeaderNames.Location);
			set => this[HeaderNames.Location] = value;
		}

		public string ProxyAuthenticate
		{
			get => GetHeader(HeaderNames.ProxyAuthenticate) as string;
			set => this[HeaderNames.ProxyAuthenticate] = value;
		}

		public string RetryAfter
		{
			get => GetHeader(HeaderNames.RetryAfter);
			set => this[HeaderNames.RetryAfter] = value;
		}

		public string Server
		{
			get => GetHeader(HeaderNames.Server);
			set => this[HeaderNames.Server] = value;
		}

		public string Vary
		{
			get => GetHeader(HeaderNames.Vary);
			set => this[HeaderNames.Vary] = value;
		}

		// ReSharper disable once InconsistentNaming
		public string WWWAuthenticate
		{
			get => GetHeader(HeaderNames.WWWAuthenticate);
			set => this[HeaderNames.WWWAuthenticate] = value;
		}

		public string CacheControl
		{
			get => GetHeader(HeaderNames.CacheControl);
			set => this[HeaderNames.CacheControl] = value;
		}

		public string Connection
		{
			get => GetHeader(HeaderNames.Connection);
			set => this[HeaderNames.Connection] = value;
		}

		public string Pragma
		{
			get => GetHeader(HeaderNames.Pragma);
			set => this[HeaderNames.Pragma] = value;
		}

		public string Trailer
		{
			get => GetHeader(HeaderNames.Trailer);
			set => this[HeaderNames.Trailer] = value;
		}

		public string TransferEncoding
		{
			get => GetHeader(HeaderNames.TransferEncoding);
			set => this[HeaderNames.TransferEncoding] = value;
		}

		public string Upgrade
		{
			get => GetHeader(HeaderNames.Upgrade);
			set => this[HeaderNames.Upgrade] = value;
		}

		public string Via
		{
			get => GetHeader(HeaderNames.Via);
			set => this[HeaderNames.Via] = value;
		}

		public string Warning
		{
			get => GetHeader(HeaderNames.Warning);
			set => this[HeaderNames.Warning] = value;
		}

		public DateTimeOffset? Date
		{
			get => GetHeader(HeaderNames.Date);
			set => this[HeaderNames.Date] = value;
		}

		public TimeSpan? Age
		{
			get => GetHeader(HeaderNames.Age);
			set => this[HeaderNames.Age] = value;
		}

		public Encoding Encoding
		{
			get => GetHeader(nameof(Encoding));
			set => this[nameof(Encoding)] = value;
		}

		public HttpStatusCode? HttpStatusCode {
			get => GetHeader(nameof(HttpStatusCode));
			set => this[nameof(HttpStatusCode)] = value;
		}

		public bool? ConnectionClose => Connection?.ToLower().Contains("close");

		public bool? TransferEncodingChunked { get; set; }

		private dynamic GetHeader(string name)
		{
			return ContainsKey(name) ? this[name] : null;
		}
	}
}
