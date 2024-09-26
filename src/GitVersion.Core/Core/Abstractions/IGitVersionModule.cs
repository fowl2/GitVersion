using GitVersion.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace GitVersion;

public interface IGitVersionModule
{
    void RegisterTypes(IServiceCollection services);

#if NETCOREAPP3_0_OR_GREATER
    static IEnumerable<Type> FindAllDerivedTypes<T>(Assembly? assembly)
        => GitVersionModule.FindAllDerivedTypes<T>(assembly);
#endif
}

internal static class GitVersionModule
{
    public static IEnumerable<Type> FindAllDerivedTypes<T>(Assembly? assembly)
    {
        assembly.NotNull();

        var derivedType = typeof(T);
        return assembly.GetTypes().Where(t => t != derivedType && derivedType.IsAssignableFrom(t));
    }
}
