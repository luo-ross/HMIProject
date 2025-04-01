using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using RS.Annotation.SQLite.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RS.Annotation.SQLite.DbContexts
{
    public class AnnotationDbContexts : DbContext
    {
        public AnnotationDbContexts()
        {

        }

        /// <summary>
        /// 项目
        /// </summary>
        public DbSet<Projects> Projects { get; set; }

        /// <summary>
        /// 图像
        /// </summary>
        public DbSet<Pictures> Pictures { get; set; }

        /// <summary>
        /// 标注矩形
        /// </summary>
        public DbSet<Rects> Rects { get; set; }

        /// <summary>
        /// 标注类别
        /// </summary>
        public DbSet<Tags> Tags { get; set; }

        public AnnotationDbContexts(string databasePath)
        {
            this.DatabasePath = databasePath;
            MigrationDataBase();
        }

        private void MigrationDataBase()
        {
            if (this.Database.GetPendingMigrations().Any())
            {
                this.Database.Migrate();
            }
        }

        private string databasePath;

        /// <summary>
        /// 数据库文件存储路径
        /// </summary>
        public string DatabasePath
        {
            get { return databasePath; }
            set { databasePath = value; }
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
                .Where(type => type.Namespace == "RS.Annotation.SQLite.Mapping");
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
