using IdGen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RS.Commons.Extensions;
using RS.HMI.Client.Models;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Views.Areas
{
    public class UserViewModel : NotifyBase
    {
        private readonly IIdGenerator<long> IdGenerator;
        public UserViewModel()
        {
            //获取分布式主键生成服务
            this.IdGenerator = ServiceProviderExtensions.GetService<IIdGenerator<long>>();
            UserModelList = new ObservableCollection<UserModel>();
            for (int i = 0; i < 100; i++)
            {
                UserModelList.Add(new UserModel()
                {
                    Id = this.IdGenerator.CreateId(),
                    Email=$"184596029{i}@qq.com",
                    NickName="Ross",
                    Phone="111111111",
                    UserPic="sfsdfsdf",
                });
            }
        }

        private ObservableCollection<UserModel> userModelList;
        /// <summary>
        /// 用户数据
        /// </summary>
        public ObservableCollection<UserModel> UserModelList
        {
            get { return userModelList; }
            set
            {
                this.OnPropertyChanged(ref userModelList, value);
            }
        }
    }
}
