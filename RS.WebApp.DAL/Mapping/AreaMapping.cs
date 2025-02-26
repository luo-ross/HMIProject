using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.WebApp.Entity;

namespace RS.WebApp.DAL.Mapping
{
    /// <summary>
    /// 区域表配置映射
    /// </summary>
    internal class AreaMapping : IEntityTypeConfiguration<AreaEntity>
    {
        public void Configure(EntityTypeBuilder<AreaEntity> builder)
        {
            builder.ToTable("Area").HasKey(t => t.Id);
        }
    }
}