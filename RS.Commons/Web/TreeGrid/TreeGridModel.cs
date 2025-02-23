using System;

namespace RS.Commons.Web.TreeGrid
{
    /// <summary>
    /// 该类为Tree得核心类详情可参考Tree得光放API。方法收集来源网络整理而得 
    /// </summary>
    public class TreeGridModel
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 父键
        /// </summary>
        public string parentId { get; set; }

        /// <summary>
        /// 文本类容
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// 是否有展开图标
        /// </summary>
        public bool isLeaf { get; set; }

        /// <summary>
        /// 是否展开
        /// </summary>
        public bool expanded { get; set; }

        /// <summary>
        /// 是否显示加载
        /// </summary>
        public bool loaded { get; set; }

        /// <summary>
        /// 每一行所对那个得实体Json数据
        /// </summary>
        public string entityJson { get; set; }
       
        public object sortby { get; set; }
        public object thenby { get; set; }
    }
}
