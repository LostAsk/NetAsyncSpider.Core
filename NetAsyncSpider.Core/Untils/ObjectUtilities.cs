using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;

namespace NetAsyncSpider.Core.Untils
{
	/// <summary>
	/// Obj助手 主要提供dispose扩展
	/// </summary>
	public class ObjectUtilities
	{
		public static void DisposeSafely(params object[] objs)
		{
			foreach (var obj in objs)
			{
				try
				{
					(obj as IDisposable)?.Dispose();
				}
				catch (System.Exception)
				{
					// ignored
				}
			}
		}

		public static void DisposeSafely(ILogger logger, params object[] objs)
		{
			DisposeSafely(logger, objs.AsEnumerable());
		}

		public static void DisposeSafely(ILogger logger, IEnumerable<object> objs)
		{
			foreach (var obj in objs)
			{
				try
				{
					(obj as IDisposable)?.Dispose();
				}
				catch (System.Exception e)
				{
					logger.LogWarning($"Dispose {obj} failed: {e}");
				}
			}
		}

		public static void DisposeByDic(params IDictionary<string, dynamic>[] dics) {
			foreach (var dic in dics)
			{
				foreach (var ky in dic) {
					try
					{
						(ky.Value as IDisposable)?.Dispose();
					}
					catch (System.Exception e)
					{
                        Console.WriteLine($"Dispose {ky.Key} failed: {e}");
					}
				}
				dic.Clear();
			}

		}
	}
}
