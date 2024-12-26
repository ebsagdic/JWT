using System;
using JWT.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Data.ModelConfigurations
{
    public class UserRefreshTokenConfiguration :IEntityTypeConfiguration<UserRefreshToken>
    {

        public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
        {
            builder.Property(x => x.UserId).HasMaxLength(450);
            builder.Property(x => x.Code).HasMaxLength(200);
        }
    }
}
