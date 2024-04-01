namespace DatabaseBackupTool.ConsoleApp.Exceptions;

/// <summary>
/// Exception thrown when an error occurs while running the 'pg_dump' process.
/// </summary>
public sealed class PgDumpProcessException : Exception
{
    public PgDumpProcessException(string processErrorOutput) : base($"An error occurred while running the 'pg_dump' process:\n\n{processErrorOutput}")
    {
        ErrorOutput = processErrorOutput;
    }

    public PgDumpProcessException(string processErrorOutput, Exception innerException) : base($"An error occurred while running the 'pg_dump' process:\n\n{processErrorOutput}", innerException)
    {
        ErrorOutput = processErrorOutput;
    }

    public string ErrorOutput { get; set; } = null!;
}