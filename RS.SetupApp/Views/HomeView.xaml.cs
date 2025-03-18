using IWshRuntimeLibrary;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using RS.SetupApp.Controls;
using RS.SetupApp.Models;
using RS.Widgets.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using File = System.IO.File;


namespace RS.SetupApp.Views
{
    /// <summary>
    /// HomeView.xaml 的交互逻辑
    /// </summary>
    public partial class HomeView : RSWindow
    {
        public HomeViewModel ViewModel { get; set; }

        /// <summary>
        /// 这是安装程序逻辑
        /// </summary>
        List<InstallTask> InstallTaskList = new List<InstallTask>();

        /// <summary>
        /// 用户预安装的实际路径
        /// </summary>
        private string InstallActualPath;
        public HomeView()
        {
            InitializeComponent();
            this.ViewModel = this.DataContext as HomeViewModel;
            this.Loaded += HomeView_Loaded;
        }

        private async Task<bool> HomeView_BeforeWinClose()
        {
            //if (this.ViewModel.InstallTaskStatus == TaskStatus.Running)
            //{
            //    this.InstallCTS.Cancel();
            //    await this.MessageBox.Show("任务正在进行中！");
            //    return false;
            //}
            return true;
        }

        private async void HomeView_Closing(object? sender, CancelEventArgs e)
        {

        }


        private void HomeView_Loaded(object sender, RoutedEventArgs e)
        {
            //获取安装包实际大小
            this.ViewModel.DiskSizeNeed = GetProgramSize();
            //检查安装路径是否可用
            this.CheckInstallDirIsAvailable();
        }


        private void BtnUserServiceAgreement_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Navigate(e);
        }

        private void BtnPrivacyPolicy_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Navigate(e);
        }

        private void Navigate(System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start("explorer", e.Uri.ToString());
            e.Handled = true;
        }

        /// <summary>
        /// 自定义安装路径
        /// </summary>
        private void CkCustomInstallDir_Click(object sender, RoutedEventArgs e)
        {
            if (this.CkCustomInstall.IsChecked == true)
            {
                this.Height = 460 + 100;
            }
            else
            {
                this.Height = 460;
            }
        }

        private void BtnChooseDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "请选择一个文件夹";
            dialog.UseDescriptionForTitle = true;
            dialog.RootFolder = Environment.SpecialFolder.Desktop;
            dialog.ShowNewFolderButton = true;
            dialog.Multiselect = false;
            IntPtr hwnd = ((HwndSource)PresentationSource.FromVisual(this)).Handle;
            if (dialog.ShowDialog(hwnd) == true)
            {
                string selectFolder = dialog.SelectedPath;
                this.ViewModel.InstallDir = selectFolder;
            }
            var istaFolderBrowserDialog = new VistaFolderBrowserDialog();
        }

        private void TxtInstallDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.ViewModel == null)
            {
                return;
            }
            this.CheckInstallDirIsAvailable();
        }


        /// <summary>
        /// 检查安装路径是否可用
        /// </summary>
        public void CheckInstallDirIsAvailable()
        {
            this.ViewModel.ClearErrorTooltip();

            //判断路径空间是否够
            this.ViewModel.DiskSizeFree = 0;

            string installDir = this.ViewModel.InstallDir;
            //判断这个路径是否存在
            if (!Directory.Exists(installDir))
            {
                DirectoryInfo directoryInfo = null;
                try
                {
                    //通过这种方法暂时解决验证用户输入的合法性，并切能够准确判断给
                    //出具体的错误提示
                    directoryInfo = Directory.CreateDirectory(installDir);
                    //保存下用户预安装的实际路径
                    this.InstallActualPath = directoryInfo.FullName;
                }
                catch (NotSupportedException)
                {
                    this.ViewModel.ErrorTooltip = "路径包含一个冒号字符（：），该字符不是驱动器标签（“C:\\”）的一部分";
                    this.ViewModel.IsCanInstall = false;
                    return;
                }
                catch (DirectoryNotFoundException)
                {
                    this.ViewModel.ErrorTooltip = "指定的路径无效（例如，它位于未映射的驱动器上）。";
                    this.ViewModel.IsCanInstall = false;
                    return;
                }
                catch (PathTooLongException)
                {
                    this.ViewModel.ErrorTooltip = "指定的路径、文件名或两者都超过了系统定义的最大长度。";
                    this.ViewModel.IsCanInstall = false;
                    return;
                }
                catch (IOException)
                {
                    this.ViewModel.ErrorTooltip = "路径指定的目录是一个文件-或-网络名称未知";
                    this.ViewModel.IsCanInstall = false;
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    this.ViewModel.ErrorTooltip = "没有所需的权限";
                    this.ViewModel.IsCanInstall = false;
                    return;
                }
                catch (ArgumentNullException)
                {
                    this.ViewModel.ErrorTooltip = "路径不能为空";
                    this.ViewModel.IsCanInstall = false;
                    return;
                }
                catch (ArgumentException)
                {
                    this.ViewModel.ErrorTooltip = "路径无效";
                    this.ViewModel.IsCanInstall = false;
                    return;
                }
                finally
                {
                    if (directoryInfo != null)
                    {
                        directoryInfo.Delete();
                    }
                }
            }
            else
            {
                this.InstallActualPath = this.ViewModel.InstallDir;
            }

            string driveLetter = Path.GetPathRoot(this.InstallActualPath);
            if (!string.IsNullOrEmpty(driveLetter))
            {
                try
                {
                    DriveInfo driveInfo = new DriveInfo(driveLetter);
                    if (driveInfo.IsReady)
                    {
                        this.ViewModel.DiskSizeFree = driveInfo.AvailableFreeSpace;
                        //在这里判断可用空间是否满足装需求 安装控件需要比实际需要多100M
                        if (this.ViewModel.DiskSizeFree > this.ViewModel.DiskSizeNeed + 100 * 1024 * 1024)
                        {
                            this.ViewModel.IsCanInstall = true;
                            return;
                        }
                        else
                        {
                            this.ViewModel.ErrorTooltip = $"盘符 {driveLetter}可用空间不足。";
                        }
                    }
                    else
                    {
                        this.ViewModel.ErrorTooltip = $"盘符 {driveLetter} 不可用。";
                    }
                }
                catch (ArgumentException)
                {
                    this.ViewModel.ErrorTooltip = $"无效驱动器";
                }
            }
            else
            {
                this.ViewModel.ErrorTooltip = "路径格式错误，无法提取盘符。";
            }

            this.ViewModel.IsCanInstall = false;
        }




        private long GetProgramSize()
        {
            long totalSize = 0;
            using (MemoryStream memoryStream = new MemoryStream(Resource.MyPublish))
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        totalSize += entry.Length;
                    }
                }
            }
            return totalSize;
        }

        private int GetProgramZipList()
        {
            using (MemoryStream memoryStream = new MemoryStream(Resource.MyPublish))
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    return archive.Entries.Count;
                }
            }
        }


        private QiyiMessageBox QiyiMessageBox;
        private bool ShowQiyiMessageBox(object resourceKey)
        {
            return this.Dispatcher.Invoke(() =>
             {
                 var readAndAgreementConfirmMessage = this.TryFindResource(resourceKey);
                 QiyiMessageBox = new QiyiMessageBox()
                 {
                     MessageContent = readAndAgreementConfirmMessage,
                     Owner = this,
                 };
                 return QiyiMessageBox.ShowDialog() == true;
             });
        }


        private async void BtnInsall_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ClearErrorTooltip();
            //假如用户没有勾选阅读并同意用户协议和隐私政策
            //则提示用户进行选择
            if (!this.ViewModel.IsReadAndAgree)
            {
                var result = this.ShowQiyiMessageBox("ReadAndAgreementConfirmMessage");
                if (!result)
                {
                    return;
                }
                this.ViewModel.IsReadAndAgree = true;
            }

            //检查当前运行进程里是否有我们的程序正在运行
            //假如有运行的话我就提示用户线先关闭程序再进行安装
            var killProgramResult = KillProgram(true);
            if (killProgramResult != null)
            {
                this.ViewModel.ErrorTooltip = killProgramResult;
                return;
            }

            //恢做一些安装前的准备工作
            //回去窗口高度
            this.Height = 460;

            //检查通过则开始进行安装
            await InstallProgramAsync();
        }

        private string KillProgram(bool isNeedConfirm)
        {
            //检查当前运行进程里是否有我们的程序正在运行
            //假如有运行的话我就提示用户线先关闭程序再进行安装
            string targetProcessName = "RS.WPFApp";
            (bool isRunning, Process process) = CheckProgressIsRunning(targetProcessName);
            if (isRunning)
            {
                if (isNeedConfirm)
                {
                    var result = this.ShowQiyiMessageBox("IsContinueInstallConfirmMessage");
                    if (!result)
                    {
                        return null;
                    }
                }

                //如果程序在打开的情况下用户依然选择继续安装
                //我们则需要主动结束进程
                try
                {
                    process.Kill();
                }
                catch (Win32Exception)
                {
                    return "无法终止关联的进程";
                }
                catch (NotSupportedException)
                {
                    return "您正在尝试呼叫系统。诊断。过程。终止在远程计算机上运行的进程。该方法仅适用于在本地计算机上运行的进程。";
                }
                catch (InvalidOperationException)
                {
                    return "没有与此系统关联的进程。诊断。处理对象。";
                }
            }
            return null;
        }

        /// <summary>
        /// 消息提示框确认
        /// </summary>
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            this.QiyiMessageBox.DialogResult = true;
            this.QiyiMessageBox?.Close();
        }

        /// <summary>
        /// 消息提示框取消
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.QiyiMessageBox.DialogResult = false;
            this.QiyiMessageBox?.Close();
        }

        /// <summary>
        /// 安装可取消任务
        /// </summary>
        private CancellationTokenSource InstallCTS { get; set; }

        /// <summary>
        /// 检查程序是否正在运行
        /// </summary>
        private (bool isRunning, Process process) CheckProgressIsRunning(string targetProcessName)
        {
            bool isRunning = false;
            // 获取当前运行的所有进程
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName.Equals(targetProcessName, StringComparison.OrdinalIgnoreCase))
                {
                    isRunning = true;
                    return (isRunning, process);
                }
            }
            return (false, null);
        }



        /// <summary>
        /// 解压文件
        /// </summary>
        private async Task<TaskStatus> UnZipProgramFileAsync(CancellationToken token)
        {
            //创建安装目录
            if (!Directory.Exists(this.InstallActualPath))
            {
                try
                {
                    Directory.CreateDirectory(this.InstallActualPath);
                }
                catch (Exception)
                {
                    this.ViewModel.RunningTooltip = $"创建安装目录失败";
                    return TaskStatus.Faulted;
                }
            }

            if (token.IsCancellationRequested)
            {
                return TaskStatus.Canceled;
            }

            //解压缩文件
            using (MemoryStream stream = new MemoryStream(Resource.MyPublish))
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    //循环解压
                    for (int i = 0; i < archive.Entries.Count; i++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return TaskStatus.Canceled;
                        }
                        var entry = archive.Entries[i];

                        //获取到加压路径
                        var targetPath = Path.Combine(this.InstallActualPath, entry.FullName);
                        this.ViewModel.RunningTooltip = @$"正在解压 {targetPath}";
                        if (entry.Length == 0)
                        {
                            Directory.CreateDirectory(targetPath);
                        }
                        else
                        {
                            using (var inputStream = entry.Open())
                            {
                                if (File.Exists(targetPath))
                                {
                                    int checkCount = 0;
                                IL001:
                                    if (IsFileLocked(targetPath))
                                    {
                                        checkCount++;
                                        this.ViewModel.RunningTooltip = "检测到文件被占用，请及时关闭主程序！";
                                        await Task.Delay(10 * 1000);
                                        if (checkCount < 2)
                                        {
                                            goto IL001;
                                        }
                                        //否则我们主动结束程序进程
                                        var killProgramResult = KillProgram(false);
                                        if (killProgramResult != null)
                                        {
                                            this.ViewModel.RunningTooltip = killProgramResult;
                                            return TaskStatus.Canceled;
                                        }
                                        goto IL001;
                                    }
                                }

                                using (var outputStream = File.Create(targetPath))
                                {
                                    try
                                    {
                                        await inputStream.CopyToAsync(outputStream, token);
                                    }
                                    catch (TaskCanceledException ex)
                                    {

                                    }
                                }
                            }
                        }

                        await Task.Delay(20);
                        this.ViewModel.AccumulatedSteps += 1;
                    }

                }
            }

            return TaskStatus.RanToCompletion;
        }





        /// <summary>
        /// 创建桌面快捷方式
        /// </summary>
        private async Task<TaskStatus> CreateDesktopShortCutAsync(CancellationToken token)
        {
            string shorCutPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{SoftwareName}.lnk");
            //删除掉再更新快捷方式
            if (File.Exists(shorCutPath))
            {
                File.Delete(shorCutPath);
            }
            string targetExePath = GetTargetExePath();
            CreateShortCut(shorCutPath, targetExePath, this.InstallActualPath);
            return TaskStatus.RanToCompletion;
        }

        private const string SoftwareName = "标注软件";
        private void CreateShortCut(string shorCutPath, string targetPath, string workingDirectory)
        {
            WshShell shell = new WshShell();

            //通过该对象的 CreateShortcut 方法来创建 IWshShortcut 接口的实例对象
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shorCutPath);

            //设置快捷方式的目标所在的位置(源程序完整路径)
            shortcut.TargetPath = targetPath;

            //应用程序的工作目录
            //当用户没有指定一个具体的目录时，快捷方式的目标应用程序将使用该属性所指定的目录来装载或保存文件。
            shortcut.WorkingDirectory = workingDirectory;

            //目标应用程序窗口类型(1.Normal window普通窗口,3.Maximized最大化窗口,7.Minimized最小化)
            shortcut.WindowStyle = 1;

            //快捷方式的描述
            shortcut.Description = $"这是Ross开发的{SoftwareName}，请大家多多点赞！哈哈哈哈";

            //可以自定义快捷方式图标.(如果不设置, 则将默认源文件图标.)
            //shortcut.IconLocation = System.Environment.SystemDirectory + "\\" + "shell32.dll, 165";

            //设置应用程序的启动参数(如果应用程序支持的话)
            //shortcut.Arguments = "/myword /d4s";

            //设置快捷键(如果有必要的话.)
            //shortcut.Hotkey = "CTRL+ALT+D";

            //保存快捷方式
            shortcut.Save();

        }


        /// <summary>
        /// 添加开始菜单快捷方式
        /// </summary>
        private async Task<TaskStatus> CreateStartMenuShortCutAsync(CancellationToken token)
        {
            string shorCutPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), $"{SoftwareName}.lnk"); ;
            //删除掉再更新快捷方式
            if (File.Exists(shorCutPath))
            {
                File.Delete(shorCutPath);
            }
            string targetExePath = GetTargetExePath();
            CreateShortCut(shorCutPath, targetExePath, this.InstallActualPath);
            return TaskStatus.RanToCompletion;
        }

        /// <summary>
        /// 添加程序菜单快捷方式
        /// </summary>
        private async Task<TaskStatus> CreateStartMenuProgramShortCutAsync(CancellationToken token)
        {
            //添加卸载快捷方式
            string shorCutPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "卸载.lnk");

            //删除掉再更新快捷方式
            if (File.Exists(shorCutPath))
            {
                File.Delete(shorCutPath);
            }
            //这里实际应该替换成卸载的程序路径
            string targetExePath = GetTargetExePath();
            CreateShortCut(shorCutPath, targetExePath, this.InstallActualPath);
            return TaskStatus.RanToCompletion;
        }


        /// <summary>
        /// 添加程序和功能
        /// </summary>
        private async Task<TaskStatus> AddProgramAsync(CancellationToken token)
        {
            // 打开指定的注册表主键（HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall）
            RegistryKey uninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", true);

            // 在该主键下创建名为Annotation的子项
            RegistryKey annotationKey = uninstallKey.OpenSubKey("Annotation", true) ?? uninstallKey.CreateSubKey("Annotation");
            if (annotationKey != null)
            {
                // 设置字符串值Comments
                annotationKey.SetValue("Comments", $"这是Ross开发的{SoftwareName}", RegistryValueKind.String);

                // 设置字符串值DisplayIcon
                string targetExePath = GetTargetExePath();
                annotationKey.SetValue("DisplayIcon", targetExePath, RegistryValueKind.String);

                // 设置字符串值DisplayName
                annotationKey.SetValue("DisplayName", $"{SoftwareName}", RegistryValueKind.String);

                // 设置字符串值DisplayVersion
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(targetExePath);
                annotationKey.SetValue("DisplayVersion", $"{versionInfo.FileVersion}", RegistryValueKind.String);

                // 设置字符串值程序大小EstimatedSize
                var estimatedSize = this.ViewModel.DiskSizeNeed / 1024;
                annotationKey.SetValue("EstimatedSize", estimatedSize, RegistryValueKind.DWord);

                // 设置字符串值HelpLink
                annotationKey.SetValue("HelpLink", $"www.douyin.com", RegistryValueKind.String);

                // 设置是否可以进行程序更改 1是没有 0代表有
                annotationKey.SetValue("NoModify", 1, RegistryValueKind.DWord);

                // 设置是否可以进行进行修复 1是没有 0代表有
                annotationKey.SetValue("NoRepair", 1, RegistryValueKind.DWord);

                // 设置Publisher
                annotationKey.SetValue("Publisher", "这是Ross发布的", RegistryValueKind.String);

                // 设置SetupPath
                annotationKey.SetValue("SetupPath", $"{this.InstallActualPath}", RegistryValueKind.String);

                // 设置SetupType
                annotationKey.SetValue("SetupType", $"{1}", RegistryValueKind.String);

                // 设置UninstallString
                annotationKey.SetValue("UninstallString", $@"d:\Program Files\IQIYI Video\LStyle\12.8.5.8632\QyUninst.exe", RegistryValueKind.String);

                // 设置URLInfoAbout
                annotationKey.SetValue("URLInfoAbout", $@"https://www.iqiyi.com?src=clienticon", RegistryValueKind.String);

                // 设置URLUpdateInfo
                annotationKey.SetValue("URLUpdateInfo", $@"https://www.iqiyi.com?src=clienticon", RegistryValueKind.String);

                annotationKey.Close();
            }

            uninstallKey.Close();
            return TaskStatus.RanToCompletion;
        }

        private string GetTargetExePath()
        {
            return Path.Join(this.InstallActualPath, "RS.WPFApp.exe");
        }

        /// <summary>
        /// 设置开机自启动
        /// </summary>
        private async Task<TaskStatus> SetAutoRunAsync(CancellationToken token)
        {
            RegistryKey runKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (runKey != null)
            {
                //设置程序自启动的完整路径
                string targetExePath = GetTargetExePath();
                runKey.SetValue("RSAnnotation", targetExePath, RegistryValueKind.String);
                runKey.Close();
            }
            return TaskStatus.RanToCompletion;
        }

        /// <summary>
        /// 注册自定义文件rsdl文件打开格式
        /// </summary>
        private async Task<TaskStatus> SetAutoOpenRsdlFileAsync(CancellationToken token)
        {
            RegistryKey classesRoot = Registry.ClassesRoot;
            if (classesRoot != null)
            {
                //设置程序自启动的完整路径
                string targetExePath = GetTargetExePath();

                //创建.rsdl扩展名默认打开程序的文件
                RegistryKey rsdlKey = classesRoot.OpenSubKey(".rsdl", true) ?? classesRoot.CreateSubKey(".rsdl");
                if (rsdlKey != null)
                {
                    RegistryKey openWithProgidsKey = rsdlKey.OpenSubKey("OpenWithProgids", true) ?? rsdlKey.CreateSubKey("OpenWithProgids");
                    if (openWithProgidsKey != null)
                    {
                        openWithProgidsKey.SetValue("RsdlFile", "");
                        openWithProgidsKey.Close();
                    }
                    rsdlKey.Close();
                }

                //注册打开默认文件程序
                RegistryKey rsdlFileKey = classesRoot.OpenSubKey("RsdlFile", true) ?? classesRoot.CreateSubKey("RsdlFile");
                if (rsdlFileKey != null)
                {
                    RegistryKey defaultIconKey = rsdlFileKey.OpenSubKey("DefaultIcon", true) ?? rsdlFileKey.CreateSubKey("DefaultIcon");
                    if (defaultIconKey != null)
                    {
                        //表示程序exe中的第一个图标（索引为 0）。
                        defaultIconKey.SetValue("", @$"{this.GetTargetExePath()},0");
                        defaultIconKey.Close();
                    }
                    RegistryKey shellIconKey = rsdlFileKey.OpenSubKey("shell", true) ?? rsdlFileKey.CreateSubKey("shell");
                    if (shellIconKey != null)
                    {
                        RegistryKey openKey = shellIconKey.OpenSubKey("open", true) ?? shellIconKey.CreateSubKey("open");
                        if (openKey != null)
                        {
                            openKey.SetValue("", @$"RsdlFile");
                            openKey.SetValue("FriendlyAppName", @$"RsdlFile");
                            RegistryKey commandKey = openKey.OpenSubKey("command", true) ?? openKey.CreateSubKey("command");
                            if (commandKey != null)
                            {
                                commandKey.SetValue("", @$"{this.GetTargetExePath()} %1");
                                commandKey.Close();
                            }
                            openKey.Close();
                        }
                        shellIconKey.Close();
                    }
                    rsdlFileKey.Close();
                }
                classesRoot.Close();
            }
            return TaskStatus.RanToCompletion;
        }


        /// <summary>
        /// 文件是否锁定
        /// </summary>
        /// <param name="filePath">文件全路径</param>
        /// <returns></returns>
        private bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    // 如果能成功打开文件并且获取独占读取权限，说明文件没有被其他进程占用
                    return false;
                }
            }
            catch (IOException)
            {
                // 如果抛出IOException异常，很可能是因为文件被其他进程占用
                return true;
            }
        }


        /// <summary>
        /// 安装程序主任务
        /// </summary>
        /// <returns></returns>
        private async Task InstallProgramAsync()
        {
            //设置安装步骤进入到安装进行中
            this.ViewModel.InstallTaskStatus = TaskStatus.Running;
            this.InstallCTS = new CancellationTokenSource();
            //获取取消Token
            var cancellationToken = this.InstallCTS.Token;

            this.InstallTaskList = new List<InstallTask>();
            //添加解压文件事件
            this.InstallTaskList.Add(new InstallTask()
            {
                InstallAction = UnZipProgramFileAsync,
                InstallTooltip = "正在解压文件中",
                TaskStatus = TaskStatus.Created,
                GetTaskStepAsync = (token) =>
                {
                    return GetProgramZipList();
                }
            });

            //创建桌面快捷方式
            this.InstallTaskList.Add(new InstallTask()
            {
                InstallAction = CreateDesktopShortCutAsync,
                InstallTooltip = "正在添加桌面快捷方式",
                TaskStatus = TaskStatus.Created,
                GetTaskStepAsync = (token) =>
                {
                    //返回获取任务个数
                    return 1;
                }
            });

            //添加开始菜单
            this.InstallTaskList.Add(new InstallTask()
            {
                InstallAction = CreateStartMenuShortCutAsync,
                InstallTooltip = "正在添加开始菜单快捷方式",
                TaskStatus = TaskStatus.Created,
                GetTaskStepAsync = (token) =>
                {
                    //返回获取任务个数
                    return 1;
                }
            });

            //添加程序开始菜单
            this.InstallTaskList.Add(new InstallTask()
            {
                InstallAction = CreateStartMenuProgramShortCutAsync,
                InstallTooltip = "正在添加程序开始菜单",
                TaskStatus = TaskStatus.Created,
                GetTaskStepAsync = (token) =>
                {
                    //返回获取任务个数
                    return 1;
                }
            });

            //添加程序和功能
            this.InstallTaskList.Add(new InstallTask()
            {
                InstallAction = AddProgramAsync,
                InstallTooltip = "正在注册程序...",
                TaskStatus = TaskStatus.Created,
                GetTaskStepAsync = (token) =>
                {
                    //返回获取任务个数
                    return 1;
                }
            });

            //注册自定义文件打开格式
            this.InstallTaskList.Add(new InstallTask()
            {
                InstallAction = SetAutoOpenRsdlFileAsync,
                InstallTooltip = "正在注册默认文件打开程序...",
                TaskStatus = TaskStatus.Created,
                GetTaskStepAsync = (token) =>
                {
                    //返回获取任务个数
                    return 1;
                }
            });

            //设置开机自启动
            if (this.ViewModel.IsAllowAutoRun)
            {
                this.InstallTaskList.Add(new InstallTask()
                {
                    InstallAction = SetAutoRunAsync,
                    InstallTooltip = "正在设置开机自启动...",
                    TaskStatus = TaskStatus.Created,
                    GetTaskStepAsync = (token) =>
                    {
                        //返回获取任务个数
                        return 1;
                    }
                });
            }

            //开始执行安装任务
            var taskStatus = await await Task.Factory.StartNew(async () =>
            {
                try
                {
                    this.ViewModel.IsCloseBtnVisibility = Visibility.Collapsed;

                    //重置进度条
                    this.ViewModel.ResetProgress();

                    //在执行之前我需要去计算总任务步数
                    foreach (var installTask in this.InstallTaskList)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return TaskStatus.Canceled;
                        }
                        if (installTask.GetTaskStepAsync != null)
                        {
                            this.ViewModel.TotalProgressStep += installTask.GetTaskStepAsync(cancellationToken);
                        }
                    }

                    //这里循环进行任务安装
                    for (int i = 0; i < this.InstallTaskList.Count; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return TaskStatus.Canceled;
                        }
                        var installTask = InstallTaskList[i];
                        //每个安装任务都是可等待并且是可取消的
                        var result = await installTask?.InstallAction?.Invoke(cancellationToken);
                    }
                    return TaskStatus.RanToCompletion;
                }
                finally
                {
                    this.ViewModel.IsCloseBtnVisibility = Visibility.Visible;
                }
            }, cancellationToken);


            switch (taskStatus)
            {
                case TaskStatus.Created:
                    break;
                case TaskStatus.WaitingForActivation:
                    break;
                case TaskStatus.WaitingToRun:
                    break;
                case TaskStatus.Running:
                    break;
                case TaskStatus.WaitingForChildrenToComplete:
                    break;
                case TaskStatus.RanToCompletion:  //如果安装成功
                    //则进行安装完成的界面
                    this.ViewModel.InstallTaskStatus = TaskStatus.RanToCompletion;
                    //判断是否再次显示提示用户是否设置开机自动启动
                    if (!this.ViewModel.IsAllowAutoRun)
                    {
                        this.CkIsAllowAutoRun.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.CkIsAllowAutoRun.Visibility = Visibility.Collapsed;
                    }
                    break;
                case TaskStatus.Canceled:
                    break;
                case TaskStatus.Faulted:
                    break;
                default:
                    break;
            }
            return;
        }

        private async void BtnEnterMainProgram_Click(object sender, RoutedEventArgs e)
        {
            //判断用户是否设置了允许开机自启动
            if (this.ViewModel.IsAllowAutoRun)
            {
                await SetAutoRunAsync(CancellationToken.None);
            }
            string targetExePath = this.GetTargetExePath();
            // 获取当前进程
            Process currentProcess = Process.GetCurrentProcess();
            // 启动另一个程序
            Process.Start(targetExePath);
            // 强制关闭当前进程（这种方式相对比较“强硬”，可能不会触发一些正常关闭相关的清理逻辑等）
            currentProcess.Kill();
        }


    }
}
