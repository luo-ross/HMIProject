using Microsoft.Extensions.DependencyInjection;
using RS.Annotation.SQLite.DbContexts;
using RS.Annotation.SQLite.Entities;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extend;
using RS.Widgets.Controls;
using System.Reflection;
using System.Windows;

namespace RS.Annotation.Views.Home
{
    
    [ServiceInjectConfig(ServiceLifetime.Singleton)]
    public partial class HomeView : RSWindow
    {
        public HomeViewModel? ViewModel { get; set; }
        public HomeView()
        {
            InitializeComponent();
            this.ViewModel = this.DataContext as HomeViewModel;
            //程序设置默认项目是第一个
            this.RadBtnPorjectView.IsChecked = true;
            this.Loaded += HomeView_Loaded;
        }

        private void HomeView_Loaded(object sender, RoutedEventArgs e)
        {

            Assembly assembly = Assembly.GetExecutingAssembly();
            Version? version = assembly?.GetName()?.Version;
            if (this.ViewModel != null)
            {
                this.ViewModel.Version = version?.ToString();
            }
        }


        public void ActivatePicturesView()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.RadBtnPicturesView.Visibility = Visibility.Visible;
                this.RadBtnPicturesView.IsChecked = true;
            });
        }


        public void ActivateAnnotaionView()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.RadAnnotationView.Visibility = Visibility.Visible;
                this.RadAnnotationView.IsChecked = true;
            });
        }

        /// <summary>
        /// 保存项目
        /// </summary>
        private async void BtnSavae_Click(object sender, RoutedEventArgs e)
        {
            var projectModel = this.ProjectView.ViewModel.ProjectModelSelect;
            if (projectModel == null)
            {
                return;
            }
            await this.GetLoading().InvokeLoadingActionAsync(async (cancellationToken) =>
            {
                //获取项目
                var projectModel = this.ProjectView.ViewModel.ProjectModelSelect;
                //保存项目
                Projects project = new Projects()
                {
                    CreateTime = projectModel.CreateTime.ToTimeStamp(),
                    Description = projectModel.Description,
                    Id = projectModel.Id,
                    ProjectName = projectModel.ProjectName,
                    ProjectPath = projectModel.ProjectPath,
                    UpdateTime = DateTime.Now.ToTimeStamp()
                };

                //保存图像
                List<Pictures> pictureList = new List<Pictures>();
                //保存标注矩形块
                List<Rects> rectsList = new List<Rects>();
                foreach (var imgModel in projectModel.ImgModelList)
                {
                    Pictures pictures = new Pictures()
                    {
                        Id = imgModel.Id,
                        ImgName = imgModel.ImgName,
                        ImgPath = imgModel.ImgPath,
                        IsSelect = imgModel.IsSelect,
                        IsWroking = imgModel.IsWorking,
                        ProjectId = imgModel.ProjectId,
                    };

                    pictureList.Add(pictures);

                    //保存每个矩形块的标注数据
                    foreach (var rectModel in imgModel.RectModelList)
                    {
                        Rects rects = new Rects()
                        {
                            Angle = rectModel.Angle,
                            CanvasLeft = rectModel.CanvasLeft,
                            CanvasTop = rectModel.CanvasTop,
                            Height = rectModel.Height,
                            Id = rectModel.Id,
                            PictureId = rectModel.PictureId,
                            ProjectId = rectModel.ProjectId,
                            TagId = rectModel.TagModel.Id,
                            Width = rectModel.Width,
                        };
                        rectsList.Add(rects);
                    }

                }

                //保存标签
                List<Tags> tagList = new List<Tags>();
                foreach (var tagModel in projectModel.TagModelList)
                {
                    Tags tags = new Tags()
                    {
                        IsShortCutAuto = tagModel.IsShortCutAuto,
                        ClassName = tagModel.ClassName,
                        Id = tagModel.Id,
                        IsSelect = tagModel.IsSelect,
                        ProjectId = tagModel.ProjectId,
                        ShortCut = tagModel.ShortCut,
                        TagColor = tagModel.TagColor,
                    };
                }

                //所有数据保存到数据库
                using (var db = new AnnotationDbContexts(projectModel.ProjectPath))
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            await db.Projects.AddAsync(project);
                            await db.Pictures.AddRangeAsync(pictureList);
                            await db.Rects.AddRangeAsync(rectsList);
                            await db.Tags.AddRangeAsync(tagList);
                            await trans.CommitAsync();
                        }
                        catch (Exception)
                        {
                            await trans.RollbackAsync();
                            throw;
                        }
                    }
                }

                return OperateResult.CreateSuccessResult();
            });
        }


        /// <summary>
        /// 项目另存为
        /// </summary>
        private void BtnSavaeAs_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
