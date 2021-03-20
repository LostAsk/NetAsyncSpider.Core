using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.Untils
{
	internal class HeaderDumpUtilities
	{
		internal static void DumpHeaders(StringBuilder sb, params Dictionary<string, dynamic>[] headers)
		{
			sb.AppendLine("{");
			foreach (var t in headers)
			{
				if (t == null)
				{
					continue;
				}

				foreach (var keyValuePair in t)
				{
					sb.Append("  ");
					sb.Append(keyValuePair.Key);
					sb.Append(": ");
					sb.AppendLine(keyValuePair.Value);
				}
			}

			sb.Append('}');
		}

		internal static void DumpHeaders<T>(StringBuilder sb, params Dictionary<string, T>[] headers)
		{
			if (headers == null) return;
			sb.AppendLine("{");
			foreach (var t in headers)
			{
				if (t == null)
				{
					continue;
				}

				foreach (var keyValuePair in t)
				{
					sb.Append("  ");
					sb.Append(keyValuePair.Key);
					sb.Append(": ");
					sb.AppendLine(keyValuePair.Value.ToString());
				}
			}

			sb.Append('}');
		}
	}
}
