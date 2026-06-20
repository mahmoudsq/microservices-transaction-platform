using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;

namespace BuildingBlocks.Localization;

public class JsonStringLocalizerFactory(IHostEnvironment env, IMemoryCache cache) : IStringLocalizerFactory
{
    public IStringLocalizer Create(Type resourceSource) => CreateLocalizer();
    public IStringLocalizer Create(string baseName, string location) => CreateLocalizer();

    private JsonStringLocalizer CreateLocalizer()
        => new(Path.Combine(env.ContentRootPath, "Culture"), cache);
}
