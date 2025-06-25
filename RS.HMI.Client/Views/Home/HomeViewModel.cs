using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.HMI.Client.Models;
using RS.HMI.Client.Views.Areas;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Input;

namespace RS.HMI.Client.Views
{
    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public class HomeViewModel : NotifyBase
    {
        /// <summary>
        /// 获取或设置搜索按钮点击时执行的命令
        /// </summary>
        public ICommand NavCommand { get; }
        public HomeViewModel(RoleViewModel roleViewModel)
        {
            this.NavCommand = new RelayCommand(NavigateClick);

            this.NavigateModelList.Add(new NavigateModel()
            {
                Name = "监控中心",
                IconKey = IconKey.Home,
            });

            this.ViewModelSelect = roleViewModel;
        }

        private void NavigateClick()
        {

        }

        private ObservableCollection<NavigateModel> navigateModelList;

        public ObservableCollection<NavigateModel> NavigateModelList
        {
            get
            {
                if (navigateModelList==null)
                {
                    navigateModelList = new ObservableCollection<NavigateModel>();
                }
                return navigateModelList;
            }
            set
            {
                this.SetProperty(ref navigateModelList, value);
            }
        }

        private NotifyBase viewModelSelect;

        public NotifyBase ViewModelSelect
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

                return treeModelList;
            }
            set { treeModelList = value; }
        }
    }
}
