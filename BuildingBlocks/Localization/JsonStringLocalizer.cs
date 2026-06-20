using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;

namespace BuildingBlocks.Localization;

public class JsonStringLocalizer(string basePath, IMemoryCache cache) : IStringLocalizer
{
    public LocalizedString this[string name] => Translate(name);

    public LocalizedString this[string name, params object[] arguments]
        => new(name, string.Format(Translate(name).Value, arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var dict = Load(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        return dict.Select(kv => new LocalizedString(kv.Key, kv.Value));
    }

    private LocalizedString Translate(string name)
    {
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        if (TryGet(culture, name, out var value))
            return new LocalizedString(name, value!);

        // fallback to English
        if (culture != "en" && TryGet("en", name, out value))
            return new LocalizedString(name, value!);

        return new LocalizedString(name, name, resourceNotFound: true);
    }

    private bool TryGet(string culture, string name, out string? value)
        => Load(culture).TryGetValue(name, out value);

    private Dictionary<string, string> Load(string culture)
    {
        return cache.GetOrCreate($"json_loc_{culture}_{basePath}", _ =>
        {
            var path = Path.Combine(basePath, $"{culture}.json");
            if (!File.Exists(path)) return new Dictionary<string, string>();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
        })!;
    }
}
