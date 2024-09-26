using Microsoft.Extensions.DependencyInjection;

namespace GitVersion.Agents;

public class GitVersionBuildAgentsModule : IGitVersionModule
{
    public void RegisterTypes(IServiceCollection services)
    {
#if NET
        var buildAgents = IGitVersionModule.FindAllDerivedTypes<BuildAgentBase>(Assembly.GetAssembly(GetType()));
#else
        var buildAgents = GitVersionModule.FindAllDerivedTypes<BuildAgentBase>(Assembly.GetAssembly(GetType()));
#endif
        foreach (var buildAgent in buildAgents)
        {
            services.AddSingleton(typeof(IBuildAgent), buildAgent);
        }
    }
}
