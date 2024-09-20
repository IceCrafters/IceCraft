// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests;

using System.Security.Cryptography;
using System.Text;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Client;
using IceCraft.Core.Archive.Checksums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

public class ChecksumTests
{
    #region Setup Helpers
    private static readonly byte[] TestData = Encoding.UTF8.GetBytes("Hello World");
    private static readonly string TestHashSha256 = Convert.ToHexString(SHA256.HashData(TestData));
    private static readonly string TestHashSha512 = Convert.ToHexString(SHA512.HashData(TestData));

    private static readonly RemoteArtefact TestArtefactSha256 = new RemoteArtefact
    {
        ChecksumType = "sha256",
        Checksum = TestHashSha256
    };
    
    private readonly ILoggerFactory _loggerFactory;

    public ChecksumTests(ITestOutputHelper outputHelper)
    {
        _loggerFactory = LoggerFactory
            .Create(builder =>
            {
                builder
                    .AddXunit(outputHelper);
                // Add other loggers, e.g.: AddConsole, AddDebug, etc.
            });
    }
    #endregion

    [Fact]
    public async Task Sha256_Run()
    {
        // Arrange
        var validator = new Sha256ChecksumValidator();
        
        // Act
        string checkResult;

        await using (var memStream = new MemoryStream(TestData))
        {
            var checkCode = await validator.GetChecksumBinaryAsync(memStream);
            checkResult = validator.GetChecksumString(checkCode);
        }
        
        // Assert
        Assert.Equal(TestHashSha256, checkResult, StringComparer.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async Task Sha512_Run()
    {
        // Arrange
        var validator = new Sha512ChecksumValidator();
        
        // Act
        string checkResult;

        await using (var memStream = new MemoryStream(TestData))
        {
            var checkCode = await validator.GetChecksumBinaryAsync(memStream);
            checkResult = validator.GetChecksumString(checkCode);
        }
        
        // Assert
        Assert.Equal(TestHashSha512, checkResult, StringComparer.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async Task DependencyRunner_ValidateLocal_Stream()
    {
        // Arrange
        var diMock = new Mock<IKeyedServiceProvider>();
        diMock.Setup(x => x.GetKeyedService(typeof(IChecksumValidator), "sha256"))
            .Returns(new Sha256ChecksumValidator());
        
        var runner = new DependencyChecksumRunner(diMock.Object,
            Mock.Of<IManagerConfiguration>(),
            _loggerFactory.CreateLogger<DependencyChecksumRunner>());
        
        var memStream = new MemoryStream(TestData);
        
        // Act
        var result = await runner.ValidateLocal(TestArtefactSha256, memStream);
        
        // Assert
        Assert.True(result);
    }
}