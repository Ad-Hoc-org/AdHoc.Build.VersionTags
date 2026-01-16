# Version Tags

A MSBuild task library that enables **feature-specific versioning** through tag-based package variants.
Version Tags allow you to create multiple NuGet packages from the same codebase with different feature sets,
optimized size, and clear dependency resolution.

## What It Does

Version Tags provides a build-time system for:
- **Conditional Feature Compilation**: Define features through tags that generate compile-time constants
- **Semantic Version Enhancement**: Automatically append build metadata to package versions based on active tags
- **Feature Discovery**: Enable consumers to determine which features are included in a package variant
- **Minimal Dependencies**: Only include framework-specific dependencies when features require them

## Why Use Version Tags?

### Traditional Approaches vs. Version Tags

| Strategy | Problem | Version Tags Solution |
|----------|---------|----------------------|
| **Single Package** | Forces all consumers to download unused features and dependencies | Create lean variants with only needed features |
| **Multiple Projects** | Code duplication, maintenance overhead, divergence risk | Single codebase with conditional compilation |
| **Build Configurations** | Manual management, unclear versioning, poor NuGet support | Automatic versioning with semantic resolution |
| **Preprocessor Directives Only** | No runtime discovery, unclear capabilities | Tags embedded in metadata and constants |

## Key Benefits

### 1. **Semantic Versioning Compliance with NuGet**
NuGet's strict semantic versioning means package resolution relies on version numbers.
Version Tags solves this by using the **build version** component with `{Version}.{TagCount}-{tag1}-{tag2}...` to indicate higher version and feature richness:
`1.0.0, 1.0.0.1-tag0, 1.0.0.1-tag1, 1.0.0.2-tag0-tag1`

### 2. **Minimal Package Size**
Reduce download overhead by creating purpose-built packages:
- Production package excludes debug symbols and verbose logging
- ASP.NET-specific variant only includes `IResponseMetaDataProvider` when needed
- Framework-specific implementations loaded conditionally

### 3. **Clear Feature Communication**
```xml
<VersionTags>aspnet;debug</VersionTags>
```

Generates:
- **Constants**: `FEATURE_{uppercase tag}` like `FEATURE_DEBUG`, `FEATURE_ASPNET`
- **Package Version**: `1.0.0.2-aspnet-debug`
- **Compile-time Optimization**: Dead code elimination for unused features

## Usage

### 1. Install the Package
```xml
<PackageReference Include="AdHoc.Build.VersionTags" PrivateAssets="all" />
```

### 2. Define Version Tags

In your `.csproj`:
```xml
<PropertyGroup>
    <Version>1.0.0</Version>
    <VersionTags>debug;aspnet</VersionTags>
</PropertyGroup>
```
Result: Package version becomes `1.0.0.2-aspnet-debug`.

Add required references conditionally:
```xml
<ItemGroup Condition="$(VersionTags.Contains('aspnet'))">
	<FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

### 3. Use in Code
```csharp
public partial class MyResult { /* ... */ }
#if FEATURE_ASPNET
public partial class MyResult : Microsoft.AspNetCore.Http.IResult  { /* ... */ }
#endif
```

### 4. Build and Pack
Run your build as usual with your tags specified:
```bash
dotnet [build|pack] /p:VersionTags=
```
Because of msbuild caching behavior, you may need to clean first:
```bash
dotnet clean
```

## Tag Naming Conventions

### Rules
- **Lowercase alphanumeric only**: `a-z`, `0-9`
- **Separators**: `;`, `,`, or `-` between tags
- **Automatic sorting**: Tags are ordered alphabetically in version string


## Contributing

If you have suggestions for how this project could be improved, or want to report a bug, open an issue! We'd love all and any contributions.

For more, check out the [Contributing Guide](CONTRIBUTING.md).

## License

This project is licensed under the MIT License - see the LICENSE file for details.
