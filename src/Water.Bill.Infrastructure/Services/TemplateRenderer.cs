using System.Text.RegularExpressions;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.Infrastructure.Services;

public class TemplateRenderer : ITemplateRenderer
{
    private static readonly Regex PlaceholderRegex = new(@"\{\{\s*(?<key>[A-Za-z0-9_]+)\s*\}\}", RegexOptions.Compiled);

    public string Render(string template, IReadOnlyDictionary<string, string?> values)
    {
        if (string.IsNullOrEmpty(template))
            return string.Empty;

        return PlaceholderRegex.Replace(template, match =>
        {
            var key = match.Groups["key"].Value;
            return values.TryGetValue(key, out var value) ? value ?? string.Empty : string.Empty;
        });
    }

    public IReadOnlyList<string> ExtractPlaceholders(string template)
        => PlaceholderRegex.Matches(template ?? string.Empty)
            .Select(x => x.Groups["key"].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
}

