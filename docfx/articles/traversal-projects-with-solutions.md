# MSBuild Traversal Projects with Solution Generation

The [`Microsoft.Build.Traversal`](https://github.com/Microsoft/MSBuildSdks/tree/master/src/Traversal)
MSBuild project SDK allows project tree owners the ability to define what
projects should be built. Visual Studio solution files are more targeted for
end-users and are not good for build systems. Additionally, large project
trees usually have several Visual Studio solution files scoped to different
parts of the tree.

In an enterprise-level build, you want to have a way to control what projects
are a built in your hosted build system. Traversal projects allow you to
define a set of projects at any level of your folder structure and can be
built locally or in a hosted build environment.

To supplement traversal projects, the `Xamarin.MSBuild.Sdk` MSBuild project
SDK adds a `GenerateSolution` target. This allows the traversal project to
define the canonical build _and_ generate solutions suitable for loading into
IDEs - without having to maintain both the traversal project, solutions, or
configuration mappings.

## Defining Solutions

Solutions support mapping an outer-level solution configuration to inner-level
project configurations for each project in the solution. This mapping is
typically maintained in the IDE as the solution format is very tedious to
update by hand. Because of this solutions often become out of sync when using
non-solution-driven build definitions, such as traversal projects.

With `Xamarin.MSBuild.Sdk`'s `GenerateSolution` target however, defining these
mappings directly in the traversal project is easy, and even supports
solution folders for nicer in-IDE project tree grouping.

A traversal project that generates a solution must import the SDK and must
have at least one `<SolutionConfiguration>` item.

```xml
<Project Sdk="Microsoft.Build.Traversal">
  <Sdk Name="Xamarin.MSBuild.Sdk"/>

  <ItemGroup>
    <SolutionConfiguration Include="Debug"/>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="SomeProject.csproj"/>
  </ItemGroup>
</Project>
```

When `msbuild /t:GenerateSolution` is run against the traversal project, a
solution will be generated that includes a `Debug|Any CPU` solution
configuration that maps to building `SomeProject.csproj` setting `/p:Configuration=Debug` and `/p:Platform=AnyCPU`

### The `<SolutionConfiguration>` MSBuild Item

Any number of these items may be defined in the traversal project. Its
`Include` attribute defines the list of solution configurations and each
should take the form of `$(Configuration)|$(Platform)` per standard solution
syntax. Configuration and platform values are always case insensitive.

Note that the `|$(Platform)` component may be omitted which will imply
`Any CPU`. Additionally, `Any CPU` and `AnyCPU` are treated as equal.

The following three `<SolutionConfiguration>` items are identical and
would only result in a single actual solution configuration at the time
of generation:

```xml
<ItemGroup>
  <SolutionConfiguration Include="Debug"/>
  <SolutionConfiguration Include="Debug|Any CPU"/>
  <SolutionConfiguration Include="Debug|anycpu"/>
</ItemGroup>
```

Each item may provide metadata that influences the configuration. These
metadata properties will be passed to `msbuild` when evaluating the traversal
project. Any number of properties may be provided as metadata. This is best
demonstrated by example.

#### Multi-platform Build and Solution

```xml
<Project Sdk="Microsoft.Build.Traversal">
  <Sdk Name="Xamarin.MSBuild.Sdk"/>

  <!-- Set platform helper properties -->
  <PropertyGroup>
    <IsWindows>$([MSBuild]::IsOSPlatform('Windows'))</IsWindows>
    <IsMac>$([MSBuild]::IsOSPlatform('OSX'))</IsMac>
  </PropertyGroup>

  <ItemGroup>
    <SolutionConfiguration Include="macOS Debug">
      <!-- Define the Configuration and Platform to be passed to projects -->
      <Configuration>Debug</Configuration>
      <Platform>x64</Platform>

      <!-- Override the platform helper properties via /t:GenerateSolution -->
      <IsMac>true</IsMac>
      <IsWindows>false</IsWindows>
    </SolutionConfiguration>

    <SolutionConfiguration Include="Windows Debug">
      <!-- Define the Configuration and Platform to be passed to projects -->
      <Configuration>Debug</Configuration>

      <!-- Override the platform helper properties via /t:GenerateSolution -->
      <IsMac>false</IsMac>
      <IsWindows>true</IsWindows>
    </SolutionConfiguration>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="CrossPlatformProject.csproj"/>
  </ItemGroup>

  <ItemGroup Condition="$(IsMac)">
    <ProjectReference Include="MacOnlyProject.csproj"/>
  </ItemGroup>

  <ItemGroup Condition="$(IsWindows)">
    <ProjectReference Include="WindowsOnlyProject.csproj"/>
  </ItemGroup>
</Project>
```

When the `GenerateSolution` target is run, a solution with a `macOS Debug`
and a `Windows Debug` configuration will be generated.

| Project | Built in `macOS Debug` | Built in `Windows Debug`
|:-|:-|:-|
| CrossPlatformProject.csproj | ✓ _(as `Debug|x64`)_ | ✓ _(as `Debug|AnyCPU`)_ |
| MacOnlyProject.csproj | ✓ _(as `Debug|x64`)_ | |
| WindowsOnlyProject.csproj | | ✓ _(as `Debug|AnyCPU`)_ |

### Additions to the `<ProjectReference>` MSBuild Item

The `GenerateSolution` target supports the following optional metadata
properties:

| Property | Description | Example&nbsp;Value |
|:-|:-|:-|
| `<Configuration>` | Overrides the value from `<SolutionConfiguration>` for this project only. | `AppStore` |
| `<Platform>` | Overrides the value from `<SolutionConfiguration>` for this project only. | `Arm64` |
| `<SolutionFolder>` | A relative virtual path for grouping this project in the solution. Either `\` or `/` may be used as a path separator. | `A\B\C` |

```xml
<Project Sdk="Microsoft.Build.Traversal">
  <Sdk Name="Xamarin.MSBuild.Sdk"/>

  <ItemGroup>
    <ProjectReference Include="…">
      <SolutionFolder>Client Applications</SolutionFolder>
    </ProjectReference>
  </ItemGroup>

  …
</Project>
```

### Generate Solution Automatically after Build

To avoid having to explicitly run the `/t:GenerateSolution` target, set the
`GenerateSolutionAfterBuild` to `true`. Doing so will run the target
automatically after a successful build.

```xml
<Project Sdk="Microsoft.Build.Traversal">
  <Sdk Name="Xamarin.MSBuild.Sdk"/>

  <PropertyGroup>
    <GenerateSolutionAfterBuild>true</GenerateSolutionAfterBuild>
  </PropertyGroup>

  …
</Project>
```

## Rationale for Solution Generation

While this may change in the future, the solution generator specifically
ignores any changes made to the solution itself (e.g. configuration changes
introduced via the IDE or extra metadata the IDE may add to the solution).

This is because the canonical definition of the build _should_ come from the
traversal project itself. A solution is simply a scoped view of the project
to drive the development experience in the IDE.

### Always discard solution edits/changes made by the IDE

Only changes to solution files made as a result of running
`/t:GenerateSolution` should be committed to the repository. Discard any
edits/changes made by the IDE.

## GUIDs

Solutions define the shape of the project tree based on the mapping of
two GUIDs. In older MSBuild projects, each project had a unique
`<ProjectGuid>` property. These GUIDs were reused to reference projects
in the solution and map their configurations.

With the advent of SDK-style projects however, `<ProjectGuid>` is optional
and discouraged. It is an artifact of the solution structure itself.

However, since a GUID _is_ needed _inside_ the solution, we have to provide
one. If we were to use the .NET `Guid.NewGuid` method to create one, the
solution would change _on every single generation_, because this method
creates a version 4 random GUID.

`/t:GenerateSolution` solves this by using
[version 5 SHA-1 hashed GUIDs](xref:Xamarin.GuidHelpers.GuidV5(System.Guid,System.String))
instead, which are stable: given a constant namespace GUID and a value (in
this case, the _path to the project file relative to the solution file_), a
version 5 GUID will always produce the same value and thus result in a
solution file that does not change on each generation.

> [!NOTE]
> If a project _does_ happen to provide an explicit `<ProjectGuid>`,
> that value is used instead of creating a hashed GUID based on the path.

### Example

```csharp
using static Xamarin.GuidHelpers;

var itemGuid = GuidV5 (
  // constant namespace
  new Guid ("{17ad6350-380a-4d65-9b2c-aa44b5da8111}"),
  // path to project relative to solution with normalized separators
  @"path\to\project.csproj".Replace ('\\', '/')
);

// itemGuid will always be '{5984500c-0dbf-5c42-947b-c6674ccdbe30}'
```