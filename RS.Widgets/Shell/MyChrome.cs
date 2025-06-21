using RS.Widgets.Structs;
using RS.Win32API.Standard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Shell;

namespace RS.Widgets.Shell
{
    public class MyChrome : Freezable
    {
       
       
        public static readonly DependencyProperty MyChromeProperty = DependencyProperty.RegisterAttached("MyChrome", typeof(MyChrome), typeof(MyChrome), new PropertyMetadata(null, _OnChromeChanged));
        
        public static readonly DependencyProperty IsHitTestVisibleInChromeProperty = DependencyProperty.RegisterAttached("IsHitTestVisibleInChrome", typeof(bool), typeof(MyChrome), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
    
        public static readonly DependencyProperty ResizeGripDirectionProperty = DependencyProperty.RegisterAttached("ResizeGripDirection", typeof(ResizeGripDirection), typeof(MyChrome), new FrameworkPropertyMetadata(ResizeGripDirection.None, FrameworkPropertyMetadataOptions.Inherits));
      
        public static readonly DependencyProperty CaptionHeightProperty = DependencyProperty.Register("CaptionHeight", typeof(double), typeof(MyChrome), new PropertyMetadata(0.0, delegate (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MyChrome)d)._OnPropertyChangedThatRequiresRepaint();
        }), (value) => (double)value >= 0.0);
       
        public static readonly DependencyProperty ResizeBorderThicknessProperty = DependencyProperty.Register("ResizeBorderThickness", typeof(Thickness), typeof(MyChrome), new PropertyMetadata(default(Thickness)), (value) => Utility.IsThicknessNonNegative((Thickness)value));

        public static readonly DependencyProperty GlassFrameThicknessProperty = DependencyProperty.Register("GlassFrameThickness", typeof(Thickness), typeof(MyChrome), new PropertyMetadata(default(Thickness), delegate (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MyChrome)d)._OnPropertyChangedThatRequiresRepaint();
        }, (d, o) => _CoerceGlassFrameThickness((Thickness)o)));
      
        public static readonly DependencyProperty UseAeroCaptionButtonsProperty = DependencyProperty.Register("UseAeroCaptionButtons", typeof(bool), typeof(MyChrome), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(MyChrome), new PropertyMetadata(default(CornerRadius), delegate (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MyChrome)d)._OnPropertyChangedThatRequiresRepaint();
        }), (value) => Utility.IsCornerRadiusValid((CornerRadius)value));

        public static readonly DependencyProperty NonClientFrameEdgesProperty = DependencyProperty.Register("NonClientFrameEdges", typeof(NonClientFrameEdges), typeof(MyChrome), new PropertyMetadata(NonClientFrameEdges.None, delegate (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MyChrome)d)._OnPropertyChangedThatRequiresRepaint();
        }), _NonClientFrameEdgesAreValid);

        private static readonly NonClientFrameEdges NonClientFrameEdges_All = NonClientFrameEdges.Left | NonClientFrameEdges.Top | NonClientFrameEdges.Right | NonClientFrameEdges.Bottom;


       
        public static Thickness GlassFrameCompleteThickness => new Thickness(-1.0);

      
        public double CaptionHeight
        {
            get
            {
                return (double)GetValue(CaptionHeightProperty);
            }
            set
            {
                SetValue(CaptionHeightProperty, value);
            }
        }

     
        public Thickness ResizeBorderThickness
        {
            get
            {
                return (Thickness)GetValue(ResizeBorderThicknessProperty);
            }
            set
            {
                SetValue(ResizeBorderThicknessProperty, value);
            }
        }

     
        public Thickness GlassFrameThickness
        {
            get
            {
                return (Thickness)GetValue(GlassFrameThicknessProperty);
            }
            set
            {
                SetValue(GlassFrameThicknessProperty, value);
            }
        }

      
        public bool UseAeroCaptionButtons
        {
            get
            {
                return (bool)GetValue(UseAeroCaptionButtonsProperty);
            }
            set
            {
                SetValue(UseAeroCaptionButtonsProperty, value);
            }
        }

     
        public CornerRadius CornerRadius
        {
            get
            {
                return (CornerRadius)GetValue(CornerRadiusProperty);
            }
            set
            {
                SetValue(CornerRadiusProperty, value);
            }
        }

      
        public NonClientFrameEdges NonClientFrameEdges
        {
            get
            {
                return (NonClientFrameEdges)GetValue(NonClientFrameEdgesProperty);
            }
            set
            {
                SetValue(NonClientFrameEdgesProperty, value);
            }
        }

        internal event EventHandler PropertyChangedThatRequiresRepaint;

        private static void _OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(d))
            {
                Window window = (Window)d;
                MyChrome MyChrome = (MyChrome)e.NewValue;
                MyChromeWorker MyChromeWorker = MyChromeWorker.GetMyChromeWorker(window);
                if (MyChromeWorker == null)
                {
                    MyChromeWorker = new MyChromeWorker();
                    MyChromeWorker.SetMyChromeWorker(window, MyChromeWorker);
                }

                MyChromeWorker.SetMyChrome(MyChrome);
            }
        }

      
        public static MyChrome GetMyChrome(Window window)
        {
            Verify.IsNotNull(window, "window");
            return (MyChrome)window.GetValue(MyChromeProperty);
        }

       
        public static void SetMyChrome(Window window, MyChrome chrome)
        {
            Verify.IsNotNull(window, "window");
            window.SetValue(MyChromeProperty, chrome);
        }

        
        public static bool GetIsHitTestVisibleInChrome(IInputElement inputElement)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            if (!(inputElement is DependencyObject dependencyObject))
            {
                throw new ArgumentException("The element must be a DependencyObject", "inputElement");
            }

            return (bool)dependencyObject.GetValue(IsHitTestVisibleInChromeProperty);
        }

       
        public static void SetIsHitTestVisibleInChrome(IInputElement inputElement, bool hitTestVisible)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            if (!(inputElement is DependencyObject dependencyObject))
            {
                throw new ArgumentException("The element must be a DependencyObject", "inputElement");
            }

            dependencyObject.SetValue(IsHitTestVisibleInChromeProperty, hitTestVisible);
        }

     
        public static ResizeGripDirection GetResizeGripDirection(IInputElement inputElement)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            if (!(inputElement is DependencyObject dependencyObject))
            {
                throw new ArgumentException("The element must be a DependencyObject", "inputElement");
            }

            return (ResizeGripDirection)dependencyObject.GetValue(ResizeGripDirectionProperty);
        }

       
        public static void SetResizeGripDirection(IInputElement inputElement, ResizeGripDirection direction)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            if (!(inputElement is DependencyObject dependencyObject))
            {
                throw new ArgumentException("The element must be a DependencyObject", "inputElement");
            }

            dependencyObject.SetValue(ResizeGripDirectionProperty, direction);
        }

        private static object _CoerceGlassFrameThickness(Thickness thickness)
        {
            if (!Utility.IsThicknessNonNegative(thickness))
            {
                return GlassFrameCompleteThickness;
            }

            return thickness;
        }

        private static bool _NonClientFrameEdgesAreValid(object value)
        {
            NonClientFrameEdges nonClientFrameEdges = NonClientFrameEdges.None;
            try
            {
                nonClientFrameEdges = (NonClientFrameEdges)value;
            }
            catch (InvalidCastException)
            {
                return false;
            }

            if (nonClientFrameEdges == NonClientFrameEdges.None)
            {
                return true;
            }

            if ((nonClientFrameEdges | NonClientFrameEdges_All) != NonClientFrameEdges_All)
            {
                return false;
            }

            if (nonClientFrameEdges == NonClientFrameEdges_All)
            {
                return false;
            }

            return true;
        }


        private void _OnPropertyChangedThatRequiresRepaint()
        {
            PropertyChangedThatRequiresRepaint?.Invoke(this, EventArgs.Empty);
        }

        private static readonly List<_SystemParameterBoundProperty> _BoundProperties = new List<_SystemParameterBoundProperty>
    {
        new _SystemParameterBoundProperty
        {
            DependencyProperty = CornerRadiusProperty,
            SystemParameterPropertyName = "WindowCornerRadius"
        },
        new _SystemParameterBoundProperty
        {
            DependencyProperty = CaptionHeightProperty,
            SystemParameterPropertyName = "WindowCaptionHeight"
        },
        new _SystemParameterBoundProperty
        {
            DependencyProperty = ResizeBorderThicknessProperty,
            SystemParameterPropertyName = "WindowResizeBorderThickness"
        },
        new _SystemParameterBoundProperty
        {
            DependencyProperty = GlassFrameThicknessProperty,
            SystemParameterPropertyName = "WindowNonClientFrameThickness"
        }
    };
        public MyChrome()
        {
            foreach (_SystemParameterBoundProperty boundProperty in _BoundProperties)
            {
                BindingOperations.SetBinding(this, boundProperty.DependencyProperty, new Binding
                {
                    Path = new PropertyPath("SystemParameters." + boundProperty.SystemParameterPropertyName + ""),
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
            }
        }


        protected override Freezable CreateInstanceCore()
        {
           return  new MyChrome();  
        }
    }
}
