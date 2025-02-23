using RS.Commons;
using RS.Widgets.Commons;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public class RSProject : RadioButton
    {
        static RSProject()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSProject), new FrameworkPropertyMetadata(typeof(RSProject)));
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
