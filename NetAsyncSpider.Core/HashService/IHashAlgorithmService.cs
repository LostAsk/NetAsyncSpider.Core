using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.HashService
{
    /// <summary>
    /// 计算嘻哈服务
    /// </summary>
    public interface IHashAlgorithmService
    {
        byte[] ComputeHash(byte[] bytes);
    }
}
