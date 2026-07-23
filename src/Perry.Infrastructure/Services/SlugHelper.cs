using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Perry.Infrastructure.Services;

/// <summary>URL-slug из названия (homework AdminEdit generate-slug).</summary>
public static class SlugHelper
{
    public static string FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "item";

        var normalized = name.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(c))
                sb.Append(c);
            else if (char.IsWhiteSpace(c) || c is '-' or '_')
                sb.Append('-');
        }

        var slug = Regex.Replace(sb.ToString(), "-{2,}", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "item" : slug;
    }

    public static string Unique(string baseSlug, Func<string, bool> exists)
    {
        var slug = string.IsNullOrWhiteSpace(baseSlug) ? "item" : baseSlug.Trim().ToLowerInvariant();
        if (!exists(slug))
            return slug;

        for (var i = 2; i < 1000; i++)
        {
            var candidate = $"{slug}-{i}";
            if (!exists(candidate))
                return candidate;
        }

        return $"{slug}-{Guid.NewGuid():N}"[..20];
    }
}
