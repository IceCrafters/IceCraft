namespace IceCraft.Tests;

using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Packaging;
using IceCraft.Core.Installation;
using IceCraft.Core.Platform;
using IceCraft.Core.Util;
using Microsoft.Extensions.Logging;
using Moq;
using Semver;
using Xunit.Abstractions;

public class DependencyResolverTests
{
    private static readonly PackagePluginInfo MockPluginInfo = new PackagePluginInfo("foo", "bar");

    private static readonly RemoteArtefact MockArtefact = new()
    {
        Checksum = "",
        ChecksumType = ""
    };
    
    private readonly ILoggerFactory _loggerFactory;

    public DependencyResolverTests(ITestOutputHelper outputHelper)
    {
        _loggerFactory = LoggerFactory
            .Create(builder =>
            {
                builder
                    .AddXunit(outputHelper);
                // Add other loggers, e.g.: AddConsole, AddDebug, etc.
            });

    }
    
    [Fact(Timeout = 3000)]
    public async Task Tree_Circular_Deep_ToRoot()
    {
        // Arrange
        #region An index with 5 layers of dependencies
        var targetPackage = new PackageMeta("target", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_1", SemVersionRange.Parse("0.1.0"))]);
        var dep1 = new PackageMeta("dep_1", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_2", SemVersionRange.Parse("0.1.0"))]);
        var dep2 = new PackageMeta("dep_2", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_3", SemVersionRange.Parse("0.1.0"))]);
        var dep3 = new PackageMeta("dep_3", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_4", SemVersionRange.Parse("0.1.0"))]);
        var dep4 = new PackageMeta("dep_4", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("target", SemVersionRange.Parse("0.1.0"))]);
        
        var index = new PackageIndexBuilder()
            .WithPackage(dep1, MockArtefact)
            .WithPackage(dep2, MockArtefact)
            .WithPackage(dep3, MockArtefact)
            .WithPackage(dep4, MockArtefact)
            .WithPackage(targetPackage,
                MockArtefact)
            .Build();
        #endregion

        var resolver = new DependencyResolver(Mock.Of<IPackageInstallManager>(),
            _loggerFactory.CreateLogger<DependencyResolver>(),
            Mock.Of<IFrontendApp>());
        var hashSet = new HashSet<PackageMeta>();

        // Act
        var exception = await Record.ExceptionAsync(async () 
            => await resolver.ResolveTree(targetPackage, index, hashSet));

        // Assert
        Assert.IsType<DependencyException>(exception);
    }

     [Fact(Timeout = 3000)]
    public async Task Tree_Circular_Deep_InBetween()
    {
        // Arrange
        #region An index with 5 layers of dependencies
        var targetPackage = new PackageMeta("target", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_1", SemVersionRange.Parse("0.1.0"))]);
        var dep1 = new PackageMeta("dep_1", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_2", SemVersionRange.Parse("0.1.0"))]);
        var dep2 = new PackageMeta("dep_2", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_3", SemVersionRange.Parse("0.1.0"))]);
        var dep3 = new PackageMeta("dep_3", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_4", SemVersionRange.Parse("0.1.0"))]);
        var dep4 = new PackageMeta("dep_4", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_2", SemVersionRange.Parse("0.1.0"))]);
        
        var index = new PackageIndexBuilder()
            .WithPackage(dep1, MockArtefact)
            .WithPackage(dep2, MockArtefact)
            .WithPackage(dep3, MockArtefact)
            .WithPackage(dep4, MockArtefact)
            .WithPackage(targetPackage,
                MockArtefact)
            .Build();
        #endregion

        var resolver = new DependencyResolver(Mock.Of<IPackageInstallManager>(),
            _loggerFactory.CreateLogger<DependencyResolver>(),
            Mock.Of<IFrontendApp>());
        var hashSet = new HashSet<PackageMeta>();

        // Act
        var exception = await Record.ExceptionAsync(async () 
            => await resolver.ResolveTree(targetPackage, index, hashSet));

        // Assert
        Assert.IsType<DependencyException>(exception);
    }

    [Fact(Timeout = 3000)]
    public async Task Tree_Circular_Shallow()
    {
        // Arrange
        #region An index with target and dep_1 referencing each other
        var targetPackage = new PackageMeta("target", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_1", SemVersionRange.Parse("0.1.0"))]);
        var dep1 = new PackageMeta("dep_1", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("target", SemVersionRange.Parse("0.1.0"))]);

        var index = new PackageIndexBuilder()
            .WithPackage(dep1, MockArtefact)
            .WithPackage(targetPackage,
                MockArtefact)
            .Build();
        #endregion

        var resolver = new DependencyResolver(Mock.Of<IPackageInstallManager>(),
            _loggerFactory.CreateLogger<DependencyResolver>(),
            Mock.Of<IFrontendApp>());
        var hashSet = new HashSet<PackageMeta>();

        // Act
        var exception = await Record.ExceptionAsync(async () 
            => await resolver.ResolveTree(targetPackage, index, hashSet));

        // Assert
        Assert.IsType<DependencyException>(exception);
    }

    [Fact(Timeout = 5000)]
    public async Task Recursive_FiveLayers()
    {
        // Arrange
        #region An index with 5 layers of dependencies
        var targetPackage = new PackageMeta("target", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_1", SemVersionRange.Parse("0.1.0"))]);
        var dep1 = new PackageMeta("dep_1", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_2", SemVersionRange.Parse("0.1.0"))]);
        var dep2 = new PackageMeta("dep_2", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_3", SemVersionRange.Parse("0.1.0"))]);
        var dep3 = new PackageMeta("dep_3", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_4", SemVersionRange.Parse("0.1.0"))]);
        var dep4 = new PackageMeta("dep_4", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo);
        
        var index = new PackageIndexBuilder()
            .WithPackage(dep1, MockArtefact)
            .WithPackage(dep2, MockArtefact)
            .WithPackage(dep3, MockArtefact)
            .WithPackage(dep4, MockArtefact)
            .WithPackage(targetPackage,
                MockArtefact)
            .Build();
        #endregion

        var resolver = new DependencyResolver(Mock.Of<IPackageInstallManager>(),
            _loggerFactory.CreateLogger<DependencyResolver>(),
            Mock.Of<IFrontendApp>());
        var hashSet = new HashSet<PackageMeta>();

        var expectedList =
        new PackageMeta[] 
        {
            dep1,
            dep2,
            dep3,
            dep4
        };
        
        // Act
        await resolver.ResolveTree(targetPackage, index, hashSet);
        
        // Assert
        Assert.Equal(expectedList, hashSet);
    }

    [Fact(Timeout = 3000)]
    public async Task Tree_Complex_ParentToParent()
    {
        // Arrange
        #region An index with 5 layers of dependencies
        var targetPackage = new PackageMeta("target", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_1", SemVersionRange.Parse("0.1.0"))]);
        var dep1 = new PackageMeta("dep_1", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_2", SemVersionRange.Parse("0.1.0"))]);
        var dep2 = new PackageMeta("dep_2", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_3", SemVersionRange.Parse("0.1.0"))]);
        var dep3 = new PackageMeta("dep_3", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo);
        var dep4 = new PackageMeta("dep_4", new SemVersion(0, 1, 0),
            DateTime.MinValue,
            MockPluginInfo,
            [new DependencyReference("dep_1", SemVersionRange.Parse("0.1.0"))]);
        
        var index = new PackageIndexBuilder()
            .WithPackage(dep1, MockArtefact)
            .WithPackage(dep2, MockArtefact)
            .WithPackage(dep3, MockArtefact)
            .WithPackage(dep4, MockArtefact)
            .WithPackage(targetPackage,
                MockArtefact)
            .Build();
        #endregion

        var resolver = new DependencyResolver(Mock.Of<IPackageInstallManager>(),
            _loggerFactory.CreateLogger<DependencyResolver>(),
            Mock.Of<IFrontendApp>());
        var hashSet = new HashSet<PackageMeta>();

        // Act
        var exception = await Record.ExceptionAsync(async () 
            => await resolver.ResolveTree(targetPackage, index, hashSet));

        // Assert
        Assert.Null(exception);
    }
}