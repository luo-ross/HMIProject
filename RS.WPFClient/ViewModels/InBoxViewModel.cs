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

        private List<MailModel> RawMailList = new List<MailModel>();


        private bool isPreviewMode;
        public bool IsPreviewMode
        {
            get
            {
                return isPreviewMode;
            }
            set
            {
                SetProperty(ref isPreviewMode, value);
            }
        }

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
            CreateFolderCommand = new RelayCommand(CreateFolder);
            MoveToSentCommand = new RelayCommand(MoveToSent);
            MoveToSubscriptionCommand = new RelayCommand(MoveToSubscription);

            InitTestData();
        }

        private void InitTestData()
        {
            var today = DateTime.Now;
            
            // 计算本周二
            int daysToTuesday = (int)today.DayOfWeek - (int)DayOfWeek.Tuesday;
            if (daysToTuesday < 0) daysToTuesday += 7;
            var thisTuesday = today.Date.AddDays(-daysToTuesday).AddHours(14); // 设置一个固定时间

            // 添加一些固定样式的初始数据
            RawMailList.Add(new MailModel { Account = "GitHub", Content = "广告邮件", Time = today, IsStarred = false });
            RawMailList.Add(new MailModel { Account = ".NET - UXDivers Team", Content = "Grial UI Kit Monthly Summary - December 2025 ", Time = thisTuesday, IsStarred = false });

            // 批量生成数据
            Random random = new Random();
            for (int i = 0; i < 30; i++)
            {
                int rand = random.Next(100);
                DateTime mailTime;
                if (rand < 10)
                {
                    mailTime = today.Date.AddHours(random.Next(0, 24)).AddMinutes(random.Next(0, 60));
                }
                else if (rand < 30)
                {
                    mailTime = today.Date.AddDays(-random.Next(1, 7)).AddHours(random.Next(0, 24));
                }
                else
                {
                    mailTime = today.Date.AddDays(-random.Next(8, 365)).AddHours(random.Next(0, 24));
                }

                RawMailList.Add(new MailModel
                {
                    Account = i % 3 == 0 ? "GitHub" : (i % 3 == 1 ? "TeamViewer" : "Avalonia UI"),
                    Content = $"测试邮件内容 {i + 1}",
                    Time = mailTime,
                    IsStarred = i % 10 == 0,
                    IsRead = i % 4 == 0,
                    Subject = $"测试邮件主题 {i + 1}",
                    HasAttachment = i % 8 == 0,
                    Digest = $"测试邮件摘要数据快速反击快速减肥刷卡积分快速减肥 {i + 1}",
                });
            }

            UpdateFlattenedLists();
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


     


        private ObservableCollection<object>? mailList;
        /// <summary>
        /// 邮件列表 (包含分组头和邮件项)
        /// </summary>
        public ObservableCollection<object> MailList
        {
            get
            {
                if (mailList == null)
                {
                    mailList = new ObservableCollection<object>();
                }
                return mailList;
            }
            set
            {
                SetProperty(ref mailList, value);
            }
        }

        /// <summary>
        /// 更新扁平化列表
        /// </summary>
        private void UpdateFlattenedLists()
        {
            if (RawMailList == null)
            {
                return;
            }

            var result = new ObservableCollection<object>();

            // 使用排序和分组逻辑
            var groups = RawMailList
                .OrderByDescending(m => m.Time)
                .GroupBy(m => GetGroupTitle(m.Time));

            foreach (var group in groups)
            {
                // 添加分组头
                var header = new GroupHeaderModel();
                header.GroupTitle = group.Key;
                header.ItemCount = group.Count();
                result.Add(header);

                foreach (var item in group)
                {
                    result.Add(item);
                }
            }

            MailList = result;
        }

        private string GetGroupTitle(DateTime? mailTime)
        {
            if (!mailTime.HasValue)
            {
                return "更早";
            }

            var date = mailTime.Value.Date;
            var today = DateTime.Today;

            if (date == today)
            {
                return "今天";
            }

            if (date == today.AddDays(-1))
            {
                return "昨天";
            }
            
            // 本周逻辑
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = today.AddDays(-1 * diff).Date;
            if (date >= startOfWeek)
            {
                return date.ToString("dddd");
            }

            return date.ToString("yyyy年MM月");
        }
    }
}

