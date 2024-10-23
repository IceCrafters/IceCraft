namespace FrontendTests;

using System;
using IceCraft.Api.Plugin;
using IceCraft.Plugin;
using Microsoft.Extensions.DependencyInjection;
using Moq;

public class PluginApiTests
{
    private class DoubleMock : IPlugin, IClientExtension
    {
        internal bool _initCalled;

        public PluginMetadata Metadata => new PluginMetadata()
        {
            Identifier = "s",
            Version = "s"
        };  

        public void Initialize(IServiceCollection services)
        {
            // Do nothing here
        }

        public void InitializeClient(IExtensibleClient client, IServiceProvider serviceProvider)
        {
            _initCalled = true;
        }
    }

    [Fact]
    public void PluginManager_Initialize()
    {
        // Arrange
        var plugin = new Mock<IPlugin>();
        var sevCol = Mock.Of<IServiceCollection>();

        var manager = new PluginManager();
        manager.Add(plugin.Object);

        // Act
        manager.InitializeAll(sevCol);

        // Assert
        plugin.Verify(x => x.Initialize(sevCol), Times.Once);
    }

    [Fact]
    public void PluginManager_InitializeClient()
    {
        // Arrange
        var plugin = new DoubleMock();
        var sevProv = Mock.Of<IServiceProvider>();
        var client = Mock.Of<IExtensibleClient>();

        var manager = new PluginManager();
        manager.Add(plugin);

        // Act
        manager.InitializeClient(client, sevProv);

        // Assert
        Assert.True(plugin._initCalled);
    }
}