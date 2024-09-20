// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Tests.CentralRepo;

using IceCraft.Extensions.CentralRepo.Api;
using IceCraft.Extensions.CentralRepo.Runtime.Security;

public class OsTests
{
    [LinuxFact]
    public void OsSystem_Linux()
    {
        // Arrange
        var root = new ContextApiRoot
        {
            CurrentContext = ExecutionContextType.Installation
        };
        var os = new MashiroOs(root);

        // Act
        var exception = Record.Exception(() => os.System("echo \"Hello World!\""));
        
        // Assert
        Assert.Null(exception);
    }
}