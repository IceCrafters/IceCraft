# Development Guide

This documentation explains the codebase of IceCraft, a package management
software. If you want to contribute code to this project, please also read the
[Contributors' Guide](CONTRIBUTING.md) as well.

## Requirements

- You need a .NET 8 SDK (the feature band doesn't matter).
- As IceCraft is developed on GNU/Linux, the best way to develop IceCraft is
  either develop on Linux or use Visual Studio with WSL.

## Architecture

The IceCraft application is made of three primary assemblies:

- `IceCraft`, the client/frontend application that users interact with
- `IceCraft.Api`, the interface definition of IceCraft services
- `IceCraft.Core`, the service implementations of IceCraft

The three assemblies interact with each other primarily through a DI container,
which in this case, is Autofac.

The main repository also contains several extensions and plugins. Plugins
work with the core application through the `IceCraft.Api.Plugin` assembly,
which provides interfaces for plugins to inject and compose services. Plugins
doesn't reference `IceCraft.Core`; they use DI container to get implementations
of `IceCraft.Api` interfaces, which `IceCraft.Core` happens to provide.

Plugins don't have access to Autofac; they must use `Microsoft.Extensions.DependencyInjection`
interfaces instead.

## Principles

### Testability

In an ideal situation, unit testing IceCraft don't require us to mock many
interfaces, and we don't have to mock `IServiceProvider`.

However, there are currently patterns present in IceCraft that makes
testing and mocking difficult. These includes Service Locator, and a few other
problems.

#### Service Locator

In our case Service Locator should be avoided because that impedes the 
implementation of testing. That said, in testing cases, a Poor Man's
implementation should work and all services should be properly declared through
constructors. No property injection is allowed.

| 💬 Note |
| ------- |
| Service location pattern basically means the service requesting another service locates a service themselves rather than passing the task to the client. It makes your implementations painful to mock without a DI container. |

#### Too many dependencies

One other case you also need to avoid is that when a service have too many
dependencies. If you have a class that needs more than 7 services you generally
need to reconsider that can the service be split up?

One recent example is `PackageInstallManager` implementation which had many
dependencies. By splitting installation tasks to `PackageSetupAgent`, the
amount of services requried is greatly reduced.

## Testing IceCraft

### Testing the client

If you want to test the client, the best tool is `scripts/runicr`. It sets the
IceCraft instance to the `gitignore`-d `run` directory, configures IceCraft to
not generate `PATH` and environment variable scripts, and then invokes
`dotnet run`.

If you need to attach a debugger, simply add `--debug` option. This causes
IceCraft to wait for an input before doing anything else which you can attach
a debugger there, then press any key.

By default, "known" exceptions only have their `Message` shown. You can add a
`--verbose` option to show the full stack trace and inner exception information.

The `scripts/runicr` script may only be invoked from repository root.

## Hacking the shell scripts

### Shell script principles

- All shell scripts should not include bashisms. They intends to work on FreeBSD.

### Testing installer

| 💬 Note |
| ------- |
| The IceCraft installer is currently only available for Linux, but may work for FreeBSD (the install script is checked of bashisms). |

First, create a directory under your `HOME`. For this instance, we will use
`test-home-icr`. After [building the prepped archive](BUILDING.md#building-installable-prepped-archive),
use the following command:

```sh
HOME="$HOME/test-home-icr" bin/prep/install.sh
```

This will cause IceCraft to be installed to `$HOME/test-home-icr` rather than `$HOME`.