namespace RS.Commons.Web.TreeView
{
    /// <summary>
    /// 该类为树对应的实体类，字段及名称不得随意更改，详情可以参考Jqgrid官方数据传递方法解说说明 创建人:罗思军
    /// </summary>
    public class TreeViewModel
    {
        /// <summary>
        /// 父键值 树得上一级 即父级 
        /// </summary>
        public string parentId { get; set; }

        /// <summary>
        /// 行得主键 
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 树名称 
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// 树键值 
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// 树是否被选中 
        /// </summary>
        public int? checkstate { get; set; }

        /// <summary>
        /// 是否显示多选框 
        /// </summary>
        public bool showcheck { get; set; }

        /// <summary>
        /// 是否完成 
        /// </summary>
        public bool complete { get; set; }

        /// <summary>
        /// 是否展开 
        /// </summary>
        public bool isexpand { get; set; }

        /// <summary>
        /// 是否有子集 
        /// </summary>
        public bool hasChildren { get; set; }

        /// <summary>
        /// 对应得自定义图片 
        /// </summary>
        public string img { get; set; }

        /// <summary>
        /// 名称 
        /// </summary>
        public string title { get; set; }
    }
}
