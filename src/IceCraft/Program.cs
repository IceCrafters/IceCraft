using IceCraft;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Repositories.Adoptium;

IceCraftApp.Initialize();

var manager = new RepositoryManager();
manager.RegisterProvider("adoptium", new AdoptiumRepositoryProvider());
var repos = await manager.GetRepositories();
var pkgCount = 0;
var repoCount = 0;

foreach (var repo in repos)
{
    repoCount++;
    pkgCount += repo.GetExpectedSeriesCount();
}

Console.WriteLine("{0} repositories and {1} series", repoCount, pkgCount);