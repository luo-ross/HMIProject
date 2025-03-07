using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RS.Widgets.Controls
{
    public class RSBorder : Border/*, ICommandSource*/
    {
        static RSBorder()
        {
            EventManager.RegisterClassHandler(typeof(RSBorder), UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(HandleDoubleClick), true);
            EventManager.RegisterClassHandler(typeof(RSBorder), UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(HandleDoubleClick), true);
            EventManager.RegisterClassHandler(typeof(RSBorder), UIElement.PreviewMouseRightButtonDownEvent, new MouseButtonEventHandler(HandleDoubleClick), true);
            EventManager.RegisterClassHandler(typeof(RSBorder), UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(HandleDoubleClick), true);
        }

        public RSBorder()
        {
          
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            ExecuteMouseMoveCommand();
        }
        // 第一个命令
        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register(
                nameof(MouseMoveCommand),
                typeof(ICommand),
                typeof(RSBorder),
                new PropertyMetadata(null));

        public ICommand MouseMoveCommand
        {
            get { return (ICommand)GetValue(MouseMoveCommandProperty); }
            set { SetValue(MouseMoveCommandProperty, value); }
        }

        // 第二个命令
        public static readonly DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.Register(
                nameof(DoubleClickCommand),
                typeof(ICommand),
                typeof(RSBorder),
                new PropertyMetadata(null));

        public ICommand DoubleClickCommand
        {
            get { return (ICommand)GetValue(DoubleClickCommandProperty); }
            set { SetValue(DoubleClickCommandProperty, value); }
        }


        // 触发第一个命令
        public void ExecuteMouseMoveCommand()
        {
            if (MouseMoveCommand != null && MouseMoveCommand.CanExecute(null))
            {
                MouseMoveCommand.Execute(null);
            }
        }

        // 触发第二个命令
        public void ExecuteDoubleClickCommand()
        {
            if (DoubleClickCommand != null && DoubleClickCommand.CanExecute(null))
            {
                DoubleClickCommand.Execute(null);
            }
        }

        #region 自定义双击事件

        public static readonly RoutedEvent PreviewMouseDoubleClickEvent = EventManager.RegisterRoutedEvent("PreviewMouseDoubleClick", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), typeof(RSBorder));
        public event MouseButtonEventHandler PreviewMouseDoubleClick
        {
            add { AddHandler(PreviewMouseDoubleClickEvent, value); }
            remove { RemoveHandler(PreviewMouseDoubleClickEvent, value); }
        }

        protected virtual void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            RaiseEvent(e);
        }

        public static readonly RoutedEvent MouseDoubleClickEvent = EventManager.RegisterRoutedEvent("MouseDoubleClick", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), typeof(RSBorder));

        public event MouseButtonEventHandler MouseDoubleClick
        {
            add { AddHandler(MouseDoubleClickEvent, value); }
            remove { RemoveHandler(MouseDoubleClickEvent, value); }
        }

        protected virtual void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            RaiseEvent(e);
            //if (Command != null && Command.CanExecute(CommandParameter))
            //{
            //    Command.Execute(CommandParameter);
            //}
            ExecuteDoubleClickCommand();
        }



        private static void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (e.ClickCount == 2)
            {
                RSBorder ctrl = (RSBorder)sender;
                MouseButtonEventArgs mouseButtonEventArgs = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton, e.StylusDevice);
                if ((e.RoutedEvent == UIElement.PreviewMouseLeftButtonDownEvent) ||
                    (e.RoutedEvent == UIElement.PreviewMouseRightButtonDownEvent))
                {
                    mouseButtonEventArgs.RoutedEvent = PreviewMouseDoubleClickEvent;
                    mouseButtonEventArgs.Source = e.Source;
                    ctrl.OnPreviewMouseDoubleClick(mouseButtonEventArgs);
                }
                else
                {
                    mouseButtonEventArgs.RoutedEvent = MouseDoubleClickEvent;
                    mouseButtonEventArgs.Source = e.Source;
                    ctrl.OnMouseDoubleClick(mouseButtonEventArgs);
                }

                if (mouseButtonEventArgs.Handled)
                    e.Handled = true;
            }
        }
        #endregion

    }
}
