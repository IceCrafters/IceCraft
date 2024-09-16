namespace IceCraft.Tests.CentralRepo;

using IceCraft.Api.Exceptions;
using IceCraft.Extensions.CentralRepo.Runtime;

public class ScriptEngineTests
{
    #region Test Scripts
    private const string TestScriptMirrorOriginNotSet =
        """
        setMeta(MetaBuilder
                .id("example")
                .version(SemVer("0.1.0-alpha"))
                .authors(Author("foo", "foo@gmail.com"),
                         Author("bar", "bar@gmail.com"))
                .maintainer(Author("maintainer", "maintainer@example.com"))
                .pluginMaintainer(Author("plugin", "plugin@example.com"))
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
        
        """;
    
    private const string TestScriptWithOriginMirror =
        """
        setMeta(MetaBuilder
                .id("example")
                .version(SemVer("0.1.0-alpha"))
                .authors(Author("foo", "foo@gmail.com"),
                         Author("bar", "bar@gmail.com"))
                .maintainer(Author("maintainer", "maintainer@example.com"))
                .pluginMaintainer(Author("plugin", "plugin@example.com"))
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

        """;
    
    private const string TestScriptWithOnlyOrigin =
        """
        setMeta(MetaBuilder
                .id("example")
                .version(SemVer("0.1.0-alpha"))
                .authors(Author("foo", "foo@gmail.com"),
                         Author("bar", "bar@gmail.com"))
                .maintainer(Author("maintainer", "maintainer@example.com"))
                .pluginMaintainer(Author("plugin", "plugin@example.com"))
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

        """;
    #endregion
    
    [Fact]
    public void MashiroState_Mirror_OriginNotSet()
    {
        // Arrange
        var state = MashiroRuntime.CreateState(TestScriptMirrorOriginNotSet, nameof(TestScriptMirrorOriginNotSet));
        
        // Act
        state.RunMetadata();
        var exception = Record.Exception(() => state.GetMirrors());
        
        // Assert
        Assert.IsType<KnownInvalidOperationException>(exception);
    }
    
    [Fact]
    public void MashiroState_Mirror_OriginAndMirror()
    {
        // Arrange
        var state = MashiroRuntime.CreateState(TestScriptWithOriginMirror, nameof(TestScriptWithOriginMirror));
        
        // Act
        state.RunMetadata();
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
        var state = MashiroRuntime.CreateState(TestScriptWithOnlyOrigin, nameof(TestScriptWithOnlyOrigin));
        
        // Act
        state.RunMetadata();
        var mirrors = state.GetMirrors();
        
        // Assert
        Assert.Equal(1, mirrors.Count);
        Assert.Contains(mirrors, x => x.IsOrigin);
    }
}