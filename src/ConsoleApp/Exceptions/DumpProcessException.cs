namespace DatabaseBackupTool.ConsoleApp.Exceptions;

/// <summary>
/// Exception thrown when an error occurs while running the database dump process.
/// </summary>
public sealed class DumpProcessException : Exception
{
    public DumpProcessException(string processErrorOutput) : base($"An error occurred while running the dump process:\n\n{processErrorOutput}")
    {
        ErrorOutput = processErrorOutput;
    }

    public DumpProcessException(string dumpProcessName, string processErrorOutput) : base($"An error occurred while running the '{dumpProcessName}' process:\n\n{processErrorOutput}")
    {
        DumpProcessName = dumpProcessName;
        ErrorOutput = processErrorOutput;
    }

    public DumpProcessException(string processErrorOutput, Exception innerException) : base($"An error occurred while running the dump process:\n\n{processErrorOutput}", innerException)
    {
        ErrorOutput = processErrorOutput;
    }

    public DumpProcessException(string dumpProcessName, string processErrorOutput, Exception innerException) : base($"An error occurred while running the '{dumpProcessName}' process:\n\n{processErrorOutput}", innerException)
    {
        DumpProcessName = dumpProcessName;
        ErrorOutput = processErrorOutput;
    }

    public string? DumpProcessName { get; set; }

    public string ErrorOutput { get; set; } = null!;
}