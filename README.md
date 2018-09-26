# Mirepoix

[![Build Status](https://devdiv.visualstudio.com/DevDiv/_apis/build/status/Xamarin/Mirepoix)](https://devdiv.visualstudio.com/DevDiv/_build/latest?definitionId=9683)

> A mirepoix (/mɪərˈpwɑː/ meer-PWAH) is a flavor base made from diced
vegetables that are cooked, usually with butter or oil or other fat,
for a long time on a low heat without color or browning.

This project contains a handful of useful utility libraries for constructing
applications. All libraries target .NET Standard 2.0, but may have
platform-specific components.

Most of the functionality in this project has been derived from various
utility APIs within other projects within Xamarin, particularly
[Xamarin Workbooks](https://github.com/Microsoft/workbooks).

Please [browse the full documentation][docs] for more information on
consuming Mirepoix libraries.

## Use

_Mirepoix is currently distributed on [MyGet][myget] while it is still
establishing itself. It will move to nuget.org eventually._

Add the feed to your project's `NuGet.config` to reference packages.

### NuGet.config

```xml
<configuration>
  <packageSources>
    <add
      key="mirepoix"
      value="https://www.myget.org/F/mirepoix/api/v3/index.json"/>
  </packageSources>
</configuration>
```

## Hack

[.NET Core 2.1][dotnetcore] is used to build, test, and package Mirepoix.

### Build all Projects

```
dotnet build
```

### Package all NuGets

```
dotnet pack
```

### Write

* Open the root of the repository as a workspace in VS Code (`code .`) and
  ensure OmniSharp is using `mirepoix.sln`

* Or open `mirepoix.sln` in Visual Studio or Visual Studio for Mac.

### Test

It's best to simply run `dotnet xunit` at the root of the `src/*.Tests/`
directory to run scoped tests for the library of interest.

Unfortunately running `dotnet test` at the root of the repository fails
on both Windows and macOS locally, even though VSTS is happy to do it.

### CI

VSTS is used to produce official builds. See `.vsts-ci.yml`.

# Contributing

This project welcomes contributions and suggestions.  Most contributions
require you to agree to a Contributor License Agreement (CLA) declaring that
you have the right to, and actually do, grant us the rights to use your
contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine
whether you need to provide a CLA and decorate the PR appropriately (e.g.,
label, comment). Simply follow the instructions provided by the bot. You will
only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any
additional questions or comments.

[docs]: https://xamarin.github.io/mirepoix
[myget]: https://www.myget.org/feed/index/mirepoix
[dotnetcore]: https://www.microsoft.com/net/download