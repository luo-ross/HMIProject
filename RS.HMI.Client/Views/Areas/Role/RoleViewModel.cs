using IdGen;
using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMI.Client.Models;
using RS.RESTfulApi;
using RS.Widgets.Models;
using RS.Widgets.Models.Form;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tensorflow;

namespace RS.HMI.Client.Views.Areas
{
    /// <summary>
    /// 角色管理视图模型
    /// </summary>
    [ServiceInjectConfig(ServiceLifetime.Scoped)]
    public class RoleViewModel : CRUDViewModel<RoleModel>
    {
        private readonly IIdGenerator<long> IdGenerator;

        /// <summary>
        /// 默认构造方法
        /// </summary>
        /// <param name="idGenerator"></param>
        public RoleViewModel(IIdGenerator<long> idGenerator)
        {
            this.IdGenerator = idGenerator;

            //添加测试数据
            this.PropertyList = new ObservableCollection<PropertyBase>();

            this.PropertyList.Add(new PropertyBase()
            {
                Description = "角色名称1",
            });
            this.PropertyList.Add(new PropertyBase()
            {
                Description = "角色名称2",
            });
            this.PropertyList.Add(new PropertyBase()
            {
                Description = "角色名称3",
            });
            this.PropertyList.Add(new PropertyBase()
            {
                Description = "角色名称4",
            });
            this.PropertyList.Add(new PropertyBase()
            {
                Description = "角色名称5",
            });
            this.PropertyList.Add(new PropertyBase()
            {
                Description = "角色名称6",
            });

            this.PropertyList.Add(new ComboxProperty()
            {
                Description = "所属公司",
                DataSource = CompanyList,
                SelectedValuePath = nameof(ComboBoxItemModel<>.Key),
                DisplayMemberPath = nameof(ComboBoxItemModel<>.KeyDes)
            });

            this.PropertyList.Add(new DateTimeProperty()
            {
                Description = "创建时间",
            });

            var descriptionList = this.PropertyList.Select(t => t.Description).ToList();
            var descriptionWidth = PropertyBase.GetMaxStringWidth(descriptionList, 12);
            foreach (var property in this.PropertyList)
            {
                property.DescriptionWidth = new GridLength(descriptionWidth);
            }
        }





        private ObservableCollection<object> companyList;
        /// <summary>
        /// 公司列表
        /// </summary>
        public ObservableCollection<object> CompanyList
        {
            get
            {
                if (companyList == null)
                {
                  
                   var list= new List<ComboBoxItemModel<object>>();
                    for (var i = 0; i < 3; i++)
                    {
                        list.Add(new ComboBoxItemModel<object>()
                        {
                            KeyDes = $"公司{i+1}",
                            Key = i,
                        });
                    }

                    companyList = new ObservableCollection<object> (list);

                }
                return companyList;
            }
            set
            {
                this.SetProperty(ref companyList, value);
            }
        }



        private ObservableCollection<PropertyBase> propertyList;
        /// <summary>
        /// 数据列表
        /// </summary>
        public ObservableCollection<PropertyBase> PropertyList
        {
            get { return propertyList; }
            set
            {
                this.SetProperty(ref propertyList, value);
            }
        }


        private DateTime? dateTimeSelect;
        /// <summary>
        /// 日期选择
        /// </summary>
        public DateTime? DateTimeSelect
        {
            get { return dateTimeSelect; }
            set
            {
                this.SetProperty(ref dateTimeSelect, value);
            }
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

        public override async Task PaginationAsync(Pagination pagination)
        {
            LoadingConfig loadingConfig = new LoadingConfig();
            var operateResult = await this.Loading.InvokeAsync(async (cancellationToken) =>
            {
                var dataResult = await RSAppAPI.Role.GetRole.AESHttpPostAsync<Pagination, RS.Models.PageDataModel<RoleModel>>(pagination, nameof(RSAppAPI));
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
                    this.ModelList = new ObservableCollection<RoleModel>(pageList);
                });
                return OperateResult.CreateSuccessResult();
            }, loadingConfig: loadingConfig);

            if (!operateResult.IsSuccess)
            {
                await this.MessageBox.ShowMessageAsync(operateResult.Message, "错误提示");
            }
        }


        /// <summary>
        /// 继承基类新增
        /// </summary>
        public async override Task OnFormSubmitAsync(RoleModel modelEidt)
        {
            //在这里向WebAPI发起请求提交数据
            var sumitResult = await RSAppAPI.Role.AddRole.AESHttpPostAsync(modelEidt, nameof(RSAppAPI));
            if (!sumitResult.IsSuccess)
            {

            }
        }


        #endregion

    }
}
