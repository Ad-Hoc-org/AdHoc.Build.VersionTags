// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

namespace AdHoc.Build.VersionTags;

public class VersionTags
{
    public const string RegisteredKey = "AdHoc.Build.VersionTags";
    public HashSet<string> Tags { get; set; } = [];
    public bool DefineConstants { get; set; }
    public string? Constants { get; set; }
    public string? BuildVersion { get; set; }
}
