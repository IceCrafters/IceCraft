namespace IceCraft.Core.Archive.Repositories;

using IceCraft.Api.Archive.Repositories;

public static class PackageSeriesExtensions
{
    public static async Task EnumeratePackagesAsync(this IPackageSeries series, Func<IPackage, Task> consumer)
    {
        if (series is AsyncPackageSeries asyncSeries)
        {
            await EnumeratePackagesAsync(asyncSeries, consumer);
        }
        
        foreach (var x in series.EnumeratePackages())
        {
            await consumer(x);
        }
    }

    public static async Task EnumeratePackagesAsync(this AsyncPackageSeries series, Func<IPackage, Task> consumer)
    {
        await foreach (var package in series.EnumeratePackagesAsync())
        {
            await consumer(package);
        }
    }
}