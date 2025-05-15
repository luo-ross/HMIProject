using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RS.Widgets.Models
{
    public static class CURDCommands
    {
        public static RoutedCommand AddCommand { get; private set; }
        public static RoutedCommand DeleteCommand { get; private set; }
        public static RoutedCommand UpdateCommand { get; private set; }
        public static RoutedCommand DetailsCommand { get; private set; }
        public static RoutedCommand ExportCommand { get; private set; }
        static CURDCommands()
        {
            AddCommand = new RoutedCommand(nameof(AddCommand), typeof(RSDataGrid));
            DeleteCommand = new RoutedCommand(nameof(DeleteCommand), typeof(RSDataGrid));
            UpdateCommand = new RoutedCommand(nameof(UpdateCommand), typeof(RSDataGrid));
            DetailsCommand = new RoutedCommand(nameof(DetailsCommand), typeof(RSDataGrid));
            ExportCommand = new RoutedCommand(nameof(ExportCommand), typeof(RSDataGrid));
        }
    }
}
