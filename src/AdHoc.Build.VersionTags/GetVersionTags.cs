// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using BuildTask = Microsoft.Build.Utilities.Task;


namespace AdHoc.Build.VersionTags;

public class GetVersionTags : BuildTask
{
    [Output]
    public ITaskItem[]? Tags { get; set; }

    [Output]
    public bool DefineConstants { get; set; }

    [Output]
    public string? Constants { get; set; }

    [Output]
    public string? BuildVersion { get; set; }

    public override bool Execute()
    {
        if (BuildEngine4.GetRegisteredTaskObject(VersionTags.RegisteredKey, RegisteredTaskObjectLifetime.Build) is not VersionTags versionTags)
            return true;

        Tags = [.. versionTags.Tags.Select(t => new TaskItem(t))];
        DefineConstants = versionTags.DefineConstants;
        Constants = versionTags.Constants;
        BuildVersion = versionTags.BuildVersion;
        return true;
    }
}
