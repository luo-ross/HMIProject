using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.HMIServer.Entity;

namespace RS.HMIServer.DAL.Mapping
{
    /// <summary>
    /// 实际公司表配置映射
    /// </summary>
    internal class RealCompanyMapping : IEntityTypeConfiguration<RealCompanyEntity>
    {
        public void Configure(EntityTypeBuilder<RealCompanyEntity> builder)
        {
            builder.ToTable("RealCompany").HasKey(t => t.Id);
            //设置不自动增长
            builder.Property(t => t.Id).ValueGeneratedNever();
        }
    }
}