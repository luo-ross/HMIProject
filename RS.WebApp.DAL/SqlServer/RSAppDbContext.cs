using Microsoft.EntityFrameworkCore;
using RS.WebApp.Entity;
using System.Reflection;

namespace RS.WebApp.DAL.SqlServer
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    internal class RSAppDbContext : DbContext
    {
        public RSAppDbContext(DbContextOptions<RSAppDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        /// <summary>
        /// 区域
        /// </summary>
        public virtual DbSet<AreaEntity> Area { get; set; }


        /// <summary>
        /// 数据列表权限
        /// </summary>
        public virtual DbSet<ColPermissionEntity> ColPermission { get; set; }

        /// <summary>
        /// 公司
        /// </summary>
        public virtual DbSet<CompanyEntity> Company { get; set; }

      
        /// <summary>
        /// 公司资料
        /// </summary>
        public virtual DbSet<CompanyProfileEntity> CompanyProfile { get; set; }

        /// <summary>
        /// 国家信息
        /// </summary>
        public virtual DbSet<CountryEntity> Country { get; set; }

        /// <summary>
        /// 部门信息
        /// </summary>
        public virtual DbSet<DepartmentEntity> Department { get; set; }

        /// <summary>
        /// 邮箱信息
        /// </summary>
        public virtual DbSet<EmailInfoEntity> EmailInfo { get; set; }
   

        /// <summary>
        /// 登录信息
        /// </summary>
        public virtual DbSet<LogOnEntity> LogOn { get; set; }

    
     
        /// <summary>
        /// 电话信息
        /// </summary>
        public virtual DbSet<PhoneInfoEntity> PhoneInfo { get; set; }

      
        /// <summary>
        /// 公司实名认证
        /// </summary>
        public virtual DbSet<RealCompanyEntity> RealCompany { get; set; }

        /// <summary>
        /// 人员实名认证
        /// </summary>
        public virtual DbSet<RealNameEntity> RealName { get; set; }

        /// <summary>
        /// 角色信息
        /// </summary>
        public virtual DbSet<RoleEntity> Role { get; set; }

        /// <summary>
        /// 角色权限绑定
        /// </summary>
        public virtual DbSet<RolePermissionMapEntity> RolePermissionMap { get; set; }

        /// <summary>
        /// 系统菜单按钮权限
        /// </summary>
        public virtual DbSet<SystemPermissionEntity> SystemPermission { get; set; }

        /// <summary>
        /// 数据表权限
        /// </summary>
        public virtual DbSet<TablePermissionEntity> TablePermission { get; set; }

        /// <summary>
        /// 第三方登录信息
        /// </summary>
        public virtual DbSet<ThirdPartyLogOnEntity> ThirdPartyLogOn { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public virtual DbSet<UserEntity> User { get; set; }

        /// <summary>
        /// 实体创建方法覆盖
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var typeList = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "RS.WebApp.DAL.Mapping" && t.ReflectedType == null);
            foreach (var type in typeList)
            {
                dynamic? entityMapping = Activator.CreateInstance(type);
                if (entityMapping != null)
                {
                    modelBuilder.ApplyConfiguration(entityMapping);
                }
            }
            base.OnModelCreating(modelBuilder);
        }



    }
}
