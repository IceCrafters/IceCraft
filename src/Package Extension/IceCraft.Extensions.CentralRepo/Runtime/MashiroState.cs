namespace IceCraft.Extensions.CentralRepo.Runtime;

using System.Management.Automation;
using System.Management.Automation.Runspaces;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Exceptions;
using IceCraft.Api.Package;
using Semver;

public class MashiroState : IDisposable
{
    private readonly PowerShell _shell;
    private readonly Runspace _space;
    private bool _ran;

    internal MashiroState(PowerShell shell, Runspace space)
    {
        _shell = shell;
        _space = space;
    }

    private void EnsureRan()
    {
        if (!_ran)
        {
            _shell.Invoke();
            _ran = true;
        }
        
        if (_shell.HadErrors)
        {
            throw new KnownException("Error executing script", _shell.InvocationStateInfo.Reason);
        }
    }

    public IEnumerable<ArtefactMirrorInfo> CreateMirrorInfo()
    {
        EnsureRan();
        var variables = _shell.Runspace.SessionStateProxy.PSVariable;

        var origin = variables.GetRequiredString("origin");
        var mirrors = variables.GetHashtable("mirrors");

        return MashiroRuntime.EnumerateArtefacts(origin, mirrors);
    }
    
    public RemoteArtefact CreateArtefact()
    {
        EnsureRan();
        var variables = _shell.Runspace.SessionStateProxy.PSVariable;

        var checksum = variables.GetRequiredString("SHA512");
        
        return new RemoteArtefact("sha512", checksum);
    }
    
    public PackageMeta CreateMeta()
    {
        EnsureRan();
        var variables = _shell.Runspace.SessionStateProxy.PSVariable;

        // Transcript
        
        var authors = variables.GetHashtable("Authors");
        var maintainer = variables.GetString("Maintainer");
        var pluginMaintainer = variables.GetString("PluginMaintainer");
        
        var license = variables.GetString("License");
        var description = variables.GetString("Description");

        var transcript = MashiroRuntime.CreateTranscript(authors,
            maintainer,
            pluginMaintainer,
            license,
            description);
        
        // Dependencies
        var dependencyTable = variables.GetHashtable("Dependencies");
        var dependencies = MashiroRuntime.CreateDependencies(dependencyTable);
        
        // Conflicts
        var conflictTable = variables.GetHashtable("ConflictsWith");
        var conflicts = MashiroRuntime.CreateDependencies(conflictTable);
        
        // Meta
        var id = variables.GetRequiredString("Id");
        var version = variables.GetRequiredString("Version");
        var date = variables.GetRequiredDateTime("Date");
        var unitary = variables.GetValueOrDefault<bool>("Unitary");

        return new PackageMeta
        {
            Id = id,
            Version = SemVersion.Parse(version, SemVersionStyles.Any),
            PluginInfo = new PackagePluginInfo("mashiro", "mashiro"),
            ReleaseDate = date,
            Dependencies = dependencies,
            Transcript = transcript,
            Unitary = unitary,
            ConflictsWith = conflicts
        };
    }

    public void Dispose()
    {
        _shell.Dispose();
        _space.Dispose();
        GC.SuppressFinalize(this);
    }
}