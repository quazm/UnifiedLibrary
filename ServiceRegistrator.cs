using Jellyfin.Plugin.UnifiedLibrary.Services;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.UnifiedLibrary;

public class ServiceRegistrator : IPluginServiceRegistrator
{
    // Обязательно без параметров
    public ServiceRegistrator()
    {
    }

    public void RegisterServices(
        IServiceCollection serviceCollection,
        IServerApplicationHost applicationHost)
    {
        serviceCollection.AddScoped<UnifiedLibraryQueryService>();

        // Если после исправления этой ошибки ваш контроллер не появится в API/Swagger,
        // раскомментируйте строки ниже:
        //
        // serviceCollection
        //     .AddControllers()
        //     .AddApplicationPart(typeof(ServiceRegistrator).Assembly);
    }
}