namespace IceCraft.Core.Archive.Dependency;

public class DependencyException : Exception
{
    public DependencyException(string? message, DependencyReference? offendingDependency = null) : base(message)
    {
        OffendingDependency = offendingDependency;
    }

    public DependencyException(string? message, DependencyReference offendingDependency, Exception? innerException) : base(message, innerException)
    {
        OffendingDependency = offendingDependency;
    }
    
    public DependencyReference? OffendingDependency { get; }

    public static DependencyException Unsatisfied(DependencyReference offendingDependency)
    {
        return new DependencyException($"Unable to satisfy dependency {offendingDependency.PackageId} ({offendingDependency.VersionRange})", offendingDependency);
    }
    
    public static DependencyException SelfReference(DependencyReference offendingDependency)
    {
        return new DependencyException($"Dependency {offendingDependency.PackageId} ({offendingDependency.VersionRange}) is a reference to the package that depends on it", offendingDependency);
    }

    public static DependencyException Circular(string offendingId, string offendingVersion, string referencedFrom)
    {
        return new DependencyException($"Dependency {offendingId} ({offendingVersion}) depends on dependent package {referencedFrom}");
    }

    public static DependencyException Circular(DependencyReference offendingDependency, string referencedFrom)
    {
        return new DependencyException($"Dependency {offendingDependency.PackageId} ({offendingDependency.VersionRange}) depends on dependent package {referencedFrom}", offendingDependency);
    }
}