namespace DatabaseBackupTool.ConsoleApp;

public sealed class MissingConfigurationValueException : Exception
{
    public MissingConfigurationValueException(string key) : base($"Configuration value for key '{key}' is missing.")
    {
    }

    public MissingConfigurationValueException(string key, Exception innerException) : base($"Configuration value for key '{key}' is missing.", innerException)
    {
    }
}
