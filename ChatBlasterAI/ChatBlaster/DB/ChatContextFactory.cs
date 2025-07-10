using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
namespace ChatBlaster.DB
{
    public class ChatContextFactory : IDesignTimeDbContextFactory<ChatContext>
    {
        public ChatContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ChatContext>();
            optionsBuilder.UseSqlServer(@"Server=(LocalDB)\MSSQLLocalDB;Database=ChatDb;Trusted_Connection=True;");
            var ctx = new ChatContext(optionsBuilder.Options);
            ctx.Database.EnsureCreated();


            return ctx;
        }
    }
}
