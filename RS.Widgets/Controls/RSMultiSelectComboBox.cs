using CommunityToolkit.Mvvm.Input;
using RS.Widgets.Models;
using RS.Widgets.Utilities;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    /// <summary>
    /// 多选下拉框控件
    /// 功能说明：
    /// 1. 支持从下拉框中选择多个项，显示为标签形式
    /// 2. 支持搜索筛选功能
    /// 3. 支持键盘导航（左右箭头键选择标签，Backspace/Delete删除标签）
    /// 4. 支持Single和Extended两种选择模式
    /// </summary>
    public class RSMultiSelectComboBox : DataGrid
    {
        #region 私有字段

        // 模板部件
        private ItemsControl? PART_TagContent;      // 标签容器
        private TextBox? PART_SearchContent;        // 搜索框

        // 状态标志
        private bool isSyncingSelectedItems = false; // 正在同步选中项，避免触发SelectionChanged事件
        private bool isTagSelecting = false;         // 正在处理标签选择，防止ToggleDropDownCommand执行
        private ObservableCollection<TagModel>? currentTagModelList; // 当前订阅的TagModelList

        // 标签选中状态
        private int selectedTagIndex = -1;          // 当前选中的标签索引（-1表示未选中）

        #endregion

        #region 构造函数和初始化

        static RSMultiSelectComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSMultiSelectComboBox), new FrameworkPropertyMetadata(typeof(RSMultiSelectComboBox)));
        }

        public RSMultiSelectComboBox()
        {
            // 初始化 TagModelList
            TagModelList = new ObservableCollection<TagModel>();
            
            // 初始化命令
            SetValue(TagCloseCommandPropertyKey, new RelayCommand<TagModel>(TagCloseExecute));
            SetValue(TagSelectCommandPropertyKey, new RelayCommand<TagModel>(TagSelectExecute));
            SetValue(ClearAllTagsCommandPropertyKey, new RelayCommand(ClearAllTagsExecute));
            SetValue(ToggleDropDownCommandPropertyKey, new RelayCommand(ToggleDropDownExecute));
            
            // 订阅事件
            this.SelectionChanged += OnSelectionChanged;
            this.Columns.CollectionChanged += OnColumnsCollectionChanged;
            this.PreviewKeyDown += OnPreviewKeyDown;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            // 获取模板部件
            this.PART_TagContent = this.GetTemplateChild("PART_TagContent") as ItemsControl;
            this.PART_SearchContent = this.GetTemplateChild("PART_SearchContent") as TextBox;

            // 订阅搜索框事件
            if (this.PART_SearchContent != null)
            {
                this.PART_SearchContent.KeyDown += OnSearchContentKeyDown;
            }
        }

        #endregion

        #region 依赖属性定义

        /// <summary>
        /// 标签显示成员路径（用于从数据项中获取显示文本）
        /// </summary>
        public string TagDisplayMemberPath
        {
            get { return (string)GetValue(TagDisplayMemberPathProperty); }
            set { SetValue(TagDisplayMemberPathProperty, value); }
        }
        public static readonly DependencyProperty TagDisplayMemberPathProperty =
            DependencyProperty.Register(nameof(TagDisplayMemberPath), typeof(string), typeof(RSMultiSelectComboBox), new PropertyMetadata(null));

        /// <summary>
        /// 实际选择的数据集合（绑定到外部，只包含来自ItemsSource的数据）
        /// </summary>
        public IEnumerable MultiSelectedItems
        {
            get { return (IEnumerable)GetValue(MultiSelectedItemsProperty); }
            set { SetValue(MultiSelectedItemsProperty, value); }
        }
        public static readonly DependencyProperty MultiSelectedItemsProperty =
            DependencyProperty.Register(nameof(MultiSelectedItems), typeof(IEnumerable), typeof(RSMultiSelectComboBox), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 标签数据集合（包含所有标签，包括自定义标签）
        /// 注意：TagModelList 可能包含 Custom 类型的标签，这些不会出现在 MultiSelectedItems 中
        /// </summary>
        public ObservableCollection<TagModel> TagModelList
        {
            get { return (ObservableCollection<TagModel>)GetValue(TagModelListProperty); }
            set { SetValue(TagModelListProperty, value); }
        }
        public static readonly DependencyProperty TagModelListProperty =
            DependencyProperty.Register(nameof(TagModelList), typeof(ObservableCollection<TagModel>), typeof(RSMultiSelectComboBox), 
                new FrameworkPropertyMetadata(new ObservableCollection<TagModel>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTagModelListChanged));

        /// <summary>
        /// 下拉框最大高度
        /// </summary>
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }
        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(RSMultiSelectComboBox), new PropertyMetadata(300D));

        /// <summary>
        /// 下拉框是否打开
        /// </summary>
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(RSMultiSelectComboBox), 
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDropDownOpenChanged));

        /// <summary>
        /// 搜索框内容
        /// </summary>
        public string SearchContent
        {
            get { return (string)GetValue(SearchContentProperty); }
            set { SetValue(SearchContentProperty, value); }
        }
        public static readonly DependencyProperty SearchContentProperty =
            DependencyProperty.Register(nameof(SearchContent), typeof(string), typeof(RSMultiSelectComboBox), 
                new PropertyMetadata(null, OnSearchContentChanged));

        /// <summary>
        /// 是否显示搜索框
        /// </summary>
        public bool ShowSearchBox
        {
            get { return (bool)GetValue(ShowSearchBoxProperty); }
            set { SetValue(ShowSearchBoxProperty, value); }
        }
        public static readonly DependencyProperty ShowSearchBoxProperty =
            DependencyProperty.Register(nameof(ShowSearchBox), typeof(bool), typeof(RSMultiSelectComboBox), new PropertyMetadata(true));

        /// <summary>
        /// 是否有内容（是否有标签）
        /// </summary>
        public bool HasContent
        {
            get { return (bool)GetValue(HasContentProperty); }
            set { SetValue(HasContentProperty, value); }
        }
        public static readonly DependencyProperty HasContentProperty =
            DependencyProperty.Register(nameof(HasContent), typeof(bool), typeof(RSMultiSelectComboBox), new PropertyMetadata(false));

        /// <summary>
        /// 当前选中的标签索引（只读，用于内部状态管理）
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

        #endregion

        #region 命令定义

        private static readonly DependencyPropertyKey TagCloseCommandPropertyKey =
            DependencyProperty.RegisterReadOnly("TagCloseCommand", typeof(ICommand), typeof(RSMultiSelectComboBox), new PropertyMetadata(null));
        public static readonly DependencyProperty TagCloseCommandProperty = TagCloseCommandPropertyKey.DependencyProperty;
        public ICommand TagCloseCommand => (ICommand)GetValue(TagCloseCommandProperty);

        private static readonly DependencyPropertyKey TagSelectCommandPropertyKey =
            DependencyProperty.RegisterReadOnly("TagSelectCommand", typeof(ICommand), typeof(RSMultiSelectComboBox), new PropertyMetadata(null));
        public static readonly DependencyProperty TagSelectCommandProperty = TagSelectCommandPropertyKey.DependencyProperty;
        public ICommand TagSelectCommand => (ICommand)GetValue(TagSelectCommandProperty);

        private static readonly DependencyPropertyKey ClearAllTagsCommandPropertyKey =
            DependencyProperty.RegisterReadOnly("ClearAllTagsCommand", typeof(ICommand), typeof(RSMultiSelectComboBox), new PropertyMetadata(null));
        public static readonly DependencyProperty ClearAllTagsCommandProperty = ClearAllTagsCommandPropertyKey.DependencyProperty;
        public ICommand ClearAllTagsCommand => (ICommand)GetValue(ClearAllTagsCommandProperty);

        private static readonly DependencyPropertyKey ToggleDropDownCommandPropertyKey =
            DependencyProperty.RegisterReadOnly("ToggleDropDownCommand", typeof(ICommand), typeof(RSMultiSelectComboBox), new PropertyMetadata(null));
        public static readonly DependencyProperty ToggleDropDownCommandProperty = ToggleDropDownCommandPropertyKey.DependencyProperty;
        public ICommand ToggleDropDownCommand => (ICommand)GetValue(ToggleDropDownCommandProperty);

        #endregion

        #region 依赖属性变更处理

        /// <summary>
        /// TagModelList 变更处理：订阅/取消订阅集合变更事件
        /// </summary>
        private static void OnTagModelListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RSMultiSelectComboBox)d;
            control.OnTagModelListChanged((ObservableCollection<TagModel>?)e.OldValue, (ObservableCollection<TagModel>?)e.NewValue);
        }

        private void OnTagModelListChanged(ObservableCollection<TagModel>? oldValue, ObservableCollection<TagModel>? newValue)
        {
            // 取消订阅旧的集合
            if (oldValue != null && oldValue == currentTagModelList)
            {
                oldValue.CollectionChanged -= TagModelList_CollectionChanged;
                currentTagModelList = null;
            }

            // 订阅新的集合
            if (newValue != null)
            {
                newValue.CollectionChanged += TagModelList_CollectionChanged;
                currentTagModelList = newValue;
            }

            UpdateHasContent();
        }

        private void TagModelList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateHasContent();
        }

        /// <summary>
        /// IsDropDownOpen 变更处理：打开时同步选中项，关闭时清除筛选
        /// </summary>
        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RSMultiSelectComboBox)d;
            if ((bool)e.NewValue == true)
            {
                // 下拉框打开：根据TagModelList同步SelectedItems，使下拉框显示已选择的项
                control.SyncSelectedItemsFromTagModelList();
            }
            else
            {
                // 下拉框关闭：清除筛选，清空搜索内容
                control.ClearItemsSourceFilter();
                control.SearchContent = string.Empty;
            }
        }

        /// <summary>
        /// SearchContent 变更处理：搜索内容变化时筛选数据源
        /// </summary>
        private static void OnSearchContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RSMultiSelectComboBox)d;
            control.OnSearchContentChanged((string?)e.OldValue, (string?)e.NewValue);
        }

        private void OnSearchContentChanged(string? oldValue, string? newValue)
        {
            FilterItemsSource();
            
            // 筛选后，如果下拉框是打开的，重新同步选中项
            // 这样已选择的项在筛选后的列表中仍然显示为选中状态
            if (this.IsDropDownOpen)
            {
                SyncSelectedItemsFromTagModelList();
            }
        }

        #endregion

        #region 下拉框选择处理（核心逻辑）

        /// <summary>
        /// DataGrid 选择变更事件处理
        /// 功能：当下拉框中选择项时，将选中的项添加到TagModelList，并选中对应的标签使其可见
        /// </summary>
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 如果正在同步选中项（避免循环触发），跳过处理
            if (isSyncingSelectedItems)
            {
                return;
            }

            // ========== 处理添加的项 ==========
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                HandleAddedItems(e.AddedItems);
            }
            // ========== 处理移除的项 ==========
            else if (e.RemovedItems != null && e.RemovedItems.Count > 0)
            {
                HandleRemovedItems();
            }
        }

        /// <summary>
        /// 处理添加的选中项：添加到TagModelList，并选中最后添加的标签
        /// </summary>
        private void HandleAddedItems(IList addedItems)
        {
            // Single 模式：只允许选择一个，先清除所有标签
            if (this.SelectionMode == DataGridSelectionMode.Single)
            {
                TagModelList.Clear();
                var multiSelectedItems = this.MultiSelectedItems as IList;
                multiSelectedItems?.Clear();
            }

            int lastAddedIndex = -1;      // 最后新添加的项的索引
            object? lastAddedItem = null; // 最后处理的项（可能已存在）

            // 遍历所有添加的项
            foreach (var item in addedItems)
            {
                var tagModel = CreateTagModel(item, TagItemSource.FromItemsSource);
                
                // 如果不存在，添加到TagModelList
                if (!TagModelList.Contains(tagModel))
                {
                    TagModelList.Add(tagModel);
                    lastAddedIndex = TagModelList.Count - 1;
                    
                    // 同步到 MultiSelectedItems
                    var multiSelectedItems = this.MultiSelectedItems as IList;
                    multiSelectedItems?.Add(tagModel.Data);
                }
                
                // 记录最后处理的项（无论是否新添加）
                lastAddedItem = item;
            }

            UpdateHasContent();

            // 选中最后添加/处理的标签，并确保其可见
            if (lastAddedIndex >= 0)
            {
                // 新添加的项，直接使用索引
                SelectTag(lastAddedIndex);
            }
            else if (lastAddedItem != null)
            {
                // 已存在的项，找到它的索引
                var lastTagModel = CreateTagModel(lastAddedItem, TagItemSource.FromItemsSource);
                int existingIndex = TagModelList.IndexOf(lastTagModel);
                if (existingIndex >= 0)
                {
                    SelectTag(existingIndex);
                }
            }
        }

        /// <summary>
        /// 处理移除的选中项：选中当前SelectedItems中最后一项对应的标签
        /// 注意：TagModelList不会自动移除，只有用户点击关闭按钮才会移除
        /// </summary>
        private void HandleRemovedItems()
        {
            if (this.SelectedItems != null && this.SelectedItems.Count > 0)
            {
                // 获取当前SelectedItems中的最后一项
                var lastSelectedItem = this.SelectedItems.Cast<object>().LastOrDefault();
                if (lastSelectedItem != null)
                {
                    // 找到对应的标签并选中
                    var tagModel = CreateTagModel(lastSelectedItem, TagItemSource.FromItemsSource);
                    int tagIndex = TagModelList.IndexOf(tagModel);
                    if (tagIndex >= 0)
                    {
                        SelectTag(tagIndex);
                    }
                }
            }
            else if (TagModelList.Count > 0)
            {
                // 如果SelectedItems为空，选中最后一个标签
                SelectTag(TagModelList.Count - 1);
            }
        }

        #endregion

        #region 搜索功能

        /// <summary>
        /// 搜索框回车键处理：添加第一个匹配的项，或创建自定义标签
        /// </summary>
        private void OnSearchContentKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HandleSearchEnterKey();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 处理搜索框回车键
        /// 逻辑：
        /// 1. 如果绑定了TagDisplayMemberPath，尝试从筛选结果中找到第一个未添加的项
        /// 2. 如果没找到或没绑定TagDisplayMemberPath，使用输入内容创建自定义标签
        /// </summary>
        private void HandleSearchEnterKey()
        {
            if (string.IsNullOrWhiteSpace(this.SearchContent))
            {
                return;
            }

            // 尝试从数据源中找到匹配的项
            if (!string.IsNullOrEmpty(this.TagDisplayMemberPath) && this.ItemsSource != null)
            {
                var searchText = this.SearchContent.Trim();
                var filteredItems = GetFilteredItems(searchText);
                
                // 找到第一个未添加的项
                foreach (var item in filteredItems)
                {
                    var tagModel = CreateTagModel(item, TagItemSource.FromItemsSource);
                    if (!TagModelList.Contains(tagModel))
                    {
                        AddTagFromItemSource(item);
                        this.SearchContent = string.Empty;
                        return;
                    }
                }
            }

            // 没找到匹配项，创建自定义标签
            object tagData = this.SearchContent.Trim();
            var customTagModel = CreateTagModel(tagData, TagItemSource.Custom);
            if (!TagModelList.Contains(customTagModel))
            {
                AddCustomTag(customTagModel);
            }

            this.SearchContent = string.Empty;
        }

        /// <summary>
        /// 从ItemsSource添加标签
        /// </summary>
        private void AddTagFromItemSource(object item)
        {
            // Single 模式：先清除所有标签
            if (this.SelectionMode == DataGridSelectionMode.Single)
            {
                TagModelList.Clear();
                var multiSelectedItems = this.MultiSelectedItems as IList;
                multiSelectedItems?.Clear();
            }

            var tagModel = CreateTagModel(item, TagItemSource.FromItemsSource);
            TagModelList.Add(tagModel);
            
            // 同步到 MultiSelectedItems
            var multiSelectedItems2 = this.MultiSelectedItems as IList;
            multiSelectedItems2?.Add(tagModel.Data);
            
            UpdateHasContent();
            
            // 选中并确保可见
            int addedIndex = TagModelList.Count - 1;
            SelectTag(addedIndex);
        }

        /// <summary>
        /// 添加自定义标签
        /// </summary>
        private void AddCustomTag(TagModel customTagModel)
        {
            // Single 模式：先清除所有标签
            if (this.SelectionMode == DataGridSelectionMode.Single)
            {
                TagModelList.Clear();
                var multiSelectedItems = this.MultiSelectedItems as IList;
                multiSelectedItems?.Clear();
            }

            TagModelList.Add(customTagModel);
            UpdateHasContent();
            
            // 选中并确保可见
            int addedIndex = TagModelList.Count - 1;
            SelectTag(addedIndex);
        }

        /// <summary>
        /// 根据搜索文本筛选数据源
        /// </summary>
        private void FilterItemsSource()
        {
            if (string.IsNullOrEmpty(this.TagDisplayMemberPath) || this.ItemsSource == null)
            {
                ClearItemsSourceFilter();
                return;
            }

            var searchText = this.SearchContent?.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                ClearItemsSourceFilter();
                return;
            }

            // 使用 CollectionView 进行筛选
            var collectionView = CollectionViewSource.GetDefaultView(this.ItemsSource);
            if (collectionView != null)
            {
                collectionView.Filter = item =>
                {
                    var displayValue = ReflectionHelper.GetDisplayMemberValue(item, this.TagDisplayMemberPath);
                    var displayText = displayValue?.ToString() ?? item?.ToString() ?? string.Empty;
                    return displayText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                };
            }
        }

        /// <summary>
        /// 清除数据源筛选
        /// </summary>
        private void ClearItemsSourceFilter()
        {
            if (this.ItemsSource == null)
            {
                return;
            }

            var collectionView = CollectionViewSource.GetDefaultView(this.ItemsSource);
            if (collectionView != null)
            {
                collectionView.Filter = null;
            }
        }

        /// <summary>
        /// 获取筛选后的数据项列表
        /// </summary>
        private IEnumerable<object> GetFilteredItems(string searchText)
        {
            if (this.ItemsSource == null || string.IsNullOrEmpty(searchText))
            {
                return Enumerable.Empty<object>();
            }

            var results = new List<object>();
            foreach (var item in this.ItemsSource)
            {
                var displayValue = ReflectionHelper.GetDisplayMemberValue(item, this.TagDisplayMemberPath);
                var displayText = displayValue?.ToString() ?? item?.ToString() ?? string.Empty;
                
                if (displayText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add(item);
                }
            }

            return results;
        }

        #endregion

        #region 标签操作（选择、删除、清除）

        /// <summary>
        /// 标签选择命令执行：点击标签时选中它，或切换下拉框状态
        /// </summary>
        private void TagSelectExecute(TagModel? tag)
        {
            if (tag == null)
            {
                return;
            }

            // 设置标志，防止后续的 ToggleDropDownCommand 执行
            this.isTagSelecting = true;

            try
            {
                int index = this.TagModelList.IndexOf(tag);
                if (index >= 0)
                {
                    if (this.SelectedTagIndex == index)
                    {
                        // 点击已选中的标签：切换下拉框状态
                        this.IsDropDownOpen = !this.IsDropDownOpen;
                    }
                    else
                    {
                        // 点击未选中的标签：选中它并关闭下拉框
                        SelectTag(index);
                        this.IsDropDownOpen = false;
                    }
                    this.Focus();
                }
            }
            finally
            {
                // 延迟重置标志，确保 ToggleDropDownCommand 检查时能看到这个标志
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    this.isTagSelecting = false;
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        /// <summary>
        /// 标签关闭命令执行：删除标签
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

            // 保存当前选中索引
            int currentSelectedIndex = this.SelectedTagIndex;

            // 从 TagModelList 中移除
            TagModelList.RemoveAt(index);

            // 只从 SelectedItems 中移除来自 ItemsSource 的项
            if (tag.Source == TagItemSource.FromItemsSource && tag.Data != null)
            {
                this.SelectedItems.Remove(tag.Data);
                
                // 同步到 MultiSelectedItems
                var multiSelectedItems = this.MultiSelectedItems as IList;
                multiSelectedItems?.Remove(tag.Data);
            }
            
            UpdateHasContent();

            // 调整选中索引
            if (this.TagModelList.Count == 0)
            {
                SelectedTagIndex = -1;
            }
            else
            {
                if (currentSelectedIndex == index)
                {
                    // 删除的是当前选中的标签，清除选中状态
                    SelectedTagIndex = -1;
                }
                else if (currentSelectedIndex > index)
                {
                    // 删除的标签在当前选中标签之前，需要将选中索引减1
                    SelectTag(currentSelectedIndex - 1);
                }
                // 如果 currentSelectedIndex < index，删除的标签在当前选中标签之后，不需要调整
            }
        }

        /// <summary>
        /// 清除所有标签命令执行
        /// </summary>
        private void ClearAllTagsExecute()
        {
            if (this.TagModelList.Count == 0)
            {
                return;
            }

            TagModelList.Clear();
            this.SelectedItems.Clear();
            
            var multiSelectedItems = this.MultiSelectedItems as IList;
            multiSelectedItems?.Clear();

            UpdateHasContent();
            SelectedTagIndex = -1;
        }

        /// <summary>
        /// 切换下拉框状态命令执行（用于空白区域点击）
        /// </summary>
        private void ToggleDropDownExecute()
        {
            // 如果正在处理标签选择，跳过执行，避免事件冒泡导致的重复切换
            if (this.isTagSelecting)
            {
                return;
            }

            this.IsDropDownOpen = !this.IsDropDownOpen;
        }

        #endregion

        #region 键盘导航

        /// <summary>
        /// 键盘事件处理：支持左右箭头键选择标签，Backspace/Delete删除标签
        /// </summary>
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 如果焦点在搜索框中，不处理键盘事件，让搜索框自己处理
            if (this.PART_SearchContent != null && this.PART_SearchContent.IsFocused)
            {
                return;
            }

            if (this.TagModelList.Count == 0)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                    // 左箭头：选择上一个标签
                    if (this.SelectedTagIndex > 0)
                    {
                        SelectTag(this.SelectedTagIndex - 1);
                        e.Handled = true;
                    }
                    else if (this.SelectedTagIndex == -1)
                    {
                        // 如果没有选中，选中最后一个
                        SelectTag(this.TagModelList.Count - 1);
                        e.Handled = true;
                    }
                    break;

                case Key.Right:
                    // 右箭头：选择下一个标签
                    if (this.SelectedTagIndex >= 0 && this.SelectedTagIndex < this.TagModelList.Count - 1)
                    {
                        SelectTag(this.SelectedTagIndex + 1);
                        e.Handled = true;
                    }
                    else if (this.SelectedTagIndex < 0 && this.TagModelList.Count > 0)
                    {
                        // 如果没有选中，选中第一个
                        SelectTag(0);
                        e.Handled = true;
                    }
                    break;

                case Key.Back:
                    // Backspace：删除选中的标签，或删除最后一个标签
                    if (this.SelectedTagIndex >= 0)
                    {
                        DeleteSelectedTag();
                        e.Handled = true;
                    }
                    else if (this.TagModelList.Count > 0)
                    {
                        // 没有选中时，删除最后一个标签
                        var lastTag = this.TagModelList[this.TagModelList.Count - 1];
                        TagCloseExecute(lastTag);
                        e.Handled = true;
                    }
                    break;

                case Key.Delete:
                    // Delete：删除选中的标签
                    if (this.SelectedTagIndex >= 0)
                    {
                        DeleteSelectedTag();
                        e.Handled = true;
                    }
                    break;

                case Key.Escape:
                    // Escape：清除标签选中状态
                    if (this.SelectedTagIndex >= 0)
                    {
                        ClearTagSelect();
                        e.Handled = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// 选择指定索引的标签，并确保其可见
        /// </summary>
        private void SelectTag(int index)
        {
            if (index < 0 || this.TagModelList.Count == 0)
            {
                this.SelectedTagIndex = -1;
                return;
            }

            if (index >= this.TagModelList.Count)
            {
                index = this.TagModelList.Count - 1;
            }

            this.SelectedTagIndex = index;
            ScrollTagIntoView(index);
        }

        /// <summary>
        /// 将指定索引的标签滚动到可见位置
        /// </summary>
        private void ScrollTagIntoView(int index)
        {
            if (this.PART_TagContent == null || index < 0 || index >= this.TagModelList.Count)
            {
                return;
            }

            // 使用 Dispatcher 确保 UI 已经更新
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                try
                {
                    var generator = this.PART_TagContent.ItemContainerGenerator;
                    if (generator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        var container = generator.ContainerFromIndex(index) as FrameworkElement;
                        if (container != null)
                        {
                            // 查找 Border 元素（Tag 的根元素）
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
        /// 删除当前选中的标签
        /// </summary>
        private void DeleteSelectedTag()
        {
            if (this.SelectedTagIndex < 0 || this.SelectedTagIndex >= this.TagModelList.Count)
            {
                return;
            }

            var tagToDelete = this.TagModelList[this.SelectedTagIndex];
            TagCloseExecute(tagToDelete);
        }

        /// <summary>
        /// 清除标签选中状态
        /// </summary>
        private void ClearTagSelect()
        {
            this.SelectedTagIndex = -1;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建 TagModel 实例
        /// </summary>
        private TagModel CreateTagModel(object data, TagItemSource source)
        {
            object? tagContent;
            if (source == TagItemSource.FromItemsSource)
            {
                // 来自 ItemsSource 的数据，使用 TagDisplayMemberPath 获取显示内容
                tagContent = ReflectionHelper.GetDisplayMemberValue(data, this.TagDisplayMemberPath) ?? data;
            }
            else
            {
                // 用户自定义输入的数据，直接使用数据本身作为显示内容
                tagContent = data?.ToString() ?? data;
            }

            return new TagModel()
            {
                TagContent = tagContent,
                IsSelect = false,
                Data = data,
                Source = source
            };
        }

        /// <summary>
        /// 更新 HasContent 属性
        /// </summary>
        private void UpdateHasContent()
        {
            this.HasContent = this.TagModelList.Count > 0;
        }

        /// <summary>
        /// 根据 SelectedTagIndex 更新所有标签的选中状态视觉效果
        /// </summary>
        private void UpdateTagSelectionVisual()
        {
            if (this.TagModelList == null)
            {
                return;
            }

            for (int i = 0; i < this.TagModelList.Count; i++)
            {
                this.TagModelList[i].IsSelect = (i == this.SelectedTagIndex);
            }
        }

        /// <summary>
        /// 当只有1列时，自动设置宽度为*填满可用空间
        /// </summary>
        private void AdjustColumnWidthWhenSingleColumn()
        {
            if (this.Columns.Count == 1)
            {
                this.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }

        private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            AdjustColumnWidthWhenSingleColumn();
        }

        #endregion

        #region 数据同步

        /// <summary>
        /// 根据TagModelList同步DataGrid的SelectedItems
        /// 功能：当下拉框打开时，使下拉框显示已选择的项
        /// </summary>
        private void SyncSelectedItemsFromTagModelList()
        {
            if (this.ItemsSource == null)
            {
                return;
            }

            try
            {
                isSyncingSelectedItems = true;

                // 先清除当前选中项
                this.SelectedItems.Clear();

                if (TagModelList.Count == 0)
                {
                    return;
                }

                // 获取实际的视图（考虑筛选）
                var collectionView = CollectionViewSource.GetDefaultView(this.ItemsSource);
                
                // 遍历筛选后的视图，如果项在TagModelList中且来源是FromItemsSource，则添加到SelectedItems
                // 注意：使用 collectionView 的迭代器可以正确获取筛选后的项
                if (collectionView != null)
                {
                    foreach (var item in collectionView)
                    {
                        var tagModel = CreateTagModel(item, TagItemSource.FromItemsSource);
                        if (TagModelList.Contains(tagModel))
                        {
                            this.SelectedItems.Add(item);
                            
                            // Single 模式：只添加第一个匹配的项
                            if (this.SelectionMode == DataGridSelectionMode.Single)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // 如果没有 CollectionView，直接遍历 ItemsSource
                    foreach (var item in this.ItemsSource)
                    {
                        var tagModel = CreateTagModel(item, TagItemSource.FromItemsSource);
                        if (TagModelList.Contains(tagModel))
                        {
                            this.SelectedItems.Add(item);
                            
                            // Single 模式：只添加第一个匹配的项
                            if (this.SelectionMode == DataGridSelectionMode.Single)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                isSyncingSelectedItems = false;
            }
        }

        #endregion
    }
}
