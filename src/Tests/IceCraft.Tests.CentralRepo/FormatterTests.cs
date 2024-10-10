// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests.CentralRepo;

using IceCraft.Extensions.CentralRepo.Api;
using IceCraft.Extensions.CentralRepo.Runtime;

public class FormatterTests
{
    [Fact]
    public void Format_IgnoreCss()
    {
        // Arrange
        const string template = "%cTest";

        // Act
        var result = MashiroFormatter.Format(template, "css");

        // Assert
        Assert.Equal("Test", result);
    }

    [Fact]
    public void Format_Number_IsANumber()
    {
        // Arrange
        const string template = "Test number: %d";

        // Act
        var result = MashiroFormatter.Format(template, 12345);

        // Assert
        Assert.Equal("Test number: 12345", result);
    }

    [Fact]
    public void Format_Number_IsNotANumber()
    {
        // Arrange
        const string template = "Test number: %d";

        // Act
        var exception = Record.Exception(() => MashiroFormatter.Format(template, "not a number"));

        // Assert
        Assert.IsType<MashiroFormatError>(exception);
    }

    [Fact]
    public void Format_String()
    {
        // Arrange
        const string template = "Test string: %s";

        // Act
        var result = MashiroFormatter.Format(template, "A string");

        // Assert
        Assert.Equal("Test string: A string", result);
    }

    [Fact]
    public void Format_Symbol()
    {
        // Arrange
        const string template = "Test string: %s, %%";

        // Act
        var result = MashiroFormatter.Format(template, "A string");

        // Assert
        Assert.Equal("Test string: A string, %", result);
    }

    [Fact]
    public void Format_ReturnVerbatim()
    {
        // Arrange
        const string template = "Test string: %% %%";

        // Act
        var result = MashiroFormatter.Format(template);

        // Assert
        Assert.Equal(template, result);
    }

    [Fact]
    public void Format_ReturnVerbatim_InvalidSpecifierDoesNotMatter()
    {
        // Arrange
        const string template = "Test string: %x";

        // Act
        var result = MashiroFormatter.Format(template);

        // Assert
        Assert.Equal(template, result);
    }

    [Fact]
    public void Format_Error_TooFewArguments()
    {
        // Arrange
        const string template = "%s %s %s %s %s";

        // Act
        var exception = Record.Exception(() => MashiroFormatter.Format(template, "an argument"));

        // Assert
        Assert.IsType<MashiroFormatError>(exception);
    }

    [Fact]
    public void Format_Error_UnknownSpecifier()
    {
        // Arrange
        const string template = "%x";

        // Act
        var exception = Record.Exception(() => MashiroFormatter.Format(template, "an argument"));

        // Assert
        Assert.IsType<MashiroFormatError>(exception);
    }
}
