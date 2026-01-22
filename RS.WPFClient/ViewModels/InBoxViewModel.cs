using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.WPFClient.Enums;
using RS.Models;
using RS.Server.WebAPI;
using RS.Widgets.Controls;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using RS.WPFClient.Models;
using System.Collections.ObjectModel;

namespace RS.WPFClient.ViewModels
{
    [ServiceInjectConfig(ServiceLifetime.Transient)]
    public class InBoxViewModel : ViewModelBase
    {
        public ICommand DeleteCommand { get; }
        public ICommand ReplyCommand { get; }
        public ICommand ReplyAllCommand { get; }
        public ICommand ForwardCommand { get; }
        public ICommand ReportCommand { get; }
        public ICommand MarkAllAsReadCommand { get; }
        public ICommand MarkAsReadCommand { get; }
        public ICommand MarkAsUnReadCommand { get; }
        public ICommand MarkAsStarredCommand { get; }
        public ICommand MarkAsUnStarredCommand { get; }
        public ICommand MarkAsSpamCommand { get; }
        public ICommand CreateLabelCommand { get; }
        public ICommand MoveToSentCommand { get; }
        public ICommand MoveToSubscriptionCommand { get; }
        public ICommand CreateFolderCommand { get; }

        public InBoxViewModel()
        {
            DeleteCommand = new RelayCommand(Delete);
            ReplyCommand = new RelayCommand(Reply);
            ReplyAllCommand = new RelayCommand(ReplyAll);
            ForwardCommand = new RelayCommand(Forward);
            ReportCommand = new RelayCommand(Report);
            MarkAllAsReadCommand = new RelayCommand(MarkAllAsRead);
            MarkAsReadCommand = new RelayCommand(MarkAsRead);
            MarkAsUnReadCommand = new RelayCommand(MarkAsUnRead);
            MarkAsStarredCommand = new RelayCommand(MarkAsStarred);
            MarkAsUnStarredCommand = new RelayCommand(MarkAsUnStarred);
            MarkAsSpamCommand = new RelayCommand(MarkAsSpam);
            CreateLabelCommand = new RelayCommand(CreateLabel);
            MoveToSentCommand = new RelayCommand(MoveToSent);
            MoveToSubscriptionCommand = new RelayCommand(MoveToSubscription);
            CreateFolderCommand = new RelayCommand(CreateFolder);

            InitSearchTypeList();
        }

       

        private void Delete()
        {
            /* 删除逻辑待实现 */
        }

        private void Reply()
        {
            /* 回复逻辑待实现 */
        }

        private void ReplyAll()
        {
            /* 回复全部逻辑待实现 */
        }

        private void Forward()
        {
            /* 转发逻辑待实现 */
        }

        private void Report()
        {
            /* 举报逻辑待实现 */
        }

        private void MarkAllAsRead()
        {
            /* 全部标记为已读的逻辑待实现 */
        }

        private void MarkAsRead()
        {
            /* 标记为已读逻辑待实现 */
        }

        private void MarkAsUnRead()
        {
            /* 标记为未读逻辑待实现 */
        }

        private void MarkAsStarred()
        {
            /* 标记为星标逻辑待实现 */
        }

        private void MarkAsUnStarred()
        {
            /* 取消星标逻辑待实现 */
        }

        private void MarkAsSpam()
        {
            /* 标记为广告邮件逻辑待实现 */
        }

        private void CreateLabel()
        {
            /* 新建标签逻辑待实现 */
        }

        private void MoveToSent()
        {
            Console.WriteLine("MoveToSent");
            /* 移动到已发送逻辑待实现 */
        }

        private void MoveToSubscription()
        {
            /* 移动到邮件订阅逻辑待实现 */
        }

        private void CreateFolder()
        {
            /* 新建文件夹逻辑待实现 */
        }






        private DateTime? dateTimeSelected;
        /// <summary>
        /// 日期选择
        /// </summary>
        public DateTime? DateTimeSelected
        {
            get
            {
                return dateTimeSelected;
            }
            set
            {
                SetProperty(ref dateTimeSelected, value);
            }
        }


        private MailFilterType mailFilterTypeSelect;
        /// <summary>
        /// 邮件筛选类型
        /// </summary>
        public MailFilterType MailFilterTypeSelect
        {
            get { return mailFilterTypeSelect; }
            set
            {
                this.SetProperty(ref mailFilterTypeSelect, value);
            }
        }



        private ObservableCollection<MailFilterType>? mailFilterTypeList;
        /// <summary>
        /// 邮件筛选类型
        /// </summary>
        public ObservableCollection<MailFilterType> MailFilterTypeList
        {
            get
            {
                return mailFilterTypeList;
            }
            set
            {
                SetProperty(ref mailFilterTypeList, value);
            }
        }


        /// <summary>
        /// 初始化搜索类型
        /// </summary>
        private void InitSearchTypeList()
        {
            var dataList = GetDataList<MailFilterType>();
            this.MailFilterTypeList = new ObservableCollection<MailFilterType>(dataList);
            this.MailFilterTypeSelect = this.MailFilterTypeList.FirstOrDefault(t => t == MailFilterType.AllRead);
        }

    }
}
