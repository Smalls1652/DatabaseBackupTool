using System.Text.Json.Serialization;
using DatabaseBackupTool.ConsoleApp.Models;

namespace DatabaseBackupTool.ConsoleApp;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    WriteIndented = true
)]
[JsonSerializable(typeof(BackupAppSettings))]
internal partial class CoreJsonContext : JsonSerializerContext
{}