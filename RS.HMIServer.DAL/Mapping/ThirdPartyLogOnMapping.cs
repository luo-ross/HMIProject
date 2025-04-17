using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.HMIServer.Entity;

namespace RS.HMIServer.DAL.Mapping
{
    /// <summary>
    /// 第三方登录表配置映射
    /// </summary>
    internal class ThirdPartyLogOnMapping : IEntityTypeConfiguration<ThirdPartyLogOnEntity>
    {
        public void Configure(EntityTypeBuilder<ThirdPartyLogOnEntity> builder)
        {
            builder.ToTable("ThirdPartyLogOn").HasKey(t => t.Id);
            //设置不自动增长
            builder.Property(t => t.Id).ValueGeneratedNever();
        }
    }
}