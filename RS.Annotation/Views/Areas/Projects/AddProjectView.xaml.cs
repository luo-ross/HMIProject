using Microsoft.Win32;
using RS.Annotation.Views.Home;
using RS.Annotation.Widgets.Controls;
using RS.Annotation.Widgets.Enums;
using RS.Commons;
using RS.Widgets.Controls;
using RS.Widgets.Models;
using System.IO;
using System.Windows;

namespace RS.Annotation.Views.Areas
{
    /// <summary>
    /// AddProjectView.xaml 的交互逻辑
    /// </summary>
    public partial class AddProjectView : RSDialog
    {
        public HomeView HomeView { get; set; }
        public ProjectsViewModel ViewModel { get; set; }
    
        public AddProjectView(ProjectsView projectsView)
        {
            InitializeComponent();
            
            this.DataContext = projectsView.ViewModel;
            this.ViewModel = projectsView.ViewModel;
            this.Loaded += AddProjectView_Loaded;
        }

        private void AddProjectView_Loaded(object sender, RoutedEventArgs e)
        {
            //获取父窗口
            this.HomeView = this.TryFindParent<HomeView>();
            //设置默认新增项目任务类型为目标检测
            this.TaskDetect.IsChecked = true;
            //获取默认文件存储路径
            (string projectPath, string projectName) = GetDefaultProjectPathAndName();
            ProjectModel projectModel = new ProjectModel(Guid.NewGuid().ToString());
            projectModel.ProjectName = projectName;
            projectModel.ProjectPath = projectPath;
            //设置默认任务类型
            projectModel.Tasks = TaskEnum.Detect;

            this.ViewModel.ProjectModelAdd = projectModel;
        }


        private (string projectPath, string projectName) GetDefaultProjectPathAndName(string defaultDir = null, string projectName = null)
        {
            int num = 0;
            //这里循环检测获取默认项目名称 比如在当前用户文档里已经创建了新项目
            //那么会自动递增新项目1 2 3
            if (string.IsNullOrEmpty(defaultDir))
            {
                defaultDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            string projectPath = string.Empty;
            if (string.IsNullOrEmpty(projectName))
            {
                projectName = "新项目";
            }
            do
            {
                string projectNameNew = $"{projectName}{(num == 0 ? "" : num)}";
                projectPath = System.IO.Path.Combine(defaultDir, $"{projectNameNew}.rsdl");
                num++;
            } while (File.Exists(projectPath) || this.ViewModel.ProjectModelList.Any(t => t.ProjectPath == projectPath));
            return (projectPath, projectName);
        }

        /// <summary>
        /// 添加项目事件回调 参数bool 代表用户是点击了确认或者是取消 False代表取消 True代表确定
        /// </summary>
        public event Action<bool> AddProjectCallBack;


        private async void BtnChooseProjectPath_Click(object sender, RoutedEventArgs e)
        {
            //获取当前新增默认项目路径
            string projectPath = this.ViewModel.ProjectModelAdd.ProjectPath;
            string initialDirectory = System.IO.Path.GetDirectoryName(projectPath);
            string fileName = System.IO.Path.GetFileName(projectPath);
            (projectPath, string projectName) = GetDefaultProjectPathAndName(initialDirectory, fileName);
            initialDirectory = System.IO.Path.GetDirectoryName(projectPath);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "项目文件（*.rsdl）|*.rsdl";
            saveFileDialog.Title = "选择项目文件存储路径";
            saveFileDialog.InitialDirectory = initialDirectory;
            saveFileDialog.FileName = fileName;
            saveFileDialog.CheckFileExists = false;
            //获取用户自定义的文件存储路径
            if (saveFileDialog.ShowDialog() == true)
            {
                projectPath = saveFileDialog.FileName;
                this.ViewModel.ProjectModelAdd.ProjectPath = projectPath;
            }
        }

        /// <summary>
        /// 这里用户点击任务类型赋值
        /// </summary>
        private void TaskMethod_Click(object sender, RoutedEventArgs e)
        {
            var rsDLMethod = sender as RSDLMethod;
            var projectModelAdd = this.ViewModel.ProjectModelAdd;
            projectModelAdd.Tasks = rsDLMethod.Tasks;
        }


        /// <summary>
        /// 关闭按钮
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            AddProjectCallBack?.Invoke(false);
        }

        /// <summary>
        /// 确定按钮
        /// </summary>
        private async void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            //项目名称和项目路径
            var validResult = this.ViewModel.ProjectModelAdd.ValidObject();
            if (!validResult)
            {
                return;
            }

            //AddProjectCallBack?.Invoke(true);

            var validLoginResult = await this.Loading.InvokeAsync(async (cancellationToken) =>
            {

                DateTime dateTime = DateTime.Now;
                var projectModelAdd = this.ViewModel.ProjectModelAdd;
                projectModelAdd.CreateTime = dateTime;
                projectModelAdd.UpdateTime = dateTime;

                this.HomeView.Modal?.CloseModal();
                this.HomeView.ProjectView.SetFirstProjectModel(projectModelAdd);
                return OperateResult.CreateSuccessResult();
            });
        }
    }
}
