namespace AdaptiveSopDdsop.Web.Domain;

public static class ContractRepositoryPathResolver
{
    public const string EnvironmentVariableName = "DDAE_CONTRACT_ROOT";

    public static string ResolveDefault()
    {
        return Resolve(
            AppContext.BaseDirectory,
            Environment.GetEnvironmentVariable(EnvironmentVariableName));
    }

    public static string Resolve(string startDirectory, string? configuredRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(startDirectory);

        if (!string.IsNullOrWhiteSpace(configuredRoot))
        {
            var expandedRoot = Environment.ExpandEnvironmentVariables(configuredRoot.Trim());
            var fullConfiguredRoot = Path.GetFullPath(
                Path.IsPathRooted(expandedRoot)
                    ? expandedRoot
                    : Path.Combine(startDirectory, expandedRoot));
            if (!Directory.Exists(fullConfiguredRoot))
            {
                throw new DirectoryNotFoundException(
                    $"Configured contract repository does not exist: {fullConfiguredRoot}");
            }

            return fullConfiguredRoot;
        }

        var current = new DirectoryInfo(Path.GetFullPath(startDirectory));
        while (current is not null)
        {
            var siblingCandidate = Path.Combine(current.FullName, "DDAE_INTERFACE_CONTRACT");
            if (Directory.Exists(siblingCandidate))
            {
                return Path.GetFullPath(siblingCandidate);
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Contract repository DDAE_INTERFACE_CONTRACT was not found from {Path.GetFullPath(startDirectory)}. " +
            $"Set {EnvironmentVariableName} to override its location.");
    }
}
