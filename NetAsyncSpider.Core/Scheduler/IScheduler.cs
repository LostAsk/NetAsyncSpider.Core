using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;

namespace NetAsyncSpider.Core.Scheduler
{
	/// <summary>
	/// 调度器接口
	/// </summary>
	public interface IScheduler :IDisposable
	{
		
		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="spiderId"></param>
		Task InitializeAsync(string spiderId);

		/// <summary>
		/// 获取请求队列
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		Task<IRequestParam[]> DequeueAsync(int count = 1);
		/// <summary>
		/// 请求入队
		/// </summary>
		/// <param name="requests">请求</param>
		/// <returns>入队个数</returns>
		Task EnqueueAsync(IRequestParam requestParam,IResponseParam responseParam,Func<IServiceProvider, BaseSpider, IResponseParam, Task> parse_handler = null, Func<IgnoreRequestException, Task> ignore_handler = null);


		/// <summary>
		/// 第一次请求入队
		/// </summary>
		/// <param name="requests">请求</param>
		/// <returns>入队个数</returns>
	    Task FirstEnqueueAsync(IRequestParam requestParam, Func<IServiceProvider, BaseSpider, IResponseParam, Task> parse_handler=null, Func<IgnoreRequestException, Task> ignore_handler = null);
		/// <summary>
		/// 队列中的总请求个数
		/// </summary>
		long GetTotal();

		/// <summary>
		/// 根据请求获取处理响应的委托
		/// </summary>
		/// <param name="resquest_hash"></param>
		/// <returns>(处理响应委托,IgnoreRequestException异常委托)</returns>
		(Func<IServiceProvider,BaseSpider, IResponseParam, Task>,Func<IgnoreRequestException,Task>) GetPaseHanderByRequestsHash(string resquest_hash);

		/// <summary>
		/// 重置
		/// </summary>
		/// <returns></returns>
		Task ResetDuplicateCheckAsync();

	}
}
