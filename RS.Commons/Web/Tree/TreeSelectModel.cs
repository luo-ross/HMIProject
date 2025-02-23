namespace RS.Commons.Web.Tree
{
    public class TreeSelectModel
    {
        public string id { get; set; }
        public string text { get; set; }
        public string parentId { get; set; }
        public object data { get; set; }

        public object  sortby { get; set; }

        public object thenby { get; set; }
    }
}
