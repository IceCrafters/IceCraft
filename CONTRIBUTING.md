# Contributor Guidelines

First, thank you for your interest on contributing to the IceCraft Package
Manager project.

In this document, you will learn the conventions and guidance on how to report
issues, submitting code and other contributions to the project. All
contributors are expected read and understand this document; but if you think
there is a part of this document too hard to understand or with unclear
meanings, feel free to reach out for help.

## Reporting issues

Sometimes IceCraft may not work correctly as expected; you may something useful
to be added to IceCraft; moreover, sometimes there are techinical issues within
IceCraft that may become problematic.

These requests and reports above can be reported to the [Issues section](https://github.com/icecrafters/IceCraft/issues).

### Requesting features

If you want to request a feature or improvement, there are two templates
available. A feature _request_ and feature _proposal_. The former is intended
for non-technical users to request a feature to be implemented, while the
latter is intended for technical users and maintainers to propose something
to be done to the IceCraft codebase.

### Reporting bugs

When reporting bugs, please include the following information:

- Your operating system and architecture
- Your IceCraft version (if you got from Git, the commit ID).
- The output of IceCraft
- What repositories are you using and what package are you installing/removing/reconfiguring, etc.
- Exact steps to reproduce the bug

## Contributing Code

Thanks for your interest on contributing code to this project.

Before you get started, you might want to be familiar with the [build process](BUILDING.md)
and the [Developer Guide](HACKING.md). These provides reference on how IceCraft
is build and tested, what dependencies are required to build IceCraft, and more.

Get yourself fimiliar to the build process by cloning and trying out locally.
Once you are fimiliar with the build and the development process, you can then
proceed.

### Forking the repository

You can start by [forking the IceCraft repository](https://github.com/icecrafters/IceCraft/fork).
This will create a copy of the repository under your own personal namespace
or any group that you are permitted to create repositories.

### Making changes

You can then clone your fork of IceCraft repository and starting making changes.

| 💬 **Note** |
| ----------- |
| It is not recommended to use GitPod with its default VSCode-based editor. |

Please note that it is best to [GPG-sign your commits](https://docs.github.com/en/authentication/managing-commit-signature-verification/signing-commits),
and you must agree to the [Developer's Certificate of Origin](https://developercertificate.org)
by sign-off your commits. The workspace configuration for VSCode-based editors
is set to enable sign-off.

You are recommended to follow the commit convention as described in the [Developer Guide](HACKING.md).
This way, the commit history looks tidy.

### Submitting a Pull Request

Once you are done with your changes, you can submit a Pull Request to this
repository. This will allow your changes to be reviewed.

| 💬 **Note** |
| ----------- |
| Please do not surprise us with big pull requests. Create an issue first if you are looking to implement major changes. |

Your pull request will be reviewed by a maintainer. If there is no response,
wait patiently. If there hasn't been any response for a week, please nag a
maintainer about it.
