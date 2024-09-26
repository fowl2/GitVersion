using System.Security.Cryptography;
using GitVersion.Configuration;
using GitVersion.Extensions;
using GitVersion.Git;
using GitVersion.Helpers;
using GitVersion.Logging;
using Microsoft.Extensions.Options;

namespace GitVersion.VersionCalculation.Caching;

internal class GitVersionCacheKeyFactory(
    IFileSystem fileSystem,
    ILog log,
    IOptions<GitVersionOptions> options,
    IConfigurationFileLocator configFileLocator,
    IConfigurationSerializer configurationSerializer,
    IGitRepository gitRepository,
    IGitRepositoryInfo repositoryInfo)
    : IGitVersionCacheKeyFactory
{
    private readonly IFileSystem fileSystem = fileSystem.NotNull();
    private readonly ILog log = log.NotNull();
    private readonly IOptions<GitVersionOptions> options = options.NotNull();
    private readonly IConfigurationFileLocator configFileLocator = configFileLocator.NotNull();
    private readonly IConfigurationSerializer configurationSerializer = configurationSerializer.NotNull();
    private readonly IGitRepository gitRepository = gitRepository.NotNull();
    private readonly IGitRepositoryInfo repositoryInfo = repositoryInfo.NotNull();

    public GitVersionCacheKey Create(IReadOnlyDictionary<object, object?>? overrideConfiguration)
    {
        var gitSystemHash = GetGitSystemHash();
        var configFileHash = GetConfigFileHash();
        var repositorySnapshotHash = GetRepositorySnapshotHash();
        var overrideConfigHash = GetOverrideConfigHash(overrideConfiguration);

        var compositeHash = GetHash(gitSystemHash, configFileHash, repositorySnapshotHash, overrideConfigHash);
        return new(compositeHash);
    }

    private string GetGitSystemHash()
    {
        var dotGitDirectory = this.repositoryInfo.DotGitDirectory;

        // traverse the directory and get a list of files, use that for GetHash
        var contents = CalculateDirectoryContents(PathHelper.Combine(dotGitDirectory, "refs"));

        return GetHash(contents.ToArray());
    }

    // based on https://msdn.microsoft.com/en-us/library/bb513869.aspx
    private List<string> CalculateDirectoryContents(string root)
    {
        var result = new List<string>();

        // Data structure to hold names of subfolders to be
        // examined for files.
        var dirs = new Stack<string>();

        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException($"Root directory does not exist: {root}");
        }

        dirs.Push(root);

        while (dirs.Count != 0)
        {
            var currentDir = dirs.Pop();

            var di = new DirectoryInfo(currentDir);
            result.Add(di.Name);

            string[] subDirs;
            try
            {
                subDirs = Directory.GetDirectories(currentDir);
            }
            // An UnauthorizedAccessException exception will be thrown if we do not have
            // discovery permission on a folder or file. It may or may not be acceptable
            // to ignore the exception and continue enumerating the remaining files and
            // folders. It is also possible (but unlikely) that a DirectoryNotFound exception
            // will be raised. This will happen if currentDir has been deleted by
            // another application or thread after our call to Directory.Exists. The
            // choice of which exceptions to catch depends entirely on the specific task
            // you are intending to perform and also on how much you know with certainty
            // about the systems on which this code will run.
            catch (UnauthorizedAccessException e)
            {
                this.log.Error(e.Message);
                continue;
            }
            catch (DirectoryNotFoundException e)
            {
                this.log.Error(e.Message);
                continue;
            }

            string[] files;
            try
            {
                files = Directory.GetFiles(currentDir);
            }
            catch (UnauthorizedAccessException e)
            {
                this.log.Error(e.Message);
                continue;
            }
            catch (DirectoryNotFoundException e)
            {
                this.log.Error(e.Message);
                continue;
            }

            foreach (var file in files)
            {
                try
                {
                    var fi = new FileInfo(file);
                    result.Add(fi.Name);
                    result.Add(File.ReadAllText(file));
                }
                catch (IOException e)
                {
                    this.log.Error(e.Message);
                }
            }

            // Push the subdirectories onto the stack for traversal.
            // This could also be done before handing the files.
            // push in reverse order
            for (var i = subDirs.Length - 1; i >= 0; i--)
            {
                dirs.Push(subDirs[i]);
            }
        }

        return result;
    }

    private string GetRepositorySnapshotHash()
    {
        var head = this.gitRepository.Head;
        if (head.Tip == null)
        {
            return head.Name.Canonical;
        }

        var hash = string.Join(":", head.Name.Canonical, head.Tip.Sha);
        return GetHash(hash);
    }

    private string GetOverrideConfigHash(IReadOnlyDictionary<object, object?>? overrideConfiguration)
    {
        if (overrideConfiguration?.Any() != true)
        {
            return string.Empty;
        }

        // Doesn't depend on command line representation and
        // includes possible changes in default values of Config per se.
        var configContent = configurationSerializer.Serialize(overrideConfiguration);

        return GetHash(configContent);
    }

    private string GetConfigFileHash()
    {
        // will return the same hash even when configuration file will be moved
        // from workingDirectory to rootProjectDirectory. It's OK. Configuration essentially is the same.
        var workingDirectory = this.options.Value.WorkingDirectory;
        var projectRootDirectory = this.repositoryInfo.ProjectRootDirectory;

        var configFilePath = this.configFileLocator.GetConfigurationFile(workingDirectory)
                             ?? this.configFileLocator.GetConfigurationFile(projectRootDirectory);
        if (configFilePath == null) return string.Empty;
        if (!this.fileSystem.Exists(configFilePath)) return string.Empty;

        var configFileContent = this.fileSystem.ReadAllText(configFilePath);
        return GetHash(configFileContent);
    }

    private static string GetHash(params string[] textsToHash)
    {
        var textToHash = string.Join(":", textsToHash);
        return GetHash(textToHash);
    }

    private static string GetHash(string textToHash)
    {
        if (textToHash.IsNullOrEmpty())
        {
            return string.Empty;
        }

        var bytes = Encoding.UTF8.GetBytes(textToHash);

#if NET
        return Convert.ToHexString(SHA1.HashData(bytes));
#else
        var hashedBytes = SHA1.Create().ComputeHash(bytes);
        var hashedString = BitConverter.ToString(hashedBytes);
        return hashedString.Replace("-", "");
#endif
    }
}
