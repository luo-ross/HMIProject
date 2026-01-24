using CommunityToolkit.Mvvm.ComponentModel;

namespace RS.WPFClient.Models
{
    public class GroupHeaderModel : ObservableObject
    {
        private string? groupTitle;
        public string? GroupTitle
        {
            get => groupTitle;
            set => SetProperty(ref groupTitle, value);
        }

        public bool IsHeader
        {
            get
            {
                return true;
            }
        }

        private int itemCount;
        public int ItemCount
        {
            get => itemCount;
            set => SetProperty(ref itemCount, value);
        }
    }
}
