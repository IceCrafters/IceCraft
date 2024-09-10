namespace IceCraft.Extensions.CentralRepo.Runtime;

using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net.Mail;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Package;
using Semver;

// Full Speed Astern!

public static class MashiroRuntime
{
    public static MashiroState CreateState(string scriptFile)
    {
        var ps = PowerShell.Create();
        ps.AddScript(scriptFile);

        var rs = RunspaceFactory.CreateRunspace();
        rs.Open();

        return new MashiroState(ps, rs);
    }

    public static IEnumerable<ArtefactMirrorInfo> EnumerateArtefacts(string origin, Hashtable? mirrors)
    {
        yield return new ArtefactMirrorInfo
        {
            Name = origin,
            DownloadUri = new Uri(origin),
            IsOrigin = true
        };

        if (mirrors == null)
        {
            yield break;
        }
        
        foreach (DictionaryEntry mirror in mirrors)
        {
            if (mirror.Key is not string name
                || mirror.Value is not string url)
            {
                throw new FormatException("Mirror must be string = string");
            }

            yield return new ArtefactMirrorInfo
            {
                Name = name,
                DownloadUri = new Uri(url)
            };
        }
    }
    
    public static DependencyCollection CreateDependencies(Hashtable? hashtable)
    {
        if (hashtable == null)
        {
            return [];
        }
        
        var list = new List<DependencyReference>(hashtable.Count);

        foreach (DictionaryEntry entry in hashtable)
        {
            if (entry.Key is not string package || entry.Value is not string versionStr)
            {
                throw new FormatException("Dependencies hashtable must be consisted of only strings");
            }

            var version = SemVersionRange.Parse(versionStr);
            
            list.Add(new DependencyReference(package, version));
        }

        return new DependencyCollection(list);
    }
    
    public static PackageTranscript CreateTranscript(Hashtable? authors, string? packageMaintainer,
        string? pluginMaintainer, string? license, string? description)
    {
        IReadOnlyList<PackageAuthorInfo>? authorInfos = null;
        if (authors != null)
        {
            authorInfos = ParseAuthors(authors);
        }

        PackageAuthorInfo packageMaintainerInfo = default;
        if (packageMaintainer != null)
        {
            packageMaintainerInfo = ParseAuthor(packageMaintainer);
        }

        PackageAuthorInfo pluginMaintainerInfo = default;
        if (pluginMaintainer != null)
        {
            pluginMaintainerInfo = ParseAuthor(pluginMaintainer);
        }

        return new PackageTranscript
        {
            Authors = authorInfos ?? [],
            PluginMaintainer = pluginMaintainerInfo,
            Description = description,
            License = license,
            Maintainer = packageMaintainerInfo,
        };
    }

    public static IReadOnlyList<PackageAuthorInfo> ParseAuthors(Hashtable hashtable)
    {
        var list = new List<PackageAuthorInfo>();
        foreach (DictionaryEntry entry in hashtable)
        {
            if (entry.Key is not string author || entry.Value is not string email)
            {
                throw new FormatException("Authors hashtable must be consisted of only strings");
            }
            
            list.Add(new PackageAuthorInfo(author, email));
        }

        return list.AsReadOnly();
    }
    
    public static PackageAuthorInfo ParseAuthor(string authorStr)
    {
        MailAddress.TryCreate(authorStr, out var address);
        if (address is null)
        {
            throw new KnownException($"Invalid email address: {authorStr}");
        }

        return new PackageAuthorInfo(address.User, address.Address);
    }
}