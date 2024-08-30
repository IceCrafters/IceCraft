namespace IceCraft.Core.Archive.Packaging;

using System.Diagnostics.CodeAnalysis;

public readonly struct PackageAuthorInfo
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
