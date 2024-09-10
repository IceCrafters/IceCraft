namespace IceCraft.Extensions.CentralRepo.Api;

using System.Management.Automation;
using IceCraft.Api.Package;

[Cmdlet(VerbsCommon.Get, "Author")]
public class GetAuthorCmdlet : Cmdlet
{
    [Parameter(Position = 0, Mandatory = true)]
    public required string Name { get; set; }
    
    [Parameter(Position = 1, Mandatory = false)]
    public string? Email { get; set; }

    protected override void ProcessRecord()
    {
        var result = new PackageAuthorInfo(Name, Email);
        
        WriteObject(result);
    }
}