<h1 style="text-align: center;"><img src="assets/logo.svg"/><br />IceCraft</h1>

IceCraft is a package manager that is powered by: 

- A framework for managing installation and lifetime of packages, and various caches and artefacts
- Various extensions directly consumes various metadata APIs

## Building

IceCraft currently requires .NET SDK **8.0** to build. You can get it [here](https://dot.net).

To produce your regular release build run `build.sh` / `build.cmd`. These should cover most use cases.

The `build.cmd` file contains only the invocation of the `dotnet` command that is required for building the project properly. You can try get your shell to execute it if none of the above options are usable.

## License

[GPL-3.0-or-later](COPYING)