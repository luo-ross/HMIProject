using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.WebApp.Entity;

namespace RS.WebApp.DAL.Mapping
{
    /// <summary>
    /// 国家表配置映射
    /// </summary>
    internal class CountryMapping : IEntityTypeConfiguration<CountryEntity>
    {
        public void Configure(EntityTypeBuilder<CountryEntity> builder)
        {
            builder.ToTable("Country").HasKey(t => t.Id);
        }
    }
}