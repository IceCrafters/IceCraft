namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Extensions.CentralRepo.Runtime.Security;

public class MashiroFs : ContextApi
{
    public MashiroFs(ContextApiRoot parent) : base(ExecutionContextType.Installation | ExecutionContextType.Configuration, parent)
    {
    }

    public void CopyFile(string source, string destination)
    {
        EnsureContext();
        File.Copy(source, destination);
    }
    
    public void CopyFile(string source, string destination, bool overwrite)
    {
        EnsureContext();
        File.Copy(source, destination, overwrite);
    }
    
    public void MoveFile(string source, string destination)
    {
        EnsureContext();
        File.Move(source, destination);
    }
    
    public void MoveFile(string source, string destination, bool overwrite)
    {
        EnsureContext();
        File.Move(source, destination, overwrite);
    }
    
    public void DeleteFile(string file)
    {
        EnsureContext();
        File.Delete(file);
    }

    public bool Exists(string file)
    {
        EnsureContext();
        return File.Exists(file);
    }

    public void Mkdir(string directory)
    {
        EnsureContext();
        Directory.CreateDirectory(directory);
    }

    public void Rmdir(string directory, bool recursive)
    {
        EnsureContext();
        Directory.Delete(directory, recursive);
    }
}