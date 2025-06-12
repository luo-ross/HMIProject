using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RS.Widgets.Events
{
    public delegate void RSMouseButtonEventHandler(object sender, RSMouseButtonEventArgs e);
    public class RSMouseButtonEventArgs : MouseEventArgs
    {
       
        public RSMouseButtonEventArgs(MouseDevice mouse,
                                    int timestamp,
                                    MouseButton button, int count = 1) : base(mouse, timestamp)
        {
            _button = button;
            _count = count;
        }

     
        public RSMouseButtonEventArgs(MouseDevice mouse,
                                    int timestamp,
                                    MouseButton button,
                                    StylusDevice stylusDevice,int count=1) : base(mouse, timestamp, stylusDevice)
        {
            _button = button;
            _count = count;
        }

      
        public MouseButton ChangedButton
        {
            get { return _button; }
        }

        
        public MouseButtonState ButtonState
        {
            get
            {
                MouseButtonState state = MouseButtonState.Released;

                switch (_button)
                {
                    case MouseButton.Left:
                        state = this.MouseDevice.LeftButton;
                        break;

                    case MouseButton.Right:
                        state = this.MouseDevice.RightButton;
                        break;

                    case MouseButton.Middle:
                        state = this.MouseDevice.MiddleButton;
                        break;

                    case MouseButton.XButton1:
                        state = this.MouseDevice.XButton1;
                        break;

                    case MouseButton.XButton2:
                        state = this.MouseDevice.XButton2;
                        break;
                }

                return state;
            }
        }

        /// <summary>
        ///     Read access to the button click count.
        /// </summary>
        public int ClickCount
        {
            get { return _count; }
            set { _count = value; }
        }
      
        protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
        {
            RSMouseButtonEventHandler handler = (RSMouseButtonEventHandler)genericHandler;
            handler(genericTarget, this);
        }

        private MouseButton _button;
        private int _count;
    }
}
