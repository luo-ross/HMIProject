using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.HMI.ClientData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.ClientData.Mapping
{
    public sealed class ModbusCommuConfigMap : IEntityTypeConfiguration<ModbusCommuConfig>
    {
        public void Configure(EntityTypeBuilder<ModbusCommuConfig> builder)
        {
            builder.ToTable("ModbusCommuConfig").HasKey(t=>t.Id);
        }
    }
}
