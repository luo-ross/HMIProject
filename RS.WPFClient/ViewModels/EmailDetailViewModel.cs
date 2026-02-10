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
    public class EmailDetailViewModel : ViewModelBase
    {

        public EmailDetailViewModel()
        {

        }


        private EmailModel? selectedMail;
        /// <summary>
        /// 当前选中的邮件
        /// </summary>
        public EmailModel? SelectedMail
        {
            get
            {
                return selectedMail;
            }
            set
            {
                this.SetProperty(ref selectedMail, value);
            }
        }



        private bool isBasicDetail = true;
        /// <summary>
        /// 是否是基础详情  默认True 是基础详情 False 是完整详情
        /// </summary>
        public bool IsBasicDetail
        {
            get
            {
                return isBasicDetail;
            }
            set
            {
                this.SetProperty(ref isBasicDetail, value);
            }
        }


    }
}

