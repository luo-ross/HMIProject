using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NPOI.Util;
using RS.Commons.Attributs;
using RS.Widgets.Enums;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using RS.WPFClient.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace RS.WPFClient.ViewModels
{
    [ServiceInjectConfig(ServiceLifetime.Transient)]
    public class HomeViewModel : ViewModelBase
    {
        /// <summary>
        /// 用于导航到特定视图或执行导航操作的命令
        /// </summary>
        public ICommand NavigateCommand { get; }

        private DispatcherTimer DispatcherTimer;

        private DispatcherTimer CaretDispatcherTimer;

        /// <summary>
        /// 添加账号的命令
        /// </summary>
        public ICommand AddAccountCommand { get; }

        /// <summary>
        /// 管理账号安全的命令
        /// </summary>
        public ICommand AccountSecurityCommand { get; }

        /// <summary>
        /// 打开帮助中心的命令
        /// </summary>
        public ICommand HelpCenterCommand { get; }

        /// <summary>
        /// 提供反馈的命令
        /// </summary>
        public ICommand FeedbackCommand { get; }

        /// <summary>
        /// 访问旧版邮箱的命令
        /// </summary>
        public ICommand OldEmailCommand { get; }

        /// <summary>
        /// 访问会员中心的命令
        /// </summary>
        public ICommand MembershipCenterCommand { get; }

        /// <summary>
        /// 注销的命令
        /// </summary>
        public ICommand LogoutCommand { get; }

        /// <summary>
        /// 打开会员页面的命令
        /// </summary>
        public ICommand OpenMembershipCommand { get; }

        /// <summary>
        /// 复制邮箱地址到剪贴板的命令
        /// </summary>
        public ICommand CopyEmailCommand { get; }

        public HomeViewModel()
        {
            this.NavigateCommand = new RelayCommand<NavigateModel>(Navigate);

            DispatcherTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(30), DispatcherPriority.Background, (s, e) =>
            {
                this.DateTimeNow = DateTime.Now;
            }, Application.Current.Dispatcher);

            this.CaretDispatcherTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Background, (s, e) =>
            {
                IsCaretVisible = !IsCaretVisible;
            }, Application.Current.Dispatcher);

            this.AddAccountCommand = new RelayCommand(AddAccount);
            this.AccountSecurityCommand = new RelayCommand(AccountSecurity);
            this.HelpCenterCommand = new RelayCommand(OpenHelpCenter);
            this.FeedbackCommand = new RelayCommand(ProvideFeedback);
            this.OldEmailCommand = new RelayCommand(OpenOldEmail);
            this.MembershipCenterCommand = new RelayCommand(OpenMembershipCenter);
            this.LogoutCommand = new RelayCommand(Logout);
            this.OpenMembershipCommand = new RelayCommand(OpenMembership);
            this.CopyEmailCommand = new RelayCommand(CopyEmail);

            this.ViewModelSelect=new InBoxViewModel();
        }

        private void AddAccount()
        {
            // 添加账号的逻辑
        }

        private void AccountSecurity()
        {
            // 管理账号安全的逻辑
        }

        private void OpenHelpCenter()
        {
            // 打开帮助中心的逻辑
        }

        private void ProvideFeedback()
        {
            // 提供反馈的逻辑
        }

        private void OpenOldEmail()
        {
            // 访问旧版邮箱的逻辑
        }

        private void OpenMembershipCenter()
        {
            // 访问会员中心的逻辑
        }

        private void Logout()
        {
            // 注销的逻辑
        }

        private void OpenMembership()
        {
            // 打开会员页面的逻辑
        }

        private void CopyEmail()
        {
            // 复制邮箱地址到剪贴板的逻辑
        }

        #region 测试

        private bool isCaretVisible;

        public bool IsCaretVisible
        {
            get
            {
                return isCaretVisible;
            }
            set
            {
                this.SetProperty(ref isCaretVisible, value);
            }
        }

        private string inputContent;

        public string InputContent
        {
            get
            {
                return inputContent;
            }
            set
            {
                this.SetProperty(ref inputContent, value);
            }
        }


        private double textInputWidth;

        public double TextInputWidth
        {
            get
            {
                return textInputWidth;
            }
            set
            {
                this.SetProperty(ref textInputWidth, value);
            }
        }



        private double textInputHeight;

        public double TextInputHeight
        {
            get
            {
                return textInputHeight;
            }
            set
            {
                this.SetProperty(ref textInputHeight, value);
            }
        }

        private double caretLeft=50;

        public double CaretLeft
        {
            get
            {
                return caretLeft;
            }
            set
            {
                this.SetProperty(ref caretLeft, value);
            }
        }

        private double caretTop=50;

        public double CaretTop
        {
            get
            {
                return caretTop;
            }
            set
            {
                this.SetProperty(ref caretTop, value);
            }
        }

        private double caretWidth = 1;
        public double CaretWidth
        {
            get
            {
                return caretWidth;
            }
            set
            {
                this.SetProperty(ref caretWidth, value);
            }
        }



        private double caretHeight = 15;

        public double CaretHeight
        {
            get
            {
                return caretHeight;
            }
            set
            {
                this.SetProperty(ref caretHeight, value);
            }
        }


        private double textInputLeft;

        public double TextInputLeft
        {
            get
            {
                return textInputLeft;
            }
            set
            {
                this.SetProperty(ref textInputLeft, value);
            }
        }



        private double textInputTop;

        public double TextInputTop
        {
            get
            {
                return textInputTop;
            }
            set
            {
                this.SetProperty(ref textInputTop, value);
            }
        }


        public TransformData MyTransformData { get; set; } = new TransformData();
        public TransformData MyTransformData2 { get; set; } = new TransformData();
        public TransformData MyTransformData3 { get; set; } = new TransformData();


        #endregion


        private SearchViewModel searchViewModel;

        public SearchViewModel SearchViewModel
        {
            get
            {
                if (searchViewModel == null)
                {
                    searchViewModel = new SearchViewModel();
                }
                return searchViewModel;
            }
            set
            {
                this.SetProperty(ref searchViewModel, value);
            }
        }


        private ObservableCollection<PersonModel> testList;

        public ObservableCollection<PersonModel> TestList
        {
            get { return testList; }
            set
            {
                this.SetProperty(ref testList, value);
            }
        }


        private PersonModel testSelected;

        public PersonModel TestSelected
        {
            get { return testSelected; }
            set
            {
                this.SetProperty(ref testSelected, value);
            }
        }


        public void GenerateTestData(int n)
        {
            var groupNames = new[] { "A组", "B组", "C组", "D组", "E组" };
            for (int i = 1; i <= n; i++)
            {
                PersonModelList.Add(new PersonModel
                {
                    Name = $"测试用户{i}",
                    Group = groupNames[i % groupNames.Length]
                });
            }
        }


        private void Navigate(NavigateModel? model)
        {
            //this.ViewModelSelect = model?.ViewMoel;
            //先做测试
            //this.ViewModelSelect = this.ViewModelManager.GetViewModel<INotifyPropertyChanged>(model.ViewModelKey);
        }
        public static List<NavigateModel> GenerateMenu(int maxLevel, int menuCountPerLevel, double groupProbability = 0.2)
        {
            var menuList = new List<NavigateModel>();
            int idCounter = 1;
            var rand = new Random();

            void AddMenus(string parentId, int currentLevel)
            {
                if (currentLevel > maxLevel) return;

                for (int i = 1; i <= menuCountPerLevel; i++)
                {
                    string id = (idCounter++).ToString();
                    bool isGroup = rand.NextDouble() < groupProbability; // groupProbability 概率分组
                    var menu = new NavigateModel
                    {
                        Id = id,
                        ParentId = parentId,
                        Level = currentLevel,
                        Order = i,
                        NavName = $"第{currentLevel}级菜舒服舒服sdf单-{i}" + (parentId != null ? $"(父:{parentId})" : ""),
                        HasChildren = !isGroup && currentLevel < maxLevel, // 分组不再有下级
                        IsGroupNav = isGroup,
                        ViewModelKey = i % 2 == 0 ? $"RS.WPFClient/Views.Areas.UserViewModel" : @"RS.WPFClient/Views.Areas.RoleViewModel",
                        IconKey = IconKey.Folder,
                        IsExpand = false,
                        IsSelect = false,
                        ViewMoel = null
                    };
                    menuList.Add(menu);

                    // 只有不是分组时才递归生成下级
                    if (!isGroup)
                    {
                        AddMenus(id, currentLevel + 1);
                    }
                }
            }

            AddMenus(null, 0); // 层级从0开始

            return menuList;
        }

        public List<NavigateModel> SortMenu(List<NavigateModel> menuList)
        {
            var result = new List<NavigateModel>();
            // 先按Level、Order排序
            var lookup = menuList
                .OrderBy(m => m.Level)
                .ThenBy(m => m.Order)
                .ToLookup(m => m.ParentId);

            void AddChildren(string parentId)
            {
                foreach (var menu in lookup[parentId])
                {
                    result.Add(menu);
                    AddChildren(menu.Id); // 递归添加子节点
                }
            }

            AddChildren(null); // 顶级菜单ParentId为null

            return result;
        }

        public void ChangeSource(ObservableCollection<PersonModel> newSource)
        {
            GroupedPersonsView = CollectionViewSource.GetDefaultView(newSource);
            GroupedPersonsView.GroupDescriptions.Clear();
            GroupedPersonsView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            OnPropertyChanged(nameof(GroupedPersonsView));
        }

        private ICollectionView groupedPersonsView;

        public ICollectionView GroupedPersonsView
        {
            get
            {
                return groupedPersonsView;
            }
            set
            {
                this.SetProperty(ref groupedPersonsView, value);
            }
        }


        private List<NavigateModel> navigateModelList;

        public List<NavigateModel> NavigateModelList
        {
            get
            {
                if (navigateModelList == null)
                {
                    navigateModelList = new List<NavigateModel>();
                }
                return navigateModelList;
            }
            set
            {
                this.SetProperty(ref navigateModelList, value);
            }
        }

        private ObservableCollection<PersonModel> personModelList;

        public ObservableCollection<PersonModel> PersonModelList
        {
            get
            {
                if (personModelList == null)
                {
                    personModelList = new ObservableCollection<PersonModel>();
                }
                return personModelList;
            }
            set
            {
                this.SetProperty(ref personModelList, value);
            }
        }



        private ObservableCollection<NavigateModel> test1List;
        public ObservableCollection<NavigateModel> Test1List
        {
            get
            {
                if (test1List == null)
                {
                    test1List = new ObservableCollection<NavigateModel>();
                    for (int i = 0; i < 20; i++)
                    {
                        test1List.Add(new NavigateModel()
                        {
                            NavName = $"数据集1_{i}"
                        });
                    }
                }
                return test1List;
            }
            set
            {
                this.SetProperty(ref test1List, value);
            }
        }



        private ObservableCollection<NavigateModel> test2List;

        public ObservableCollection<NavigateModel> Test2List
        {
            get
            {
                if (test2List == null)
                {
                    test2List = new ObservableCollection<NavigateModel>();
                    for (int i = 0; i < 20; i++)
                    {
                        test2List.Add(new NavigateModel()
                        {
                            NavName = $"数据集2_{i}"
                        });
                    }
                }
                return test2List;
            }
            set
            {
                this.SetProperty(ref test2List, value);
            }
        }

        private ObservableCollection<NavigateModel> test3List;

        public ObservableCollection<NavigateModel> Test3List
        {
            get
            {
                if (test3List == null)
                {
                    test3List = new ObservableCollection<NavigateModel>();
                    for (int i = 0; i < 20; i++)
                    {
                        test3List.Add(new NavigateModel()
                        {
                            NavName = $"数据集3_{i}"
                        });
                    }
                }
                return test3List;
            }
            set
            {
                this.SetProperty(ref test3List, value);
            }
        }


        private ObservableCollection<NavigateModel> test4List;

        public ObservableCollection<NavigateModel> Test4List
        {
            get
            {
                if (test4List == null)
                {
                    test4List = new ObservableCollection<NavigateModel>();
                    for (int i = 0; i < 20; i++)
                    {
                        test4List.Add(new NavigateModel()
                        {
                            NavName = $"数据集4_{i}"
                        });
                    }
                }
                return test4List;
            }
            set
            {
                this.SetProperty(ref test4List, value);
            }
        }

        private DateTime dateTimeNow;
        /// <summary>
        /// 当前时间
        /// </summary>
        public DateTime DateTimeNow
        {
            get { return dateTimeNow; }
            set
            {
                this.SetProperty(ref dateTimeNow, value);
            }
        }



        private INotifyPropertyChanged viewModelSelect;
        /// <summary>
        /// 当前选择视图
        /// </summary>
        public INotifyPropertyChanged ViewModelSelect
        {
            get { return viewModelSelect; }
            set
            {
                this.SetProperty(ref viewModelSelect, value);
            }
        }


        private bool isEnglish;
        public bool IsEnglish
        {
            get { return isEnglish; }
            set
            {
                this.SetProperty(ref isEnglish, value);
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


        private bool isFullScreen;
        public bool IsFullScreen
        {
            get { return isFullScreen; }
            set
            {
                this.SetProperty(ref isFullScreen, value);
            }
        }


        private ObservableCollection<TreeModel> treeModelList;
        public ObservableCollection<TreeModel> TreeModelList
        {
            get
            {
                if (treeModelList == null)
                {
                    treeModelList = new ObservableCollection<TreeModel>();
                }

                for (int i = 0; i < 1; i++)
                {

                    treeModelList.Add(new TreeModel()
                    {
                        TreeName = "快速访问",
                        TreeIcon = "/Assets/test.png",
                    });
                    treeModelList.Add(new TreeModel()
                    {
                        TreeName = "OneDrive-Personal",
                        TreeIcon = "/Assets/test.png",
                    });
                    treeModelList.Add(new TreeModel()
                    {
                        TreeName = "此电脑",
                        TreeIcon = "/Assets/test.png",
                        Children = new ObservableCollection<TreeModel>()
                {
                    new TreeModel()
                    {
                TreeName = "3D对象",
                TreeIcon = "/Assets/test.png",
                Children= new ObservableCollection<TreeModel>()
                {
                    new TreeModel() {
                TreeName = "OneDrive-Personal",
                TreeIcon = "/Assets/test.png",
            }
                }
            },
                           new TreeModel()
            {
                TreeName = "视频",
                TreeIcon = "/Assets/test.png",
            },
                }
                    });

                    treeModelList.Add(new TreeModel()
                    {
                        TreeName = "网络",
                        TreeIcon = "/Assets/test.png",
                    });
                    treeModelList.Add(new TreeModel()
                    {
                        TreeName = "Linux",
                        TreeIcon = "/Assets/test.png",
                    });

                }


                return treeModelList;
            }
            set { treeModelList = value; }
        }



    }
}
