using System.ComponentModel.DataAnnotations;

namespace PantryChef.Infrastructure.AI;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    [Required] public Uri Endpoint { get; set; } = new("http://localhost:11434");
    [Required] public string Model { get; set; } = "llama3.2";
    [Range(10, 600)] public int AttemptTimeoutSeconds { get; set; } = 120;
    public bool LogSensitiveData { get; set; }
}