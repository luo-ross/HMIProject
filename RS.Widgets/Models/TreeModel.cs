using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public class TreeModel : NotifyBase
    {
      
        private bool isSelect;

        public bool IsSelect
        {
            get { return isSelect; }
            set
            {
                isSelect = value;
                this.OnPropertyChanged(nameof(IsSelect));
            }
        }


        private string treeIcon;

        public string TreeIcon
        {
            get { return treeIcon; }
            set
            {
                treeIcon = value;
                this.OnPropertyChanged(nameof(TreeIcon));
            }
        }

        private string treeName;

        public string TreeName
        {
            get { return treeName; }
            set
            {
                treeName = value;
                this.OnPropertyChanged(nameof(TreeName));
            }
        }


        private ObservableCollection<TreeModel> children;

        public ObservableCollection<TreeModel> Children
        {
            get { return children; }
            set
            {
                children = value;
                this.OnPropertyChanged();
            }
        }


        private bool isExpanded;

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                isExpanded = value;
                if (isExpanded != value)
                {
                    isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                    //// Load children on expand
                    //if (isExpanded && Children.Count == 1 && Children[0].Name == "Loading...")
                    //{
                    //    Children.Clear();
                    //    LoadChildren();
                    //}
                }
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        private void LoadChildren()
        {
            // Load children logic here
        }
    }
}
