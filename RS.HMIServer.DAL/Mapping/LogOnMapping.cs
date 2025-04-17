using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.HMIServer.Entity;

namespace RS.HMIServer.DAL.Mapping
{
    /// <summary>
    /// 登录表配置映射
    /// </summary>
    internal class LogOnMapping : IEntityTypeConfiguration<LogOnEntity>
    {
        public void Configure(EntityTypeBuilder<LogOnEntity> builder)
        {
            builder.ToTable("LogOn").HasKey(t => t.Id);
            //设置不自动增长
            builder.Property(t => t.Id).ValueGeneratedNever();
        }
    }
}
