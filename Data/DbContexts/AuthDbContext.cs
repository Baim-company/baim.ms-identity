using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Identity.API.Data.Entities;
using Identity.API.Data.Enums;


namespace Identity.API.Data.DbContexts;

public class AuthDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{

    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options) { }

    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Name).HasMaxLength(50);
            entity.Property(u => u.Surname).HasMaxLength(50);
            entity.Property(u => u.Patronymic).HasMaxLength(50);
            entity.Property(u => u.Position).HasMaxLength(30);
            entity.Property(u => u.BirthDate);
            entity.Property(u => u.Gender).HasConversion(
                    v => v.ToString(),
                    v => (Gender)Enum.Parse(typeof(Gender), v));

            entity.Property(u => u.PersonalEmail).HasMaxLength(100);
            entity.Property(u => u.PhoneNumber).HasMaxLength(50);
            entity.Property(u => u.BusinessPhoneNumber).HasMaxLength(50);
            
            entity.Property(pi => pi.RefreshToken);
            entity.Property(pi => pi.RefreshTokenExpiryTime);

            entity.Property(pi => pi.AvatarPath).HasMaxLength(200);
        });
    }
}