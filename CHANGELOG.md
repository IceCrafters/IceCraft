# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

**NOTE**: Changes marked with 💥 are _breaking changes_.

### Added

- Added support for using `ICECRAFT_CACHE_HOME` environment variable to
  override the default cache directory.
- Added support for using `ICECRAFT_CONFIG_HOME` environment variable to
  override the default configuration directory.

### Changed

- Artefacts will now be saved at the cache directory.
- 💥 `dotnet-config`-based config system is now replaced with a JSON config
  system.
- The default cache directory for *nix systems (except macOS) have been
  changed to `$XDG_CACHE_HOME/IceCraft.d`.
  - By default, this will be `~/.cache/IceCraft.d`.
- 💥 The default configuration directory has changed.
  - On *nix systems, this becomes `$XDG_CONFIG_HOME/IceCraft.d`.
    - By default, this will be `~/.config/IceCraft.d`.
  - On Windows, this will become `%USERPROFILE%/AppData/Roaming/IceCraft.d`.

### Fixed

- Fixed an issue resulted in unknown sub-command errors won't be reported to
  user.

## [0.1.0-alpha.2] - 2024/10/25

### Added

- Added the `build` command to verify a Mashiro package script.

#### API additions

- Added `IPluginExtension` interface to the API.
- The client now provides an implementation to a new interface `IExtensibleClient`.
  - This interface allow adding custom commands to the IceCraft client.
    - No reference to `System.CommandLine` is required.
  - Not available for dependency injection but is passed to `IPluginExtension` initialisation method.

### Changes

- Updated dependencies.
  - Jint was updated to `4.0.3`.
  - `Microsoft.Extensions.DependencyInjection` was updated to `8.0.1`.
- The CSR extension is changed to use GitHub for fetching the repository information.

### Removed

- Removed dependency on `Downloader`.
- **API:** The `Downloader`-based `DownloadManager` is no longer available.

## [0.1.0-alpha.1] - 2024/10/22

- Initial release.

[Unreleased]: https://github.com/Icecrafters/IceCraft/compare/v0.1.0-alpha.2...HEAD
[0.1.0-alpha.2]: https://github.com/Icecrafters/IceCraft/compare/v0.1.0-alpha.1...v0.1.0-alpha.2
[0.1.0-alpha.1]: https://github.com/Icecrafters/IceCraft/releases/tag/v0.1.0-alpha.1
