// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

using IceCraft.Extensions.CentralRepo.Runtime.Security;
using JetBrains.Annotations;

[PublicAPI]
public class MashiroFs : ContextApi
{
    public MashiroFs(ContextApiRoot parent) : base(ExecutionContextType.Installation | ExecutionContextType.Configuration, parent)
    {
    }

    public string JoinPath(string path1, string path2)
    {
        EnsureContext();
        return Path.Combine(path1, path2);
    }
    
    public string JoinPath(params string[] paths)
    {
        EnsureContext();
        return Path.Combine(paths);
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