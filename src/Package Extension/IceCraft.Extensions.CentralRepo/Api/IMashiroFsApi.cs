// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft.Extensions.CentralRepo.Api;

public interface IMashiroFsApi
{
    string JoinPath(string path1, string path2);
    string JoinPath(params string[] paths);
    void CopyFile(string source, string destination);
    void CopyFile(string source, string destination, bool overwrite);
    void MoveFile(string source, string destination);
    void MoveFile(string source, string destination, bool overwrite);
    void DeleteFile(string file);
    bool Exists(string file);
    void Mkdir(string directory);
    void Rmdir(string directory, bool recursive);
}