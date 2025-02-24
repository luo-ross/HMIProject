using RS.WPFApp.Widgets.Enums;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RS.WPFApp.Widgets.Controls
{
    /// <summary>
    /// RSDLMethod.xaml 的交互逻辑
    /// </summary>
    public partial class RSDLMethod : RadioButton
    {
        public RSDLMethod()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 深度学习方法类型枚举
        /// </summary>
        public TaskEnum Tasks
        {
            get { return (TaskEnum)GetValue(TasksProperty); }
            set { SetValue(TasksProperty, value); }
        }

        public static readonly DependencyProperty TasksProperty =
            DependencyProperty.Register("Tasks", typeof(TaskEnum), typeof(RSDLMethod), new PropertyMetadata(TaskEnum.Detect));


        /// <summary>
        /// 深度学习方法名称
        /// </summary>
        public string MethodName
        {
            get { return (string)GetValue(MethodNameProperty); }
            set { SetValue(MethodNameProperty, value); }
        }

        public static readonly DependencyProperty MethodNameProperty =
            DependencyProperty.Register("MethodName", typeof(string), typeof(RSDLMethod), new PropertyMetadata(default));



        /// <summary>
        /// 方法备注
        /// </summary>
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(RSDLMethod), new PropertyMetadata(default));





        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
