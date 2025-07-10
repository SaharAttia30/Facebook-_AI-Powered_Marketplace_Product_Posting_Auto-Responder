

using ChatBlaster.Controllers.Avatar;
using ChatBlaster.DB;
using ChatBlaster.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatBlaster
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        [STAThread]
        private static void Main()
        {
            ApplicationConfigurations.Initialize();
            var services = new ServiceCollection();
            var dbOptions = new DbContextOptionsBuilder<ChatContext>()
                .UseSqlServer(@"Server=(LocalDB)\MSSQLLocalDB;Database=ChatDb;Trusted_Connection=True;")
                .Options;
            services.AddSingleton(new ChatService(dbOptions));
            services.AddSingleton<MainForm>();
            
            ServiceProvider = services.BuildServiceProvider();
            var mainForm = ServiceProvider.GetRequiredService<MainForm>();
            Application.ApplicationExit += (_, _) =>
            {
                mainForm.StopAllAvatars();
            };
            Application.Run(mainForm);
        }
    }
}