using RS.Widgets.Models;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Input;

namespace RS.HMI.Client.Views.Home
{


    public class HomeViewModel : NotifyBase
    {

        public HomeViewModel()
        {
            this.BtnClickCommand = new RelayCommand(BtnClick, CanBtnClick);
        }


        private bool isEnglish;

        public bool IsEnglish
        {
            get { return isEnglish; }
            set
            {
                this.OnPropertyChanged(ref isEnglish, value);
            }
        }

        private bool CanBtnClick(object arg)
        {
            return true;
        }

        private void BtnClick(object obj)
        {
            MessageBox.Show("这是MVVM命令事件");
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
                this.OnPropertyChanged(ref searchContent, value);
            }
        }


        private bool isFullScreen;

        public bool IsFullScreen
        {
            get { return isFullScreen; }
            set
            {
                this.OnPropertyChanged(ref isFullScreen, value);
            }
        }

        private ICommand btnClickCommand;

        public ICommand BtnClickCommand
        {
            get { return btnClickCommand; }
            set
            {
                this.OnPropertyChanged(ref btnClickCommand, value);
            }
        }

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
