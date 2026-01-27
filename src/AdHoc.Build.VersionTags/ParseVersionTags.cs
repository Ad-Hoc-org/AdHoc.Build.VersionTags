// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using BuildTask = Microsoft.Build.Utilities.Task;


namespace AdHoc.Build.VersionTags;

public class ParseVersionTags : BuildTask
{
    [Output]
    public string? VersionTags { get; set; }

    [Output]
    public ITaskItem[]? VersionTagItems { get; set; }

    [Output]
    public string? Constants { get; set; }

    [Output]
    public string? BuildVersion { get; set; }

    public override bool Execute()
    {
        var sortedTags = ToSortedTags(VersionTags);
        if (sortedTags is null)
            return false;

        VersionTags = string.Join(";", sortedTags);
        Constants = string.Join(";", sortedTags.Select(t => $"FEATURE_{t.ToUpper()}"));
        BuildVersion = GetBuildVersion(sortedTags, sorted: true);

        if (VersionTagItems is not null)
            foreach (var item in VersionTagItems)
            {
                var tags = ToSortedTags(item.ItemSpec);
                if (tags is null)
                    continue;
                item.ItemSpec = string.Join("-", tags);
            }
        return true;
    }

    private string[]? ToSortedTags(string? versionTags)
    {
        var tags = ParseTags(versionTags, Log);
        if (tags is null)
            return null;

        return tags.OrderBy(t => t).ToArray();
    }

    public static HashSet<string>? ParseTags(string? tags, TaskLoggingHelper log)
    {
        HashSet<string> parsed = [..tags?.Split([';', ',', '-'], StringSplitOptions.RemoveEmptyEntries)?
            .Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim().ToLower()) ?? []];
        if (parsed.Count == 0)
            return parsed;

        var regex = new Regex(@"^[a-z0-9]+$", RegexOptions.Compiled);
        var invalidTags = parsed.Where(t => !regex.IsMatch(t)).ToArray();
        if (invalidTags.Length > 0)
        {
            log.LogError($"Version tags may only contain lowercase alphanumeric characters (a-z, 0-9): {string.Join(", ", invalidTags)}");
            return null;
        }

        return parsed;
    }

    public static string GetBuildVersion(ICollection<string> tags, bool sorted = false) =>
        tags.Count == 0 ? string.Empty : $"{tags.Count}-{string.Join("-", sorted ? tags : tags.OrderBy(t => t))}";
}
