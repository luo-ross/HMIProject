using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RS.AnnotationSQLite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.AnnotationSQLite.Mapping
{
    public class RectsMap : IEntityTypeConfiguration<Rects>
    {
        public void Configure(EntityTypeBuilder<Rects> builder)
        {
            builder.ToTable("Rects").HasKey(t=>t.Id);
        }
    }
}
