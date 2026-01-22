using CommunityToolkit.Mvvm.Input;
using NPOI.SS.Formula.Functions;
using RS.Widgets.Controls;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using RS.WPFClient.Enums;
using RS.WPFClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RS.WPFClient.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        #region 依赖服务
        private readonly IWindowService WindowService;
        #endregion

        #region 命令

        public ICommand HideSearchCommand { get; }

        public ICommand SearchViewPreviewMouseLeftButtonUpCommand { get; }


        public ICommand SearchCommand { get; }

        public ICommand SearchCancelCommand { get; }

        #endregion
        public SearchViewModel()
        {
            this.HideSearchCommand = new RelayCommand(HideSearch);
            this.SearchViewPreviewMouseLeftButtonUpCommand = new RelayCommand(SearchViewPreviewMouseLeftButtonUp);
            this.SearchCommand = new RelayCommand(Search, CanSearch);
            this.SearchCancelCommand = new RelayCommand(SearchCancel);

            this.InitSearchTypeList();
            this.InitDateFilterTypeList();
            this.InitLocationTypeList();
            this.InitFolderTypeList();
            this.InitReadUnReadTypeList();
            this.InitAttachmentTypeList();


        }

        private void Search()
        {
            this.IsShowSearch = true;
        }

        private bool CanSearch()
        {
            return false;
        }

        private void SearchCancel()
        {
            this.IsAdvancedSearchChecked = false;
        }

        private void SearchViewPreviewMouseLeftButtonUp()
        {
            this.IsShowSearch = true;
        }

        private void HideSearch()
        {
            //用户点击搜索类型或者高级搜索时不关闭
            if (this.IsSearchTypeChecked || this.IsAdvancedSearchChecked)
            {
                return;
            }

            //搜索内容不为空 则不关闭
            if (!string.IsNullOrWhiteSpace(this.SearchContent))
            {
                return;
            }

            if (this.IsSearchHasContent)
            {
                return;
            }

            IsShowSearch = false;
        }



        private bool isShowSearch;
        /// <summary>
        /// 是否选择搜索
        /// </summary>
        public bool IsShowSearch
        {
            get { return isShowSearch; }
            set
            {
                this.SetProperty(ref isShowSearch, value);
            }
        }

        private bool isSearchHasContent;
        /// <summary>
        /// 搜索是否有内容
        /// </summary>
        public bool IsSearchHasContent
        {
            get { return isSearchHasContent; }
            set
            {
                this.SetProperty(ref isSearchHasContent, value);
            }
        }


        private bool isSearchTypeChecked;
        /// <summary>
        /// 搜索类型是否选中
        /// </summary>
        public bool IsSearchTypeChecked
        {
            get { return isSearchTypeChecked; }
            set
            {
                this.SetProperty(ref isSearchTypeChecked, value);
            }
        }


        private bool isAdvancedSearchChecked;
        /// <summary>
        /// 高级搜索是否选中
        /// </summary>
        public bool IsAdvancedSearchChecked
        {
            get { return isAdvancedSearchChecked; }
            set
            {
                this.SetProperty(ref isAdvancedSearchChecked, value);
            }
        }


        private string searchContent;
        /// <summary>
        /// 搜索内容
        /// </summary>
        public string SearchContent
        {
            get { return searchContent; }
            set
            {
                this.SetProperty(ref searchContent, value);
            }
        }




        #region 搜索类型
        /// <summary>
        /// 初始化搜索类型
        /// </summary>
        private void InitSearchTypeList()
        {
            var dataList = GetDataList<SearchType>();
            this.SearchTypeList = new ObservableCollection<SearchType>(dataList);
            this.SearchTypeSelect = this.SearchTypeList.FirstOrDefault(t => t == SearchType.Email);
        }


        private ObservableCollection<SearchType> searchTypeList;
        /// <summary>
        /// 搜索类型
        /// </summary>
        public ObservableCollection<SearchType> SearchTypeList
        {
            get { return searchTypeList; }
            set
            {
                this.SetProperty(ref searchTypeList, value);
            }
        }

        private SearchType searchTypeSelect;
        /// <summary>
        /// 搜索类型
        /// </summary>
        public SearchType SearchTypeSelect
        {
            get { return searchTypeSelect; }
            set
            {
                this.SetProperty(ref searchTypeSelect, value);
            }
        }
        #endregion



        #region 关键字
        private string keywords;
        /// <summary>
        /// 关键字位置类型
        /// </summary>
        public string Keywords
        {
            get { return keywords; }
            set
            {
                this.SetProperty(ref keywords, value);
            }
        }
        #endregion

        #region 关键字位置
        /// <summary>
        /// 初始化关键位置
        /// </summary>
        private void InitLocationTypeList()
        {
            var dataList = GetDataList<LocationType>();
            this.LocationTypeList = new ObservableCollection<LocationType>(dataList);
            this.LocationType = this.LocationTypeList.FirstOrDefault(t => t == LocationType.Any);
        }

        private ObservableCollection<LocationType> locationTypeList;
        /// <summary>
        /// 关键字位置列表
        /// </summary>
        public ObservableCollection<LocationType> LocationTypeList
        {
            get
            {
                return locationTypeList;
            }
            set
            {
                this.SetProperty(ref locationTypeList, value);
            }
        }


        private LocationType locationType;
        /// <summary>
        /// 关键字位置类型
        /// </summary>
        public LocationType LocationType
        {
            get { return locationType; }
            set
            {
                this.SetProperty(ref locationType, value);
            }
        }
        #endregion




        private ObservableCollection<TagModel> tagModelList;
        /// <summary>
        /// 标签选中
        /// </summary>
        public ObservableCollection<TagModel> TagModelList
        {
            get
            {
                if (tagModelList == null)
                {
                    tagModelList = new ObservableCollection<TagModel>();
                }
                return tagModelList;
            }
            set
            {
                this.SetProperty(ref tagModelList, value);
            }
        }


        #region 发件人

        private ObservableCollection<UserModel> emailFromSelectList;
        /// <summary>
        /// 邮件发件人列表 多选
        /// </summary>
        public ObservableCollection<UserModel> EmailFromSelectList
        {
            get
            {
                if (emailFromSelectList == null)
                {
                    emailFromSelectList = new ObservableCollection<UserModel>();
                }
                return emailFromSelectList;
            }
            set
            {
                this.SetProperty(ref emailFromSelectList, value);
            }
        }


        private ObservableCollection<UserModel> emailFromList;
        /// <summary>
        /// 邮件发件人筛选列表 默认只加载最近联系人
        /// </summary>
        public ObservableCollection<UserModel> EmailFromList
        {
            get
            {
                if (emailFromList == null)
                {
                    emailFromList = new ObservableCollection<UserModel>();
                    //加几个测试数据
                    // 循环添加100条测试数据
                    for (int i = 0; i < 100; i++)
                    {
                        emailFromList.Add(new UserModel()
                        {
                            Id = Guid.NewGuid().ToString(),
                            CreateBy = $"createUser{i}",
                            CreateId = Guid.NewGuid().ToString(),
                            CreateTime = DateTime.Now.AddMinutes(-i), // 时间随索引递减，模拟不同创建时间
                            DeleteBy = $"deleteUser{i}",
                            DeleteId = Guid.NewGuid().ToString(),
                            DeleteTime = DateTime.Now.AddMinutes(-i + 10),
                            Email = $"18455454{i}@qq.com", // 邮箱后拼接索引，保证唯一性
                            IsDelete = false,
                            IsDisabled = i % 10 == 0, // 每10条数据设置一个禁用状态，增加测试多样性
                            NickName = $"张三{i}", // 昵称后拼接索引
                            Phone = $"1380000{i:D4}", // 手机号后拼接4位索引（补零），保证格式合法
                            RealNameId = Guid.NewGuid().ToString(),
                            UpdateBy = $"updateUser{i}",
                            UpdateId = Guid.NewGuid().ToString(),
                            UpdateTime = DateTime.Now.AddMinutes(-i + 5),
                            UserPic = "https://thirdqq.qlogo.cn/ek_qqapp/AQNJwODbttyTGtqoBEHKLDGKAD3Dr9GaICvFiaX47kLnn45xjFmGWfkC6m7GPSZ65qhXheNicn6x1rn2LDBPRFAbB9qGUFJyNfnEkG4SoyIUkMjUCiaX5Kwu8y9ibREKKA/0"
                        });
                    }
                }
                return emailFromList;
            }
            set
            {
                this.SetProperty(ref emailFromList, value);
            }
        }
        #endregion

        #region 收件人

        private ObservableCollection<UserModel> emailToSelectList;
        /// <summary>
        /// 邮件收件人列表 多选
        /// </summary>
        public ObservableCollection<UserModel> EmailToSelectList
        {
            get
            {
                if (emailToSelectList == null)
                {
                    emailToSelectList = new ObservableCollection<UserModel>();
                }
                return emailToSelectList;
            }
            set
            {
                this.SetProperty(ref emailToSelectList, value);
            }
        }


        private ObservableCollection<UserModel> emailToList;
        /// <summary>
        /// 邮件收件人筛选列表 默认只加载最近联系人
        /// </summary>
        public ObservableCollection<UserModel> EmailToList
        {
            get
            {
                return emailToList;
            }
            set
            {
                this.SetProperty(ref emailToList, value);
            }
        }

        #endregion



        #region 日期
        /// <summary>
        /// 初始化日期
        /// </summary>
        private void InitDateFilterTypeList()
        {
            var dataList = GetDataList<DateFilterType>();
            this.DateFilterTypeList = new ObservableCollection<DateFilterType>(dataList);
            this.DateFilterTypeSelect = this.DateFilterTypeList.FirstOrDefault(t => t == DateFilterType.Any);
        }

        private ObservableCollection<DateFilterType> dateFilterTypeList;
        /// <summary>
        /// 日期
        /// </summary>
        public ObservableCollection<DateFilterType> DateFilterTypeList
        {
            get
            {
                return dateFilterTypeList;
            }
            set
            {
                this.SetProperty(ref dateFilterTypeList, value);
            }
        }


        private DateFilterType dateFilterTypeSelect;
        /// <summary>
        /// 日期搜索类型
        /// </summary>
        public DateFilterType DateFilterTypeSelect
        {
            get { return dateFilterTypeSelect; }
            set
            {
                this.SetProperty(ref dateFilterTypeSelect, value);
            }
        }
        #endregion




        #region 所在文件夹

        /// <summary>
        /// 初始化所在文件
        /// </summary>
        private void InitFolderTypeList()
        {
            var dataList = GetDataList<FolderType>();
            this.FolderTypeList = new ObservableCollection<FolderType>(dataList);
            this.FolderTypeSelect = this.FolderTypeList.FirstOrDefault(t => t == FolderType.Any);
        }

        private ObservableCollection<FolderType> folderTypeList;
        /// <summary>
        /// 所在文件数据
        /// </summary>
        public ObservableCollection<FolderType> FolderTypeList
        {
            get
            {
                return folderTypeList;
            }
            set
            {
                this.SetProperty(ref folderTypeList, value);
            }
        }


        private FolderType folderTypeSelect;
        /// <summary>
        /// 所在文件选择
        /// </summary>
        public FolderType FolderTypeSelect
        {
            get { return folderTypeSelect; }
            set
            {
                this.SetProperty(ref folderTypeSelect, value);
            }
        }
        #endregion


        #region 已读/未读

        /// <summary>
        /// 初始化已读/未读
        /// </summary>
        private void InitReadUnReadTypeList()
        {
            var dataList = GetDataList<ReadUnReadType>();
            this.ReadUnReadTypeList = new ObservableCollection<ReadUnReadType>(dataList);
            this.ReadUnReadTypeSelect = this.ReadUnReadTypeList.FirstOrDefault(t => t == ReadUnReadType.Any);
        }

        private ObservableCollection<ReadUnReadType> readUnReadTypeList;
        /// <summary>
        /// 已读/未读 列表
        /// </summary>
        public ObservableCollection<ReadUnReadType> ReadUnReadTypeList
        {
            get
            {
                return readUnReadTypeList;
            }
            set
            {
                this.SetProperty(ref readUnReadTypeList, value);
            }
        }


        private ReadUnReadType readUnReadTypeSelect;
        /// <summary>
        /// 已读/未读 选择
        /// </summary>
        public ReadUnReadType ReadUnReadTypeSelect
        {
            get { return readUnReadTypeSelect; }
            set
            {
                this.SetProperty(ref readUnReadTypeSelect, value);
            }
        }
        #endregion


        #region 是否包含附件

        /// <summary>
        /// 初始化是否包含附件
        /// </summary>
        private void InitAttachmentTypeList()
        {
            var dataList = GetDataList<AttachmentType>();
            this.AttachmentTypeList = new ObservableCollection<AttachmentType>(dataList);
            this.AttachmentTypeSelect = this.AttachmentTypeList.FirstOrDefault(t => t == AttachmentType.Any);
        }

        private ObservableCollection<AttachmentType> attachmentTypeList;
        /// <summary>
        /// 是否包含附件
        /// </summary>
        public ObservableCollection<AttachmentType> AttachmentTypeList
        {
            get
            {
                return attachmentTypeList;
            }
            set
            {
                this.SetProperty(ref attachmentTypeList, value);
            }
        }


        private AttachmentType attachmentTypeSelect;
        /// <summary>
        /// 是否包含附件选择
        /// </summary>
        public AttachmentType AttachmentTypeSelect
        {
            get { return attachmentTypeSelect; }
            set
            {
                this.SetProperty(ref attachmentTypeSelect, value);
            }
        }
        #endregion

    }
}
