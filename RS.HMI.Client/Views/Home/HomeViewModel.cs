using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.HMI.Client.Views.Areas;
using RS.Widgets.Models;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Input;

namespace RS.HMI.Client.Views
{
    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public partial class HomeViewModel : NotifyBase
    {

        public HomeViewModel(UserViewModel userViewModel)
        {
            this.ViewModelSelect = userViewModel;
            //this.viewModelSelect = App.ServiceProvider?.GetRequiredService<UserViewModel>();
        }

        [ObservableProperty]
        private NotifyBase viewModelSelect;



        [ObservableProperty]
        private bool isEnglish;
      
      

        private bool CanBtnClick(object arg)
        {
            return true;
        }

        [RelayCommand]
        private void BtnClick(object obj)
        {
            MessageBox.Show("这是MVVM命令事件");
        }

        /// <summary>
        /// 搜索内容
        /// </summary>
        [ObservableProperty]
        private string searchContent;



        [ObservableProperty]
        private bool isFullScreen;

     
       

        private ObservableCollection<TreeModel> treeModelList;

        public ObservableCollection<TreeModel> TreeModelList
        {
            get
            {
                if (treeModelList==null)
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
