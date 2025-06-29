using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using RS.Commons.Attributs;
using RS.HMI.Client.Models;
using RS.HMI.Client.Views.Areas;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public ICommand NavClickCommand { get; }

        public RoleViewModel RoleViewModel { get; set; }
        public UserViewModel UserViewModel { get; set; }
        public HomeViewModel(RoleViewModel viewModel,UserViewModel userViewModel)
        {
            this.RoleViewModel = viewModel;
            this.UserViewModel = userViewModel;

            this.NavClickCommand = new RelayCommand<NavigateModel>(NavClick);

            var list = GenerateRandomNavigateModels(10000);
            this.NavigateModelList = new List<NavigateModel>(list);
           
        }

        private void NavClick(NavigateModel? model)
        {
            this.ViewModelSelect = model?.ViewMoel;
        }

       
        public List<NavigateModel> GenerateRandomNavigateModels(int count)
        {
            var list = new List<NavigateModel>();
            var rand = new Random();
            int idSeed = 1;

            // 1级节点
            int level1Count = rand.Next(10, 30); // 随机生成10~30个一级节点
            var level1Nodes = new List<NavigateModel>();
            for (int i = 0; i < level1Count; i++)
            {
                var node = new NavigateModel
                {
                    Id = (idSeed++).ToString(),
                    ParentId = null,
                    Level = 0,
                    Order = i + 1,
                    IsGroupNav = false,
                    NavName = $"一级菜单_{i + 1}",
                    IconKey = (IconKey)rand.Next(0, Enum.GetValues(typeof(IconKey)).Length),
                    IsSelect = false,
                    HasChildren = true ,// 先设为true，后面再修正
                    ViewMoel= RoleViewModel
                };

                if (i%5==0)
                {
                    node.IsGroupNav = true;
                }
                level1Nodes.Add(node);
                list.Add(node);
            }



            // 2级节点
            var level2Nodes = new List<NavigateModel>();
            foreach (var parent in level1Nodes)
            {
                int childCount = rand.Next(5, 20); // 每个一级节点下5~20个二级节点
                for (int i = 0; i < childCount; i++)
                {
                    var node = new NavigateModel
                    {
                        Id = (idSeed++).ToString(),
                        ParentId = parent.Id,
                        Level = 1,
                        Order = i + 1,
                        IsGroupNav = false,
                        NavName = $"二级菜单_{parent.Order}_{i + 1}",
                        IconKey = (IconKey)rand.Next(0, Enum.GetValues(typeof(IconKey)).Length),
                        IsSelect = false,
                        HasChildren = true ,// 先设为true，后面再修正
                        ViewMoel = UserViewModel
                    };

                    if (i % 15 == 0)
                    {
                        node.IsGroupNav = true;
                    }
                    level2Nodes.Add(node);
                    list.Add(node);
                    if (list.Count >= count) break;
                }
                if (list.Count >= count) break;
            }

            // 3级节点
            foreach (var parent in level2Nodes)
            {
                int childCount = rand.Next(2, 10); // 每个二级节点下2~10个三级节点
                for (int i = 0; i < childCount; i++)
                {
                    var node = new NavigateModel
                    {
                        Id = (idSeed++).ToString(),
                        ParentId = parent.Id,
                        Level = 2,
                        Order = i + 1,
                        IsGroupNav = false,
                        NavName = $"三级菜单_{parent.Order}_{i + 1}",
                        IsSelect = false,
                        HasChildren = false // 三级节点没有子节点
                    };
                    if (i % 25 == 0)
                    {
                        node.IsGroupNav = true;
                    }
                    list.Add(node);
                    if (list.Count >= count) break;
                }
                if (list.Count >= count) break;
            }

            // 修正HasChildren属性
            var idSet = new HashSet<string>(list.Select(x => x.ParentId).Where(x => !string.IsNullOrEmpty(x)));
            foreach (var node in list)
            {
                node.HasChildren = idSet.Contains(node.Id);
            }

            // 如果数量不够，补充三级节点
            while (list.Count < count)
            {
                var parent = level2Nodes[rand.Next(level2Nodes.Count)];
                var node = new NavigateModel
                {
                    Id = (idSeed++).ToString(),
                    ParentId = parent.Id,
                    Level = 2,
                    Order = rand.Next(1, 100),
                    IsGroupNav = false,
                    NavName = $"三级菜单_补_{list.Count + 1}",
                    IconKey = (IconKey)rand.Next(0, Enum.GetValues(typeof(IconKey)).Length),
                    IsSelect = false,
                    HasChildren = false
                };
                list.Add(node);
            }

            list = SortAsTree(list);
            return list;
        }

        public  List<NavigateModel> SortAsTree(List<NavigateModel> source)
        {
            var dict = source.ToDictionary(x => x.Id, x => x);
            // 按Order排序，找出所有根节点（ParentId为空或为null）
            var roots = source.Where(x => string.IsNullOrEmpty(x.ParentId)).OrderBy(x => x.Order).ToList();
            var result = new List<NavigateModel>();

            void AddChildren(NavigateModel parent)
            {
                result.Add(parent);
                var children = source
                    .Where(x => x.ParentId == parent.Id)
                    .OrderBy(x => x.Order)
                    .ToList();
                foreach (var child in children)
                {
                    AddChildren(child);
                }
            }

            foreach (var root in roots)
            {
                AddChildren(root);
            }

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
