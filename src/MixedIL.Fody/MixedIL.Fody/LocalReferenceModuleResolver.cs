using FodyTools;
using MoreFodyHelpers;

namespace MixedIL.Fody;

internal class LocalReferenceModuleResolver : IModuleResolver
{
    private readonly IWeaverLogger _logger;
    private readonly HashSet<string> _referencePaths;
    private readonly HashSet<string> _ignoredAssemblyNames = new();

    public LocalReferenceModuleResolver(IWeaverLogger logger, IEnumerable<string> referencePaths)
    {
        _logger = logger;

        _referencePaths = new HashSet<string>(referencePaths, StringComparer.OrdinalIgnoreCase);
    }

    public ModuleDefinition? Resolve(TypeReference typeReference, string assemblyName)
    {
        if (_ignoredAssemblyNames.Contains(assemblyName))
            return null;

        var module = typeReference.Resolve()?.Module;
        if (module != null)
        {
            if (_referencePaths.Contains(module.FileName))
            {
                _logger.Info($"Merge types from assembly {assemblyName}.");
                return module;
            }
            else
            {
                _logger.Info($"Exclude assembly {assemblyName} because its not in the local references list.");
            }
        }

        _ignoredAssemblyNames.Add(assemblyName);
        return null;
    }
}
