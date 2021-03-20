using System.Security.Cryptography;

namespace NetAsyncSpider.Core.HashService
{
	public class MD5HashAlgorithmService : HashAlgorithmService
	{
		private readonly HashAlgorithm _hashAlgorithm;

		public MD5HashAlgorithmService()
		{
			_hashAlgorithm = MD5.Create();
		}

		protected override HashAlgorithm GetHashAlgorithm()
		{
			return _hashAlgorithm;
		}
	}
}
