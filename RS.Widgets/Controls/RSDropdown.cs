using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace RS.Widgets.Controls
{
    public class RSDropdown : ToggleButton
    {
        private Popup PART_Popup;
        static RSDropdown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDropdown), new FrameworkPropertyMetadata(typeof(RSDropdown)));
        }
        public RSDropdown()
        {
           
        }

        public UIElement PlacementTarget
        {
            get { return (UIElement)GetValue(PlacementTargetProperty); }
            set { SetValue(PlacementTargetProperty, value); }
        }

        public static readonly DependencyProperty PlacementTargetProperty =
            DependencyProperty.Register("PlacementTarget", typeof(UIElement), typeof(RSDropdown), new PropertyMetadata(null));



        public object DropdownContent
        {
            get { return (object)GetValue(DropdownContentProperty); }
            set { SetValue(DropdownContentProperty, value); }
        }

        public static readonly DependencyProperty DropdownContentProperty =
            DependencyProperty.Register("DropdownContent", typeof(object), typeof(RSDropdown), new PropertyMetadata(null));




        public double DropdownWidth
        {
            get { return (double)GetValue(DropdownWidthProperty); }
            set { SetValue(DropdownWidthProperty, value); }
        }

        public static readonly DependencyProperty DropdownWidthProperty =
            DependencyProperty.Register("DropdownWidth", typeof(double), typeof(RSDropdown), new PropertyMetadata(double.NaN));



        public double DropdownHeight
        {
            get { return (double)GetValue(DropdownHeightProperty); }
            set { SetValue(DropdownHeightProperty, value); }
        }

        public static readonly DependencyProperty DropdownHeightProperty =
            DependencyProperty.Register("DropdownHeight", typeof(double), typeof(RSDropdown), new PropertyMetadata(double.NaN));





        public bool IsShowDropDownIcon
        {
            get { return (bool)GetValue(IsShowDropDownIconProperty); }
            set { SetValue(IsShowDropDownIconProperty, value); }
        }

        public static readonly DependencyProperty IsShowDropDownIconProperty =
            DependencyProperty.Register(nameof(IsShowDropDownIcon), typeof(bool), typeof(RSDropdown), new PropertyMetadata(false));





        public PlacementMode Placement
        {
            get { return (PlacementMode)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register(nameof(Placement), typeof(PlacementMode), typeof(RSDropdown), new PropertyMetadata(PlacementMode.Bottom));




        public double ArrowAngle
        {
            get { return (double)GetValue(ArrowAngleProperty); }
            set { SetValue(ArrowAngleProperty, value); }
        }

        public static readonly DependencyProperty ArrowAngleProperty =
            DependencyProperty.Register(nameof(ArrowAngle), typeof(double), typeof(RSDropdown), new PropertyMetadata(90D));




     

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Popup = this.GetTemplateChild(nameof(this.PART_Popup)) as Popup;
        }


    }
}
