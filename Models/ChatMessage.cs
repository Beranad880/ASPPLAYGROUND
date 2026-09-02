namespace WebApplicationASP01.Models;

public record ChatMessage(
    string User,
    string Message,
    DateTime Timestamp,
    bool IsSystem = false
);
