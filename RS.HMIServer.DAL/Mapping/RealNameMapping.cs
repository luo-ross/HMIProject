using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.HMIServer.Entity;

namespace RS.HMIServer.DAL.Mapping
{
    /// <summary>
    /// 实名认证表配置映射
    /// </summary>
    internal class RealNameMapping : IEntityTypeConfiguration<RealNameEntity>
    {
        public void Configure(EntityTypeBuilder<RealNameEntity> builder)
        {
            builder.ToTable("RealName").HasKey(t => t.Id);
        }
    }
}