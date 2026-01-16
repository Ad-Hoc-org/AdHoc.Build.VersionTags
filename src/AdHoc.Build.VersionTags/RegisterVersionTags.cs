// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace AdHoc.Build.VersionTags;

public class RegisterVersionTags : BuildTask
{
    public bool DefineConstants { get; set; }
    public string? Tags { get; set; }

    public override bool Execute()
    {
        HashSet<string>? tags = ParseTags(Tags, Log);
        if (tags is null)
            return false;

        BuildEngine4.RegisterTaskObject(VersionTags.RegisteredKey, new VersionTags
        {
            Tags = [.. tags],
            DefineConstants = DefineConstants,
            BuildVersion = GetTagsBuildVersion.GetBuildVersion(tags),
            Constants = string.Join(";", tags.Select(t => $"FEATURE_{t.ToUpper()}"))
        }, RegisteredTaskObjectLifetime.Build, allowEarlyCollection: false);
        return true;
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
}
