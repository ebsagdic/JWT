using System;

using JWT.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.ModelConfigurations
{
    public class ProductConfiguration :  IEntityTypeConfiguration<Product> 
    {
        public  void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.StockCode).HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(200);
        }
    }
}
