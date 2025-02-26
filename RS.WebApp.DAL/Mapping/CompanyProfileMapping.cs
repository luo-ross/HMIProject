using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.WebApp.Entity;

namespace RS.WebApp.DAL.Mapping
{
    /// <summary>
    /// 公司资料表配置映射
    /// </summary>
    internal class CompanyProfileMapping : IEntityTypeConfiguration<CompanyProfileEntity>
    {
        public void Configure(EntityTypeBuilder<CompanyProfileEntity> builder)
        {
            builder.ToTable("CompanyProfile").HasKey(t => t.Id);
        }
    }
}