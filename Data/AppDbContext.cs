using Microsoft.EntityFrameworkCore;
using SocialMediaAPI.Data.Models;
using SocialMediaAPI.Data.Services;

namespace SocialMediaAPI.Data
{
    public class AppDbContext : DbContext
    {
        private IConfiguration configuration;

        public AppDbContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique();


            UserService.CreatePasswordHash(configuration.GetSection("AdminCreds:Password").Value, out byte[] passwordHash, out byte[] passwordSalt);
            modelBuilder.Entity<User>()
                .HasData(new User()
                {
                    Id = -1,
                    FirstName = configuration.GetSection("AdminCreds:FirstName").Value,
                    LastName = configuration.GetSection("AdminCreds:LastName").Value,
                    Username = configuration.GetSection("AdminCreds:Username").Value,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Email = configuration.GetSection("AdminCreds:Email").Value,
                    Role = "Admin",
                    Verified = true,
                });

            modelBuilder.Entity<Post>()
                .HasOne<User>(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comment>()
                .HasOne<User>(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.PostId }).IsUnique();

            modelBuilder.Entity<Follow>()
                .HasIndex(f => new { f.UserId, f.FollowingId }).IsUnique();

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.User)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<Verification>()
               .HasIndex(v => v.Token).IsUnique();


            modelBuilder.Entity<PasswordReset>()
               .HasIndex(v => v.Token).IsUnique();

            modelBuilder.Entity<Message>()
                .HasOne(m => m.FromUser)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.FromUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.ToUser)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ToUserId)
                .OnDelete(DeleteBehavior.NoAction);

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Verification> Verifications { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<Message> Messages { get; set; }

    }
}
