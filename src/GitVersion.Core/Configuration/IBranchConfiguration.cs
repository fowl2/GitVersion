using System.Text.RegularExpressions;
using GitVersion.VersionCalculation;

namespace GitVersion.Configuration;

public interface IBranchConfiguration
{
    DeploymentMode? DeploymentMode { get; }

    string? Label { get; }

    IncrementStrategy Increment { get; }

    IPreventIncrementConfiguration PreventIncrement { get; }

    string? LabelNumberPattern { get; }

    bool? TrackMergeTarget { get; }

    bool? TrackMergeMessage { get; }

    CommitMessageIncrementMode? CommitMessageIncrementing { get; }

    public string? RegularExpression { get; }

#if NETCOREAPP3_0_OR_GREATER
    public bool IsMatch(string branchName)
        => BranchConfigurationExtensions.IsMatch(this, branchName);
#endif

    IReadOnlyCollection<string> SourceBranches { get; }

    IReadOnlyCollection<string> IsSourceBranchFor { get; }

    bool? TracksReleaseBranches { get; }

    bool? IsReleaseBranch { get; }

    bool? IsMainBranch { get; }

    int? PreReleaseWeight { get; }

    IBranchConfiguration Inherit(IBranchConfiguration configuration);

    IBranchConfiguration Inherit(EffectiveConfiguration configuration);
}

internal static class BranchConfigurationExtensions
{
    internal static bool IsMatch(this IBranchConfiguration branchConfiguration, string branchName)
        => branchConfiguration.RegularExpression != null && Regex.IsMatch(branchName, branchConfiguration.RegularExpression, RegexOptions.IgnoreCase);
}
