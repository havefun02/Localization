using Localization;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = CreateBuilder(args).Build();
        builder.Run();
    }
    public static IHostBuilder CreateBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilders => { webBuilders.UseStartup<Startup>(); });
}