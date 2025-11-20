using FinitiGlossary.Domain.Entities.Auth.Token;
using FinitiGlossary.Domain.Entities.Terms;
using FinitiGlossary.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;


namespace FinitiGlossary.Infrastructure.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<GlossaryTerm> GlossaryTerms => Set<GlossaryTerm>();
        public DbSet<ArchivedGlossaryTerm> ArchivedGlossaryTerms => Set<ArchivedGlossaryTerm>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("tblUsers");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Username).IsRequired().HasMaxLength(150);
                entity.Property(x => x.Email).IsRequired().HasMaxLength(200);
                entity.Property(x => x.PasswordHash).IsRequired();
                entity.Property(x => x.Role).HasDefaultValue("User");
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.Property(x => x.IsAdmin).HasDefaultValue(false);
                entity.Property(x => x.IsDeleted).HasDefaultValue(false);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("tblRefreshTokens");
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<GlossaryTerm>(entity =>
            {
                entity.ToTable("tblGlossaryTerms");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Term).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Definition).IsRequired().HasMaxLength(4000);
                entity.Property(x => x.Status).HasConversion<int>();
            });

            modelBuilder.Entity<ArchivedGlossaryTerm>(entity =>
            {
                entity.ToTable("tblArchivedGlossaryTerms");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.OriginalTermId).IsRequired();
                entity.Property(x => x.Term).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Definition).IsRequired().HasMaxLength(4000);
                entity.Property(x => x.ArchivedAt).IsRequired();
                entity.Property(x => x.ArchivedById).IsRequired().HasMaxLength(255);
                entity.Property(x => x.CreatedById).IsRequired().HasMaxLength(255);
                entity.Property(x => x.ChangeSummary).HasMaxLength(1000);
            });
        }
    }
}
