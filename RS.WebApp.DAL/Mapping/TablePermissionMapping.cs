using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.WebApp.Entity;

namespace RS.WebApp.DAL.Mapping
{
    /// <summary>
    /// 数据表权限表配置映射
    /// </summary>
    internal class TablePermissionMapping : IEntityTypeConfiguration<TablePermissionEntity>
    {
        public void Configure(EntityTypeBuilder<TablePermissionEntity> builder)
        {
            builder.ToTable("TablePermission").HasKey(t => t.Id);
        }
    }
}