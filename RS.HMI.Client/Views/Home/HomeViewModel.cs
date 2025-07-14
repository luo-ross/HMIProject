using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using RS.Commons.Attributs;
using RS.HMI.Client.Models;
using RS.HMI.Client.Views.Areas;
using RS.Widgets.Enums;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace RS.HMI.Client.Views
{

    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public class HomeViewModel : NotifyBase
    {
        private readonly IViewModelManager ViewModelManager;
        /// <summary>
        /// 获取或设置搜索按钮点击时执行的命令
        /// </summary>
        public ICommand NavClickCommand { get; }

        private DispatcherTimer DispatcherTimer;

        public HomeViewModel(IViewModelManager viewModelManager)
        {
            this.ViewModelManager = viewModelManager;
            this.NavClickCommand = new RelayCommand<NavigateModel>(NavClick);
            var dataList = GenerateMenu(1, 5);
            dataList = this.SortMenu(dataList);
            this.NavigateModelList = dataList;
            //TimeSpan interval, DispatcherPriority priority, EventHandler callback, Dispatcher dispatcher
            DispatcherTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(30),
                DispatcherPriority.Background, (s, e) =>
                {
                    this.DateTimeNow = DateTime.Now;
                }, Application.Current.Dispatcher);
        }

        private void NavClick(NavigateModel? model)
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
                        ViewModelKey = i % 2 == 0 ? $"RS.HMI.Client/Views.Areas.UserViewModel" : @"RS.HMI.Client/Views.Areas.RoleViewModel",
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



        private ObservableCollection<NavigateModel> test1List;

        public ObservableCollection<NavigateModel> Test1List
        {
            get
            {
                if (test1List == null)
                {
                    test1List = new ObservableCollection<NavigateModel>();
                    for (int i = 0; i < 1000; i++)
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
                    for (int i = 0; i < 1000; i++)
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
                    for (int i = 0; i < 1000; i++)
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
                    for (int i = 0; i < 1000; i++)
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
                    new TreeModel()
            {
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
