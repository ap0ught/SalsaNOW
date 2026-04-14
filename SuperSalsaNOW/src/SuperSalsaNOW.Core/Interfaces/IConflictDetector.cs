namespace SuperSalsaNOW.Core.Interfaces;

using SuperSalsaNOW.Core.Models;

/// <summary>
/// Phase 2: Detects conflicts between mods
/// TODO: Implement when supporting multiple concurrent mods
/// </summary>
/// <remarks>
/// Future implementation will:
/// - Detect file-level conflicts (same file modified by multiple mods)
/// - Check for incompatible mod combinations
/// - Analyze load order conflicts
/// - Suggest resolution strategies
/// - Support conflict whitelisting for known compatible overwrites
/// </remarks>
public interface IConflictDetector
{
    // TODO: Phase 2
    // Task<List<ModConflict>> DetectConflictsAsync(List<ModDefinition> mods);
    // Task<bool> AreCompatibleAsync(ModDefinition mod1, ModDefinition mod2);
    // Task<List<string>> GetConflictingFilesAsync(ModDefinition mod1, ModDefinition mod2);
    // Task<ConflictResolution> SuggestResolutionAsync(ModConflict conflict);
}
