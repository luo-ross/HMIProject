using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.Widgets.Controls;
using ScottPlot.Interactivity.UserActionResponses;
using ScottPlot.Plottables;
using ScottPlot;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Forms;

namespace RS.Annotation.Views.Charts
{
    /// <summary>
    /// SingnalChartView.xaml 的交互逻辑
    /// </summary>
    [ServiceInjectConfig(ServiceLifetime.Singleton)]
    public partial class SingnalChartView : RSWindow
    {
        public SingnalChartViewModel ViewModel { get; set; }
        public SingnalChartView()
        {
            InitializeComponent();
            this.ViewModel = this.DataContext as SingnalChartViewModel;
            this.Loaded += SingnalChartView_Loaded;
        }



        private void SingnalChartView_Loaded(object sender, RoutedEventArgs e)
        {
            this.InintScottPlot();

            //PlotStyle plotStyle = new PlotStyle()
            //{
            //    DataBackgroundColor = ScottPlot.Colors.Transparent,
            //    AxisColor = ScottPlot.Colors.Transparent,
            //    FigureBackgroundColor = ScottPlot.Colors.Transparent,
            //    //GridMajorLineColor = ScottPlot.Colors.Red,
            //};
            //this.PlotChart.Plot.SetStyle(plotStyle);
            //this.PlotChart.Plot.Grid.MajorLineColor = ScottPlot.Colors.Blue;

            //this.PlotChart.Plot.Grid.XAxisStyle.FillColor1 = Colors.Red;
            //this.PlotChart.Plot.Grid.XAxisStyle.FillColor2 = Colors.Blue;

            //这里是设置网格
            this.SingnalPlot.Grid.XAxisStyle.MajorLineStyle.Color = ScottPlot.Colors.Transparent;
            this.SingnalPlot.Grid.YAxisStyle.MajorLineStyle.Color = ScottPlot.Color.FromHtml("#e0e6f1");


            ////this.PlotChart.Plot.Axes 是控制4个边框的
            //this.SingnalPlot.Axes.Right.FrameLineStyle.Width = 0;
            //this.SingnalPlot.Axes.Top.FrameLineStyle.Width = 0;
            //this.SingnalPlot.Axes.Left.FrameLineStyle.Width = 0;
            //this.SingnalPlot.Axes.Bottom.FrameLineStyle.Width = 1;
            //this.SingnalPlot.Axes.Bottom.FrameLineStyle.Color = ScottPlot.Color.FromHtml("#6e7079");

            //this.SingnalPlot.Grid.XAxis 是控制X轴的
            //主刻度
            this.SingnalPlot.Grid.XAxis.MajorTickStyle.Color = ScottPlot.Color.FromHtml("#6E7079");
            this.SingnalPlot.Grid.XAxis.RegenerateTicks(5);
            //次刻度
            this.SingnalPlot.Grid.XAxis.MinorTickStyle.Color = ScottPlot.Color.FromHtml("#6E7079");

            //this.PlotChart.Plot.Grid.YAxis 是控制Y轴的
            //主刻度
            this.SingnalPlot.Grid.YAxis.MajorTickStyle.Color = ScottPlot.Color.FromHtml("#e0e6f1");
            //次刻度
            this.SingnalPlot.Grid.YAxis.MinorTickStyle.Color = ScottPlot.Colors.Transparent;

            //这里控制轴Label样式
            this.SingnalPlot.Grid.YAxis.TickLabelStyle.ForeColor = ScottPlot.Color.FromHtml("#6E7079");
            this.SingnalPlot.Grid.XAxis.TickLabelStyle.ForeColor = ScottPlot.Color.FromHtml("#6E7079");


            this.SingnalPlot.Axes.Title.Label.Text = "Plot Title";
            this.SingnalPlot.Axes.Title.Label.ForeColor = ScottPlot.Color.FromHtml("#464646");
            this.SingnalPlot.Axes.Title.Label.Bold = false;
            //this.SingnalPlot.Axes.Title.Label.ForeColor = Colors.RebeccaPurple;
            //this.SingnalPlot.Axes.Title.Label.FontSize = 32;
            //this.SingnalPlot.Axes.Title.Label.FontName = Fonts.Serif;
            //this.SingnalPlot.Axes.Title.Label.Rotation = -5;
            //this.SingnalPlot.Axes.Title.Label.Bold = false;

            this.SingnalPlot.Axes.Left.Label.Text = "Vertical Axis";
            this.SingnalPlot.Axes.Left.Label.ForeColor = ScottPlot.Color.FromHtml("#464646");
            this.SingnalPlot.Axes.Left.Label.Bold = false;
            //this.SingnalPlot.Axes.Left.Label.ForeColor = Colors.Magenta;
            //this.SingnalPlot.Axes.Left.Label.Italic = true;

            this.SingnalPlot.Axes.Bottom.Label.Text = "Horizontal Axis";
            this.SingnalPlot.Axes.Bottom.Label.ForeColor = ScottPlot.Color.FromHtml("#464646");
            this.SingnalPlot.Axes.Bottom.Label.Bold = false;


            //this.SingnalPlot.Axes.Bottom.Label.Bold = false;
            //this.SingnalPlot.Axes.Bottom.Label.FontName = Fonts.Monospace;

            //this.SingnalPlot.Axes.Bottom.MajorTickStyle.Length = 10;
            //this.SingnalPlot.Axes.Bottom.MajorTickStyle.Width = 3;
            //this.SingnalPlot.Axes.Bottom.MajorTickStyle.Color = Colors.Magenta;
            //this.SingnalPlot.Axes.Bottom.MinorTickStyle.Length = 5;
            //this.SingnalPlot.Axes.Bottom.MinorTickStyle.Width = 0.5f;
            //this.SingnalPlot.Axes.Bottom.MinorTickStyle.Color = Colors.Green;
            //this.SingnalPlot.Axes.Bottom.FrameLineStyle.Color = Colors.Blue;
            //this.SingnalPlot.Axes.Bottom.FrameLineStyle.Width = 3;
            //this.SingnalPlot.Axes.Right.FrameLineStyle.Width = 0;



            // create a custom tick generator using your custom label formatter
            ScottPlot.TickGenerators.NumericAutomatic myTickGenerator = new ScottPlot.TickGenerators.NumericAutomatic()
            {
                LabelFormatter = CustomFormatter
            };

            // tell an axis to use the custom tick generator
            this.SingnalPlot.Axes.Bottom.TickGenerator = myTickGenerator;



            MyCrosshair = this.SingnalPlot.Add.Crosshair(0, 0);
            MyCrosshair.TextColor = ScottPlot.Colors.White;
            MyCrosshair.FontBold = false;
            MyCrosshair.FontSize = 11;
            MyCrosshair.TextBackgroundColor = ScottPlot.Color.FromHtml("#6E7079");
            MyCrosshair.LineColor = ScottPlot.Color.FromHtml("#d1d2d3");

            PlotChart.MouseMove += (obj, e) =>
            {
                var position = e.GetPosition(this.PlotChart);
                Pixel mousePixel = new Pixel(position.X, position.Y);
                Coordinates mouseLocation = PlotChart.Plot.GetCoordinates(mousePixel);
                //this.Text = $"X={mouseLocation.X:N8}, Y={mouseLocation.Y:N8}";
                MyCrosshair.Position = mouseLocation;
                MyCrosshair.VerticalLine.Text = $"{mouseLocation.X:N3}";
                MyCrosshair.HorizontalLine.Text = $"{mouseLocation.Y:N3}";

                DataPoint nearest = SignalXY.Data.GetNearest(mouseLocation, PlotChart.Plot.LastRender);
                // place the crosshair over the highlighted point
                if (nearest.IsReal)
                {
                    MyCrosshair.MarkerShape = MarkerShape.OpenCircle;
                    MyCrosshair.MarkerSize = 15;
                    MyCrosshair.MarkerLineColor = ScottPlot.Color.FromHtml("#FF0000").WithAlpha(0.5);
                    MyCrosshair.IsVisible = true;
                    MyCrosshair.Position = nearest.Coordinates;
                    //Text = $"Selected Index={nearest.Index}, X={nearest.X:0.##}, Y={nearest.Y:0.##}";
                }
                else
                {
                    MyCrosshair.MarkerShape = MarkerShape.None;
                }
                PlotChart.Refresh();
            };
            //关闭Rendered in 提示
            PlotChart.UserInputProcessor.DoubleLeftClickBenchmark(false);

            var userActionResponses = PlotChart.UserInputProcessor.UserActionResponses.ToList();
            foreach (var item in userActionResponses)
            {
                if (item is MouseWheelZoom)
                {
                    PlotChart.UserInputProcessor.UserActionResponses.Remove(item);
                }
            }
            PlotChart.MouseWheel += PlotChart_MouseWheel;

            ManualResetEvent = new ManualResetEvent(false);
            PlotChart.MouseDoubleClick += PlotChart_MouseDoubleClick;


            PlotChart.UserInputProcessor.LeftClickDragPan(enable: false, horizontal: false, vertical: false);
            PlotChart.UserInputProcessor.RightClickDragZoom(enable: false, horizontal: false, vertical: false);


            this.PlotChart.Refresh();
            TaskActionAsyn();
            GenerateData();
        }

      

        public Plot SingnalPlot { get; set; }

        private void InintScottPlot()
        {
            this.SingnalPlot = this.PlotChart.Plot;
            this.SingnalPlot.Add.Signal(ScottPlot.Generate.Sin());
            this.SingnalPlot.Add.Signal(ScottPlot.Generate.Cos());
        }


   
        public SignalXY SignalXY;
        private ScottPlot.Plottables.Crosshair MyCrosshair;

        public double ZoomLevel = 1;



        public Pixel MouseWheelPixel { get; set; }


        private void PlotChart_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                var newZoomeLevel = this.ZoomLevel + 0.01;
                if (newZoomeLevel > 1.15)
                {
                    return;
                }
                this.ZoomLevel = newZoomeLevel;
            }
            else
            {
                var newZoomeLevel = this.ZoomLevel - 0.01;
                if (newZoomeLevel < 0.1)
                {
                    return;
                }
                this.ZoomLevel = newZoomeLevel;
            }
            var position = e.GetPosition(this.PlotChart);
            MouseWheelPixel = new Pixel(position.X, position.Y);

        }

        private void PlotChart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsRun)
            {
                ManualResetEvent.Reset();
            }
            else
            {
                ManualResetEvent.Set();
            }
            IsRun = !IsRun;
        }



        public bool IsRun { get; set; } = true;
        private void PlotChart_Click(object sender, EventArgs e)
        {

        }



        private string CustomFormatter(double arg)
        {
            var dt = DateTime.FromOADate(arg);
            return $"{dt.ToString($"yyyy-MM-dd")}\n{dt.ToString("HH:mm:ss")}";
        }



        public List<double> TimeList = new List<double>();


        public List<double> DataSource = new List<double>();

        public (List<double> timeList, List<double> dataSource) GetData()
        {
            lock (dataLock)
            {
                return (TimeList.ToList(), DataSource.ToList());
            }
        }

        public static object dataLock = new object();
        private void GenerateData()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    if (DataSource.Count > 100)
                    {
                        lock (dataLock)
                        {
                            DataSource.RemoveAt(0);
                            TimeList.RemoveAt(0);
                        }
                    }
                    lock (dataLock)
                    {
                        DataSource.Add(new Random(Guid.NewGuid().GetHashCode()).Next(-10000, 10000));
                        TimeList.Add(DateTime.Now.ToOADate());
                    }
                    await Task.Delay(1);
                }
            });
        }


        //public List<double> xList = new List<double>();
        //public List<double> yList = new List<double>();

        public ManualResetEvent ManualResetEvent;
        public double Xmin { get; set; }
        public double Xmax { get; set; }
        public double Ymin { get; set; }
        public double Ymax { get; set; }
        private void TaskActionAsyn()
        {
            ManualResetEvent.Set();
            //xList = Generate.Range(1, 50, 1).ToList();
            //yList = Generate.NoisyExponential(50).ToList();
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    ManualResetEvent.WaitOne();

                    (List<double> timeList, List<double> dataSource) = GetData();
                    if (timeList.Count == 0)
                    {
                        await Task.Delay(33);
                        continue;
                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        var listCopy = this.SingnalPlot.PlottableList.ToList();
                        foreach (var item in listCopy)
                        {
                            if (!(item is Crosshair))
                            {
                                this.SingnalPlot.PlottableList.Remove(item);
                            }
                        }
                        //this.SingnalPlot.Clear();

                        var firstDate = timeList.First();
                        var lastDate = timeList.Last();
                        var lastDateNew = DateTime.FromOADate(lastDate).AddSeconds(0.2).ToOADate();

                        //在这里通过计算我们的范围来设置边界值

                        var dataSourceMin = dataSource.Min();
                        Xmin = firstDate;
                        Xmax = lastDateNew;
                        Ymin = dataSource.Min() - 4000;
                        Ymax = dataSource.Max() + 4000;


                        if (MouseWheelPixel != default)
                        {
                            var mouseWheelCoordinates = this.SingnalPlot.GetCoordinates(MouseWheelPixel);
                            var mouseX = mouseWheelCoordinates.X;
                            var mouseY = mouseWheelCoordinates.Y;

                            // 计算新的视窗范围
                            double newXMin = mouseX - (mouseX - Xmin) * this.ZoomLevel;
                            double newXMax = mouseX + (Xmax - mouseX) * this.ZoomLevel;
                            double newYMin = mouseY - (mouseY - Ymin) * this.ZoomLevel;
                            double newYMax = mouseY + (Ymax - mouseY) * this.ZoomLevel;

                            // 设置新的坐标轴范围
                            this.SingnalPlot.Axes.SetLimits(newXMin, newXMax, newYMin, newYMax);
                        }
                        else
                        {
                            this.SingnalPlot.Axes.SetLimits(Xmin, Xmax, Ymin, Ymax);
                        }




                        // apply our custom tick formatter
                        List<double> doubles = new List<double>();

                        var distance = lastDate - firstDate;
                        doubles.Add(firstDate);
                        doubles.Add(firstDate + distance * 0.2);
                        doubles.Add(firstDate + distance * 0.4);
                        doubles.Add(firstDate + distance * 0.6);
                        doubles.Add(firstDate + distance * 0.8);
                        doubles.Add(lastDate);


                        // create a manual tick generator and add ticks
                        ScottPlot.TickGenerators.NumericManual ticks = new ScottPlot.TickGenerators.NumericManual();
                        // add major ticks with their labels
                        for (int i = 0; i < doubles.Count; i++)
                        {
                            ticks.AddMajor(doubles[i], CustomFormatter(doubles[i]));
                        }
                        this.SingnalPlot.Axes.Bottom.TickGenerator = ticks;


                        this.SignalXY = this.SingnalPlot.Add.SignalXY(timeList.ToArray(), dataSource.ToArray(), color: ScottPlot.Color.FromHtml("#5470c6"));

                        // plot data using DateTime values on the horizontal axis
                        DateTime[] xs = Generate.ConsecutiveHours(100);
                        double[] ys = Generate.RandomWalk(100);
                        this.SingnalPlot.Add.Scatter(xs, ys);

                        this.PlotChart.Refresh();
                    });
                    await Task.Delay(1);
                }
            });
        }
    }
}
