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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace RS.HMI.Client.Views.Areas
{
    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public partial class RoleViewModel : ViewModelBase
    {
        private readonly IIdGenerator<long> IdGenerator;

        /// <summary>
        /// 默认构造方法
        /// </summary>
        /// <param name="idGenerator"></param>
        public RoleViewModel(IIdGenerator<long> idGenerator)
        {
            this.IdGenerator = idGenerator;
        }

        #region 依赖属性
       

        private ObservableCollection<RoleModel> roleModelList;
        /// <summary>
        /// 角色数据
        /// </summary>
        public ObservableCollection<RoleModel> RoleModelList
        {
            get
            {
                if (roleModelList == null)
                {
                    roleModelList = new ObservableCollection<RoleModel>();
                }
                return roleModelList;
            }
            set
            {
                this.SetProperty(ref roleModelList, value);
            }
        }

        /// <summary>
        /// 编辑实体
        /// </summary>
        [ObservableProperty]
        private RoleModel roleModelEdit;


        /// <summary>
        /// 选中实体
        /// </summary>
        [ObservableProperty]
        private RoleModel roleModelSelect;
        #endregion

        #region 命令
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
            this.RoleModelEdit = new RoleModel();
            WeakReferenceMessenger.Default.Send(new RoleFormMessage()
            {
                CRUD = CRUD.Add,
                ViewModel = this,
                FormData = this.RoleModelEdit
            });
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        [RelayCommand]
        public void DeleteClick(object obj)
        {
            if (this.RoleModelSelect != null)
            {
                this.RoleModelList.Remove(this.RoleModelSelect);
            }
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        [RelayCommand]
        public void UpdateClick(object obj)
        {
            if (this.RoleModelSelect == null)
            {
                return;
            }

            this.RoleModelEdit = this.RoleModelSelect.Copy();
            WeakReferenceMessenger.Default.Send(new RoleFormMessage()
            {
                CRUD = CRUD.Update,
                ViewModel = this,
                FormData = this.RoleModelEdit
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
            await this.Dialog.GetLoading().InvokeLoadingActionAsync(async (cancellationToken) =>
            {
               
                //pagination.Records = this.TestData.Count();
                //var pageList = this.TestData.Skip((pagination.Page - 1) * (pagination.Rows)).Take(pagination.Rows).ToList();
                //// 回到UI线程更新集合
                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    this.UserModelList = new ObservableCollection<UserModel>(pageList);
                //});
                return OperateResult.CreateSuccessResult();
            }, loadingConfig: loadingConfig);

            //await this.Dialog.GetWinMessageBox().ShowMessageAsync("数据加载成功");
        }


        /// <summary>
        /// 继承基类新增
        /// </summary>
        public override async Task Submit(object obj)
        {
            if (obj is RoleModel roleModel)
            {
                await this.Dialog.GetLoading().InvokeLoadingActionAsync(async (cancellationToken) =>
                {
                    //在这里向WebAPI发起请求提交数据
                    var sumitResult = await RSAppAPI.User.GetUser.AESHttpPostAsync(roleModel, nameof(RSAppAPI));
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
