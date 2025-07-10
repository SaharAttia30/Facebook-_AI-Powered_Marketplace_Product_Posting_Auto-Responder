using ChatBlaster.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBlaster.DB
{
    public class ChatContext : DbContext
    {
        //public ChatContext(DbContextOptions options) : base(options)
        //{
        //}
        public DbSet<Company> Companies { get; set; }
        public DbSet<ServiceArea> ServiceAreas { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Avatar> Avatars { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ChatBlaster.Models.Message> Messages { get; set; }

        public ChatContext(DbContextOptions<ChatContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.Avatar)
                .WithMany()
                .HasForeignKey(c => c.AvatarId);

            modelBuilder.Entity<Avatar>()
                .HasOne(a => a.Service)
                .WithMany(s => s.Avatars)
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Group>()
                .HasOne(g => g.Avatar)
                .WithMany(a => a.Groups)
                .HasForeignKey(g => g.AvatarId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlServer(@"Server=(LocalDB)\MSSQLLocalDB;Database=ChatDb;Trusted_Connection=True;");
            }
        }
    }
}
