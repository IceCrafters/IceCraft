namespace IceCraft.Extensions.CentralRepo.Runtime;

using System.Collections;
using System.Management.Automation;
using IceCraft.Api.Exceptions;

public static class PSVariableExtensions
{
    private static KnownException ExExpectedToBe(string variable, string expected)
    {
        return new KnownException($"Expects variable {variable} to be {expected} but was something else");
    }
    
    public static PSVariable GetRequired(this PSVariableIntrinsics intrinsics, string name)
    {
        // Variable returns null if not found
        var variable = intrinsics.Get(name);
        if (variable == null)
        {
            throw new KnownException($"Variable {name} is required but not found");
        }

        return variable;
    }
    
    public static string GetRequiredString(this PSVariableIntrinsics intrinsics, string name)
    {
        var variable = intrinsics.GetRequired(name);

        if (variable.Value is not string value)
        {
            throw ExExpectedToBe(name, "String");
        }

        return value;
    }

    public static string? GetString(this PSVariableIntrinsics intrinsics, string name)
    {
        var variable = intrinsics.Get(name);
        if (variable == null)
        {
            return null;
        }

        if (variable.Value is not string value)
        {
            throw ExExpectedToBe(name, "String");
        }

        return value;
    }

    public static Hashtable? GetHashtable(this PSVariableIntrinsics intrinsics, string name)
    {
        var variable = intrinsics.Get(name);
        if (variable == null)
        {
            return null;
        }

        if (variable.Value is not Hashtable value)
        {
            throw ExExpectedToBe(name, "Hashtable");
        }

        return value;
    }
    
    public static Hashtable? GetRequiredHashtable(this PSVariableIntrinsics intrinsics, string name)
    {
        var variable = intrinsics.GetRequired(name);

        if (variable.Value is not Hashtable value)
        {
            throw ExExpectedToBe(name, "Hashtable");
        }

        return value;
    }

    public static DateTime GetRequiredDateTime(this PSVariableIntrinsics intrinsics, string name)
    {
        var variable = intrinsics.GetRequired(name);

        if (variable.Value is not DateTime value)
        {
            throw ExExpectedToBe(name, "DateTime");
        }

        return value;
    }
    
    public static T? GetValueOrDefault<T>(this PSVariableIntrinsics intrinsics, string name)
    {
        var variable = intrinsics.Get(name);
        if (variable == null)
        {
            return default;
        }

        if (variable.Value is not T value)
        {
            throw ExExpectedToBe(name, "String");
        }

        return value;
    }
}