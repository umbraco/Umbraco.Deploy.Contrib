[![Build Status](https://dev.azure.com/umbraco/Umbraco%20Deploy%20Contrib/_apis/build/status/Umbraco%20Deploy%20Contrib%20v9%20(Continuous)?branchName=v10%2Fdev)](https://dev.azure.com/umbraco/Umbraco%20Deploy%20Contrib/_build/latest?definitionId=345&branchName=v10%2Fdev) [![NuGet release](https://img.shields.io/nuget/v/Umbraco.Deploy.Contrib.svg)](https://www.nuget.org/packages/Umbraco.Deploy.Contrib)

# Umbraco Deploy Contrib

This project contains community contributions for Umbraco Deploy targetted for version 8 and above.

Primarily this project offers connectors for the most popular Umbraco community packages - these are used by Deploy to aid with the deployment and transferring of content/property-data between environments on [Umbraco Cloud](https://umbraco.com/cloud).

## Branching

The main branches corresponding to the Umbraco and Umbraco Deploy major releases are:

- Umbraco 8/Deploy 4: `v4/dev`
- Umbraco 9/Deploy 9: `v9/dev`
- Umbraco 10/Deploy 10: `v10/dev`

## Connectors

This project offers Umbraco Deploy connectors for the following community packages:

- (none)

Value connectors for certain core property editors are also included:

- Block List
- Multi URL Picker
- Nested Content

---

## Getting Started

### Installation

When working with Umbraco 8, you can install the NuGet package using `Install-Package UmbracoDeploy.Contrib`.

For Umbraco 9+ the package is available for install by: `Install-Package Umbraco.Deploy.Contrib`

### Building and packaging

```powershell
dotnet restore Umbraco.Deploy.Contrib.sln
dotnet build Umbraco.Deploy.Contrib.sln --configuration Release --no-restore
dotnet pack Umbraco.Deploy.Contrib.sln --configuration Release --no-build -p:BuildProjectReferences=false --output build.out
```


---
## Contributing to this project

Anyone who wishes to get involved with this project is more than welcome to contribute. Please take a moment to review the [guidelines for contributing](CONTRIBUTING.md), this applies for any bug reports, feature requests and pull requests.

* [Bug reports](CONTRIBUTING.md#bugs)
* [Feature requests](CONTRIBUTING.md#features)
* [Pull requests](CONTRIBUTING.md#pull-requests)


### Issues

We encourage you to report any issues you might find with these connectors, so we can have them fixed for everyone!

When reporting issues with a connector it will help us a whole lot if you can reduce your report to being the absolute minimum required to encounter the error you are seeing.

This means try removing anything unnecessary or unrelated to the actual issue, from your setup and also try reducing the steps to reproduce, to only cover exactly what we would need to do in order to see the error you are getting.

Please use the [Umbraco Deploy Issue Tracker (for both Deploy and Deploy.Contrib)](https://github.com/umbraco/Umbraco.Deploy.Issues/issues).

## Credits

Special thanks to the following community members for assisting on this project.

* [Umbrella](https://github.com/UmbrellaInc) and [Lee Kelleher](https://github.com/leekelleher)
* [Matt Brailsford](https://github.com/mattbrailsford)

## License

Copyright &copy; 2016 Umbraco

Copyright &copy; 2016 Our Umbraco and [other contributors](https://github.com/umbraco/Umbraco.Deploy.Contrib/graphs/contributors)

Copyright &copy; 2014 Lee Kelleher, Umbrella Inc

Licensed under the [MIT License](LICENSE.md)
