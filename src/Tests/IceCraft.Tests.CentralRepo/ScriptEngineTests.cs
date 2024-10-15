// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests.CentralRepo;

using System.IO.Abstractions.TestingHelpers;
using IceCraft.Api.Client;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Package;
using IceCraft.Api.Platform;
using IceCraft.Extensions.CentralRepo.Network;
using IceCraft.Extensions.CentralRepo.Runtime;
using IceCraft.Extensions.CentralRepo.Runtime.Security;
using IceCraft.Tests.CentralRepo.Helpers;
using Jint;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Semver;
using Xunit.Abstractions;

public class ScriptEngineTests
{
    #region Test Scripts
    private const string TestScriptMirrorOriginNotSet =
        """
        setMeta(MetaBuilder
                .id("example")
                .version(semVer("0.1.0-alpha"))
                .authors(author("foo", "foo@gmail.com"),
                         author("bar", "bar@gmail.com"))
                .maintainer(author("maintainer", "maintainer@example.com"))
                .pluginMaintainer(author("plugin", "plugin@example.com"))
                .license("MIT")
                .date(new Date())
                .build()
        )
        
        onExpand(function (artefact, to) {
            CompressedArchive.expand(atrefact, to)
        })
        
        onConfigure(function (meta, path) {
            Binary.register("example", "example")
        })

        onExportEnv(function (path) {
        })
        
        """;
    
    private const string TestScriptWithOriginMirror =
        """
        setMeta(MetaBuilder
                .id("example")
                .version(semVer("0.1.0-alpha"))
                .authors(author("foo", "foo@gmail.com"),
                         author("bar", "bar@gmail.com"))
                .maintainer(author("maintainer", "maintainer@example.com"))
                .pluginMaintainer(author("plugin", "plugin@example.com"))
                .license("MIT")
                .date(new Date())
                .build()
        )
        
        setOrigin("https://example.com/src.zip")
        addMirror("mirror", "https://contoso.com/src.zip")

        onExpand(function (artefact, to) {
            CompressedArchive.expand(atrefact, to)
        })

        onConfigure(function (meta, path) {
            Binary.register("example", "example")
        })

        onExportEnv(function (path) {
        })

        """;
    
    private const string TestScriptWithOnlyOrigin =
        """
        setMeta(MetaBuilder
                .id("example")
                .version(semVer("0.1.0-alpha"))
                .authors(author("foo", "foo@gmail.com"),
                         author("bar", "bar@gmail.com"))
                .maintainer(author("maintainer", "maintainer@example.com"))
                .pluginMaintainer(author("plugin", "plugin@example.com"))
                .license("MIT")
                .date(new Date())
                .build()
        )

        setOrigin("https://example.com/src.zip")

        onExpand(function (artefact, to) {
            CompressedArchive.expand(atrefact, to)
        })

        onConfigure(function (meta, path) {
            Binary.register("example", "example")
        })

        onExportEnv(function (path) {
        })

        """;
    
    // Intentionally same as TestScriptWithOnlyOrigin for clarity
    private const string TestScriptWithNoPreprocessor =
        """
        setMeta(MetaBuilder
                .id("example-no-preproc")
                .version(semVer("0.1.0-alpha"))
                .authors(author("foo", "foo@gmail.com"),
                         author("bar", "bar@gmail.com"))
                .maintainer(author("maintainer", "maintainer@example.com"))
                .pluginMaintainer(author("plugin", "plugin@example.com"))
                .license("MIT")
                .date(new Date())
                .build()
        )

        setOrigin("https://example.com/src.zip")

        onExpand(function (artefact, to) {
            CompressedArchive.expand(atrefact, to)
        })

        onConfigure(function (meta, path) {
            Binary.register("example", "example")
        })

        onExportEnv(function (path) {
        })

        """;
    
    private const string TestScriptWithPreprocessor =
        """
        setMeta(MetaBuilder
                .id("example-no-preproc")
                .version(semVer("0.1.0-alpha"))
                .authors(author("foo", "foo@gmail.com"),
                         author("bar", "bar@gmail.com"))
                .maintainer(author("maintainer", "maintainer@example.com"))
                .pluginMaintainer(author("plugin", "plugin@example.com"))
                .license("MIT")
                .date(new Date())
                .build()
        )

        setOrigin("https://example.com/src.zip")

        onExpand(function (artefact, to) {
            CompressedArchive.expand(atrefact, to)
        })
        
        onPreprocess(function (temp, to) {
            let projFile = Fs.combinePath(temp, "Example/Example.csproj")
        
            Os.system(`dotnet build ${projFile} --configuration Release`)
        })

        onConfigure(function (meta, path) {
            Binary.register("example", "example")
        })

        onExportEnv(function (path) {
        })

        """;
    
    private const string TestScriptWithNoDelegates =
        """
        setMeta(MetaBuilder
                .id("example-no-preproc")
                .version(semVer("0.1.0-alpha"))
                .authors(author("foo", "foo@gmail.com"),
                         author("bar", "bar@gmail.com"))
                .maintainer(author("maintainer", "maintainer@example.com"))
                .pluginMaintainer(author("plugin", "plugin@example.com"))
                .license("MIT")
                .date(new Date())
                .build()
        )

        setOrigin("https://example.com/src.zip")
        """;
    
    private const string TestScriptWithRequiredDelegates =
        """
        setMeta(MetaBuilder
                .id("example-no-preproc")
                .version(semVer("0.1.0-alpha"))
                .authors(author("foo", "foo@gmail.com"),
                         author("bar", "bar@gmail.com"))
                .maintainer(author("maintainer", "maintainer@example.com"))
                .pluginMaintainer(author("plugin", "plugin@example.com"))
                .license("MIT")
                .date(new Date())
                .build()
        )

        setOrigin("https://example.com/src.zip")

        onExpand(function (artefact, to) {
            CompressedArchive.expand(atrefact, to)
        })

        onConfigure(function (path) {
            Binary.register("example", "example")
        })
        
        onUnConfigure(function (path) {
            Binary.unregister("example")
        })
        
        onRemove(function (path) {
            Fs.Rmdir(path, true)
        })

        onExportEnv(function (path) {
        })

        """;
    #endregion

    private readonly FrontendAppHelper _frontend;

    public ScriptEngineTests(ITestOutputHelper outputHelper)
    {
        _frontend = new FrontendAppHelper(outputHelper);
    }

    [Fact]
    public void MashiroState_Meta_NoPreprocessor()
    {
        // Arrange
        var apiMock = new Mock<IMashiroApiProvider>();
        var apiRoot = new ContextApiRoot();

        var state = new MashiroState(apiMock.Object, 
            MashiroRuntime.CreateJintEngine(), 
            _frontend, 
            apiRoot,
            new MashiroMetaTransfer());
        state.AddApis();
        state.SetScript(TestScriptWithNoPreprocessor, nameof(TestScriptWithNoPreprocessor));
        
        // Act
        state.EnsureMetadata();
        var metadata = state.GetPackageMeta();
        
        // Assert
        Assert.NotNull(metadata);
        Assert.Null(metadata.PluginInfo.PreProcessorRef);
    }
    
    [Fact]
    public void MashiroState_Meta_WithPreprocessor()
    {
        // Arrange
        var apiMock = new Mock<IMashiroApiProvider>();
        var apiRoot = new ContextApiRoot();

        var state = new MashiroState(apiMock.Object, 
            MashiroRuntime.CreateJintEngine(), 
            _frontend, 
            apiRoot,
            new MashiroMetaTransfer());
        state.AddApis();
        state.SetScript(TestScriptWithPreprocessor, nameof(TestScriptWithPreprocessor));
        
        // Act
        state.EnsureMetadata();
        var metadata = state.GetPackageMeta();
        
        // Assert
        Assert.NotNull(metadata);
        Assert.NotNull(metadata.PluginInfo.PreProcessorRef);
    }
    
    [Fact]
    public void MashiroState_Mirror_OriginNotSet()
    {
        // Arrange
        var apiMock = new Mock<IMashiroApiProvider>();
        var apiRoot = new ContextApiRoot();

        var state = new MashiroState(apiMock.Object, 
            MashiroRuntime.CreateJintEngine(), 
            _frontend, 
            apiRoot,
            new MashiroMetaTransfer());
        state.AddApis();
        state.SetScript(TestScriptMirrorOriginNotSet, nameof(TestScriptMirrorOriginNotSet));
        
        // Act
        state.EnsureMetadata();
        var exception = Record.Exception(() => state.GetMirrors());
        
        // Assert
        Assert.IsType<KnownInvalidOperationException>(exception);
    }
    
    [Fact]
    public void MashiroState_Mirror_OriginAndMirror()
    {
        // Arrange
        var apiMock = new Mock<IMashiroApiProvider>();
        var apiRoot = new ContextApiRoot();

        var state = new MashiroState(apiMock.Object, 
            MashiroRuntime.CreateJintEngine(), 
            _frontend, 
            apiRoot,
            new MashiroMetaTransfer());
        state.AddApis();
        state.SetScript(TestScriptWithOriginMirror, nameof(TestScriptWithOriginMirror));
        
        // Act
        state.EnsureMetadata();
        var mirrors = state.GetMirrors();
        
        // Assert
        Assert.Equal(2, mirrors.Count);
        Assert.Multiple(() => Assert.Contains(mirrors, x => x.IsOrigin),
            () => Assert.Contains(mirrors, x => !x.IsOrigin));
    }
    
    [Fact]
    public void MashiroState_Mirror_OriginOnly()
    {
        // Arrange
        var apiMock = new Mock<IMashiroApiProvider>();
        var apiRoot = new ContextApiRoot();

        var state = new MashiroState(apiMock.Object, 
            MashiroRuntime.CreateJintEngine(), 
            _frontend, 
            apiRoot,
            new MashiroMetaTransfer());
        state.AddApis();
        state.SetScript(TestScriptWithOnlyOrigin, nameof(TestScriptWithOnlyOrigin));
        
        // Act
        state.EnsureMetadata();
        var mirrors = state.GetMirrors();
        
        // Assert
        Assert.Equal(1, mirrors.Count);
        Assert.Contains(mirrors, x => x.IsOrigin);
    }
    
    [Fact]
    public void MashiroState_Delegate_WithNone()
    {
        // Arrange
        var apiMock = new Mock<IMashiroApiProvider>();
        var apiRoot = new ContextApiRoot();

        var state = new MashiroState(apiMock.Object, 
            MashiroRuntime.CreateJintEngine(), 
            _frontend, 
            apiRoot,
            new MashiroMetaTransfer());
        state.AddApis();
        state.SetScript(TestScriptWithNoDelegates, nameof(TestScriptWithNoDelegates));
        
        // Act
        state.EnsureMetadata();
        var exception = Record.Exception(() => state.VerifyRequiredDelegates());
        
        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }
    
    [Fact]
    public void MashiroState_Delegate_WithRequired()
    {
        // Arrange
        var apiMock = new Mock<IMashiroApiProvider>();
        var apiRoot = new ContextApiRoot();

        var state = new MashiroState(apiMock.Object, 
            MashiroRuntime.CreateJintEngine(), 
            _frontend, 
            apiRoot,
            new MashiroMetaTransfer());
        state.AddApis();
        state.SetScript(TestScriptWithRequiredDelegates, nameof(TestScriptWithRequiredDelegates));
        
        // Act
        state.EnsureMetadata();
        var exception = Record.Exception(() => state.VerifyRequiredDelegates());
        
        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void MashiroRuntime_SemVerAvailable()
    {
        // Arrange
        var engine = MashiroRuntime.CreateJintEngine();
        var expected = new SemVersion(1, 0, 0);
        
        // Act
        var result = engine.Evaluate("return semVer(\"1.0.0\")");
        var obj = result.AsObject().ToObject();
        
        // Assert
        var version = Assert.IsType<SemVersion>(obj);
        Assert.Equal(expected, version);
    }

    [Fact]
    public void MashiroRuntime_AuthorAvailable()
    {
        // Arrange
        var engine = MashiroRuntime.CreateJintEngine();
        var expected = new PackageAuthorInfo("test", "test@example.com");
        
        // Act
        var result = engine.Evaluate("return author(\"test\", \"test@example.com\")");
        var obj = result.AsObject().ToObject();
        
        // Assert
        var version = Assert.IsType<PackageAuthorInfo>(obj);
        Assert.Equal(expected, version);
    }
}