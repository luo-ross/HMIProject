using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RS.Widgets.Interfaces
{
    public interface IDragDropStrategy
    {
        void OnDragStart(UIElement sender, MouseEventArgs e);
        void OnDrop(UIElement sender, DragEventArgs e);
    }
}
