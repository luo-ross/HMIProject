using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.HMIServer.Entity;

namespace RS.HMIServer.DAL.Mapping
{
    /// <summary>
    /// 手机信息表配置映射
    /// </summary>
    internal class PhoneInfoMapping : IEntityTypeConfiguration<PhoneInfoEntity>
    {
        public void Configure(EntityTypeBuilder<PhoneInfoEntity> builder)
        {
            builder.ToTable("PhoneInfo").HasKey(t => t.Id);
        }
    }
}