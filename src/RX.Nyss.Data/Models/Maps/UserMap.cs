﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models.Maps
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.HasOne(u => u.ApplicationLanguage).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(u => u.UserNationalSocieties);
            builder.HasDiscriminator(u => u.Role)
                .HasValue<SupervisorUser>(Role.Supervisor)
                .HasValue<DataManagerUser>(Role.DataManager)
                .HasValue<AdministratorUser>(Role.Administrator)
                .HasValue<GlobalCoordinatorUser>(Role.GlobalCoordinator)
                .HasValue<DataConsumerUser>(Role.DataConsumer)
                .HasValue<TechnicalAdvisorUser>(Role.TechnicalAdvisor);
            
            builder.Property(u => u.Name).HasMaxLength(100).IsRequired();
            builder.Property(u => u.IdentityUserId);
            builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(50).IsRequired();
            builder.Property(u => u.EmailAddress).HasMaxLength(100).IsRequired();
            builder.Property(u => u.PhoneNumber).HasMaxLength(20).IsRequired();
            builder.Property(u => u.AdditionalPhoneNumber).HasMaxLength(20);
            builder.Property(u => u.Organization).HasMaxLength(100);
            builder.Property(u => u.IsFirstLogin).IsRequired();
        }
    }
}
