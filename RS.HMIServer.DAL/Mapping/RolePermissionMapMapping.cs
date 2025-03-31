using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Hosting;
using RS.HMIServer.Entity;

namespace RS.HMIServer.DAL.Mapping
{
    /// <summary>
    /// 角色权限映射表配置映射
    /// </summary>
    internal class RolePermissionMapMapping : IEntityTypeConfiguration<RolePermissionMapEntity>
    {
        public void Configure(EntityTypeBuilder<RolePermissionMapEntity> builder)
        {
            builder.ToTable("RolePermissionMap").HasAlternateKey(t => new { t.PermissionId, t.RoleId });
        }
    }
}