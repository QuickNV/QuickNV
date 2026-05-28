using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickNV.Model
{
    /// <summary>
    /// 分页结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedListResult<T>
    {
        /// <summary>
        /// 总数量
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// 数据节点
        /// </summary>
        public T[] Root { get; set; }
    }
}
