using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.HMIServer.Entity;

namespace RS.HMIServer.DAL.Mapping
{
    /// <summary>
    /// 职位表配置映射
    /// </summary>
    internal class DutyMapping : IEntityTypeConfiguration<DutyEntity>
    {
        public void Configure(EntityTypeBuilder<DutyEntity> builder)
        {
            builder.ToTable("Duty").HasKey(t => t.Id);
            //设置不自动增长
            builder.Property(t => t.Id).ValueGeneratedNever();
        }
    }
}