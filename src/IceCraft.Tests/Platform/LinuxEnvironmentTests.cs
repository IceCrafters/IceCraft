// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests.Platform;

using System.IO.Abstractions.TestingHelpers;
using System.Runtime.Versioning;
using IceCraft.Api.Platform;
using IceCraft.Core.Platform.Linux;
using IceCraft.Tests.Helpers;
using Moq;
using Xunit.Abstractions;

[SupportedOSPlatform("linux")]
public class LinuxEnvironmentTests
{
    private readonly FrontendAppHelper _frontend;

    public LinuxEnvironmentTests(ITestOutputHelper outputHelper)
    {
        _frontend = new FrontendAppHelper(outputHelper);
    }

    [LinuxFact]
    public void AddSimpleVariable_EnvScript()
    {
        var envProvider = new Mock<IEnvironmentProvider>();
        
        envProvider.Setup(x => x.GetUserProfile())
            .Returns("/home");

        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("/home");
        fileSystem.AddDirectory("/tmp/__icecraft_test_temp/");
        fileSystem.AddEmptyFile("/home/.zshrc");

        var envManager = new LinuxEnvironmentManager(_frontend,
            fileSystem,
            envProvider.Object);

        // Act
        envManager.AddUserVariable("A", "B");

        // Assert
        var envs = fileSystem.GetFile("/home/.ice_craft_envs");

        Assert.Contains("export A='B'", envs.TextContents);
    }

    [LinuxTheory]
    [InlineData(".bash_profile")]
    [InlineData(".bash_login")]
    [InlineData(".profile")]
    [InlineData(".bashrc")]
    [InlineData(".zshrc")]
    public void AddSimpleVariable_Profiles(string shProfile)
    {
        // Arrange
        var shProfileFile = $"/home/{shProfile}";

        var envProvider = new Mock<IEnvironmentProvider>();
        
        envProvider.Setup(x => x.GetUserProfile())
            .Returns("/home");

        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("/home");
        fileSystem.AddDirectory("/tmp/__icecraft_test_temp/");
        fileSystem.AddEmptyFile(shProfileFile);

        var envManager = new LinuxEnvironmentManager(_frontend,
            fileSystem,
            envProvider.Object);

        // Act
        envManager.ApplyProfile();

        // Assert
        var zshrc = fileSystem.GetFile(shProfileFile);

        Assert.Contains(". $HOME/.ice_craft_envs", zshrc.TextContents);
    }
}
