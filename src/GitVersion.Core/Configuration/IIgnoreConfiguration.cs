namespace GitVersion.Configuration;

public interface IIgnoreConfiguration
{
    DateTimeOffset? Before { get; }

#if NETCOREAPP3_0_OR_GREATER
    IReadOnlySet<string> Shas { get; }
#else
    ISet<string> Shas { get; }
#endif

#if NETCOREAPP3_0_OR_GREATER
    public bool IsEmpty => IgnoreConfigurationExtensions.IsEmpty(this);
#endif
}
