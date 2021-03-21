using System;
using System.Collections.Generic;
namespace NetAsyncSpider.Core
{
	/// <summary>
	/// 全局配置
	/// </summary>
	public class SpiderOptions
	{

		/// <summary>
		/// 请求链接深度限制
		/// </summary>
		public int Depth { get; set; } = 3;

		/// <summary>
		/// 请求重试次数限制
		/// </summary>
		public int RetriedTimes { get; set; } = 0;

		/// <summary>
		/// 当队列中无链接超时后退出爬虫 (秒)<br></br>
		/// </summary>
		public int EmptySleepTime { get; set; } = 10;

		/// <summary>
		/// 爬虫采集速度，1 表示 1 秒钟一个请求，0.5 表示 1 秒钟 0.5 个请求，5 表示 1 秒钟 5 个请求 一般不超过系统的默认并发20
		/// </summary>
		private double speed = 20;
		public double Speed { get => speed; set => speed = Math.Abs(value) <= 50 ? Math.Abs(value) : 50; }

		/// <summary>
		/// 一次请求队列获取多少个请求
		/// </summary>
		public uint Batch { get; set; } = 2;
		/// <summary>
		/// 每次请求队列间隔时间,默认2秒
		/// </summary>
		public uint Delay { get; set; } = 2;
		/// <summary>
		/// 管道处理中间件
		/// </summary>
		public List<string> ItemPipelines { get; set; } = new List<string>();
		/// <summary>
		/// 下载中间件
		/// </summary>
		public List<string> DownloaderPipelines { get; set; } = new List<string>();
		/// <summary>
		/// 额外参数
		/// </summary>
		public dynamic Object { get;set; }
	}
}
