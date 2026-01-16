// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.Build.Framework;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace AdHoc.Build.VersionTags;

public class GetTagsBuildVersion : BuildTask
{
    [Required]
    public string? Tags { get; set; }

    public bool WithVersionSeparator { get; set; } = false;

    [Output]
    public string? BuildVersion { get; set; }

    public override bool Execute()
    {
        var tags = RegisterVersionTags.ParseTags(Tags, Log);
        if (tags is null)
            return false;
        BuildVersion = GetBuildVersion(tags);
        if (WithVersionSeparator && BuildVersion.Length > 0)
            BuildVersion = $".{BuildVersion}";
        return true;
    }

    public static string GetBuildVersion(HashSet<string> tags) =>
        tags.Count == 0 ? string.Empty : $"{tags.Count}-{string.Join("-", tags.OrderBy(t => t))}";
}
