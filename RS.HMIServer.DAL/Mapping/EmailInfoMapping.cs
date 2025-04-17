using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.HMIServer.Entity;

namespace RS.HMIServer.DAL.Mapping
{
    /// <summary>
    /// 邮箱信息表配置映射
    /// </summary>
    internal class EmailInfoMapping : IEntityTypeConfiguration<EmailInfoEntity>
    {
        public void Configure(EntityTypeBuilder<EmailInfoEntity> builder)
        {
            builder.ToTable("EmailInfo").HasKey(t => t.Id);
            //设置不自动增长
            builder.Property(t => t.Id).ValueGeneratedNever();
        }
    }
}