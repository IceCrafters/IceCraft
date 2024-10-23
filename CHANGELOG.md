# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Added the `build` command to verify a Mashiro package script.

#### API additions

- Added `IPluginExtension` interface to the API.
- The client now provides an implementation to a new interface `IExtensibleClient`.
  - This interface allow adding custom commands to the IceCraft client.
    - No reference to `System.CommandLine` is required.
  - Not available for dependency injection but is passed to `IPluginExtension` initialisation method.

## [0.1.0-alpha.1] - 2024/10/22

- Initial release.

[Unreleased]: https://gitlab.com/Icecrafters/IceCraft/-/compare/v0.1.0-alpha.1...HEAD
[0.1.0-alpha.1]: https://gitlab.com/Icecrafters/IceCraft/-/tree/v0.1.0-alpha.1