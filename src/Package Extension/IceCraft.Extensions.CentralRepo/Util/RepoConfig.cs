// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Util;

using IceCraft.Api.Configuration;
using IceCraft.Extensions.CentralRepo.Network;

public class RepoConfigFactory
{
    private static readonly RemoteRepositoryInfo OfficialRepository = new(
        new Uri("https://github.com/IceCrafters/CSR/archive/refs/heads/main.tar.gz"),
        "CSR-main"
    );

    private readonly IConfigManager _configMan;

    public RepoConfigFactory(IConfigManager configMan)
    {
        _configMan = configMan;
    }

    public record RepoConfigData(RemoteRepositoryInfo Repository);

    public RepoConfigData GetData()
    {
        var defValue = new RepoConfigData(OfficialRepository);
        return _configMan.GetJsonConfigFile("csr", defValue)
            ?? defValue;
    }
}
