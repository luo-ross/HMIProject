using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.WebApp.Entity;

namespace RS.WebApp.DAL.Mapping
{
    /// <summary>
    /// 公司表配置映射
    /// </summary>
    internal class CompanyMapping : IEntityTypeConfiguration<CompanyEntity>
    {
        public void Configure(EntityTypeBuilder<CompanyEntity> builder)
        {
            builder.ToTable("Company").HasKey(t => t.Id);
        }
    }
}