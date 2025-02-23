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
        /// 银行卡绑定
        /// </summary>
        public virtual DbSet<BankCardMapEntity> BankCardMap { get; set; }

        /// <summary>
        /// 账单
        /// </summary>
        public virtual DbSet<BillsEntity> Bills { get; set; }

        /// <summary>
        /// 客人证件信息
        /// </summary>
        public virtual DbSet<CardInfoEntity> CardInfo { get; set; }

        /// <summary>
        /// 数据列表权限
        /// </summary>
        public virtual DbSet<ColPermissionEntity> ColPermission { get; set; }

        /// <summary>
        /// 公司
        /// </summary>
        public virtual DbSet<CompanyEntity> Company { get; set; }

        /// <summary>
        /// 公司价格配置
        /// </summary>
        public virtual DbSet<CompanyPriceConfigEntity> CompanyPriceConfig { get; set; }

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
        /// 团队价格配置
        /// </summary>
        public virtual DbSet<GroupPriceConfigEntity> GroupPriceConfig { get; set; }

        /// <summary>
        /// 团队资料
        /// </summary>
        public virtual DbSet<GroupProfileEntity> GroupProfile { get; set; }

        /// <summary>
        /// 客人资料
        /// </summary>
        public virtual DbSet<GuestProfileEntity> GuestProfile { get; set; }


        /// <summary>
        /// 散客价格配置
        /// </summary>
        public virtual DbSet<IndividualPriceConfigEntity> IndividualPriceConfig { get; set; }

        /// <summary>
        /// 开票信息
        /// </summary>
        public virtual DbSet<InvoiceInfoEntity> InvoiceInfo { get; set; }

        /// <summary>
        /// 发票信息
        /// </summary>
        public virtual DbSet<InvoicesEntity> Invoices { get; set; }

        /// <summary>
        /// 登录信息
        /// </summary>
        public virtual DbSet<LogOnEntity> LogOn { get; set; }

        /// <summary>
        /// 会员信息
        /// </summary>
        public virtual DbSet<MemberInfoEntity> MemberInfo { get; set; }

        /// <summary>
        /// 付款数据
        /// </summary>
        public virtual DbSet<PaymentsEntity> Payments { get; set; }

        /// <summary>
        /// 电话信息
        /// </summary>
        public virtual DbSet<PhoneInfoEntity> PhoneInfo { get; set; }

        /// <summary>
        /// 入账代码
        /// </summary>
        public virtual DbSet<PostCodeEntity> PostCode { get; set; }

        /// <summary>
        /// 入账代码合并详情
        /// </summary>
        public virtual DbSet<PostCodeMergeEntity> PostCodeMerge { get; set; }

        /// <summary>
        /// 公司实名认证
        /// </summary>
        public virtual DbSet<RealCompanyEntity> RealCompany { get; set; }

        /// <summary>
        /// 人员实名认证
        /// </summary>
        public virtual DbSet<RealNameEntity> RealName { get; set; }

        /// <summary>
        /// 预定信息
        /// </summary>
        public virtual DbSet<ReservationsEntity> Reservations { get; set; }

        /// <summary>
        /// 角色信息
        /// </summary>
        public virtual DbSet<RoleEntity> Role { get; set; }

        /// <summary>
        /// 角色权限绑定
        /// </summary>
        public virtual DbSet<RolePermissionMapEntity> RolePermissionMap { get; set; }

        /// <summary>
        /// 房间信息
        /// </summary>
        public virtual DbSet<RoomInfoEntity> RoomInfo { get; set; }

        /// <summary>
        /// 房价信息
        /// </summary>
        public virtual DbSet<RoomRateInfoEntity> RoomRateInfo { get; set; }

        /// <summary>
        /// 房间状态信息
        /// </summary>
        public virtual DbSet<RoomStatusInfoEntity> RoomStatusInfo { get; set; }

        /// <summary>
        /// 房型
        /// </summary>
        public virtual DbSet<RoomTypeEntity> RoomType { get; set; }

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


        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Area>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("省市区级联"));

        //        entity.Property(e => e.Id)
        //            .HasMaxLength(50)
        //            .HasComment("主键");
        //        entity.Property(e => e.Level).HasComment("级别");
        //        entity.Property(e => e.Name)
        //            .IsRequired()
        //            .HasMaxLength(50)
        //            .HasComment("名称");
        //        entity.Property(e => e.ParentId)
        //            .IsRequired()
        //            .HasMaxLength(50)
        //            .HasDefaultValueSql("((0))")
        //            .HasComment("父级");
        //    });

        //    modelBuilder.Entity<BankCardMap>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("银行卡信息"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.CardNo)
        //            .HasMaxLength(50)
        //            .HasComment("银行卡号");
        //        entity.Property(e => e.PaymentId)
        //            .HasMaxLength(50)
        //            .HasComment("付费绑定");

        //        entity.HasOne(d => d.Payment).WithMany(p => p.BankCardMap)
        //            .HasForeignKey(d => d.PaymentId)
        //            .HasConstraintName("FK_BankCardMap_PaymentId_Payments_Id");
        //    });

        //    modelBuilder.Entity<Bills>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("个人账单数据"));

        //        entity.Property(e => e.Id)
        //            .HasMaxLength(50)
        //            .HasComment("主键");
        //        entity.Property(e => e.Description)
        //            .HasMaxLength(200)
        //            .HasComment("描述");
        //        entity.Property(e => e.Group).HasComment("所属分组");
        //        entity.Property(e => e.Money)
        //            .HasComment("金额")
        //            .HasColumnType("decimal(18, 2)");
        //        entity.Property(e => e.PostCode)
        //            .HasMaxLength(50)
        //            .HasComment("入账代码");
        //        entity.Property(e => e.PostDate).HasComment("入账日期");
        //        entity.Property(e => e.ReservationId)
        //            .HasMaxLength(50)
        //            .HasComment("预定绑定");

        //        entity.HasOne(d => d.PostCodeNavigation).WithMany(p => p.Bills)
        //            .HasForeignKey(d => d.PostCode)
        //            .HasConstraintName("FK_Bills_PostCode_PostCode_Code");

        //        entity.HasOne(d => d.Reservation).WithMany(p => p.Bills)
        //            .HasForeignKey(d => d.ReservationId)
        //            .HasConstraintName("FK_Bills_ReservationId_Reservations_Id");
        //    });

        //    modelBuilder.Entity<CardInfo>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("客人证件信息"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.CardNo)
        //            .HasMaxLength(50)
        //            .HasComment("证件号");
        //        entity.Property(e => e.GuestId).HasMaxLength(50);
        //        entity.Property(e => e.Type).HasComment("证件类型");

        //        entity.HasOne(d => d.Guest).WithMany(p => p.CardInfo)
        //            .HasForeignKey(d => d.GuestId)
        //            .HasConstraintName("FK_CardInfo_GuestId_GuestProfile_Id");
        //    });

        //    modelBuilder.Entity<ColPermission>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("数据表列权限"));

        //        entity.Property(e => e.Id)
        //            .HasMaxLength(50)
        //            .HasComment("主键");
        //        entity.Property(e => e.Description)
        //            .HasMaxLength(50)
        //            .HasComment("描述");
        //        entity.Property(e => e.Name)
        //            .HasMaxLength(50)
        //            .HasComment("名称");
        //        entity.Property(e => e.ParentId)
        //            .HasMaxLength(50)
        //            .HasDefaultValueSql("((0))")
        //            .HasComment("父级");
        //        entity.Property(e => e.Sort)
        //            .HasDefaultValueSql("((0))")
        //            .HasComment("排序");
        //    });

        //    modelBuilder.Entity<Company>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("公司"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.Area)
        //            .HasMaxLength(50)
        //            .HasComment("所在地区");
        //        entity.Property(e => e.Name)
        //            .HasMaxLength(100)
        //            .HasComment("公司名称");
        //        entity.Property(e => e.RealCompanyId)
        //            .HasMaxLength(50)
        //            .HasComment("关联公司认证");
        //        entity.Property(e => e.UserId).HasMaxLength(50);

        //        entity.HasOne(d => d.RealCompany).WithMany(p => p.Company)
        //            .HasForeignKey(d => d.RealCompanyId)
        //            .HasConstraintName("FK_Company_Id_RealCompany_RealCompanyId");

        //        entity.HasOne(d => d.User).WithMany(p => p.Company)
        //            .HasForeignKey(d => d.UserId)
        //            .HasConstraintName("FK_User_Id_Company_UserId");
        //    });

        //    modelBuilder.Entity<CompanyPriceConfig>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("公司房价配置"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.CompanyId)
        //            .HasMaxLength(50)
        //            .HasComment("公司资料");
        //        entity.Property(e => e.Date).HasComment("房价日期");
        //        entity.Property(e => e.PostCode)
        //            .HasMaxLength(50)
        //            .HasComment("入账代码 就是房价");
        //        entity.Property(e => e.RoomType)
        //            .HasMaxLength(50)
        //            .HasComment("房型代码");

        //        entity.HasOne(d => d.Company).WithMany(p => p.CompanyPriceConfig)
        //            .HasForeignKey(d => d.CompanyId)
        //            .HasConstraintName("FK_CompanyPriceConfig_CompanyId_CompanyProfile_Id");

        //        entity.HasOne(d => d.PostCodeNavigation).WithMany(p => p.CompanyPriceConfig)
        //            .HasForeignKey(d => d.PostCode)
        //            .HasConstraintName("FK_CompanyPriceConfig_PostCode_PostCode_Code");

        //        entity.HasOne(d => d.RoomTypeNavigation).WithMany(p => p.CompanyPriceConfig)
        //            .HasForeignKey(d => d.RoomType)
        //            .HasConstraintName("FK_CompanyPriceConfig_RoomType_RoomType_Code");
        //    });

        //    modelBuilder.Entity<CompanyProfile>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("公司资料"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.Address)
        //            .HasMaxLength(200)
        //            .HasComment("公司地址");
        //        entity.Property(e => e.ChName)
        //            .HasMaxLength(50)
        //            .HasComment("中文名称");
        //        entity.Property(e => e.Code)
        //            .HasMaxLength(50)
        //            .HasComment("统一社会信用代码");
        //        entity.Property(e => e.CreatorId).HasMaxLength(50);
        //        entity.Property(e => e.Email).HasMaxLength(100);
        //        entity.Property(e => e.EnName)
        //            .HasMaxLength(50)
        //            .HasComment("英文名称");
        //        entity.Property(e => e.LegalPerson)
        //            .HasMaxLength(10)
        //            .IsFixedLength()
        //            .HasComment("法定代表人");
        //        entity.Property(e => e.Phone).HasMaxLength(50);
        //        entity.Property(e => e.RegisterTime).HasComment("注册时间");
        //        entity.Property(e => e.RegisteredCapital)
        //            .HasComment("注册资本")
        //            .HasColumnType("decimal(18, 4)");
        //        entity.Property(e => e.WebSite)
        //            .HasMaxLength(10)
        //            .IsFixedLength()
        //            .HasComment("公司网站");

        //        entity.HasOne(d => d.Creator).WithMany(p => p.CompanyProfile)
        //            .HasForeignKey(d => d.CreatorId)
        //            .HasConstraintName("FK_CompanyProfile_CreatorId_User_Id");
        //    });

        //    modelBuilder.Entity<Country>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("国家信息"));

        //        entity.Property(e => e.Id)
        //            .HasMaxLength(50)
        //            .HasComment("主键");
        //        entity.Property(e => e.Abbr)
        //            .HasMaxLength(50)
        //            .HasComment("缩写");
        //        entity.Property(e => e.ChName)
        //            .HasMaxLength(100)
        //            .HasComment("中文名称");
        //        entity.Property(e => e.EnName)
        //            .HasMaxLength(100)
        //            .HasComment("英文名称");
        //        entity.Property(e => e.PhoneCode)
        //            .HasMaxLength(50)
        //            .HasComment("电话代码");
        //    });

        //    modelBuilder.Entity<Department>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("部门表"));

        //        entity.Property(e => e.Id)
        //            .HasMaxLength(50)
        //            .HasComment("主键");
        //        entity.Property(e => e.Abbr)
        //            .HasMaxLength(50)
        //            .HasComment("缩写简称");
        //        entity.Property(e => e.ChName)
        //            .HasMaxLength(50)
        //            .HasComment("中文名称");
        //        entity.Property(e => e.CompanyId).HasMaxLength(50);
        //        entity.Property(e => e.EnName)
        //            .HasMaxLength(50)
        //            .HasComment("英文名称");

        //        entity.HasOne(d => d.Company).WithMany(p => p.Department)
        //            .HasForeignKey(d => d.CompanyId)
        //            .HasConstraintName("FK_Department_CompanyId_Company_Id");
        //    });

        //    modelBuilder.Entity<EmailInfo>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("邮箱绑定"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.Address)
        //            .HasMaxLength(100)
        //            .HasComment("邮箱地址");
        //        entity.Property(e => e.GuestId)
        //            .HasMaxLength(50)
        //            .HasComment("客人资料主键");

        //        entity.HasOne(d => d.Guest).WithMany(p => p.EmailInfo)
        //            .HasForeignKey(d => d.GuestId)
        //            .HasConstraintName("FK_EmailInfo_GuestId_GuestProfile_Id");
        //    });

        //    modelBuilder.Entity<GroupPriceConfig>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("团队价格配置"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.Date).HasComment("房价日期");
        //        entity.Property(e => e.GroupId)
        //            .HasMaxLength(50)
        //            .HasComment("所胡团队");
        //        entity.Property(e => e.PostCode)
        //            .HasMaxLength(50)
        //            .HasComment("入账代码 就是房价");
        //        entity.Property(e => e.RoomType)
        //            .HasMaxLength(50)
        //            .HasComment("房间类型的代码");

        //        entity.HasOne(d => d.Group).WithMany(p => p.GroupPriceConfig)
        //            .HasForeignKey(d => d.GroupId)
        //            .HasConstraintName("FK_GroupPriceConfig_GroupId_GroupProfile_Id");
        //    });

        //    modelBuilder.Entity<GroupProfile>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("团队资料"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.ChName)
        //            .HasMaxLength(50)
        //            .HasComment("中文名称");
        //        entity.Property(e => e.CreatorId).HasMaxLength(50);
        //        entity.Property(e => e.EnName)
        //            .HasMaxLength(50)
        //            .HasComment("英文名称");

        //        entity.HasOne(d => d.Creator).WithMany(p => p.GroupProfile)
        //            .HasForeignKey(d => d.CreatorId)
        //            .HasConstraintName("FK_GroupProfile_CreatorId_User_Id");
        //    });

        //    modelBuilder.Entity<GuestProfile>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("客人资料"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.BirthDay).HasComment("出身日期");
        //        entity.Property(e => e.ChName)
        //            .HasMaxLength(50)
        //            .HasComment("中文姓名");
        //        entity.Property(e => e.CreateTime).HasComment("创建日期");
        //        entity.Property(e => e.CreatorId)
        //            .HasMaxLength(50)
        //            .HasComment("创建人");
        //        entity.Property(e => e.EnName)
        //            .HasMaxLength(50)
        //            .HasComment("英文姓名");
        //        entity.Property(e => e.Gender).HasComment("性别");
        //        entity.Property(e => e.GuestPic)
        //            .HasMaxLength(200)
        //            .HasComment("客人头像");

        //        entity.HasOne(d => d.Creator).WithMany(p => p.GuestProfile)
        //            .HasForeignKey(d => d.CreatorId)
        //            .HasConstraintName("FK_GuestProfile_CreatorId_User_Id");

        //        entity.HasMany(d => d.Company).WithMany(p => p.Guest)
        //            .UsingEntity<Dictionary<string, object>>(
        //                "CompanyContactMap",
        //                r => r.HasOne<CompanyProfile>().WithMany()
        //                    .HasForeignKey("CompanyId")
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_CompanyContactMap_CompanyId_CompanyProfile_Id"),
        //                l => l.HasOne<GuestProfile>().WithMany()
        //                    .HasForeignKey("GuestId")
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_CompanyContactMap_GuestId_GuestProfile_Id"),
        //                j =>
        //                {
        //                    j.HasKey("GuestId", "CompanyId");
        //                    j.ToTable(tb => tb.HasComment("公司联系人绑定"));
        //                    j.IndexerProperty<string>("GuestId")
        //                        .HasMaxLength(50)
        //                        .HasComment("客人资料");
        //                    j.IndexerProperty<string>("CompanyId")
        //                        .HasMaxLength(50)
        //                        .HasComment("公司资料");
        //                });

        //        entity.HasMany(d => d.Reservation).WithMany(p => p.Guest)
        //            .UsingEntity<Dictionary<string, object>>(
        //                "ReservationGuestMap",
        //                r => r.HasOne<Reservations>().WithMany()
        //                    .HasForeignKey("ReservationId")
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_ReservationGuestMap_ReservationId_Reservations_Id"),
        //                l => l.HasOne<GuestProfile>().WithMany()
        //                    .HasForeignKey("GuestId")
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_ReservationGuestMap_GuestId_GuestProfile_Id"),
        //                j =>
        //                {
        //                    j.HasKey("GuestId", "ReservationId");
        //                    j.ToTable(tb => tb.HasComment("预定入住客人映射"));
        //                    j.IndexerProperty<string>("GuestId").HasMaxLength(50);
        //                    j.IndexerProperty<string>("ReservationId").HasMaxLength(50);
        //                });
        //    });

        //    modelBuilder.Entity<IndividualPriceConfig>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("散客房价配置"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.Date).HasComment("日期");
        //        entity.Property(e => e.PostCode)
        //            .HasMaxLength(50)
        //            .HasComment("入账代码 就是房价");
        //        entity.Property(e => e.RoomType)
        //            .HasMaxLength(50)
        //            .HasComment("房型 关联房型Code");

        //        entity.HasOne(d => d.PostCodeNavigation).WithMany(p => p.IndividualPriceConfig)
        //            .HasForeignKey(d => d.PostCode)
        //            .HasConstraintName("FK_IndividualPriceConfig_PostCode_PostCode_Code");

        //        entity.HasOne(d => d.RoomTypeNavigation).WithMany(p => p.IndividualPriceConfig)
        //            .HasForeignKey(d => d.RoomType)
        //            .HasConstraintName("FK_IndividualPriceConfig_RoomType_RoomType_Code");
        //    });

        //    modelBuilder.Entity<InvoiceInfo>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("发票信息"));

        //        entity.Property(e => e.Id)
        //            .HasMaxLength(50)
        //            .HasComment("主键");
        //        entity.Property(e => e.Account)
        //            .HasMaxLength(50)
        //            .HasComment("开户账号");
        //        entity.Property(e => e.Bank)
        //            .HasMaxLength(100)
        //            .HasComment("开户银行");
        //        entity.Property(e => e.CompanyId)
        //            .HasMaxLength(50)
        //            .HasComment("公司资料");

        //        entity.HasOne(d => d.Company).WithMany(p => p.InvoiceInfo)
        //            .HasForeignKey(d => d.CompanyId)
        //            .HasConstraintName("FK_InvoiceInfo_CompanyId_CompanyProfile_Id");
        //    });

        //    modelBuilder.Entity<Invoices>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("发票"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.BillId).HasMaxLength(50);
        //        entity.Property(e => e.CheckCode)
        //            .HasMaxLength(200)
        //            .HasComment("校验码");
        //        entity.Property(e => e.Code)
        //            .HasMaxLength(50)
        //            .HasComment("发票代码");
        //        entity.Property(e => e.Date).HasComment("开票日期");
        //        entity.Property(e => e.IssuedbyId).HasMaxLength(50);
        //        entity.Property(e => e.Number)
        //            .HasMaxLength(50)
        //            .HasComment("发票号");
        //        entity.Property(e => e.PayeeId).HasMaxLength(50);
        //        entity.Property(e => e.ReviewerId).HasMaxLength(50);
        //        entity.Property(e => e.Serial)
        //            .IsRequired()
        //            .HasMaxLength(50)
        //            .HasComment("发票编号");

        //        entity.HasOne(d => d.Issuedby).WithMany(p => p.InvoicesIssuedby)
        //            .HasForeignKey(d => d.IssuedbyId)
        //            .HasConstraintName("FK_Invoices_IssuedbyId_User_Id");

        //        entity.HasOne(d => d.Payee).WithMany(p => p.InvoicesPayee)
        //            .HasForeignKey(d => d.PayeeId)
        //            .HasConstraintName("FK_Invoices_PayeeId_User_Id");

        //        entity.HasOne(d => d.Reviewer).WithMany(p => p.InvoicesReviewer)
        //            .HasForeignKey(d => d.ReviewerId)
        //            .HasConstraintName("FK_Invoices_ReviewerId_User_Id");
        //    });

        //    modelBuilder.Entity<LogOn>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("用户登录信息"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.IsEnable).HasComment("是否禁用");
        //        entity.Property(e => e.Password)
        //            .HasMaxLength(50)
        //            .HasComment("密码");
        //        entity.Property(e => e.Salt)
        //            .HasMaxLength(50)
        //            .HasComment("密码加盐");
        //        entity.Property(e => e.UserId)
        //            .IsRequired()
        //            .HasMaxLength(50);

        //        entity.HasOne(d => d.User).WithMany(p => p.LogOn)
        //            .HasForeignKey(d => d.UserId)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK_User_Id_LogOn_UserId");
        //    });

        //    modelBuilder.Entity<MemberInfo>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("客人会员信息"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.CardNo)
        //            .HasMaxLength(50)
        //            .HasComment("会员卡号");
        //        entity.Property(e => e.GuestId)
        //            .HasMaxLength(50)
        //            .HasComment("客人资料");
        //        entity.Property(e => e.Level)
        //            .HasMaxLength(50)
        //            .HasComment("会员等级");
        //        entity.Property(e => e.Type).HasComment("会员类型");

        //        entity.HasOne(d => d.Guest).WithMany(p => p.MemberInfo)
        //            .HasForeignKey(d => d.GuestId)
        //            .HasConstraintName("FK_MemberInfo_GuestId_GuestProfile_Id");
        //    });

        //    modelBuilder.Entity<Payments>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("付费信息"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.BillId)
        //            .HasMaxLength(50)
        //            .HasComment("账单");
        //        entity.Property(e => e.Description)
        //            .HasMaxLength(50)
        //            .HasComment("备注说明");
        //        entity.Property(e => e.Method).HasComment("付费方式");
        //        entity.Property(e => e.Money)
        //            .HasComment("付费金额")
        //            .HasColumnType("decimal(18, 2)");

        //        entity.HasOne(d => d.Bill).WithMany(p => p.Payments)
        //            .HasForeignKey(d => d.BillId)
        //            .HasConstraintName("FK_Payments_BillId_Bills_Id");
        //    });

        //    modelBuilder.Entity<PhoneInfo>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("电话信息"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.CountryCode)
        //            .HasMaxLength(20)
        //            .HasComment("国家电话代码");
        //        entity.Property(e => e.GuestId)
        //            .HasMaxLength(50)
        //            .HasComment("客人资料主键");
        //        entity.Property(e => e.Phone)
        //            .HasMaxLength(50)
        //            .HasComment("电话号码");

        //        entity.HasOne(d => d.Guest).WithMany(p => p.PhoneInfo)
        //            .HasForeignKey(d => d.GuestId)
        //            .HasConstraintName("FK_PhoneInfo_GuestId_GuestProfile_Id");
        //    });

        //    modelBuilder.Entity<PostCode>(entity =>
        //    {
        //        entity.HasKey(e => e.Code);

        //        entity.ToTable(tb => tb.HasComment("入账代码配置"));

        //        entity.Property(e => e.Code).HasMaxLength(50);
        //        entity.Property(e => e.Description)
        //            .HasMaxLength(200)
        //            .HasComment("描述");
        //        entity.Property(e => e.IsMerge).HasComment("是否合并");
        //        entity.Property(e => e.Money)
        //            .HasComment("金额")
        //            .HasColumnType("decimal(18, 2)");
        //        entity.Property(e => e.Sort).HasComment("排序");
        //    });

        //    modelBuilder.Entity<PostCodeMerge>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("入账代码合并列表"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.Code)
        //            .HasMaxLength(50)
        //            .HasComment("绑定PostCode的Code");
        //        entity.Property(e => e.Money)
        //            .HasComment("金额设置")
        //            .HasColumnType("decimal(18, 2)");
        //        entity.Property(e => e.ParentId)
        //            .HasMaxLength(50)
        //            .HasComment("父级绑定PostCode主键Code");

        //        entity.HasOne(d => d.CodeNavigation).WithMany(p => p.PostCodeMergeCodeNavigation)
        //            .HasForeignKey(d => d.Code)
        //            .HasConstraintName("FK_PostCodeMerge_Code_PostCode_Code");

        //        entity.HasOne(d => d.Parent).WithMany(p => p.PostCodeMergeParent)
        //            .HasForeignKey(d => d.ParentId)
        //            .HasConstraintName("FK_PostCodeMerge_ParentId_PostCode_Code");
        //    });

        //    modelBuilder.Entity<RealCompany>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("公司认证"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.Address)
        //            .HasMaxLength(200)
        //            .HasComment("公司地址");
        //        entity.Property(e => e.ChName)
        //            .HasMaxLength(50)
        //            .HasComment("中文名称");
        //        entity.Property(e => e.Code)
        //            .HasMaxLength(50)
        //            .HasComment("统一社会信用代码也叫做纳税人识别号");
        //        entity.Property(e => e.Email)
        //            .HasMaxLength(100)
        //            .HasComment("邮箱");
        //        entity.Property(e => e.EnName)
        //            .HasMaxLength(50)
        //            .HasComment("英文名称");
        //        entity.Property(e => e.LegalPerson)
        //            .HasMaxLength(50)
        //            .HasComment("法定代表人");
        //        entity.Property(e => e.LicenseLink)
        //            .HasMaxLength(200)
        //            .HasComment("营业执照图片链接");
        //        entity.Property(e => e.Phone)
        //            .HasMaxLength(50)
        //            .HasComment("电话");
        //        entity.Property(e => e.RegisterTime).HasComment("注册时间");
        //        entity.Property(e => e.RegisteredCapital)
        //            .HasComment("注册资本")
        //            .HasColumnType("decimal(18, 4)");
        //        entity.Property(e => e.WebSite)
        //            .HasMaxLength(200)
        //            .HasComment("网址");
        //    });

        //    modelBuilder.Entity<RealName>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("实名认证"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.Address)
        //            .HasMaxLength(200)
        //            .HasComment("地址");
        //        entity.Property(e => e.BackLink)
        //            .HasMaxLength(200)
        //            .HasComment("身份证反面地址");
        //        entity.Property(e => e.BirthDay).HasComment("出身日期");
        //        entity.Property(e => e.FrontLink)
        //            .HasMaxLength(200)
        //            .HasComment("身份证正面地址");
        //        entity.Property(e => e.Gender).HasComment("性别");
        //        entity.Property(e => e.IDNo)
        //            .HasMaxLength(20)
        //            .HasComment("身份证");
        //        entity.Property(e => e.Name)
        //            .HasMaxLength(50)
        //            .HasComment("姓名");
        //    });

        //    modelBuilder.Entity<Reservations>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("预定单"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.Adults).HasComment("成人数量");
        //        entity.Property(e => e.Arrival).HasComment("预抵时间");
        //        entity.Property(e => e.AverageRate)
        //            .HasComment("平均房价")
        //            .HasColumnType("decimal(18, 2)");
        //        entity.Property(e => e.Children).HasComment("小孩数量");
        //        entity.Property(e => e.Company)
        //            .HasMaxLength(50)
        //            .HasComment("公司名称");
        //        entity.Property(e => e.CreateTime).HasComment("创建时间");
        //        entity.Property(e => e.Departure).HasComment("离店时间");
        //        entity.Property(e => e.Description)
        //            .HasMaxLength(200)
        //            .HasComment("备注信息");
        //        entity.Property(e => e.Name)
        //            .HasMaxLength(50)
        //            .HasComment("预定姓名");
        //        entity.Property(e => e.Parent)
        //            .HasMaxLength(50)
        //            .HasComment("父级 用作拆分订单使用");
        //        entity.Property(e => e.Phone)
        //            .HasMaxLength(50)
        //            .HasComment("联系电话");
        //        entity.Property(e => e.ReservationNo)
        //            .HasMaxLength(50)
        //            .HasComment("预定号");
        //        entity.Property(e => e.Rooms).HasComment("房间数量");
        //        entity.Property(e => e.Status).HasComment("预定状态");
        //        entity.Property(e => e.SumRate)
        //            .HasComment("总价")
        //            .HasColumnType("decimal(18, 2)");
        //    });

        //    modelBuilder.Entity<Role>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("角色表(岗位职务)"));

        //        entity.Property(e => e.Id)
        //            .HasMaxLength(50)
        //            .HasComment("表主键");
        //        entity.Property(e => e.CompanyId).HasMaxLength(50);
        //        entity.Property(e => e.DepartmentId).HasMaxLength(50);
        //        entity.Property(e => e.Description)
        //            .HasMaxLength(50)
        //            .HasComment("备注");
        //        entity.Property(e => e.Level).HasComment("层级");
        //        entity.Property(e => e.Name)
        //            .HasMaxLength(50)
        //            .HasComment("角色名称");
        //        entity.Property(e => e.ParentId)
        //            .IsRequired()
        //            .HasMaxLength(50)
        //            .HasDefaultValueSql("((0))")
        //            .HasComment("父级");
        //        entity.Property(e => e.Sort).HasComment("排序");

        //        entity.HasOne(d => d.Company).WithMany(p => p.Role)
        //            .HasForeignKey(d => d.CompanyId)
        //            .HasConstraintName("FK_Role_CompanyId_Company_Id");

        //        entity.HasOne(d => d.Department).WithMany(p => p.Role)
        //            .HasForeignKey(d => d.DepartmentId)
        //            .HasConstraintName("FK_Role_DepartmentId_Department_Id");
        //    });

        //    modelBuilder.Entity<RolePermissionMap>(entity =>
        //    {
        //        entity.HasKey(e => new { e.RoleId, e.PermissionId });

        //        entity.ToTable(tb => tb.HasComment("角色和权限映射"));

        //        entity.Property(e => e.RoleId)
        //            .HasMaxLength(50)
        //            .HasComment("角色主键");
        //        entity.Property(e => e.PermissionId)
        //            .HasMaxLength(50)
        //            .HasComment("权限主键");
        //        entity.Property(e => e.C)
        //            .HasDefaultValueSql("((0))")
        //            .HasComment("是否可以创建");
        //        entity.Property(e => e.D)
        //            .HasDefaultValueSql("((0))")
        //            .HasComment("是否可以删除");
        //        entity.Property(e => e.R)
        //            .HasDefaultValueSql("((0))")
        //            .HasComment("是否可以读取");
        //        entity.Property(e => e.Type).HasComment("权限类型");
        //        entity.Property(e => e.U)
        //            .HasDefaultValueSql("((0))")
        //            .HasComment("是否可以更新");

        //        entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissionMap)
        //            .HasForeignKey(d => d.PermissionId)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK_RolePermissionMap_PermissionId_ColPermission_Id");

        //        entity.HasOne(d => d.PermissionNavigation).WithMany(p => p.RolePermissionMap)
        //            .HasForeignKey(d => d.PermissionId)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK_RolePermissionMap_PermissionId_SystemPermission_Id");

        //        entity.HasOne(d => d.Permission1).WithMany(p => p.RolePermissionMap)
        //            .HasForeignKey(d => d.PermissionId)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK_RolePermissionMap_PermissionId_TablePermission_Id");

        //        entity.HasOne(d => d.Role).WithMany(p => p.RolePermissionMap)
        //            .HasForeignKey(d => d.RoleId)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK_RolePermissionMap_RoldId_Role_Id");
        //    });

        //    modelBuilder.Entity<RoomInfo>(entity =>
        //    {
        //        entity.HasKey(e => new { e.RoomNo, e.Floor });

        //        entity.ToTable(tb => tb.HasComment("客房信息"));

        //        entity.Property(e => e.RoomNo).HasMaxLength(50);
        //        entity.Property(e => e.Floor).HasMaxLength(50);
        //        entity.Property(e => e.Status)
        //            .HasMaxLength(50)
        //            .HasComment("房态");
        //        entity.Property(e => e.Type).HasMaxLength(50);

        //        entity.HasOne(d => d.StatusNavigation).WithMany(p => p.RoomInfo)
        //            .HasForeignKey(d => d.Status)
        //            .HasConstraintName("FK_RoomInfo_Status_RoomStatusInfo_Code");

        //        entity.HasOne(d => d.TypeNavigation).WithMany(p => p.RoomInfo)
        //            .HasForeignKey(d => d.Type)
        //            .HasConstraintName("FK_RoomInfo_Type_RoomType_Code");
        //    });

        //    modelBuilder.Entity<RoomRateInfo>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("房价信息"));

        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.Description)
        //            .HasMaxLength(200)
        //            .HasComment("描述备注");
        //        entity.Property(e => e.Discount).HasComment("折扣");
        //        entity.Property(e => e.Money)
        //            .HasComment("实际金额")
        //            .HasColumnType("decimal(18, 2)");
        //        entity.Property(e => e.PostCode).HasMaxLength(50);
        //        entity.Property(e => e.Reason)
        //            .HasMaxLength(50)
        //            .HasComment("折扣原因");
        //        entity.Property(e => e.ReservationId).HasMaxLength(50);
        //        entity.Property(e => e.RoomType).HasMaxLength(50);

        //        entity.HasOne(d => d.PostCodeNavigation).WithMany(p => p.RoomRateInfo)
        //            .HasForeignKey(d => d.PostCode)
        //            .HasConstraintName("FK_RoomRateInfo_PostCode_PostCode_Code");

        //        entity.HasOne(d => d.Reservation).WithMany(p => p.RoomRateInfo)
        //            .HasForeignKey(d => d.ReservationId)
        //            .HasConstraintName("FK_RoomRateInfo_ReservationId_Reservations_Id");

        //        entity.HasOne(d => d.RoomTypeNavigation).WithMany(p => p.RoomRateInfo)
        //            .HasForeignKey(d => d.RoomType)
        //            .HasConstraintName("FK_RoomRateInfo_RoomType_RoomType_Code");
        //    });

        //    modelBuilder.Entity<RoomStatusInfo>(entity =>
        //    {
        //        entity.HasKey(e => e.Code);

        //        entity.ToTable(tb => tb.HasComment("房态信息"));

        //        entity.Property(e => e.Code)
        //            .HasMaxLength(50)
        //            .HasComment("房态编码");
        //        entity.Property(e => e.ChName)
        //            .HasMaxLength(50)
        //            .HasComment("中文名称");
        //        entity.Property(e => e.EnName)
        //            .HasMaxLength(50)
        //            .HasComment("英文名称");
        //    });

        //    modelBuilder.Entity<RoomType>(entity =>
        //    {
        //        entity.HasKey(e => e.Code);

        //        entity.ToTable(tb => tb.HasComment("房型信息"));

        //        entity.Property(e => e.Code)
        //            .HasMaxLength(50)
        //            .HasComment("房型编码");
        //        entity.Property(e => e.ChName)
        //            .HasMaxLength(50)
        //            .HasComment("中文名称");
        //        entity.Property(e => e.EnName)
        //            .HasMaxLength(50)
        //            .HasComment("英文名称");
        //    });

        //    modelBuilder.Entity<SystemPermission>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("系统菜单按钮权限"));

        //        entity.Property(e => e.Id)
        //            .HasMaxLength(50)
        //            .HasComment("主键");
        //        entity.Property(e => e.Description)
        //            .HasMaxLength(50)
        //            .HasComment("描述");
        //        entity.Property(e => e.Name)
        //            .HasMaxLength(50)
        //            .HasComment("名称");
        //        entity.Property(e => e.ParentId)
        //            .IsRequired()
        //            .HasMaxLength(50)
        //            .HasDefaultValueSql("((0))")
        //            .HasComment("父级");
        //        entity.Property(e => e.Sort).HasComment("排序");
        //    });

        //    modelBuilder.Entity<TablePermission>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("数据表权限"));

        //        entity.Property(e => e.Id)
        //            .HasMaxLength(50)
        //            .HasComment("主键");
        //        entity.Property(e => e.Description)
        //            .HasMaxLength(50)
        //            .HasComment("描述");
        //        entity.Property(e => e.Name)
        //            .HasMaxLength(50)
        //            .HasComment("名称");
        //        entity.Property(e => e.ParentId)
        //            .IsRequired()
        //            .HasMaxLength(50)
        //            .HasDefaultValueSql("((0))")
        //            .HasComment("父级");
        //        entity.Property(e => e.Sort).HasComment("排序");
        //    });

        //    modelBuilder.Entity<ThirdPartyLogOn>(entity =>
        //    {
        //        entity.Property(e => e.Id).HasMaxLength(50);
        //        entity.Property(e => e.IsEnable)
        //            .IsRequired()
        //            .HasDefaultValueSql("((1))")
        //            .HasComment("是否禁用 默认启用");
        //        entity.Property(e => e.NickName)
        //            .HasMaxLength(100)
        //            .HasComment("第三方平台昵称");
        //        entity.Property(e => e.OpenId)
        //            .HasMaxLength(50)
        //            .HasComment("平台唯一标识");
        //        entity.Property(e => e.Pic)
        //            .HasMaxLength(50)
        //            .HasComment("第三方平台头像");
        //        entity.Property(e => e.PlatformType).HasComment("平台类别");
        //        entity.Property(e => e.UserId).HasMaxLength(50);

        //        entity.HasOne(d => d.User).WithMany(p => p.ThirdPartyLogOn)
        //            .HasForeignKey(d => d.UserId)
        //            .HasConstraintName("FK_User_Id_ThirdPartyLogOn_UserId");
        //    });

        //    modelBuilder.Entity<User>(entity =>
        //    {
        //        entity.ToTable(tb => tb.HasComment("用户表"));

        //        entity.Property(e => e.Id)
        //            .HasMaxLength(50)
        //            .HasComment("主键");
        //        entity.Property(e => e.Email)
        //            .HasMaxLength(50)
        //            .HasComment("邮箱");
        //        entity.Property(e => e.NickName)
        //            .HasMaxLength(50)
        //            .HasComment("用户昵称");
        //        entity.Property(e => e.Phone)
        //            .HasMaxLength(50)
        //            .HasComment("电话");
        //        entity.Property(e => e.RealNameId)
        //            .HasMaxLength(50)
        //            .HasComment("关联实名认证");
        //        entity.Property(e => e.UserPic)
        //            .HasMaxLength(500)
        //            .HasComment("用户头像");

        //        entity.HasOne(d => d.RealName).WithMany(p => p.User)
        //            .HasForeignKey(d => d.RealNameId)
        //            .HasConstraintName("FK_UserReal_NameId_RealName_Id");

        //        entity.HasMany(d => d.Company1).WithMany(p => p.UserNavigation)
        //            .UsingEntity<Dictionary<string, object>>(
        //                "UserCompanyMap",
        //                r => r.HasOne<Company>().WithMany()
        //                    .HasForeignKey("CompanyId")
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_UserCompanyMap_CompanyId_Company_Id"),
        //                l => l.HasOne<User>().WithMany()
        //                    .HasForeignKey("UserId")
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_UserCompanyMap_UserId_User_Id"),
        //                j =>
        //                {
        //                    j.HasKey("UserId", "CompanyId");
        //                    j.ToTable(tb => tb.HasComment("用户和公司关系映射"));
        //                    j.IndexerProperty<string>("UserId").HasMaxLength(50);
        //                    j.IndexerProperty<string>("CompanyId").HasMaxLength(50);
        //                });

        //        entity.HasMany(d => d.CompanyNavigation).WithMany(p => p.User)
        //            .UsingEntity<Dictionary<string, object>>(
        //                "CompanySaleMap",
        //                r => r.HasOne<CompanyProfile>().WithMany()
        //                    .HasForeignKey("CompanyId")
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_CompanySaleMap_CompanyId_CompanyProfile_Id"),
        //                l => l.HasOne<User>().WithMany()
        //                    .HasForeignKey("UserId")
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_CompanySaleMap_UserId_User_Id"),
        //                j =>
        //                {
        //                    j.HasKey("UserId", "CompanyId");
        //                    j.ToTable(tb => tb.HasComment("公司销售映射"));
        //                    j.IndexerProperty<string>("UserId").HasMaxLength(50);
        //                    j.IndexerProperty<string>("CompanyId").HasMaxLength(50);
        //                });

        //        entity.HasMany(d => d.Department).WithMany(p => p.User)
        //            .UsingEntity<Dictionary<string, object>>(
        //                "UserDepartmentMap",
        //                r => r.HasOne<Department>().WithMany()
        //                    .HasForeignKey("DepartmentId")
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_UserDepartmentMap_DepartmentId_Department_Id"),
        //                l => l.HasOne<User>().WithMany()
        //                    .HasForeignKey("UserId")
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_UserDepartmentMap_UserId_User_Id"),
        //                j =>
        //                {
        //                    j.HasKey("UserId", "DepartmentId");
        //                    j.ToTable(tb => tb.HasComment("用户和部门关系映射"));
        //                    j.IndexerProperty<string>("UserId").HasMaxLength(50);
        //                    j.IndexerProperty<string>("DepartmentId").HasMaxLength(50);
        //                });

        //        entity.HasMany(d => d.Role).WithMany(p => p.User)
        //            .UsingEntity<Dictionary<string, object>>(
        //                "UserRoleMap",
        //                r => r.HasOne<Role>().WithMany()
        //                    .HasForeignKey("RoleId")
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_UserRoleMap_RoleId_Role_Id"),
        //                l => l.HasOne<User>().WithMany()
        //                    .HasForeignKey("UserId")
        //                    .OnDelete(DeleteBehavior.ClientSetNull)
        //                    .HasConstraintName("FK_UserRoleMap_UserId_User_Id"),
        //                j =>
        //                {
        //                    j.HasKey("UserId", "RoleId");
        //                    j.ToTable(tb => tb.HasComment("用户表和角色表关系映射"));
        //                    j.IndexerProperty<string>("UserId")
        //                        .HasMaxLength(50)
        //                        .HasComment("用户表主键");
        //                    j.IndexerProperty<string>("RoleId")
        //                        .HasMaxLength(50)
        //                        .HasComment("角色表主键");
        //                });
        //    });

        //    OnModelCreatingPartial(modelBuilder);
        //}
    }
}
