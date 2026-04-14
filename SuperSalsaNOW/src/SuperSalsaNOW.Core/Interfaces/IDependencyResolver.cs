namespace SuperSalsaNOW.Core.Interfaces;

using SuperSalsaNOW.Core.Models;

/// <summary>
/// Phase 2: Resolves mod dependencies and installation order
/// TODO: Implement when supporting multiple mods with dependencies
/// </summary>
/// <remarks>
/// Future implementation will:
/// - Parse mod dependency declarations
/// - Calculate optimal installation order
/// - Detect circular dependencies
/// - Identify missing required mods
/// - Support version constraints
/// </remarks>
public interface IDependencyResolver
{
    // TODO: Phase 2
    // Task<List<ModDefinition>> ResolveInstallOrderAsync(List<ModDefinition> mods);
    // Task<List<string>> GetMissingDependenciesAsync(ModDefinition mod);
    // Task<bool> HasCircularDependenciesAsync(List<ModDefinition> mods);
    // Task<Dictionary<string, string>> GetVersionConstraintsAsync(ModDefinition mod);
}
