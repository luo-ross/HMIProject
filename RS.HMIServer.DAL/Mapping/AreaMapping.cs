using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.HMIServer.Entity;

namespace RS.HMIServer.DAL.Mapping
{
    /// <summary>
    /// 区域表配置映射
    /// </summary>
    internal class AreaMapping : IEntityTypeConfiguration<AreaEntity>
    {
        public void Configure(EntityTypeBuilder<AreaEntity> builder)
        {
            builder.ToTable("Area").HasKey(t => t.Id);
            //设置不自动增长
            builder.Property(t => t.Id).ValueGeneratedNever();
        }
    }
}