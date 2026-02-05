using CommunityToolkit.Mvvm.Input;
using RS.Widgets.Models;
using RS.Widgets.Utilities;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RS.Widgets.Controls
{
    public class RSMultiSelectTextBox : TextBox
    {
        private Grid PART_GridHost;
        private Grid PART_TagHost;
        private ScrollViewer PART_ContentHost;
        private ScrollViewer? PART_TagScrollViewer;
        private ItemsControl PART_TagContent;
        private Button PART_ClearAllButton;
        private ToggleButton? PART_MoreButton;
        private int selectedTagIndex = -1;
        private ObservableCollection<TagModel>? currentTagModelList;

        public event Action<List<TagModel>> OnTagModelDeleteCallBack;
        static RSMultiSelectTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSMultiSelectTextBox), new FrameworkPropertyMetadata(typeof(RSMultiSelectTextBox)));
        }

        public RSMultiSelectTextBox()
        {
            // 初始化 TagModelList
            TagModelList = new ObservableCollection<TagModel>();
            
            SetValue(TagCloseCommandPropertyKey, new RelayCommand<TagModel>(TagCloseExecute));
            SetValue(TagSelectCommandPropertyKey, new RelayCommand<TagModel>(TagSelectExecute));
            SetValue(ClearAllTagsCommandPropertyKey, new RelayCommand(ClearAllTags));
            this.TextChanged += RSMultiSelectTextBox_TextChanged;
            this.SizeChanged += RSMultiSelectTextBox_SizeChanged;
            this.PreviewKeyDown += RSMultiSelectTextBox_PreviewKeyDown;
            this.LostFocus += RSMultiSelectTextBox_LostFocus;
            this.GotFocus += RSMultiSelectTextBox_GotFocus;
        }

        private void RSMultiSelectTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateTagHostLayout();
            // 滚动到最后一个标签，让最后一个标签和输入框看起来连在一起
            ScrollToLastTag();
        }

        private void RSMultiSelectTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ResetTagContentMargin();
            this.UpdateTagHostLayout();
        }

        /// <summary>
        /// 重置Tag内容区域的边距为零
        /// </summary>
        internal void ResetTagContentMargin()
        {
            if (this.PART_TagContent == null)
            {
                return;
            }
            this.PART_TagContent.Margin = new Thickness(0, 0, 0, 0);
        }


        /// <summary>
        /// 显示标签内容的属性路径。
        /// </summary>
        public string TagDisplayMemberPath
        {
            get { return (string)GetValue(TagDisplayMemberPathProperty); }
            set { SetValue(TagDisplayMemberPathProperty, value); }
        }

        public static readonly DependencyProperty TagDisplayMemberPathProperty =
            DependencyProperty.Register(nameof(TagDisplayMemberPath), typeof(string), typeof(RSMultiSelectTextBox), new PropertyMetadata(null));

        /// <summary>
        /// 设置已选择的数据项集合（双向绑定）。
        /// </summary>
        public IEnumerable MultiSelectedItems
        {
            get { return (IEnumerable)GetValue(MultiSelectedItemsProperty); }
            set { SetValue(MultiSelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty MultiSelectedItemsProperty =
            DependencyProperty.Register(nameof(MultiSelectedItems), typeof(IEnumerable), typeof(RSMultiSelectTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMultiSelectedItemsChanged));

        private static void OnMultiSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RSMultiSelectTextBox)d;
            control.SyncTagsFromBoundItems();
        }

        /// <summary>
        /// 从绑定的MultiSelectedItems同步标签数据到TagModelList
        /// </summary>
        private void SyncTagsFromBoundItems()
        {
            if (this.MultiSelectedItems == null)
            {
                return;
            }

            TagModelList.Clear();
            foreach (var item in this.MultiSelectedItems)
            {
                var tagModel = CreateTagModel(item);
                // 检查是否已存在，避免重复添加
                if (!TagModelList.Contains(tagModel))
                {
                    TagModelList.Add(tagModel);
                }
            }

            // 不需要同步回 MultiSelectedItems，因为数据源就是 MultiSelectedItems
            UpdateHasContent();
        }

        /// <summary>
        /// 创建 TagModel 实例
        /// </summary>
        private TagModel CreateTagModel(object data)
        {
            return new TagModel()
            {
                TagContent = ReflectionHelper.GetDisplayMemberValue(data, this.TagDisplayMemberPath) ?? data,
                IsSelect = false,
                Data = data
            };
        }

        /// <summary>
        /// 同步 TagModelList 到 MultiSelectedItems
        /// </summary>
        private void SyncToMultiSelectedItems()
        {
            var multiSelectedItems = this.MultiSelectedItems as IList;
            if (multiSelectedItems == null)
            {
                return;
            }

            multiSelectedItems.Clear();
            foreach (var tagModel in TagModelList)
            {
                if (tagModel.Data != null)
                {
                    multiSelectedItems.Add(tagModel.Data);
                }
            }
        }

        /// <summary>
        /// 处理标签选择命令
        /// </summary>
        private void TagSelectExecute(TagModel? tag)
        {
            if (tag == null)
            {
                return;
            }

            // 找到点击的Tag的索引
            int index = this.TagModelList.IndexOf(tag);
            if (index >= 0)
            {
                SelectTag(index);
            }

            this.UpdateTagHostLayout();
        }

        /// <summary>
        /// 关闭（删除）标签的命令。
        /// </summary>
        public ICommand TagCloseCommand => (ICommand)GetValue(TagCloseCommandProperty);

        private static readonly DependencyPropertyKey TagCloseCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TagCloseCommand),
                typeof(ICommand),
                typeof(RSMultiSelectTextBox),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TagCloseCommandProperty = TagCloseCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// 选择标签的命令。
        /// </summary>
        public ICommand TagSelectCommand => (ICommand)GetValue(TagSelectCommandProperty);

        private static readonly DependencyPropertyKey TagSelectCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TagSelectCommand),
                typeof(ICommand),
                typeof(RSMultiSelectTextBox),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TagSelectCommandProperty = TagSelectCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// 清除所有标签的命令。
        /// </summary>
        public ICommand ClearAllTagsCommand => (ICommand)GetValue(ClearAllTagsCommandProperty);

        private static readonly DependencyPropertyKey ClearAllTagsCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ClearAllTagsCommand),
                typeof(ICommand),
                typeof(RSMultiSelectTextBox),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ClearAllTagsCommandProperty = ClearAllTagsCommandPropertyKey.DependencyProperty;




        public GridLength TagColumnWidth
        {
            get { return (GridLength)GetValue(TagColumnWidthProperty); }
            private set { SetValue(TagColumnWidthPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey TagColumnWidthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TagColumnWidth),
                typeof(GridLength),
                typeof(RSMultiSelectTextBox),
                new PropertyMetadata(GridLength.Auto));

        public static readonly DependencyProperty TagColumnWidthProperty = TagColumnWidthPropertyKey.DependencyProperty;



        /// <summary>
        /// 当前选中的Tag索引，-1表示没有Tag被选中（焦点在文本输入区）
        /// </summary>
        public int SelectedTagIndex
        {
            get { return selectedTagIndex; }
            private set
            {
                if (selectedTagIndex != value)
                {
                    selectedTagIndex = value;
                    UpdateTagSelectionVisual();
                }
            }
        }


        private void RSMultiSelectTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    // 当光标在输入框最左端或已有Tag被选中时，处理Tag导航
                    if (this.CaretIndex == 0 || this.SelectedTagIndex >= 0)
                    {
                        if (this.TagModelList.Count == 0)
                        {
                            return;
                        }

                        if (this.SelectedTagIndex < 0)
                        {
                            // 没有Tag被选中，选中最后一个Tag
                            SelectTag(this.TagModelList.Count - 1);
                        }
                        else if (this.SelectedTagIndex > 0)
                        {
                            // 选中前一个Tag
                            SelectTag(this.SelectedTagIndex - 1);
                        }
                        // 如果已经是第一个Tag，不做任何操作
                        e.Handled = true;
                    }
                    break;

                case Key.Right:
                    // 只有当有Tag被选中时才处理
                    if (this.SelectedTagIndex >= 0)
                    {
                        if (this.SelectedTagIndex < this.TagModelList.Count - 1)
                        {
                            // 选中下一个Tag
                            SelectTag(this.SelectedTagIndex + 1);
                        }
                        else
                        {
                            // 当前是最后一个Tag，取消选中并返回输入框
                            ClearTagSelect();
                            this.CaretIndex = 0;
                        }
                        e.Handled = true;
                    }
                    break;

                case Key.Enter:
                    // 按回车创建标签
                    if (!string.IsNullOrWhiteSpace(this.Text))
                    {
                        CreateTagFromText();
                        e.Handled = true;
                    }
                    break;

                case Key.Back:
                    if (this.SelectedTagIndex >= 0)
                    {
                        // 有Tag被选中，删除它
                        DeleteSelectedTag();
                        e.Handled = true;
                    }
                    else if (this.Text.Length == 0 && this.TagModelList.Count > 0)
                    {
                        // 输入框为空，删除最后一个标签
                        var lastTag = this.TagModelList[this.TagModelList.Count - 1];
                        TagCloseExecute(lastTag);
                        e.Handled = true;
                    }
                    break;

                case Key.Delete:
                    if (this.SelectedTagIndex >= 0)
                    {
                        // 有Tag被选中，删除它
                        DeleteSelectedTag();
                        e.Handled = true;
                    }
                    break;

                case Key.Escape:
                    // 取消所有Tag的选中状态
                    if (this.SelectedTagIndex >= 0)
                    {
                        ClearTagSelect();
                        e.Handled = true;
                    }
                    break;
            }
        }



        /// <summary>
        /// 处理标签关闭命令
        /// </summary>
        private void TagCloseExecute(TagModel? tag)
        {
            if (tag == null)
            {
                return;
            }

            int index = this.TagModelList.IndexOf(tag);
            if (index < 0)
            {
                return;
            }

            // 保存当前选中索引（删除前的）
            int currentSelectedIndex = this.SelectedTagIndex;

            // 从 TagModelList 中移除
            TagModelList.Remove(tag);

            // 同步到 MultiSelectedItems
            SyncToMultiSelectedItems();
            UpdateHasContent();

            // 调整选中索引
            // 删除标签后，清除选中状态，避免继续按Backspace时连续删除
            if (this.TagModelList.Count == 0)
            {
                SelectedTagIndex = -1;
            }
            else
            {
                // 如果删除的是当前选中的标签，清除选中状态（不自动选中其他标签）
                if (currentSelectedIndex == index)
                {
                    SelectedTagIndex = -1;
                }
                else if (currentSelectedIndex > index)
                {
                    // 删除的标签在当前选中标签之前，需要将选中索引减1
                    SelectTag(currentSelectedIndex - 1);
                }
                // 如果 currentSelectedIndex < index，删除的标签在当前选中标签之后，不需要调整
            }

            OnTagModelDeleteCallBack?.Invoke(new List<TagModel>() { tag });
        }

        private void RSMultiSelectTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateTagHostLayout();
        }

        /// <summary>
        /// 是否有内容（包括文本输入或标签）。
        /// </summary>
        public bool HasContent
        {
            get { return (bool)GetValue(HasContentProperty); }
            set { SetValue(HasContentProperty, value); }
        }

        public static readonly DependencyProperty HasContentProperty =
            DependencyProperty.Register(nameof(HasContent), typeof(bool), typeof(RSMultiSelectTextBox), new FrameworkPropertyMetadata(false,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 获是否包含至少一个标签（只读）。
        /// </summary>
        public bool HasTags
        {
            get { return (bool)GetValue(HasTagsProperty); }
            private set { SetValue(HasTagsPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey HasTagsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(HasTags), typeof(bool), typeof(RSMultiSelectTextBox), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasTagsProperty = HasTagsPropertyKey.DependencyProperty;



        /// <summary>
        /// 设置标签模型集合（双向绑定）。
        /// </summary>
        public ObservableCollection<TagModel> TagModelList
        {
            get { return (ObservableCollection<TagModel>)GetValue(TagModelListProperty); }
            set { SetValue(TagModelListProperty, value); }
        }

        public static readonly DependencyProperty TagModelListProperty =
            DependencyProperty.Register(nameof(TagModelList), typeof(ObservableCollection<TagModel>), typeof(RSMultiSelectTextBox), new FrameworkPropertyMetadata(new ObservableCollection<TagModel>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTagModelListChanged));

        private static void OnTagModelListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RSMultiSelectTextBox)d;
            control.OnTagModelListChanged((ObservableCollection<TagModel>?)e.OldValue, (ObservableCollection<TagModel>?)e.NewValue);
        }

        private void OnTagModelListChanged(ObservableCollection<TagModel>? oldValue, ObservableCollection<TagModel>? newValue)
        {
            // 取消订阅旧的集合的CollectionChanged事件
            if (oldValue != null && oldValue == currentTagModelList)
            {
                oldValue.CollectionChanged -= TagModelList_CollectionChanged;
                currentTagModelList = null;
            }

            // 订阅新的集合的CollectionChanged事件
            if (newValue != null)
            {
                newValue.CollectionChanged += TagModelList_CollectionChanged;
                currentTagModelList = newValue;
            }

            // 更新HasTags属性和布局
            UpdateHasContent();
            UpdateTagHostLayout();
        }

        private void TagModelList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 当TagModelList集合发生变化时，更新HasTags属性和布局
            UpdateHasContent();
            UpdateTagHostLayout();
        }

        private void RSMultiSelectTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 用户输入文本时，取消Tag的选中状态
            if (this.SelectedTagIndex >= 0 && this.Text.Length > 0)
            {
                ClearTagSelect();
            }
            this.UpdateHasContent();
            this.UpdateTagHostLayout();
            // 保持最后一个标签可见，让标签和输入框看起来连在一起
            ScrollToLastTag();
        }

        /// <summary>
        /// 更新标签宿主区域的布局（宽度分配）
        /// </summary>
        private void UpdateTagHostLayout()
        {
            if (this.PART_GridHost == null || this.PART_TagHost == null || this.PART_TagContent == null)
            {
                return;
            }

            var gridHostActualWidth = this.PART_GridHost.ActualWidth;
            var tagContentActualWidth = this.PART_TagContent.ActualWidth;
            var hasTags = this.TagModelList != null && this.TagModelList.Count > 0;

            // 如果没有Tag，TagColumnWidth设为0，输入框占满
            if (!hasTags)
            {
                this.TagColumnWidth = new GridLength(0);
                return;
            }

            // 如果没有焦点，TagColumnWidth设为Auto，让TagList占满（输入框会被隐藏）
            if (!this.IsFocused)
            {
                this.TagColumnWidth = GridLength.Auto;
                return;
            }

            // 有Tag且有焦点的情况
            // 计算More按钮和清除按钮的宽度
            double moreButtonWidth = this.PART_MoreButton?.ActualWidth ?? 0;
            double clearButtonWidth = (this.PART_ClearAllButton?.Visibility == Visibility.Visible) ? (this.PART_ClearAllButton?.ActualWidth ?? 0) : 0;
            double buttonsWidth = moreButtonWidth + clearButtonWidth;

            // 输入框的最小宽度（用于计算，但不限制TagColumnWidth）
            const double minTextBoxWidth = 50;

            // 计算可用宽度（控件宽度 - 按钮宽度）
            double availableWidth = gridHostActualWidth - buttonsWidth;

            // 输入框的实际内容宽度（使用ExtentWidth，如果为0则使用最小宽度）
            double textBoxContentWidth = minTextBoxWidth;
            if (this.PART_ContentHost != null && this.PART_ContentHost.ExtentWidth > 0)
            {
                // 使用ExtentWidth，但至少保证最小宽度
                textBoxContentWidth = Math.Max(minTextBoxWidth, this.PART_ContentHost.ExtentWidth);
            }

            // 如果输入框内容宽度 > 可用宽度，优先让Tag向左滚动（减少TagColumnWidth）
            // 直到TagColumnWidth为0（输入框对齐左边），然后输入框文本才会滚动
            if (textBoxContentWidth > availableWidth)
            {
                // 输入框内容超出，优先让Tag让出空间，TagColumnWidth设为0
                // 这样Tag会向左滚动，输入框对齐到左边，然后输入框文本才会滚动
                this.TagColumnWidth = new GridLength(0);
            }
            else
            {
                // 输入框内容未超出，计算总宽度需求
                double totalRequiredWidth = tagContentActualWidth + textBoxContentWidth;

                // 如果总需求宽度 <= 可用宽度，TagList使用Auto（占满所需空间）
                if (totalRequiredWidth <= availableWidth)
                {
                    this.TagColumnWidth = GridLength.Auto;
                }
                else
                {
                    // 如果总需求宽度 > 可用宽度，但输入框未超出，让TagList让出部分空间
                    // TagList的最大宽度 = 可用宽度 - 输入框内容宽度
                    double maxTagColumnWidth = availableWidth - textBoxContentWidth;
                    this.TagColumnWidth = new GridLength(Math.Max(0, maxTagColumnWidth));
                }
            }
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_ContentHost = this.GetTemplateChild(nameof(PART_ContentHost)) as ScrollViewer;
            this.PART_GridHost = this.GetTemplateChild(nameof(PART_GridHost)) as Grid;
            this.PART_TagHost = this.GetTemplateChild(nameof(PART_TagHost)) as Grid;
            this.PART_TagContent = this.GetTemplateChild(nameof(PART_TagContent)) as ItemsControl;
            this.PART_ClearAllButton = this.GetTemplateChild(nameof(PART_ClearAllButton)) as Button;
            this.PART_MoreButton = this.GetTemplateChild(nameof(PART_MoreButton)) as ToggleButton;

            // 获取 Tag 的 ScrollViewer
            if (this.PART_TagHost != null)
            {
                this.PART_TagScrollViewer = this.PART_TagHost.Children.OfType<ScrollViewer>().FirstOrDefault();
            }

            if (this.PART_ClearAllButton != null)
            {
                this.PART_ClearAllButton.Click -= PART_ClearAllButton_Click;
                this.PART_ClearAllButton.Click += PART_ClearAllButton_Click;
            }

            if (this.PART_TagContent != null)
            {
                this.PART_TagContent.SizeChanged += PART_TagContent_SizeChanged;
            }

            // 初始化布局
            this.UpdateTagHostLayout();
        }

        private void PART_TagContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateTagHostLayout();
        }


        private void PART_ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAllTags();
        }

        /// <summary>
        /// 清除所有标签
        /// </summary>
        private void ClearAllTags()
        {
            if (this.TagModelList.Count == 0)
            {
                return;
            }

            List<TagModel> deletedTags = this.TagModelList.ToList();

            // 清除所有Tag
            TagModelList.Clear();
            this.Text = string.Empty;

            // 同步到 MultiSelectedItems
            SyncToMultiSelectedItems();
            UpdateHasContent();

            // 清除选中索引
            SelectedTagIndex = -1;

            OnTagModelDeleteCallBack?.Invoke(deletedTags);
        }


        private void UpdateHasContent()
        {
            var hasContent = this.Text.Length > 0 || this.TagModelList.Count > 0;
            this.HasContent = hasContent;
            this.HasTags = this.TagModelList != null && this.TagModelList.Count > 0;
        }

        /// <summary>
        /// 从当前输入的文本创建标签
        /// </summary>
        private void CreateTagFromText()
        {
            string inputText = this.Text.Trim();
            if (string.IsNullOrEmpty(inputText))
            {
                return;
            }

            // 创建标签数据对象（使用文本作为数据）
            object tagData = inputText;

            // 创建 TagModel
            var tagModel = CreateTagModel(tagData);

            // 如果已经存在相同的标签，不重复添加
            if (TagModelList.Contains(tagModel))
            {
                this.Text = string.Empty;
                return;
            }

            // 添加到 TagModelList
            TagModelList.Add(tagModel);

            // 同步到 MultiSelectedItems
            SyncToMultiSelectedItems();
            UpdateHasContent();

            // 清空输入框
            this.Text = string.Empty;

            // 滚动到最后一个标签，让最新添加的标签可见
            ScrollToLastTag();

            // 保持焦点在输入框
            this.Focus();
        }

        private void ClearTagSelect()
        {
            this.SelectedTagIndex = -1;
        }

        /// <summary>
        /// 选中指定索引的标签
        /// </summary>
        private void SelectTag(int tagIndex)
        {
            if (tagIndex < 0 || this.TagModelList.Count == 0)
            {
                this.SelectedTagIndex = -1;
                return;
            }

            if (tagIndex >= this.TagModelList.Count)
            {
                tagIndex = this.TagModelList.Count - 1;
            }

            this.SelectedTagIndex = tagIndex;

            // 使选中的标签滚动到可见位置
            ScrollTagIntoView(tagIndex);
        }

        /// <summary>
        /// 滚动到最后一个标签，让最后一个标签和输入框看起来连在一起
        /// </summary>
        private void ScrollToLastTag()
        {
            if (this.PART_TagScrollViewer == null || this.TagModelList == null || this.TagModelList.Count == 0)
            {
                return;
            }

            // 使用 Dispatcher 确保 UI 已经更新
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                try
                {
                    // 滚动到最右边（最后一个标签）
                    this.PART_TagScrollViewer.ScrollToHorizontalOffset(this.PART_TagScrollViewer.ExtentWidth);
                }
                catch
                {
                    // 忽略错误，避免影响正常功能
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// 将指定索引的标签滚动到可见位置
        /// </summary>
        private void ScrollTagIntoView(int tagIndex)
        {
            if (this.PART_TagContent == null || tagIndex < 0 || tagIndex >= this.TagModelList.Count)
            {
                return;
            }

            // 使用 Dispatcher 确保 UI 已经更新
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                try
                {
                    // 通过 ItemContainerGenerator 获取容器
                    var generator = this.PART_TagContent.ItemContainerGenerator;
                    if (generator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        var container = generator.ContainerFromIndex(tagIndex) as FrameworkElement;
                        if (container != null)
                        {
                            // 查找 Border 元素（标签的根元素）
                            var border = container.FindChild<Border>("PART_TagItemBorder");
                            if (border != null)
                            {
                                border.BringIntoView();
                            }
                            else
                            {
                                // 如果找不到 Border，直接让容器滚动到可见位置
                                container.BringIntoView();
                            }
                        }
                    }
                }
                catch
                {
                    // 忽略错误，避免影响正常功能
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// 根据SelectedTagIndex更新所有标签的选中状态视觉效果
        /// </summary>
        private void UpdateTagSelectionVisual()
        {
            if (this.TagModelList == null)
            {
                return;
            }

            for (int tagIndex = 0; tagIndex < this.TagModelList.Count; tagIndex++)
            {
                this.TagModelList[tagIndex].IsSelect = (tagIndex == this.SelectedTagIndex);
            }
        }

        /// <summary>
        /// 删除当前选中的标签
        /// </summary>
        private void DeleteSelectedTag()
        {
            if (this.SelectedTagIndex < 0 || this.SelectedTagIndex >= this.TagModelList.Count)
            {
                return;
            }

            var selectedTag = this.TagModelList[this.SelectedTagIndex];
            TagCloseExecute(selectedTag);
        }
    }
}
