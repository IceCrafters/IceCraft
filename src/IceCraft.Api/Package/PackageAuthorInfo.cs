// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Api.Package;

using System.Diagnostics.CodeAnalysis;

public readonly record struct PackageAuthorInfo
{
    [SetsRequiredMembers]
    public PackageAuthorInfo(string name, string? mailAddress = null)
    {
        Name = name;
        MailAddress = mailAddress;
    }

    public required string Name { get; init; }
    public string? MailAddress { get; init; }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(MailAddress))
        {
            return $"{Name} <{MailAddress}>";
        }

        return Name;
    }
}
