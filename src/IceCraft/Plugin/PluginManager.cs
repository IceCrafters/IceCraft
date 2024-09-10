namespace IceCraft.Plugin;

using System.Reflection;
using IceCraft.Api.Plugin;

public class PluginManager
{
    private readonly string _pluginsDir;

    public PluginManager(string pluginsDir)
    {
        _pluginsDir = Path.GetFullPath(pluginsDir);
    }
    
    private Assembly LoadPlugin(string relativePath)
    {
        var pluginLocation = Path.GetFullPath(Path.Combine(_pluginsDir, 
            relativePath.Replace('\\', Path.DirectorySeparatorChar)));
        
        var pluginContext = new PluginLoadContext(pluginLocation);
        return pluginContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
    }

    public IEnumerable<IEnumerable<IPlugin>> LoadPlugins(IEnumerable<string> fileNames)
    {
        return fileNames.Select(LoadPlugin).Select(CreatePluginInstance);
    }

    private static IEnumerable<IPlugin> CreatePluginInstance(Assembly pluginAssembly)
    {
        foreach (var type in pluginAssembly.GetExportedTypes())
        {
            if (!typeof(IPlugin).IsAssignableFrom(type)) continue;
            if (Activator.CreateInstance(type) is IPlugin result)
            {
                yield return result;
            }
        }
    }
}