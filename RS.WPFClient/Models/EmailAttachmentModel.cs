using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.WPFClient.Models
{
    public class EmailAttachmentModel : ObservableObject
    {
        public string? Id { get; set; }


        private string? attachName;
        /// <summary>
        /// 附件名称
        /// </summary>
        public string? AttachName
        {
            get
            {
                return attachName;
            }
            set
            {
                this.SetProperty(ref attachName, value);
            }
        }


        private string? attachSuffix;
        /// <summary>
        /// 附件类型
        /// </summary>
        public string? AttachSuffix
        {
            get
            {
                return attachSuffix;
            }
            set
            {
                this.SetProperty(ref attachSuffix, value);
            }
        }

        private string? sourceUrl;
        /// <summary>
        /// 附件地址
        /// </summary>
        public string? SourceUrl
        {
            get
            {
                return sourceUrl;
            }
            set
            {
                this.SetProperty(ref sourceUrl, value);
            }
        }
    }
}
