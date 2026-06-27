using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GymManagement.DAL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.DAL.Data.Configurations
{
    internal class MembershipConfiguration : IEntityTypeConfiguration<MemberShipViewModel>
    {
        public void Configure(EntityTypeBuilder<MemberShipViewModel> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CreatedAt)
                .HasColumnName("StartDate")
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
