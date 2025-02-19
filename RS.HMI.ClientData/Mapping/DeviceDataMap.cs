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
    public class DeviceDataMap : IEntityTypeConfiguration<DeviceData>
    {
        public void Configure(EntityTypeBuilder<DeviceData> builder)
        {
            builder.ToTable("DeviceData").HasKey(t=>t.Id);
        }
    }
}
