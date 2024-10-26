<h1 style="text-align: center;"><img src="assets/logo.svg" alt="IceCraft logo"/><br />IceCraft</h1>

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/IceCrafters/IceCraft/dotnet.yml?style=flat-square&logo=github&link=https%3A%2F%2Fgithub.com%2FIceCrafters%2FIceCraft%2Factions%2Fworkflows%2Fdotnet.yml)
![Sonar Quality Gate](https://img.shields.io/sonar/quality_gate/IceCrafters_IceCraft?server=https%3A%2F%2Fsonarcloud.io&style=flat-square&logo=sonarcloud&link=https%3A%2F%2Fsonarcloud.io%2Fproject%2Foverview%3Fid%3DIceCrafters_IceCraft)
![Sonar Coverage](https://img.shields.io/sonar/coverage/IceCrafters_IceCraft?server=https%3A%2F%2Fsonarcloud.io&style=flat-square&logo=sonarcloud)
![GitHub commit activity](https://img.shields.io/github/commit-activity/m/IceCrafters/IceCraft?style=flat-square)

IceCraft is a package manager that consists of: 

- A framework for managing installation and lifetime of packages
- Additional services that handle download and storage of package atrefacts,
  as well as other miscellaneous services
- Various extensions directly consumes various metadata APIs
- as well as a CSR  extension that provides building source packages directly 
  from their original tarballs

## Building

IceCraft currently requires .NET SDK **8.0** to build. You can get it [here](https://dot.net).

To produce installable builds, use `scripts/build-prepped`. Additional
instructions are available [here](BUILDING.md).

## Contributing

Contributions are welcome. You can report issues, suggest features or even
submit code to this project!

View the [Contributors' Guide](CONTRIBUTING.md) for details.

## License

[GPL-3.0-or-later](COPYING)