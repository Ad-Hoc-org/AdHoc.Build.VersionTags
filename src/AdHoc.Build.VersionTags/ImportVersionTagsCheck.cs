// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Text.RegularExpressions;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace AdHoc.Build.VersionTags;

public class ImportVersionTagsCheck : BuildTask
{
    public string? PackageId { get; set; }
    public string? OutputPath { get; set; }

    public ITaskItem[]? Items { get; set; }

    [Output]
    public ITaskItem[]? PackItems { get; set; }
    [Output]
    public ITaskItem[]? RemoveItems { get; set; }


    private string PackageTargetsPath => field ??= Path.Combine("build", $"{PackageId}.targets");
    private string PackageTransitiveTargetsPath => field ??= Path.Combine("buildTransitive", $"{PackageId}.targets");
    private string Import => field ??= $@"<Import Project=""../buildTransitive/{PackageId}.VersionTags.targets""/>";

    public override bool Execute()
    {
        if (PackageId is null)
        {
            Log.LogError($"{nameof(PackageId)} is required.");
            return false;
        }

        var packItems = new List<ITaskItem>();
        var removeItems = new List<ITaskItem>();

        try
        {
            Directory.CreateDirectory(Path.Combine(OutputPath!, "buildTransitive"));
        }
        catch { }

        var targets = Path.Combine(OutputPath!, $"buildTransitive/{PackageId}.VersionTags.targets");
        packItems.Add(new TaskItem(targets, new Dictionary<string, string>()
        {
            ["PackagePath"] = "buildTransitive",
            ["Pack"] = "true"
        }));
        File.WriteAllText(targets, $@"<Project>
	<UsingTask TaskName=""AdHoc.Build.VersionTags.CheckVersionTags"" AssemblyFile=""../tools/AdHoc.Build.VersionTags.Transitive.dll"" />

	<Target Name=""CheckVersionTags_{PackageId.Replace('.', '_')}"" AfterTargets=""ResolvePackageAssets"">
		<AdHoc.Build.VersionTags.CheckVersionTags PackageId=""{PackageId}"" Version=""@(ResolvedCompileFileDefinitions->WithMetadataValue('NuGetPackageId', '{PackageId}')->Metadata('NuGetPackageVersion'))"" />
	</Target>
</Project>");

        var packageTargets = GetPackageTargets().ToArray();
        foreach (var target in new string[] { PackageTargetsPath, PackageTransitiveTargetsPath })
        {
            var existing = packageTargets.FirstOrDefault(p => p.Item2.Equals(target, StringComparison.InvariantCultureIgnoreCase));
            string? content = null;
            if (existing != default)
            {
                content = GetAppendContent(existing.Item1);
                if (content is not null)
                    removeItems.Add(new TaskItem(existing.Item3));
            }
            var importTargets = Path.Combine(OutputPath!, target);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(importTargets)!);
            }
            catch { }
            File.WriteAllText(importTargets, content ?? $"<Project>{Import}</Project>");
            packItems.Add(new TaskItem(importTargets, new Dictionary<string, string>
            {
                ["PackagePath"] = target.StartsWith("buildTransitive") ? "buildTransitive" : "build",
                ["Pack"] = "true"
            }));
        }

        PackItems = [.. packItems];
        RemoveItems = [.. removeItems];
        return true;
    }

    private string? GetAppendContent(string? path)
    {
        if (path is null) return null;

        var content = File.ReadAllText(path);
        var regex = new Regex(@"</\s*Project\s*>", RegexOptions.IgnoreCase);
        var matches = regex.Matches(content);
        if (matches.Count == 0)
            return null;

        return content.Insert(matches[0].Index, Import);
    }

    private IEnumerable<(string, string, ITaskItem)> GetPackageTargets() =>
        Items?.Where(i =>
            bool.TryParse(i.GetMetadata("Pack"), out var result) && result
        ).SelectMany(i =>
        {
            var identity = i.GetMetadata("Identity");
            var paths = i.GetMetadata("PackagePath");
            return paths.Split(';')
                .Select(path => (identity, Path.Combine(path, Path.GetFileName(identity)), i));
        }).Where(i =>
            i.Item2.Equals(PackageTargetsPath, StringComparison.InvariantCultureIgnoreCase)
                || i.Item2.Equals(PackageTransitiveTargetsPath, StringComparison.InvariantCultureIgnoreCase)
        ) ?? [];
}
