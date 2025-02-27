using Microsoft.EntityFrameworkCore;
using RS.Commons.Helper;
using RS.HMI.ClientData.Entities;
using System.Data;
using System.Reflection;

namespace RS.HMI.ClientData.DbContexts
{
    public sealed class HMIClientDataDbContexts : DbContext
    {
        public HMIClientDataDbContexts()
        {
            this.DatabasePath = PathHelper.MapPath("Database/HMIProject.hmi");
            this.MigrationDataBase();
        }

        /// <summary>
        /// 通讯站
        /// </summary>
        public DbSet<CommuStation> CommuStation { get; set; }

        /// <summary>
        /// Modbus 通讯配置
        /// </summary>
        public DbSet<ModbusCommuConfig> ModbusCommuConfig { get; set; }

        /// <summary>
        /// 串口通讯配置
        /// </summary>
        public DbSet<SerialPortConfig> SerialPortConfig { get; set; }



        private void MigrationDataBase()
        {
            if (this.Database.GetPendingMigrations().Any())
            {
                this.Database.Migrate();
            }
        }

        private  string databasePath;

        /// <summary>
        /// 数据库文件存储路径
        /// </summary>
        public  string DatabasePath
        {
            get { return databasePath; }
            private set { databasePath = value; }
        }


        /// <summary>
        /// 这是SQLite数据库连接字符串
        /// </summary>
        public string SqliteConnectionString
        {
            get
            {
                return $"Data Source={DatabasePath}";
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Assembly asm = Assembly.GetExecutingAssembly().ManifestModule.Assembly;
            //获取所有要注册的类
            var typeToRegister = asm.ExportedTypes
                .Where(type => string.IsNullOrEmpty(type.Namespace))
                .Where(type => type.Namespace == "RS.HMI.ClientData.Mapping");
            foreach (var type in typeToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                modelBuilder.ApplyConfiguration(configurationInstance);
            }
            base.OnModelCreating(modelBuilder);
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //这里如果未配置
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(this.SqliteConnectionString);
            }
            base.OnConfiguring(optionsBuilder);
        }

    }
}
