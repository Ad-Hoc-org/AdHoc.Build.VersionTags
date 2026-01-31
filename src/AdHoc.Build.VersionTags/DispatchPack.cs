// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.Build.Framework;
using BuildTask = Microsoft.Build.Utilities.Task;


namespace AdHoc.Build.VersionTags;

public class DispatchPack : BuildTask
{
    [Required]
    public string? ProjectFile { get; set; }
    [Required]
    public string? PackageId { get; set; }

    public string? VersionTags { get; set; }
    public ITaskItem[]? VersionTagItems { get; set; }

    public override bool Execute()
    {
        if (PackageId is null)
        {
            Log.LogError($"{nameof(PackageId)} is required.");
            return false;
        }

        if (ProjectFile is null)
        {
            Log.LogError($"{nameof(ProjectFile)} is required.");
            return false;
        }
        VersionTags ??= string.Empty;
        VersionTagItems ??= [];

        HashSet<string> versionTags = [.. VersionTagItems.Select(item =>
            string.Join("-", ParseVersionTags.ParseTags(item.ItemSpec, Log)?.OrderBy(t => t).ToArray() ?? [])
        )];
        if (!string.IsNullOrWhiteSpace(VersionTags))
            versionTags.Add("");
        versionTags.Remove(string.Join("-", ParseVersionTags.ParseTags(VersionTags, Log)?.OrderBy(t => t).ToArray() ?? []));

        foreach (var tags in versionTags)
            if (!BuildEngine.BuildProjectFile(
                ProjectFile,
                ["Pack"],
                new Dictionary<string, string>
                {
                    ["VersionTags"] = tags,
                    ["_IsVersionTagsPack"] = "true"
                },
                new Dictionary<string, object>()
            ))
                return false;

        return true;
    }
}
