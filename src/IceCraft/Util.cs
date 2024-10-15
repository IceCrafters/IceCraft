// Copyright (C) WithLithum & IceCraft contributors 2024.
// Licensed under GNU General Public License, version 3 or (at your opinion)
// any later version. See COPYING in repository root.

namespace IceCraft;
using System;
using System.Runtime.CompilerServices;
using Autofac;
using IceCraft.Api.Archive.Artefacts;
using IceCraft.Api.Archive.Indexing;
using IceCraft.Api.Archive.Repositories;
using IceCraft.Api.Installation;
using IceCraft.Api.Installation.Database;
using IceCraft.Api.Installation.Dependency;
using IceCraft.Api.Network;
using IceCraft.Api.Platform;
using IceCraft.Core.Archive.Artefacts;
using IceCraft.Core.Archive.Checksums;
using IceCraft.Core.Archive.Dependency;
using IceCraft.Core.Archive.Indexing;
using IceCraft.Core.Archive.Repositories;
using IceCraft.Core.Installation;
using IceCraft.Core.Installation.Analysis;
using IceCraft.Core.Installation.Execution;
using IceCraft.Core.Installation.Storage;
using IceCraft.Core.Network;
using IceCraft.Core.Platform;
using IceCraft.Core.Platform.Linux;
using IceCraft.Core.Platform.Windows;
using IceCraft.Core.Util;

internal static class Util
{
    internal static void PopulateIceCraftCore(this ContainerBuilder builder)
    {
        builder.RegisterType<MirrorSearcher>().As<IMirrorSearcher>().SingleInstance();
        builder.RegisterType<RepositoryManager>().As<IRepositorySourceManager>().SingleInstance();
        builder.RegisterType<CachedIndexer>().As<IPackageIndexer>().SingleInstance();
        builder.RegisterType<ExecutableManager>().As<IExecutableManager>().SingleInstance();
        builder.RegisterType<DependencyResolver>().As<IDependencyResolver>().SingleInstance();
        builder.RegisterType<DependencyMapper>().As<IDependencyMapper>().SingleInstance();
        builder.RegisterType<MissionDownloadManager>().As<IDownloadManager>().SingleInstance();
        builder.RegisterType<PackageInstallManager>().As<IPackageInstallManager>().SingleInstance();
        builder.RegisterType<DependencyChecksumRunner>().As<IChecksumRunner>().SingleInstance();
        builder.RegisterType<ArtefactManager>().As<IArtefactManager>().SingleInstance();
        builder.RegisterType<EnvironmentWrapper>().As<IEnvironmentProvider>().SingleInstance();

        builder.RegisterType<DatabaseReadHandleImpl>().As<ILocalDatabaseReadHandle>().InstancePerDependency();
        builder.RegisterType<LocalDatabaseMutatorImpl>().As<ILocalDatabaseMutator>().InstancePerDependency();
        builder.RegisterType<PackageSetupAgent>().As<IPackageSetupAgent>().InstancePerDependency();
        builder.RegisterType<LocalPackageImporter>().As<ILocalPackageImporter>().InstancePerDependency();

        builder.RegisterType<Sha256ChecksumValidator>().As<IChecksumValidator>().Keyed<IChecksumValidator>("sha256");
        builder.RegisterType<Sha512ChecksumValidator>().As<IChecksumValidator>().Keyed<IChecksumValidator>("sha512");
        builder.RegisterType<VirtualInstaller>().As<IPackageInstaller>().Keyed<IPackageInstaller>("virtual");
        builder.RegisterType<VirtualConfigurator>().As<IPackageConfigurator>().Keyed<IPackageConfigurator>("virtual");

        if (OperatingSystem.IsLinux())
        {
            builder.RegisterType<LinuxEnvironmentManager>().As<IEnvironmentManager>().SingleInstance();
            builder.RegisterType<PosixExecutionScriptGenerator>().As<IExecutionScriptGenerator>().SingleInstance();
        }
        else if (OperatingSystem.IsWindows())
        {
            builder.RegisterType<WindowsEnvironmentManager>().As<IEnvironmentManager>().SingleInstance();
        }
        else
        {
            // TODO add a no-op environment manager
        }
    }

    /// <summary>
    /// Assets that the file name is valid.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="argName">The name of the argument of the file name.</param>
    /// <exception cref="ArgumentException">The file name is invalid.</exception>
    /// <exception cref="ArgumentNullException">The file name is null.</exception>
    internal static void CheckFileName(string? fileName, [CallerArgumentExpression(nameof(fileName))] string? argName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var x in invalidChars)
        {
            if (fileName.Contains(x))
            {
                throw new ArgumentException($"File name '{fileName}' is invalid.", argName);
            }
        }
    }
}
