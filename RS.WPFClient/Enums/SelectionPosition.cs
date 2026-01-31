namespace RS.WPFClient.Enums
{
    /// <summary>
    /// 描述邮件项在选中块中的位置
    /// </summary>
    public enum SelectionPosition
    {
        /// <summary>
        /// 未选中
        /// </summary>
        None,
        /// <summary>
        /// 独立选中（上下均未选中）
        /// </summary>
        Single,
        /// <summary>
        /// 选中块的顶部（上方未选中，下方已选中）
        /// </summary>
        Top,
        /// <summary>
        /// 选中块的中间（上下均已选中）
        /// </summary>
        Middle,
        /// <summary>
        /// 选中块的底部（上方已选中，下方未选中）
        /// </summary>
        Bottom
    }
}
