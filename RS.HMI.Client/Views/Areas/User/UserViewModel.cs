using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IdGen;
using Microsoft.Extensions.DependencyInjection;
using NPOI.Util;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Enums;
using RS.Commons.Extensions;
using RS.HMI.Client.Messages;
using RS.HMI.Client.Models;
using RS.RESTfulApi;
using RS.Widgets.Controls;
using RS.Widgets.Interface;
using RS.Widgets.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace RS.HMI.Client.Views.Areas
{
    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public partial class UserViewModel : ViewModelBase
    {
        private readonly IIdGenerator<long> IdGenerator;

        /// <summary>
        /// 默认构造方法
        /// </summary>
        /// <param name="idGenerator"></param>
        public UserViewModel(IIdGenerator<long> idGenerator)
        {
            this.IdGenerator = idGenerator;
        }


        #region 依赖属性

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


        /// <summary>
        /// 选中实体
        /// </summary>
        [ObservableProperty]
        private UserModel userModelSelect;
        #endregion

        #region 命令

        /// <summary>
        /// 查询命令
        /// </summary>
        [RelayCommand]
        public async Task SearchClick(object obj)
        {
            LoadingConfig loadingConfig = new LoadingConfig();
            var operateResult = await this.Dialog.GetLoading().InvokeLoadingActionAsync(async (cancellationToken) =>
            {
               await Task.Delay(2000);
                return OperateResult.CreateSuccessResult();
            }, loadingConfig: loadingConfig);

            if (!operateResult.IsSuccess)
            {
                await this.Dialog.GetMessageBox().ShowMessageAsync(operateResult.Message, "错误提示");
            }
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
        public async Task AddClick(object obj)
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
            if (this.UserModelSelect != null)
            {
                this.UserModelList.Remove(this.UserModelSelect);
            }

        }

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
            LoadingConfig loadingConfig = new LoadingConfig();
            var operateResult = await this.Dialog.GetLoading().InvokeLoadingActionAsync(async (cancellationToken) =>
               {
                   var dataResult = await RSAppAPI.User.GetUser.AESHttpPostAsync<Pagination, RS.Models.PageDataModel<UserModel>>(pagination, nameof(RSAppAPI));
                   if (!dataResult.IsSuccess)
                   {
                       return dataResult;
                   }
                   var pageDataModel = dataResult.Data;
                   pagination.Records = pageDataModel.Pagination.records;

                   var pageList = pageDataModel.DataList;

                   //回到UI线程更新集合
                   Application.Current.Dispatcher.Invoke(() =>
                   {
                       this.UserModelList = new ObservableCollection<UserModel>(pageList);
                   });
                   return OperateResult.CreateSuccessResult();
               }, loadingConfig: loadingConfig);

            if (!operateResult.IsSuccess)
            {
                await this.Dialog.GetMessageBox().ShowMessageAsync(operateResult.Message,"错误提示");
            }
        }


        /// <summary>
        /// 继承基类新增
        /// </summary>
        public override async Task Submit(object obj)
        {
            if (obj is UserModel userModel)
            {
                await this.Dialog.GetLoading().InvokeLoadingActionAsync(async (cancellationToken) =>
                {
                    //在这里向WebAPI发起请求提交数据
                    var sumitResult = await RSAppAPI.User.GetUser.AESHttpPostAsync(userModel, nameof(RSAppAPI));
                    if (!sumitResult.IsSuccess)
                    {
                        return sumitResult;
                    }

                    return OperateResult.CreateSuccessResult();
                });
            }
        }

        /// <summary>
        /// 继承基类更新
        /// </summary>
        public override async Task Update(object userModel)
        {
            var sdf = 1;
            await Task.Delay(3000);
        }

        [RelayCommand]
        public async Task UserEnableClick(object obj)
        {
            var sdf = 1;
            await Task.Delay(3000);
        }


        [RelayCommand]
        public async Task UserDisableClick(object obj)
        {
            var sdf = 1;
            await Task.Delay(3000);
        }

        #endregion

    }
}
