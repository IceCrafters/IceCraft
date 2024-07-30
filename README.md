# IceCraft

IceCraft is a "syndicate source" package manager - that is, instead of a single central source or multiple sources of same format, it relies on extensions fetching from various public APIs and maybe even VCS endpoints and translate them to be accepted through a standard interface. Thus, no manual maintainence is needed as long as upstream is up to date.

## Building

To produce your regular release build run `build.sh` / `build.cmd`. These should cover most use cases.

The `build.cmd` file contains only the invocation of the `dotnet` command that is required for building the project properly. You can try get your shell to execute it if none of the above options are usable.

## License

[GPL-3.0-or-later](COPYING)