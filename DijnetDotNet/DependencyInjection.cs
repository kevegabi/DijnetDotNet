using Microsoft.Extensions.DependencyInjection;

namespace Dijnet.Net
{
    public static class DependencyInjection
    {
        public static void AddDijnetDotNet(this IServiceCollection services)
        {
            services.AddScoped<IDijnetService, DijnetService>();
        }
    }
}
