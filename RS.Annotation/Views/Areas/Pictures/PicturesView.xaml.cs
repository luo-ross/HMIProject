using IdGen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using RS.Commons;
using RS.Commons.Helper;
using RS.Widgets.Controls;
using RS.Widgets.Models;
using RS.Annotation.Views.Areas.Pictures;
using RS.Annotation.Views.Home;
using RS.Annotation.Widgets.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace RS.Annotation.Views.Areas
{
   
    public partial class PicturesView : RSDialog
    {
        public IdGenerator IdGenerator { get; set; }
        public PicturesViewModel ViewModel { get; set; }

        private readonly int ImgBorderMarginTop = 3;
        private readonly int ImgBorderMarginLeft = 3;
        private readonly int ImgBorderMarginBottom = 3;
        private readonly int ImgBorderMarginRight = 3;
        public HomeView HomeView { get; set; }
        public PicturesView()
        {
            InitializeComponent();
            this.IdGenerator = App.ServiceProvider?.GetRequiredService<IdGenerator>();
            this.ViewModel = this.DataContext as PicturesViewModel;
            this.Loaded += PicturesView_Loaded;
        }
        private void PicturesView_Loaded(object sender, RoutedEventArgs e)
        {
            this.HomeView = this.TryFindParent<HomeView>();
        }

        public ProjectsView ProjectsView
        {
            get { return (ProjectsView)GetValue(ProjectsViewProperty); }
            set { SetValue(ProjectsViewProperty, value); }
        }

        public static readonly DependencyProperty ProjectsViewProperty =
            DependencyProperty.Register("ProjectsView", typeof(ProjectsView), typeof(PicturesView), new PropertyMetadata(default));

        public ProjectModel ProjectModelSelect
        {
            get { return (ProjectModel)GetValue(ProjectModelSelectProperty); }
            set { SetValue(ProjectModelSelectProperty, value); }
        }

        public static readonly DependencyProperty ProjectModelSelectProperty =
            DependencyProperty.Register("ProjectModelSelect", typeof(ProjectModel), typeof(PicturesView), new PropertyMetadata(default, OnProjectModelSelectPropertyChanged));

        private static void OnProjectModelSelectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picturesView = d as PicturesView;

            if (e.OldValue is ProjectModel projectModel)
            {
                foreach (var imgModel in projectModel.ImgModelList)
                {
                    if (imgModel.BorderHost != null && imgModel.BorderHost.Child is RSImg rsImg)
                    {
                        BindingOperations.ClearAllBindings(rsImg);
                        imgModel.ThubnailImg = null;
                        imgModel.IsCanRead = null;
                    }
                    imgModel.BorderHost = null;
                }
            }
            picturesView.RefreshImgSource(true, true);
            picturesView.SetDefaultImgModelSelect(null, false);
        }

        private CancellationTokenSource LoadImgSourceTaskCTS;
        private static object RefreshImgModelDisplayLock = new object();
        public void RefreshImgModelDisplay(bool isReinitBorderHost = false)
        {
            //this.SetDefaultImgModelSelect(null, false);
        }


        private void RefreshImgSource(bool isScrollDown, bool isClearCanvas, bool isRefreshZoom = false)
        {

            CancellationToken token;
            lock (RefreshImgModelDisplayLock)
            {
                //在这里检测是否有异步任务正在执行
                if (this.LoadImgSourceTaskCTS != null)
                {
                    //如果有正在执行的异步任务，我们取消它
                    this.LoadImgSourceTaskCTS.Cancel();
                }
                this.LoadImgSourceTaskCTS = new CancellationTokenSource();
                token = LoadImgSourceTaskCTS.Token;
            }


            try
            {
                Task.Factory.StartNew(() =>
                {
                    ProjectModel projectModel = null;
                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            projectModel = this.ProjectModelSelect;
                        }, DispatcherPriority.Background, token);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }

                    if (this.ViewModel == null || projectModel == null)
                    {
                        return;
                    }

                    if (isRefreshZoom)
                    {
                        this.UpdateImgZoom(token: token);
                    }

                    if (isClearCanvas)
                    {
                        try
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                this.ImgModelCanvas.Children.Clear();
                            }, DispatcherPriority.Background, token);
                        }
                        catch (TaskCanceledException)
                        {
                            return;
                        }
                    }

                    var imgModelList = projectModel.ImgModelList;
                    double verticalOffset = 0;

                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            verticalOffset = this.ImgModelScrollViewer.VerticalOffset;
                        }, DispatcherPriority.Background, token);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }


                    double viewWidth = 0;
                    double viewHeight = 0;

                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            viewWidth = this.ImgModelScrollViewer.ActualWidth;
                            viewHeight = this.ImgModelScrollViewer.ActualHeight;
                        }, DispatcherPriority.Background, token);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }

                    if (viewWidth == 0 || viewHeight == 0)
                    {
                        return;
                    }

                    if (this.ViewModel.ZoomLevel == 0)
                    {
                        this.UpdateImgZoom(5, token);
                    }

                    var imgActualWidth = this.ViewModel.ImgWidth + this.ImgBorderMarginLeft + this.ImgBorderMarginRight;
                    var imgActualheight = this.ViewModel.ImgHeight + this.ImgBorderMarginTop + this.ImgBorderMarginBottom;

                    //计算出有多少列
                    var cols = (int)(viewWidth / imgActualWidth);

                    //计算出一个视窗他能够显示多上行
                    var verticalNums = (int)(viewHeight / imgActualheight);

                    //计算出一共有多上行
                    var rows = (int)Math.Ceiling(imgModelList.Count / (double)cols);
                    var totalHeight = imgActualheight * rows;

                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            this.ImgModelCanvas.Height = totalHeight;
                        }, DispatcherPriority.Background, token);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }



                    //获取第一行
                    var firstRow = (int)(verticalOffset / imgActualheight);

                    //获取最行一行
                    var lastRow = (int)((verticalOffset + viewHeight) / imgActualheight) + 1;

                    //这样我们就获取到当前视窗一行显示多个图片
                    var dataList = imgModelList.Skip((firstRow - 1) * cols).Take((lastRow - firstRow + 1) * cols).ToList();

                    for (int i = 0; i < dataList.Count; i++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        var imgModel = dataList[i];
                        var imgModelIndex = imgModelList.IndexOf(imgModel);
                        //用数据的索引除以列数就获取到当前数据所在第一行
                        var row = imgModelIndex / cols;
                        //获取到当前图像所在列
                        var col = imgModelIndex % cols;
                        //如果已经存在我们只更新尺寸和位置
                        if (imgModel.BorderHost != null)
                        {

                            try
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    imgModel.BorderHost.Width = this.ViewModel.ImgWidth;
                                    imgModel.BorderHost.Height = this.ViewModel.ImgHeight;
                                    Canvas.SetLeft(imgModel.BorderHost, col * imgActualWidth);
                                    Canvas.SetTop(imgModel.BorderHost, row * imgActualheight);
                                    if (!this.ImgModelCanvas.Children.Contains(imgModel.BorderHost))
                                    {
                                        this.ImgModelCanvas.Children.Add(imgModel.BorderHost);
                                    }
                                }, DispatcherPriority.Render, token);
                            }
                            catch (TaskCanceledException)
                            {
                                return;
                            }
                            continue;
                        }

                        try
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                Border parentBorder = new Border();
                                this.ImgModelCanvas.Children.Add(parentBorder);

                                //如果不存在我们则添加
                                parentBorder.Name = GetBorderName(imgModel.Id);
                                parentBorder.Width = this.ViewModel.ImgWidth;
                                parentBorder.Height = this.ViewModel.ImgHeight;
                                parentBorder.Margin = new Thickness(this.ImgBorderMarginLeft, this.ImgBorderMarginTop, this.ImgBorderMarginRight, this.ImgBorderMarginBottom);
                                parentBorder.BorderThickness = new Thickness(1);
                                parentBorder.CornerRadius = new CornerRadius(5);
                                parentBorder.FocusVisualStyle = null;
                                Canvas.SetLeft(parentBorder, col * imgActualWidth);
                                Canvas.SetTop(parentBorder, row * imgActualheight);

                                TextBlock textBlock = new TextBlock();
                                parentBorder.Child = textBlock;
                                textBlock.Text = imgModel.ImgName;
                                textBlock.VerticalAlignment = VerticalAlignment.Top;
                                textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                                textBlock.FontSize = 14;
                                textBlock.Focusable = false;
                                textBlock.Padding = new Thickness(3, 15, 3, 3);
                                textBlock.TextTrimming = TextTrimming.CharacterEllipsis;

                                imgModel.BorderHost = parentBorder;
                            }, DispatcherPriority.Render, token);
                        }
                        catch (TaskCanceledException)
                        {
                            return;
                        }
                    }


                    if (!isScrollDown)
                    {
                        dataList = dataList.OrderByDescending(t => t.Id).ToList();
                    }

                    LoadImgSourceAsync(dataList, token);
                }, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

        }



        private void LoadImgSourceAsync(List<ImgModel> dataList, CancellationToken token)
        {
            //获取Canvas上所有的占位符
            List<Border> borderList = new List<Border>();
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    borderList = this.ImgModelCanvas.Children.Cast<Border>().ToList();
                }, DispatcherPriority.Background, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }


            if (borderList.Count == 0)
            {
                return;
            }
            ContextMenu rsImgContextMenu = null;
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    rsImgContextMenu = (ContextMenu)this.FindResource("RSImgContextMenuTemplate");
                }, DispatcherPriority.Background, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }


            ParallelOptions parallelOptions = new ParallelOptions()
            {
                //这里一定要设置为1 设置多个反而速度变慢
                //因为磁盘的读写熟读他是固定的 你就是多线程去度 没意义 磁盘开销就那么大
                MaxDegreeOfParallelism = 1,
                CancellationToken = token,
            };

            try
            {
                Parallel.ForEach(dataList, parallelOptions, imgModel =>
                {
                    if (this.LoadImgSourceTaskCTS.IsCancellationRequested)
                    {
                        return;
                    }
                    if (imgModel.ThubnailImg == null)
                    {
                        try
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                try
                                {
                                    Stopwatch stopwatch = Stopwatch.StartNew();
                                    var imgMat = Cv2.ImRead(imgModel.ImgPath);
                                    Cv2.Resize(imgMat, imgMat, new OpenCvSharp.Size(200, 160));
                                    var thubnailImg = imgMat.ToBitmapSource();
                                    imgModel.ThubnailImg = thubnailImg;
                                    imgModel.IsCanRead = true;
                                    Debug.WriteLine($"测试1结果：{stopwatch.ElapsedMilliseconds}");

                                    //    var stopwatch = Stopwatch.StartNew();
                                    //    var thubnailImg = ImgHelper.GetBitmapSource(imgModel.ImgPath, 200);
                                    //    imgModel.ThubnailImg = thubnailImg;
                                    //    imgModel.IsCanRead = true;
                                    //    Debug.WriteLine($"测试2结果：{stopwatch.ElapsedMilliseconds}");
                                }
                                catch (Exception ex)
                                {
                                    imgModel.ThubnailImg = null;
                                    imgModel.IsCanRead = false;
                                    return;
                                }
                            }, DispatcherPriority.Background, token);

                        }
                        catch (TaskCanceledException)
                        {
                            return;
                        }
                    }
                    if (this.LoadImgSourceTaskCTS.IsCancellationRequested)
                    {
                        return;
                    }
                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            //找到对应的占位符
                            Border border = borderList.FirstOrDefault(t => t.Name == GetBorderName(imgModel.Id));
                            if (border == null)
                            {
                                return;
                            }
                            if (border.Child is RSImg rsImg)
                            {
                                return;
                            }

                            rsImg = new RSImg();
                            rsImg.MouseDoubleClick += RsImg_MouseDoubleClick;
                            rsImg.Click += RsImg_Click;
                            rsImg.PreviewMouseRightButtonDown += RsImg_PreviewMouseRightButtonDown;
                            rsImg.ContextMenu = rsImgContextMenu;
                            BindingOperations.ClearAllBindings(rsImg);


                            Binding imgModelBinding = new Binding();
                            imgModelBinding.Source = imgModel;
                            imgModelBinding.BindsDirectlyToSource = true;
                            rsImg.SetBinding(RSImg.ImgModelProperty, imgModelBinding);

                            Binding brightBinding = new Binding();
                            brightBinding.Source = this.ViewModel;
                            brightBinding.Path = new PropertyPath("Brightness");
                            rsImg.SetBinding(RSImg.BrightnessProperty, brightBinding);

                            Binding contrastBinding = new Binding();
                            contrastBinding.Source = this.ViewModel;
                            contrastBinding.Path = new PropertyPath("Contrast");
                            rsImg.SetBinding(RSImg.ContrastProperty, contrastBinding);
                            border.Child = rsImg;
                        }, DispatcherPriority.Background, token);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                });
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }



        private bool ThumbnailCallback()
        {
            return true;
        }

        private void RsImg_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            RsImg_Click(sender, e);
        }

        public ImgModel ImgModelSelectHistory;
        public ImgModel ShiftImgModelSelectFirst = null;
        public ImgModel ShiftImgModelSelectSecond = null;
        public bool IsImgModelSelectAll = false;
        public List<ImgModel> ShiftImgModelSelectHistoryList = new List<ImgModel>();
        public Border BorderSelect;
        private void RsImg_Click(object sender, RoutedEventArgs e)
        {
            var rsImg = sender as RSImg;
            var currentImgModelSelect = rsImg.ImgModel;

            bool isMultiSelect = false;

            //这里按下Ctrl 多选的逻辑
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                isMultiSelect = true;
                ShiftImgModelSelectFirst = null;
                ShiftImgModelSelectHistoryList.Clear();
                if (!currentImgModelSelect.IsSelect)
                {
                    this.ViewModel.ImgModelSelectList.Remove(currentImgModelSelect);
                    currentImgModelSelect = this.ViewModel.ImgModelSelectList.LastOrDefault();
                }
            }
            //这里是进行批量选择的逻辑
            else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                isMultiSelect = true;
                if (ShiftImgModelSelectFirst == null)
                {
                    ShiftImgModelSelectFirst = ImgModelSelectHistory;
                }

                if (ShiftImgModelSelectFirst == null)
                {
                    ShiftImgModelSelectFirst = currentImgModelSelect;
                }
                else
                {
                    //清除上一次shift选中
                    foreach (var imgModel in ShiftImgModelSelectHistoryList)
                    {
                        imgModel.IsSelect = false;
                        if (this.ViewModel.ImgModelSelectList.Contains(imgModel))
                        {
                            this.ViewModel.ImgModelSelectList.Remove(imgModel);
                        }
                    }

                    ShiftImgModelSelectHistoryList.Clear();

                    var shiftImgModelSelectFirstId = ShiftImgModelSelectFirst.Id;
                    var currentImgModelSelectId = currentImgModelSelect.Id;

                    var minId = Math.Min(shiftImgModelSelectFirstId, currentImgModelSelectId);
                    var maxId = Math.Max(shiftImgModelSelectFirstId, currentImgModelSelectId);

                    //这里只管第一个选择和最后一个选择之间的 因为最后一个会自动添加
                    var addImgModelList = ProjectModelSelect.ImgModelList.Where(t => t.Id >= minId && t.Id <= maxId).ToList();

                    foreach (var imgModel in addImgModelList)
                    {
                        imgModel.IsSelect = true;
                        if (!this.ViewModel.ImgModelSelectList.Contains(imgModel))
                        {
                            this.ViewModel.ImgModelSelectList.Add(imgModel);
                            ShiftImgModelSelectHistoryList.Add(imgModel);
                        }
                    }
                }
            }
            //单选 逻辑
            else
            {
                this.ShiftImgModelSelectFirst = null;
                this.ShiftImgModelSelectHistoryList.Clear();
                foreach (var imgModel in this.ViewModel.ImgModelSelectList)
                {
                    imgModel.IsSelect = false;
                }

                this.ViewModel.ImgModelSelectList.Clear();
                currentImgModelSelect.IsSelect = true;
            }


            SetDefaultImgModelSelect(currentImgModelSelect, isMultiSelect);


            e.Handled = true;
        }

        private void SetDefaultImgModelSelect(ImgModel? defaultImgModelSelect, bool isMultiSelect)
        {
            ProjectModel projectModel = null;
            this.Dispatcher.Invoke(() =>
            {
                projectModel = ProjectModelSelect;
            });

            if (this.ViewModel.ImgModelSelect == null && defaultImgModelSelect == null)
            {
                if (this.ViewModel.ImgModelSelectList.Count > 0)
                {
                    defaultImgModelSelect = this.ViewModel.ImgModelSelectList.FirstOrDefault(t => t.IsWorking);
                    if (defaultImgModelSelect == null)
                    {
                        defaultImgModelSelect = this.ViewModel.ImgModelSelectList.FirstOrDefault(t => t.IsSelect);
                    }
                    if (defaultImgModelSelect == null)
                    {
                        defaultImgModelSelect = this.ViewModel.ImgModelSelectList.FirstOrDefault();
                    }
                }
                else
                {
                    defaultImgModelSelect = projectModel.ImgModelList.FirstOrDefault(t => t.IsWorking);
                    if (defaultImgModelSelect == null)
                    {
                        defaultImgModelSelect = projectModel.ImgModelList.FirstOrDefault(t => t.IsSelect);
                    }
                    if (defaultImgModelSelect == null)
                    {
                        defaultImgModelSelect = projectModel.ImgModelList.FirstOrDefault();
                    }
                }
            }

            if (defaultImgModelSelect == null)
            {
                return;
            }

            //如果不是多选
            if (!isMultiSelect)
            {
                //清除历史选择
                if (this.ViewModel.ImgModelSelect != null)
                {
                    this.ViewModel.ImgModelSelect.IsSelect = false;
                    this.ViewModel.ImgModelSelect.IsWorking = false;
                    if (this.ViewModel.ImgModelSelectList.Contains(this.ViewModel.ImgModelSelect))
                    {
                        this.ViewModel.ImgModelSelectList.Remove(this.ViewModel.ImgModelSelect);
                    }
                }
            }
            else
            {
                //清除历史选择
                if (this.ViewModel.ImgModelSelect != null)
                {
                    this.ViewModel.ImgModelSelect.IsWorking = false;
                }
            }


            this.ViewModel.ImgModelSelect = defaultImgModelSelect;
            this.ViewModel.ImgModelSelect.IsSelect = true;
            this.ViewModel.ImgModelSelect.IsWorking = true;
            if (!this.ViewModel.ImgModelSelectList.Contains(this.ViewModel.ImgModelSelect))
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.ViewModel.ImgModelSelectList.Add(this.ViewModel.ImgModelSelect);
                });
            }


            //获取图像尺寸
            try
            {

              var image= ImgHelper.GetBitmapSource(this.ViewModel.ImgModelSelect.ImgPath);

                this.ViewModel.ImgModelSelect.Width = image.PixelWidth;
                this.ViewModel.ImgModelSelect.Height = image.PixelHeight;
            }
            catch (Exception)
            {

            }

            //设置历史选中
            this.ImgModelSelectHistory = this.ViewModel.ImgModelSelect;

            //设置全选状态
            if (this.ViewModel.ImgModelSelectList.Count == projectModel.ImgModelList.Count)
            {
                this.IsImgModelSelectAll = true;
            }
            else
            {
                this.IsImgModelSelectAll = false;
            }

            List<Border> borderList = new List<Border>();
            this.Dispatcher.Invoke(() =>
            {
                borderList = this.ImgModelCanvas.Children.Cast<Border>().ToList();
                BorderSelect = borderList.FirstOrDefault(t => t.Name == GetBorderName(this.ViewModel.ImgModelSelect.Id));
            });
        }

        private void RsImg_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.OpenAnnotationView(this.ViewModel.ImgModelSelect);
        }

        private string GetBorderName(long id)
        {
            return $"Border{id}";
        }


        /// <summary>
        /// 添加图像
        /// </summary>
        private async void BtnAddImg_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "支持格式(*.jpg,*.png,*.jpeg,*.bmp,*.tif,*.gif)|*.jpg;*.png;*.jpeg;*.bmp;*.tif;*.gif|JPG (*.jpg)|*.jpg|PNG (*.png)|*.png|JPEG (*.jpeg)|*.jpeg|BMP (*.bmp)|*.bmp|TIF (*.tif)|*.tif|GIF (*.gif)|*.gif";
            openFileDialog.Title = "选择图像";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                var fileNames = openFileDialog.FileNames;
                var validLoginResult = await this.HomeView.InvokeLoadingActionAsync(async (cancellationToken) =>
                {
                    LoadImgModelFromDisk(fileNames.ToList());
                    return OperateResult.CreateSuccessResult();
                });
            }

        }

        /// <summary>
        /// 从文件夹选择图像
        /// </summary>
        private async void BtnAddImgFolder_Click(object sender, RoutedEventArgs e)
        {
            if (this.ProjectModelSelect == null)
            {
                return;
            }

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
                var validLoginResult = await this.HomeView.InvokeLoadingActionAsync(async (cancellationToken) =>
                {
                    string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".tif" };
                    var fileNames = imageExtensions.SelectMany(ext => Directory.EnumerateFiles(selectFolder,
                        $"*{ext}", SearchOption.AllDirectories)).ToList();
                    LoadImgModelFromDisk(fileNames);
                    return OperateResult.CreateSuccessResult();
                });
            }
        }

        private void LoadImgModelFromDisk(List<string> fileNames)
        {
            this.InitImgModelList(fileNames);
            this.RefreshImgSource(true, false);
            this.SetDefaultImgModelSelect(null, false);
        }

        private List<ImgModel> InitImgModelList(List<string> fileNames)
        {

            List<ImgModel> imgModelList = new List<ImgModel>();
            ProjectModel projectModelSelect = null;
            this.Dispatcher.Invoke(() =>
            {
                projectModelSelect = this.ProjectModelSelect; 
                imgModelList = projectModelSelect.ImgModelList.ToList();
            });
            for (int i = 0; i < fileNames.Count; i++)
            {
                var fileName = fileNames[i];
                var imgModel = new ImgModel(IdGenerator.CreateId(), projectModelSelect.Id);
                imgModel.ImgName = Path.GetFileName(fileName);
                imgModel.ImgPath = fileName;
  

                this.Dispatcher.Invoke(() =>
                {
                    imgModelList.Add(imgModel);
                });
            }

            //图像去重
            imgModelList = imgModelList.DistinctBy(t => t.ImgPath).ToList();

            //把这些文件重新赋值给项目
            this.Dispatcher.Invoke(() =>
            {
                this.ProjectModelSelect.ImgModelList = new ObservableCollection<ImgModel>(imgModelList);
            });
            return imgModelList;
        }

        private void BtnGo2AnnotationView_Click(object sender, RoutedEventArgs e)
        {
            this.OpenAnnotationView(this.ViewModel.ImgModelSelect);
        }

        private void OpenAnnotationView(ImgModel imgModel)
        {
            if (imgModel == null || imgModel.IsCanRead == false)
            {
                return;
            }
            this.HomeView.ActivateAnnotaionView();
        }

        bool IsScrollDown;

        public DateTime? LstScrollTime = null;
        public const int ScollThreshold = 33;//毫秒
        public bool IsImgModelScrollViewer_ScrollChanged = false;
        private void ImgModelScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            IsScrollDown = e.VerticalChange >= 0 ? true : false;

            //IsImgModelScrollViewer_ScrollChanged = true;
            //DateTime currentTime = DateTime.Now;
            //if (LstScrollTime.HasValue)
            //{
            //    TimeSpan timeSpan = currentTime - LstScrollTime.Value;
            //    if (timeSpan.TotalMilliseconds > ScollThreshold)
            //    {
            //        this.RefreshImgSource(IsScrollDown);
            //    }
            //}
            //else
            //{
            //    this.RefreshImgSource(IsScrollDown);
            //}

            //LstScrollTime = currentTime;

            this.RefreshImgSource(IsScrollDown, true, true);
        }



        private void ImgModelScrollViewer_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (IsImgModelScrollViewer_ScrollChanged)
            //{
            //    this.IsImgModelScrollViewer_ScrollChanged = false;
            //    this.RefreshImgSource(IsScrollDown);
            //}
        }

        public void UpdateImgZoom(int zoomLevel = 5, CancellationToken token = default)
        {
            this.ViewModel.ZoomLevel = zoomLevel;
            double actualWidth = 0;
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    actualWidth = this.ImgModelHost.ActualWidth;
                }, DispatcherPriority.Background, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            var imgwidthShould = (actualWidth - zoomLevel * (this.ImgBorderMarginLeft + this.ImgBorderMarginRight)) / zoomLevel;

            //设置图像显示比例
            var HWRatio = 6 / 7D;
            this.ViewModel.ImgWidth = imgwidthShould;
            this.ViewModel.ImgHeight = imgwidthShould * HWRatio;
        }

        private void ImgModelScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                int zoomLevel = e.Delta > 0 ? this.ViewModel.ZoomLevel + 1 : this.ViewModel.ZoomLevel - 1;
                zoomLevel = Math.Max(2, zoomLevel);
                zoomLevel = Math.Min(10, zoomLevel);
                if (zoomLevel == this.ViewModel.ZoomLevel)
                {
                    return;
                }
                UpdateImgZoom(zoomLevel);
                this.RefreshImgSource(IsScrollDown, true);
            }
            e.Handled = false;
        }



        private void BtnOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            var imgModelSelect = this.ViewModel.ImgModelSelect;
            if (File.Exists(imgModelSelect.ImgPath))
            {
                FileHelper.ExplorerFile(imgModelSelect.ImgPath);
            }
        }

        public void OpenFileLocation()
        {
            if (this.ViewModel.ImgModelSelect == null)
            {
                return;
            }
        }

        private void BtnDeleteImgSelect_Click(object sender, RoutedEventArgs e)
        {
            DeleteImgSelect();
        }

        public void DeleteImgSelect()
        {
            string confirmDes = null;
            if (this.ViewModel.ImgModelSelectList.Count > 1)
            {
                confirmDes = $"你确定要删除这{this.ViewModel.ImgModelSelectList.Count}个图像吗？";
            }
            RemoveImgView removeImgView = new RemoveImgView(this, confirmDes);
            removeImgView.OnReveImgCallBack += RemoveImgView_OnReveImgCallBack;
            this.HomeView.ShowModal(removeImgView);
        }

        private void RemoveImgView_OnReveImgCallBack(bool obj)
        {

        }

        private void BtnSaveImgAs_Click(object sender, RoutedEventArgs e)
        {

        }



        /// <summary>
        /// 图像Canvas键盘事件 实现键盘方向键 WASD 进行快速图像选择
        /// </summary>
        private void ImgModelCanvas_KeyDown(object sender, KeyEventArgs e)
        {

            //获取用户按键类型
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            this.ViewModel.Log = $"{key}";
            switch (key)
            {
                case Key.None:
                    break;
                case Key.Cancel:
                    break;
                case Key.Back:
                    break;
                case Key.Tab:
                    break;
                case Key.LineFeed:
                    break;
                case Key.Clear:
                    break;
                case Key.Enter:
                    break;
                //case Key.Return:
                //    break;
                case Key.Pause:
                    break;
                case Key.Capital:
                    break;
                //case Key.CapsLock:
                //    break;
                case Key.HangulMode:
                    break;
                //case Key.KanaMode:
                //    break;
                case Key.JunjaMode:
                    break;
                case Key.FinalMode:
                    break;
                case Key.HanjaMode:
                    break;
                //case Key.KanjiMode:
                //    break;
                case Key.Escape:
                    break;
                case Key.ImeConvert:
                    break;
                case Key.ImeNonConvert:
                    break;
                case Key.ImeAccept:
                    break;
                case Key.ImeModeChange:
                    break;
                case Key.Space:
                    break;
                case Key.PageUp:
                    break;
                //case Key.Prior:
                //    break;
                //case Key.Next:
                //    break;
                case Key.PageDown:
                    break;
                case Key.End:
                    break;
                case Key.Home:
                    break;
                #region 键盘方向键
                case Key.Left:
                    SetImgModelSelectWithArrowKey(key);
                    break;
                case Key.Up:
                    SetImgModelSelectWithArrowKey(key);
                    break;
                case Key.Right:
                    SetImgModelSelectWithArrowKey(key);
                    break;
                case Key.Down:
                    SetImgModelSelectWithArrowKey(key);
                    break;
                #endregion
                case Key.Select:
                    break;
                case Key.Print:
                    break;
                case Key.Execute:
                    break;
                case Key.PrintScreen:
                    break;
                //case Key.Snapshot:
                //    break;
                case Key.Insert:
                    break;
                case Key.Delete:
                    break;
                case Key.Help:
                    break;
                case Key.D0:
                    break;
                case Key.D1:
                    break;
                case Key.D2:
                    break;
                case Key.D3:
                    break;
                case Key.D4:
                    break;
                case Key.D5:
                    break;
                case Key.D6:
                    break;
                case Key.D7:
                    break;
                case Key.D8:
                    break;
                case Key.D9:
                    break;
                case Key.A:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        this.SelectAllEvent();
                    }
                    else
                    {
                        SetImgModelSelectWithArrowKey(Key.Left);
                    }
                    break;
                case Key.B:
                    break;
                case Key.C:
                    break;
                case Key.D:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        this.DeleteImgSelect();
                    }
                    else
                    {
                        SetImgModelSelectWithArrowKey(Key.Right);
                    }
                    break;
                case Key.E:
                    break;
                case Key.F:
                    break;
                case Key.G:
                    break;
                case Key.H:
                    break;
                case Key.I:
                    break;
                case Key.J:
                    break;
                case Key.K:
                    break;
                case Key.L:
                    break;
                case Key.M:
                    break;
                case Key.N:
                    break;
                case Key.O:
                    break;
                case Key.P:
                    break;
                case Key.Q:
                    break;
                case Key.R:
                    break;
                case Key.S:
                    SetImgModelSelectWithArrowKey(Key.Down);
                    break;
                case Key.T:
                    break;
                case Key.U:
                    break;
                case Key.V:
                    break;
                case Key.W:
                    SetImgModelSelectWithArrowKey(Key.Up);
                    break;
                case Key.X:
                    break;
                case Key.Y:
                    break;
                case Key.Z:
                    break;
                case Key.LWin:
                    break;
                case Key.RWin:
                    break;
                case Key.Apps:
                    break;
                case Key.Sleep:
                    break;
                case Key.NumPad0:
                    break;
                case Key.NumPad1:
                    break;
                case Key.NumPad2:
                    break;
                case Key.NumPad3:
                    break;
                case Key.NumPad4:
                    break;
                case Key.NumPad5:
                    break;
                case Key.NumPad6:
                    break;
                case Key.NumPad7:
                    break;
                case Key.NumPad8:
                    break;
                case Key.NumPad9:
                    break;
                case Key.Multiply:
                    break;
                case Key.Add:
                    break;
                case Key.Separator:
                    break;
                case Key.Subtract:
                    break;
                case Key.Decimal:
                    break;
                case Key.Divide:
                    break;
                case Key.F1:
                    break;
                case Key.F2:
                    break;
                case Key.F3:
                    break;
                case Key.F4:
                    break;
                case Key.F5:
                    break;
                case Key.F6:
                    break;
                case Key.F7:
                    break;
                case Key.F8:
                    break;
                case Key.F9:
                    break;
                case Key.F10:
                    break;
                case Key.F11:
                    break;
                case Key.F12:
                    break;
                case Key.F13:
                    break;
                case Key.F14:
                    break;
                case Key.F15:
                    break;
                case Key.F16:
                    break;
                case Key.F17:
                    break;
                case Key.F18:
                    break;
                case Key.F19:
                    break;
                case Key.F20:
                    break;
                case Key.F21:
                    break;
                case Key.F22:
                    break;
                case Key.F23:
                    break;
                case Key.F24:
                    break;
                case Key.NumLock:
                    break;
                case Key.Scroll:
                    break;
                case Key.LeftShift:
                    break;
                case Key.RightShift:
                    break;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    if (Keyboard.IsKeyDown(Key.A))
                    {
                        this.SelectAllEvent();
                    }
                    else if (Keyboard.IsKeyDown(Key.D))
                    {
                        this.DeleteImgSelect();
                    }
                    break;
                case Key.LeftAlt:
                    break;
                case Key.RightAlt:
                    break;
                case Key.BrowserBack:
                    break;
                case Key.BrowserForward:
                    break;
                case Key.BrowserRefresh:
                    break;
                case Key.BrowserStop:
                    break;
                case Key.BrowserSearch:
                    break;
                case Key.BrowserFavorites:
                    break;
                case Key.BrowserHome:
                    break;
                case Key.VolumeMute:
                    break;
                case Key.VolumeDown:
                    break;
                case Key.VolumeUp:
                    break;
                case Key.MediaNextTrack:
                    break;
                case Key.MediaPreviousTrack:
                    break;
                case Key.MediaStop:
                    break;
                case Key.MediaPlayPause:
                    break;
                case Key.LaunchMail:
                    break;
                case Key.SelectMedia:
                    break;
                case Key.LaunchApplication1:
                    break;
                case Key.LaunchApplication2:
                    break;
                case Key.Oem1:
                    break;
                //case Key.OemSemicolon:
                //    break;
                case Key.OemPlus:
                    break;
                case Key.OemComma:
                    break;
                case Key.OemMinus:
                    break;
                case Key.OemPeriod:
                    break;
                case Key.Oem2:
                    break;
                //case Key.OemQuestion:
                //    break;
                case Key.Oem3:
                    break;
                //case Key.OemTilde:
                //    break;
                case Key.AbntC1:
                    break;
                case Key.AbntC2:
                    break;
                case Key.Oem4:
                    break;
                //case Key.OemOpenBrackets:
                //    break;
                case Key.Oem5:
                    break;
                //case Key.OemPipe:
                //    break;
                case Key.Oem6:
                    break;
                //case Key.OemCloseBrackets:
                //    break;
                case Key.Oem7:
                    break;
                //case Key.OemQuotes:
                //    break;
                case Key.Oem8:
                    break;
                case Key.Oem102:
                    break;
                //case Key.OemBackslash:
                //    break;
                case Key.ImeProcessed:
                    break;
                case Key.System:
                    break;
                case Key.DbeAlphanumeric:
                    break;
                //case Key.OemAttn:
                //    break;
                case Key.DbeKatakana:
                    break;
                //case Key.OemFinish:
                //    break;
                case Key.DbeHiragana:
                    break;
                //case Key.OemCopy:
                //    break;
                case Key.DbeSbcsChar:
                    break;
                //case Key.OemAuto:
                //    break;
                case Key.DbeDbcsChar:
                    break;
                //case Key.OemEnlw:
                //    break;
                case Key.DbeRoman:
                    break;
                //case Key.OemBackTab:
                //    break;
                case Key.Attn:
                    break;
                //case Key.DbeNoRoman:
                //    break;
                case Key.CrSel:
                    break;
                //case Key.DbeEnterWordRegisterMode:
                //    break;
                case Key.DbeEnterImeConfigureMode:
                    break;
                //case Key.ExSel:
                //    break;
                case Key.DbeFlushString:
                    break;
                //case Key.EraseEof:
                //    break;
                case Key.DbeCodeInput:
                    break;
                //case Key.Play:
                //    break;
                case Key.DbeNoCodeInput:
                    break;
                //case Key.Zoom:
                //    break;
                case Key.DbeDetermineString:
                    break;
                //case Key.NoName:
                //    break;
                case Key.DbeEnterDialogConversionMode:
                    break;
                //case Key.Pa1:
                //    break;
                case Key.OemClear:
                    break;
                case Key.DeadCharProcessed:
                    break;
                default:
                    break;
            }

            e.Handled = true;
        }

        private void SelectAllEvent()
        {
            this.IsImgModelSelectAll = !this.IsImgModelSelectAll;
            this.ShiftImgModelSelectFirst = null;
            this.ImgModelSelectHistory = null;
            this.ShiftImgModelSelectHistoryList.Clear();

            var imgModelList = this.ProjectModelSelect.ImgModelList;

            if (IsImgModelSelectAll)
            {
                var exceptList = imgModelList.Except(this.ViewModel.ImgModelSelectList);
                foreach (var imgModel in exceptList)
                {
                    imgModel.IsSelect = true;
                }
                var concatList = this.ViewModel.ImgModelSelectList.Concat(exceptList);
                this.ViewModel.ImgModelSelectList = new ObservableCollection<ImgModel>(concatList);
            }
            else
            {
                foreach (var imgModel in this.ViewModel.ImgModelSelectList)
                {
                    imgModel.IsSelect = false;
                }

                this.ViewModel.ImgModelSelectList.Clear();

                if (this.ViewModel.ImgModelSelect != null)
                {
                    this.ViewModel.ImgModelSelect.IsSelect = false;
                }
            }

            this.SetDefaultImgModelSelect(null, true);

        }

        public void SetImgModelSelectWithArrowKey(Key key)
        {
            //获取用户所有选择项
            var imgModelList = this.ProjectModelSelect.ImgModelList;
            //获取用户当前选择项
            var imgModelSelect = this.ViewModel.ImgModelSelect;
            //获取用户当前选择项的索引
            var imgModelSelectIndex = imgModelList.IndexOf(imgModelSelect);

            //获取当前图像管理器视图的实际宽度和高度
            var hostActualWidth = this.ImgModelHost.ActualWidth;
            var hostActualHeight = this.ImgModelHost.ActualHeight;

            //获取当前图像管理器每个图像的实际宽度和高度
            var imgModelActualWidth = this.ViewModel.ImgWidth + this.ImgBorderMarginLeft + this.ImgBorderMarginRight;
            var imgModelActualHeight = this.ViewModel.ImgHeight + this.ImgBorderMarginTop + this.ImgBorderMarginBottom;

            //获取当前图像管理器视图每一行一共有多少列
            var cols = (int)(hostActualWidth / imgModelActualWidth);
            //获取当前图像管理器视图一共有多少行
            var rows = (int)(hostActualHeight / imgModelActualHeight);

            //用索引所以除以列数获取所在行
            var row = imgModelSelectIndex / cols;
            //用索引所以除以列数取余获取当前元素所在列索引
            var col = imgModelSelectIndex % cols;

            switch (key)
            {
                #region 键盘方向键
                case Key.Left:
                    col = col - 1;
                    if (col < 0)
                    {
                        col = cols - 1;
                        row = row - 1;
                        if (row < 0)
                        {
                            col = 0;
                            row = 0;
                        }
                    }
                    break;
                case Key.Up:
                    row = row - 1;
                    break;
                case Key.Right:
                    col = col + 1;
                    if (col >= cols)
                    {
                        row = row + 1;
                        if (row >= rows)
                        {
                            row = rows - 1;
                            col = cols - 1;
                        }
                        else
                        {
                            col = 0;
                        }
                    }
                    break;
                case Key.Down:
                    row = row + 1;
                    break;
                    #endregion
            }

            //我们现在不就获取到新选择项所在的行列了吗
            if (row < 0)
            {
                row = 0;
            }
            if (row >= rows)
            {
                row = rows - 1;
            }

            //我们就获取到新选择项的索引
            imgModelSelectIndex = row * cols + col;

            if (imgModelSelectIndex < 0)
            {
                imgModelSelectIndex = 0;
            }

            if (imgModelSelectIndex >= imgModelList.Count)
            {
                imgModelSelectIndex = imgModelList.Count - 1;
            }

            //获取到最新选择图像
            var imgModelSelectIndexNew = this.ProjectModelSelect.ImgModelList[imgModelSelectIndex];

            this.SetDefaultImgModelSelect(imgModelSelectIndexNew, false);

            ScrollImgModelSelectIntoView();
        }

        private void ScrollImgModelSelectIntoView()
        {
            //重新获取图像尺寸 保证我们获取到的图像尺寸是最新的
            var imgModelActualWidth = this.ViewModel.ImgWidth + this.ImgBorderMarginLeft + this.ImgBorderMarginRight;
            var imgModelActualHeight = this.ViewModel.ImgHeight + this.ImgBorderMarginTop + this.ImgBorderMarginBottom;

            var imgModelList = this.ProjectModelSelect.ImgModelList;

            var imgModelSelect = this.ViewModel.ImgModelSelect;

            var imgModelSelectIndex = imgModelList.IndexOf(imgModelSelect);


            //获取当前图像管理器视图的实际宽度和高度
            var hostActualWidth = this.ImgModelHost.ActualWidth;
            var hostActualHeight = this.ImgModelHost.ActualHeight;

            //获取当前图像管理器视图每一行一共有多少列
            var cols = (int)(hostActualWidth / imgModelActualWidth);
            //获取当前图像管理器视图一共有多少行
            var rows = (int)(hostActualHeight / imgModelActualHeight);

            //用索引所以除以列数获取所在行
            var row = imgModelSelectIndex / cols;
            //用索引所以除以列数取余获取当前元素所在列索引
            var col = imgModelSelectIndex % cols;

            var verticalOffset = this.ImgModelScrollViewer.VerticalOffset;

            //获取到滚动条实际应该滚动到位置
            var verticalOffsetShould = row * imgModelActualHeight;

            if (!(verticalOffsetShould >= verticalOffset && verticalOffsetShould <= verticalOffset + this.ImgModelScrollViewer.ActualHeight - imgModelActualHeight))
            {
                this.ImgModelScrollViewer.ScrollToVerticalOffset(verticalOffsetShould);
            }

            this.ImgModelCanvas.Focus();
        }

        private void ImgModelCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            SetElementFocus(this.ImgModelCanvas);
        }

        public void SetElementFocus(FrameworkElement frameworkElement)
        {
            frameworkElement.Focus();
            InputMethod.SetPreferredImeState(frameworkElement, InputMethodState.Off);
            InputMethod.SetIsInputMethodEnabled(frameworkElement, false);
        }

        private void BtnCancelAllSelect_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in this.ViewModel.ImgModelSelectList)
            {
                item.IsSelect = false;
            }
            this.ViewModel.ImgModelSelectList.Clear();

            this.IsImgModelSelectAll = false;
            this.ImgModelSelectHistory = null;
            this.ShiftImgModelSelectFirst = null;
            this.ShiftImgModelSelectHistoryList.Clear();
        }


        public void RemoveImgModelSelect(bool isDeleteFileFromSystem)
        {
            //获取当前用户选择的图像
            var removeList = this.ViewModel.ImgModelSelectList.OrderBy(t => t.Id).ToList();
            if (removeList.Count == 0)
            {
                return;
            }

            var firstOrDefault = removeList.FirstOrDefault();
            var lastOrDefault = removeList.LastOrDefault();

            var firstOrDefaultIndex = this.ProjectModelSelect.ImgModelList.IndexOf(firstOrDefault) - 1;
            var lastOrDefaultIndex = this.ProjectModelSelect.ImgModelList.IndexOf(lastOrDefault) + 1;


            ImgModel defaultShoudSelect = null;
            if (lastOrDefaultIndex >= 0 && lastOrDefaultIndex < this.ProjectModelSelect.ImgModelList.Count)
            {
                defaultShoudSelect = this.ProjectModelSelect.ImgModelList[lastOrDefaultIndex];
            }
            if (defaultShoudSelect == null)
            {
                if (firstOrDefaultIndex >= 0 && firstOrDefaultIndex < this.ProjectModelSelect.ImgModelList.Count)
                {
                    defaultShoudSelect = this.ProjectModelSelect.ImgModelList[firstOrDefaultIndex];
                }
            }

            foreach (var imgModel in removeList)
            {
                imgModel.IsSelect = false;
                imgModel.IsWorking = false;
                ProjectModelSelect.ImgModelList.Remove(imgModel);
            }

            this.RefreshImgSource(true, true);

            //如果用户选择从系统删除这个图像
            if (isDeleteFileFromSystem)
            {
                foreach (var imgModel in removeList)
                {
                    if (File.Exists(imgModel.ImgPath))
                    {
                        try
                        {
                            File.Delete(imgModel.ImgPath);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }

            this.ViewModel.ImgModelSelectList.Clear();
            this.ViewModel.ImgModelSelect = null;
            this.SetDefaultImgModelSelect(defaultShoudSelect, false);
        }

        internal void UpdateTagSumModelList(ImgModel imgModel)
        {
            if (imgModel == null)
            {
                return;
            }

            var tagSumModelList = imgModel.RectModelList.GroupBy(t => t.TagModel)
                .Select(group => new TagSumModel()
                {
                    TagModel=group.Key,
                    Count=group.Count(),
                });
            imgModel.TagSumModelList = new ObservableCollection<TagSumModel>(tagSumModelList);
        }

       
    }
}
