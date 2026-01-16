// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.Build.Framework;
using System.Text.RegularExpressions;
using BuildTask = Microsoft.Build.Utilities.Task;


namespace AdHoc.Build.VersionTags;

public class CheckVersionTags : BuildTask
{
    private const string VersionRegex = @"\d+.\d+.\d+[^""]*.(\d+)-([^""]+)";
    private const int TagsIndex = 2;
    private const int BuildIndex = 1;

    public string? AssetsFile { get; set; }

    [Required]
    public string? PackageId { get; set; }

    public string? Version { get; set; }

    public override bool Execute()
    {
        if (string.IsNullOrWhiteSpace(PackageId))
            return false;

        if (string.IsNullOrWhiteSpace(Version))
        {
            Log.LogError($"{PackageId} package version could not be resolved.");
            return false;
        }

        AssetsFile ??= "obj/project.assets.json";
        HashSet<string> tags = [.. new Regex($@"""{Regex.Escape(PackageId)}"":\s*""{VersionRegex}""")
            .Matches(File.ReadAllText(AssetsFile)).Cast<Match>()
            .SelectMany(m => m.Groups[TagsIndex].Value.Split(['-'], StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim().ToLower())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .OrderBy(t => t) ];
        if (tags.Count == 0)
            return true;

        var versionMatch = new Regex($"^{VersionRegex}$").Match(Version);
        if (!versionMatch.Success)
        {
            Log.LogError($"{PackageId} package version {Version} does not match expected format.");
            return false;
        }

        var versionTags = versionMatch.Groups[TagsIndex].Value
            .Split(['-'], StringSplitOptions.RemoveEmptyEntries);

        var missingTags = tags.Where(t => !versionTags.Contains(t)).ToArray();
        if (missingTags.Length > 0)
        {
            HashSet<string> allTags = [.. missingTags, .. versionTags];
            Log.LogError($@"{PackageId} package version {Version} does not contain tags: {string.Join(", ", missingTags)}
Expected version: {Version!.Substring(0, versionMatch.Groups[BuildIndex].Index)}{allTags.Count}-{string.Join("-", allTags)}");
            return false;
        }

        return true;
    }
}
