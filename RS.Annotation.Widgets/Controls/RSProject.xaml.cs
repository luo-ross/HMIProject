using RS.Widgets.Models;
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

namespace RS.Annotation.Widgets.Controls
{
    /// <summary>
    /// RSProject.xaml 的交互逻辑
    /// </summary>
    public partial class RSProject : RadioButton
    {
        public RSProject()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 项目实体类
        /// </summary>

        public ProjectModel ProjectModel
        {
            get { return (ProjectModel)GetValue(ProjectModelProperty); }
            set { SetValue(ProjectModelProperty, value); }
        }

        public static readonly DependencyProperty ProjectModelProperty =
            DependencyProperty.Register("ProjectModel", typeof(ProjectModel), typeof(RSProject), new PropertyMetadata(default, OnProjectModelPropertyChanged));

        private static void OnProjectModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsProject = d as RSProject;
            var sdf = rsProject.ProjectModel;
        }



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
