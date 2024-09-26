using GitVersion.Extensions;
using GitVersion.Git;

namespace GitVersion.Configuration;

internal static class IgnoreConfigurationExtensions
{
    public static IEnumerable<ITag> Filter(this IIgnoreConfiguration ignore, IEnumerable<ITag> source)
    {
        ignore.NotNull();
        source.NotNull();

        if (!ignore.IsEmpty())
        {
            return source.Where(element => ShouldBeIgnored(element.Commit, ignore));
        }
        return source;
    }

    public static IEnumerable<ICommit> Filter(this IIgnoreConfiguration ignore, IEnumerable<ICommit> source)
    {
        ignore.NotNull();
        source.NotNull();

        if (!ignore.IsEmpty())
        {
            return source.Where(element => ShouldBeIgnored(element, ignore));
        }
        return source;
    }

    internal static bool IsEmpty(this IIgnoreConfiguration ignoreConfiguration)
        => ignoreConfiguration.Before == null && ignoreConfiguration.Shas.Count == 0;

    private static bool ShouldBeIgnored(ICommit commit, IIgnoreConfiguration ignore)
        => !(commit.When <= ignore.Before) && !ignore.Shas.Contains(commit.Sha);
}
