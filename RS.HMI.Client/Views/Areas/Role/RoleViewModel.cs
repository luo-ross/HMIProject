using IdGen;
using Microsoft.Extensions.DependencyInjection;
using NPOI.Util;
using RS.Commons;
using RS.Commons.Attributs;
using RS.HMI.Client.Models;
using RS.Widgets.Models;

namespace RS.HMI.Client.Views.Areas
{
    /// <summary>
    /// 角色管理视图模型
    /// </summary>
    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public  class RoleViewModel : CRUDViewModel<RoleModel>
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

       
        #region 命令
        /// <summary>
        /// 关闭命令
        /// </summary>
        public void CloseClick(object obj)
        {

        }

        /// <summary>
        /// 新增数据
        /// </summary>
        public async Task AddClick(object obj)
        {
            this.ModelEdit = new RoleModel();
            //WeakReferenceMessenger.Default.Send(new RoleFormMessage()
            //{
            //    CRUD = CRUD.Add,
            //    ViewModel = this,
            //    FormData = this.ModelEdit
            //});
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        public void DeleteClick(object obj)
        {
            if (this.ModelSelect != null)
            {
                this.ModelList.Remove(this.ModelSelect);
            }
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        public void UpdateClick(object obj)
        {
            if (this.ModelSelect == null)
            {
                return;
            }

            this.ModelEdit = this.ModelSelect.Copy();
            //WeakReferenceMessenger.Default.Send(new RoleFormMessage()
            //{
            //    CRUD = CRUD.Update,
            //    ViewModel = this,
            //    FormData = this.ModelEdit
            //});
        }

        /// <summary>
        /// 查看数据
        /// </summary>
        public void DetailsClick(object obj)
        {

        }


        /// <summary>
        /// 导出数据
        /// </summary>
        public void ExportClick(object obj)
        {

        }

        private bool CanPaginationAsync(Pagination pagination)
        {
            return true;
        }

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
        public override async void FormSubmitClick()
        {
            //if (obj is RoleModel roleModel)
            //{
            //    await this.Dialog.GetLoading().InvokeLoadingActionAsync(async (cancellationToken) =>
            //    {
            //        //在这里向WebAPI发起请求提交数据
            //        var sumitResult = await RSAppAPI.User.GetUser.AESHttpPostAsync(roleModel, nameof(RSAppAPI));
            //        if (!sumitResult.IsSuccess)
            //        {
            //            return sumitResult;
            //        }
            //        return OperateResult.CreateSuccessResult();
            //    });
            //}
        }

        #endregion

    }
}
