using RS.Widgets.Interfaces;
using System.Windows;

namespace RS.Widgets.Controls
{

    public partial class RSWinModal : RSWindow, IWinModal, IWinMessage
    {
        public RSWinModal(Window owner)
        {
            InitializeComponent();
            this.BorderCornerRadius = new CornerRadius(5);
            this.Owner = owner;
            if (this.Owner == null)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
        }

        public void ShowModal(object content)
        {
            this.Content = content;
            base.Show();
        }

        public void CloseModal()
        {
            base.Close();
        }

        public void ShowDialog(object content)
        {
            this.Content = content;
            base.ShowDialog();
        }

    }
}
