using CommunityToolkit.Mvvm.ComponentModel;
using RS.WPFClient.Enums;
using System;

namespace RS.WPFClient.Models
{
    public class MailModel : ObservableObject
    {
        private string? account;
        /// <summary>
        /// 账户
        /// </summary>
        public string? Account
        {
            get
            {
                return account;
            }
            set
            {
                this.SetProperty(ref account, value);
            }
        }

        private string? content;
        /// <summary>
        /// 邮件内容
        /// </summary>
        public string? Content
        {
            get
            {
                return content;
            }
            set
            {
                this.SetProperty(ref content, value);
            }
        }

        private DateTime? time;
        /// <summary>
        /// 邮件时间
        /// </summary>
        public DateTime? Time
        {
            get
            {
                return time;
            }
            set
            {
                this.SetProperty(ref time, value);
            }
        }

        private bool isStarred;
        /// <summary>
        /// 是否星标
        /// </summary>
        public bool IsStarred
        {
            get
            {
                return isStarred;
            }
            set
            {
                this.SetProperty(ref isStarred, value);
            }
        }


        private bool isRead;
        /// <summary>
        /// 是否已读 true代表已读 false 代表未读
        /// </summary>
        public bool IsRead
        {
            get
            {
                return isRead;
            }
            set
            {
                this.SetProperty(ref isRead, value);
            }
        }


        private string subject;
        /// <summary>
        /// 邮件主题
        /// </summary>
        public string Subject
        {
            get
            {
                return subject;
            }
            set
            {
                this.SetProperty(ref subject, value);
            }
        }


        private bool hasAttachment;
        /// <summary>
        /// 是否有附件
        /// </summary>
        public bool HasAttachment
        {
            get
            {
                return hasAttachment;
            }
            set
            {
                this.SetProperty(ref hasAttachment, value);
            }
        }


        private string digest;
        /// <summary>
        /// 邮件摘要
        /// </summary>
        public string Digest
        {
            get
            {
                return digest;
            }
            set
            {
                this.SetProperty(ref digest, value);
            }
        }


        private bool isSelect;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelect
        {
            get
            {
                return isSelect;
            }
            set
            {
                this.SetProperty(ref isSelect, value);
            }
        }


        public bool IsHeader
        {
            get
            {
                return false;
            }
        }
        private SelectionPosition selectionPosition;
        /// <summary>
        /// 选中位置状态
        /// </summary>
        public SelectionPosition SelectionPosition
        {
            get
            {
                return selectionPosition;
            }
            set
            {
                this.SetProperty(ref selectionPosition, value);
            }
        }

        private string? email;
        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string? Email
        {
            get
            {
                return email;
            }
            set
            {
                this.SetProperty(ref email, value);
            }
        }

        private string? avatar;
        /// <summary>
        /// 头像
        /// </summary>
        public string? Avatar
        {
            get
            {
                return avatar;
            }
            set
            {
                this.SetProperty(ref avatar, value);
            }
        }
    }
}
