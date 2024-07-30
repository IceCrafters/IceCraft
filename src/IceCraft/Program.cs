using System.Diagnostics;
using IceCraft;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Repositories.Adoptium;

var app = IceCraftApp.Initialize();

var manager = new RepositoryManager();
manager.RegisterProvider("adoptium", new AdoptiumRepositoryProvider(app.CachingManager));

var stopwatch = Stopwatch.StartNew();
var repos = await manager.GetRepositories();
stopwatch.Stop();
Console.WriteLine("Acquiring repositories took {0}ms", stopwatch.ElapsedMilliseconds);

var pkgCount = 0;
var repoCount = 0;

foreach (var repo in repos)
{
    repoCount++;
    pkgCount += repo.GetExpectedSeriesCount();
}

Console.WriteLine("{0} repositories and {1} series", repoCount, pkgCount);