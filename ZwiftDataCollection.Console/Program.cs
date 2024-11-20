using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ZwiftDataCollectionAgent.Console
{

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            var services = new ServiceCollection();
            IConfigurationSection whatIsThis = configuration.GetSection("BespokeConfig");
            services.Configure<BespokeConfig>(whatIsThis);
            var serviceProvider = services.BuildServiceProvider();
            BespokeConfig? config = serviceProvider.GetService<IOptions<BespokeConfig>>()?.Value;

            var runnable = new Process(config!);
            await runnable.Run();
        }

    }
}
