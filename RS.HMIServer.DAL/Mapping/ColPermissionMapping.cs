using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.HMIServer.Entity;

namespace RS.HMIServer.DAL.Mapping
{
    /// <summary>
    /// 列权限表配置映射
    /// </summary>
    internal class ColPermissionMapping : IEntityTypeConfiguration<ColPermissionEntity>
    {
        public void Configure(EntityTypeBuilder<ColPermissionEntity> builder)
        {
            builder.ToTable("ColPermission").HasKey(t => t.Id);
        }
    }
}