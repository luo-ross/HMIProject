using Microsoft.Win32;
using RS.Annotation.SQLite.DbContexts;
using RS.Commons.Extend;
using RS.Widgets.Controls;
using RS.Widgets.Models;
using RS.Annotation.Models;
using RS.Annotation.Views.Home;
using RS.Annotation.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RS.Annotation.Views.Areas
{
    /// <summary>
    /// ProjectsView.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectsView : RSUserControl
    {
        public HomeView HomeView { get; set; }
        public ProjectsViewModel ViewModel { get; set; }
        public ProjectsView()
        {
            InitializeComponent();
            this.ViewModel = this.DataContext as ProjectsViewModel;
            this.Loaded += ProjectsView_Loaded;
        }

        private void ProjectsView_Loaded(object sender, RoutedEventArgs e)
        {
            this.HomeView = this.TryFindParent<HomeView>();
        }

        private void BtnAddProject_Click(object sender, RoutedEventArgs e)
        {
            //在这里显示新增项目内容
            AddProjectView addProjectView = new AddProjectView(this);
            addProjectView.AddProjectCallBack += AddProjectView_AddProjectCallBack;
            this.HomeView.ShowModal(addProjectView);
        }

        private void AddProjectView_AddProjectCallBack(bool isConfirmed)
        {
            //如果确定
            if (isConfirmed)
            {

            }
            else
            {

            }
            this.HomeView.HideModal();
        }

        /// <summary>
        /// 设置首个项目
        /// </summary>
        /// <param name="projectModel"></param>
        public void SetFirstProjectModel(ProjectModel projectModel)
        {
            var projectModelList = this.ViewModel.ProjectModelList;
            if (projectModel != null)
            {
                //先移除
                if (projectModelList.Contains(projectModel))
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        projectModelList.Remove(projectModel);
                    });
                }

                //然后再插入到第一个位置
                this.Dispatcher.Invoke(() =>
                {
                    projectModelList.Insert(0, projectModel);
                });
            }

            if (this.ViewModel.ProjectModelSelect != null)
            {
                this.ViewModel.ProjectModelSelect.IsSelect = false;
            }

            this.ViewModel.ProjectModelSelect = projectModel;
            if (this.ViewModel.ProjectModelSelect != null)
            {
                this.ViewModel.ProjectModelSelect.IsSelect = true;
            }
        }

        /// <summary>
        /// 进入图像资源管理界面
        /// </summary>
        public void Go2PicturesView()
        {
            if (this.ViewModel.ProjectModelSelect == null)
            {
                return;
            }
            this.HomeView.ActivatePicturesView();
        }

        private void RadioBtnProject_Click(object sender, RoutedEventArgs e)
        {
            var rsProject = sender as RSProject;
            var projectModel = rsProject.Tag as ProjectModel;
            this.ViewModel.ProjectModelSelect = rsProject.ProjectModel;
        }


        private void RadioBtnProject_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            this.HomeView.ActivatePicturesView();
        }

        private void BtnGo2PicturesView_Click(object sender, RoutedEventArgs e)
        {
            this.HomeView.ActivatePicturesView();
        }

        private async void BtnOpenProject_Click(object sender, RoutedEventArgs e)
        {
            string initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "支持格式(*.rsdl)|*.rsdl";
            openFileDialog.Title = "选择项目";
            openFileDialog.InitialDirectory = initialDirectory;
            openFileDialog.CheckPathExists = true;
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true)
            {
                string projectPath = openFileDialog.FileName;

                //await this.HomeView.LoadingInvokeAsync(() =>
                //{
                //    var sdf =

                //});
            }
        }


        private ProjectModel LoadProjectActionAsync(string projectPath)
        {
            if (File.Exists(projectPath))
            {
                return null;
            }

            using (var db = new AnnotationDbContexts(projectPath))
            {
                var projectEnity = db.Projects.FirstOrDefault();
                if (projectEnity == null)
                {
                    return null;
                }

                ProjectModel projectModel = new ProjectModel(projectEnity.Id)
                {
                    CreateTime = projectEnity.CreateTime.ToTimeDate(),
                    Description = projectEnity.Description,
                    ProjectName = projectEnity.ProjectName,
                    ProjectPath = projectEnity.ProjectPath,
                    UpdateTime = projectEnity.UpdateTime.ToTimeDate(),
                };

                projectModel.IsSaved = true;

                //从数据库里加载项目都配置了哪些标签
                var tagModelList = db.Tags.Select(t => new TagModel(t.Id, t.ProjectId)
                {
                    IsShortCutAuto = t.IsShortCutAuto,
                    ClassName = t.ClassName,
                    IsSelect = t.IsSelect,
                    ShortCut = t.ShortCut,
                    TagColor = t.TagColor,
                }).ToList();

                //从数据库里加载项目都配置了哪些图像
                var imgModelList = db.Pictures.Select(t => new ImgModel(t.Id, t.ProjectId)
                {
                    ImgName = t.ImgName,
                    ImgPath = t.ImgPath,
                    IsWorking = t.IsWroking,
                    IsSelect = t.IsSelect,
                }).ToList();

                //从数据库里获取每张图像都标注了多少矩形
                var rectModelList = db.Rects.Where(t => t.ProjectId == projectModel.Id)
                    .Select(t => new RectModel(t.Id, t.PictureId, t.ProjectId)
                    {
                        Angle = t.Angle,
                        CanvasLeft = t.CanvasLeft,
                        CanvasTop = t.CanvasTop,
                        Height = t.Height,
                        ProjectId = projectModel.Id,
                        Width = t.Width,
                        TagModel = null
                    }).ToList();


                //获取每个标注矩形框的具体类别 比如颜色 标签等
                rectModelList = rectModelList.Join(tagModelList,
                    a => a.TagModel.Id,
                    b => b.Id,
                    (a, b) =>
                    {
                        a.TagModel = b;
                        return a;
                    }).ToList();


                //在这里查询每张图像具体的标注矩形
                imgModelList = imgModelList.GroupJoin(rectModelList,
                    a => a.Id,
                    b => b.PictureId,
                    (a, b) =>
                    {
                        a.RectModelList = new ObservableCollection<RectModel>(b);
                        return a;
                    }).ToList();


                projectModel.ImgModelList = new ObservableCollection<ImgModel>(imgModelList);
                projectModel.TagModelList = new ObservableCollection<TagModel>(tagModelList);




                foreach (var tagModel in projectModel.TagModelList)
                {
                    tagModel.IsSaved = true;
                }

                foreach (var imgModel in projectModel.ImgModelList)
                {
                    imgModel.IsSaved = true;
                }

                rectModelList = projectModel.ImgModelList.SelectMany(t => t.RectModelList).ToList();
                foreach (var rectModel in rectModelList)
                {
                    rectModel.IsSaved = true;
                }
                return projectModel;
            }
        }


        public void UpdateProjectThubailAsync(ProjectModel projectModel)
        {
            Task.Factory.StartNew(() =>
            {
                var imgModel = projectModel.ImgModelList.FirstOrDefault();
                if (imgModel == null)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        projectModel.ThubnailImg = null;
                    });
                    return;
                }

                if (imgModel.ThubnailImg == null)
                {

                }
            });
        }



    }
}
