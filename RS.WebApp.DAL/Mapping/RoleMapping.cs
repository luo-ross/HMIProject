using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.WebApp.Entity;

namespace RS.WebApp.DAL.Mapping
{
    /// <summary>
    /// 角色表配置映射
    /// </summary>
    internal class RoleMapping : IEntityTypeConfiguration<RoleEntity>
    {
        public void Configure(EntityTypeBuilder<RoleEntity> builder)
        {
            builder.ToTable("Role").HasKey(t => t.Id);
        }
    }
}