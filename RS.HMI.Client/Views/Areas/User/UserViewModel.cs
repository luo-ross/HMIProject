using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IdGen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NPOI.Util;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Enums;
using RS.Commons.Extensions;
using RS.HMI.Client.Messages;
using RS.HMI.Client.Models;
using RS.Widgets.Controls;
using RS.Widgets.Interface;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RS.HMI.Client.Views.Areas
{
    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public partial class UserViewModel : ModelBase
    {
        private readonly IIdGenerator<long> IdGenerator;
        private readonly ILoadingService rsLoading;
        public List<UserModel> TestData = new List<UserModel>();
        public UserViewModel(IIdGenerator<long> idGenerator)
        {
            this.IdGenerator = idGenerator;

            for (int i = 0; i < 100; i++)
            {
                this.TestData.Add(new UserModel()
                {
                    Id = this.IdGenerator.CreateId(),
                    Email = $"184596029{i}@qq.com",
                    NickName = "Ross",
                    Phone = "111111111",
                    UserPic = "sfsdfsdf",
                });
            }
        }



        public override void Submit(object obj)
        {
            var sdf = 1;
        }


        public override void Update(object obj)
        {
            var sdf = 1;
        }


        /// <summary>
        /// 关闭命令
        /// </summary>
        [RelayCommand]
        public void CloseClick(object obj)
        {

        }

        /// <summary>
        /// 新增数据
        /// </summary>
        [RelayCommand]
        public void AddClick(object obj)
        {
            this.UserModelEdit = new UserModel();
            WeakReferenceMessenger.Default.Send(new UserFormMessage()
            {
                CRUD = CRUD.Add,
                ViewModel = this,
                FormData = this.UserModelEdit
            });
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        [RelayCommand]
        public void DeleteClick(object obj)
        {
            if (obj is UserModel userModel)
            {
                this.UserModelList.Remove(userModel);
            }
        }

        public UserModel MyProperty { get; set; }

        /// <summary>
        /// 修改数据
        /// </summary>
        [RelayCommand]
        public void UpdateClick(object obj)
        {
            if (this.UserModelSelect == null)
            {
                return;
            }

            this.UserModelEdit = this.UserModelSelect.Copy();
            WeakReferenceMessenger.Default.Send(new UserFormMessage()
            {
                CRUD = CRUD.Update,
                ViewModel = this,
                FormData = this.UserModelEdit
            });
        }

        /// <summary>
        /// 查看数据
        /// </summary>
        [RelayCommand]
        public void DetailsClick(object obj)
        {

        }


        /// <summary>
        /// 导出数据
        /// </summary>
        [RelayCommand]
        public void ExportClick(object obj)
        {

        }

        private bool CanPaginationAsync(Pagination pagination)
        {
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanPaginationAsync))]
        public async Task PaginationAsync(Pagination pagination)
        {
            // 发送开始加载消息
            WeakReferenceMessenger.Default.Send(new UserLoadingMessage
            {
                LoadingFuncAsync = async (config) =>
                {
                    //config.LoadingTextStringFormat = "当前加载进度 {0}%";
                    //config.IsShowLoadingText = true;
                    //for (var i = 0; i < 100; i++)
                    //{
                    //    config.Value = Math.Round(i / 100D * 100, 2);
                    //    config.LoadingText = $"当前加载进度 {config.Value}%";
                    //    await Task.Delay(2);
                    //}
                    pagination.Records = this.TestData.Count();
                    var pageList = this.TestData.Skip((pagination.Page - 1) * (pagination.Rows)).Take(pagination.Rows).ToList();
                    // 回到UI线程更新集合
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.UserModelList = new ObservableCollection<UserModel>(pageList);
                    });
                    return OperateResult.CreateSuccessResult();
                }
            });
        }

        private ObservableCollection<UserModel> userModelList;
        /// <summary>
        /// 用户数据
        /// </summary>
        public ObservableCollection<UserModel> UserModelList
        {
            get
            {
                if (userModelList == null)
                {
                    userModelList = new ObservableCollection<UserModel>();
                }
                return userModelList;
            }
            set
            {
                this.SetProperty(ref userModelList, value);
            }
        }

        /// <summary>
        /// 编辑实体
        /// </summary>
        [ObservableProperty]
        private UserModel userModelEdit;

       

        private UserModel userModelSelect;

        /// <summary>
        /// 选中实体
        /// </summary>
        public UserModel UserModelSelect
        {
            get { return userModelSelect; }
            set
            {
                this.SetProperty(ref userModelSelect,value);
            }
        }
      
    }
}
