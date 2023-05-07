using ChatBotWithSignalR.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatBotWithSignalR.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ChatGroup> ChatGroups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<ConversationFile> ConversationFiles { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>(entity =>
                entity.ToTable("Users", "Identity")
            );
            builder.Entity<IdentityRole>(entity =>
               entity.ToTable("Roles", "Identity")
            );
            builder.Entity<IdentityUserRole<string>>(entity =>
               entity.ToTable("UserRoles", "Identity")
            );
            builder.Entity<IdentityUserClaim<string>>(entity =>
               entity.ToTable("UserClaims", "Identity")
            );
            builder.Entity<IdentityUserLogin<string>>(entity =>
               entity.ToTable("UserLogins", "Identity")
            );
            builder.Entity<IdentityUserToken<string>>(entity =>
               entity.ToTable("UserTokens", "Identity")
            );
            builder.Entity<IdentityRoleClaim<string>>(entity =>
               entity.ToTable("RoleClaims", "Identity")
            );
        }

    }
}