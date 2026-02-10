using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.WPFClient.Enums;
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
    public class InBoxViewModel : ViewModelBase
    {
        private List<EmailModel> RawMailList = new List<EmailModel>();
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
        public ICommand ToggleStarCommand { get; }

        public ICommand EmailSelectAllCommand { get; }
        public ICommand EmailSelectCommand { get; }
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
            ToggleStarCommand = new RelayCommand<EmailModel>(ToggleStar);
            EmailSelectAllCommand = new RelayCommand(EmailSelectAll);
            EmailSelectCommand = new RelayCommand(EmailSelect);
            EmailDetailViewModel = new EmailDetailViewModel();

            InitTestData();
        }

        private void EmailSelect()
        {
            UpdateIsSelectedAll();
            UpdateCornerRadius();
        }

        private void EmailSelectAll()
        {
            if (!IsSelectedAll.HasValue)
            {
                return;
            }
            bool isSelectAll = this.IsSelectedAll.Value;
            foreach (var mail in RawMailList)
            {
                mail.IsSelect = isSelectAll;
            }
            UpdateIsSelectedAll();
            UpdateCornerRadius();
        }



        private EmailDetailViewModel? mailDetailViewModel;
        /// <summary>
        /// 邮件详情 ViewModel，负责管理邮件详情的显示和数据
        /// </summary>
        public EmailDetailViewModel? EmailDetailViewModel
        {
            get
            {
                return mailDetailViewModel;
            }
            set
            {
                this.SetProperty(ref mailDetailViewModel, value);
            }
        }

        private void InitTestData()
        {
            var today = DateTime.Now;

            // 计算本周二
            int daysToTuesday = (int)today.DayOfWeek - (int)DayOfWeek.Tuesday;
            if (daysToTuesday < 0) daysToTuesday += 7;
            var thisTuesday = today.Date.AddDays(-daysToTuesday).AddHours(14); // 设置一个固定时间


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

                string accountName = i % 3 == 0 ? "GitHub" : (i % 3 == 1 ? "TeamViewer" : "Avalonia UI");
                string emailAddr = i % 3 == 0 ? "noreply@github.com" : (i % 3 == 1 ? "support@teamviewer.com" : "contact@avalonia.com");

                // 模拟一个 HTML 内容
                string htmlContent = $@"
                    <div style='font-family: sans-serif; color: #333;'>
                        <h2 style='color: #0078d4;'>欢迎使用 MultiVerseKit</h2>
                        <p>这是您的测试邮件内容。<strong>{accountName}</strong> 向您发送了重要通知。</p>
                        <p style='background: #f4f4f4; padding: 10px; border-radius: 4px;'>ID: {i + 1} | Time: {mailTime:yyyy-MM-dd HH:mm}</p>
                        <p>请点击下方按钮查看更多详情：</p>
                        <a href='https://www.baidu.com' style='display: inline-block; padding: 10px 20px; background: #0078d4; color: white; text-decoration: none; border-radius: 20px;'>立即查看</a>
                    </div>";

                RawMailList.Add(new EmailModel
                {
                    Account = accountName,
                    Email = emailAddr,
                    Content = htmlContent,
                    Time = mailTime,
                    IsStarred = i % 10 == 0,
                    IsRead = i % 4 == 0,
                    Subject = $"测试邮件主题 {i + 1}",
                    HasAttachment = i % 8 == 0,
                    Digest = $"测试邮件摘要数据 {i + 1}...",
                    EmailAttachmentModelList = new ObservableCollection<EmailAttachmentModel>()
                    {
                        new EmailAttachmentModel(){
                            Id=Guid.NewGuid().ToString(),
                            AttachName="R04162495",
                            AttachSuffix=".pdf",
                        }
                    },
                    IsCarbonCopy = i % 2 == 0,
                    ProxyName=$"bounce-md_31242303.69459033.v1-9a25bc4332a446bd9df8360c6fdf1a75@mandrillapp.com"
                });
            }

            UpdateFlattenedLists();
            UpdateCornerRadius();

            // 默认选中第一个
            if (RawMailList.Count > 0)
            {
                EmailDetailViewModel!.SelectedMail = RawMailList[0];
            }
        }


        private void UpdateIsSelectedAll()
        {
            bool hasSelected = false;
            bool hasUnselected = false;

            foreach (var mail in RawMailList)
            {
                if (mail.IsSelect)
                {
                    hasSelected = true;
                }
                else
                {
                    hasUnselected = true;
                }

                if (hasSelected && hasUnselected)
                {
                    break;
                }
            }

            if (hasSelected && hasUnselected)
            {
                IsSelectedAll = null;
            }
            else if (hasSelected)
            {
                IsSelectedAll = true;
            }
            else
            {
                IsSelectedAll = false;
            }

            HasItemIsChecked = hasSelected;
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

        private void ToggleStar(EmailModel mail)
        {
            if (mail != null)
            {
                mail.IsStarred = !mail.IsStarred;
            }
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




        private bool? isSelectedAll = false;
        public bool? IsSelectedAll
        {
            get
            {
                return isSelectedAll;
            }
            set
            {
                this.SetProperty(ref isSelectedAll, value);
            }
        }

        private bool hasItemIsChecked;
        public bool HasItemIsChecked
        {
            get
            {
                return hasItemIsChecked;
            }
            set
            {
                SetProperty(ref hasItemIsChecked, value);
            }
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
        public ObservableCollection<object> EmailList
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

            // 1. 排序 (不使用 lambda)
            List<EmailModel> sortedList = new List<EmailModel>(RawMailList);
            sortedList.Sort(new Comparison<EmailModel>(CompareMailsByTime));

            // 2. 分组 (使用 Dictionary 手动分组)
            Dictionary<string, List<EmailModel>> groups = new Dictionary<string, List<EmailModel>>();
            List<string> groupOrder = new List<string>();

            foreach (var mail in sortedList)
            {
                string title = GetGroupTitle(mail.Time);
                if (!groups.ContainsKey(title))
                {
                    groups[title] = new List<EmailModel>();
                    groupOrder.Add(title);
                }
                groups[title].Add(mail);
            }

            // 3. 构建 Flattened List
            ObservableCollection<object> result = new ObservableCollection<object>();
            foreach (var title in groupOrder)
            {
                GroupHeaderModel header = new GroupHeaderModel();
                header.GroupTitle = title;
                header.ItemCount = groups[title].Count;
                result.Add(header);

                foreach (var item in groups[title])
                {
                    result.Add(item);
                }
            }

            EmailList = result;
        }

        private int CompareMailsByTime(EmailModel x, EmailModel y)
        {
            if (x.Time == y.Time)
            {
                return 0;
            }
            if (x.Time == null)
            {
                return 1;
            }
            if (y.Time == null)
            {
                return -1;
            }
            return y.Time.Value.CompareTo(x.Time.Value); // 倒序
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


        private void UpdateCornerRadius()
        {
            if (EmailList == null)
            {
                return;
            }

            var list = EmailList.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (!(list[i] is EmailModel currentMail))
                {
                    continue;
                }

                if (!currentMail.IsSelect)
                {
                    currentMail.SelectionPosition = SelectionPosition.None;
                    continue;
                }

                bool prevSelected = (i > 0 && list[i - 1] is EmailModel prevMail && prevMail.IsSelect);
                bool nextSelected = (i < list.Count - 1 && list[i + 1] is EmailModel nextMail && nextMail.IsSelect);

                if (prevSelected && nextSelected)
                {
                    currentMail.SelectionPosition = SelectionPosition.Middle;
                }
                else if (prevSelected)
                {
                    currentMail.SelectionPosition = SelectionPosition.Bottom;
                }
                else if (nextSelected)
                {
                    currentMail.SelectionPosition = SelectionPosition.Top;
                }
                else
                {
                    currentMail.SelectionPosition = SelectionPosition.Single;
                }
            }
        }
    }
}

