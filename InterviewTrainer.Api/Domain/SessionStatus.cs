
namespace InterviewTrainer.Api.Domain;

public record SessionStatus
{
    public static SessionStatus Started => new("Started");
    public static SessionStatus InProgress => new("InProgress");
    public static SessionStatus Completed => new("Completed");
    public static SessionStatus Cancelled => new("Cancelled");

    public string Value { get; init; }
    private SessionStatus(string value)
    {
        Value = value;
    }

    public bool CanChangeTo(SessionStatus nextStatus)
    {
        return Value switch
        {
            "Started" => nextStatus.Value is "InProgress" or "Cancelled",
            "InProgress" => nextStatus.Value is "Completed" or "Cancelled",
            "Completed" => false,
            "Cancelled" => false,
            _ => false
        };
    }
    public static SessionStatus FromValue(string value) => value switch
    {
        "Started" => Started,
        "InProgress" => InProgress,
        "Completed" => Completed,
        "Cancelled" => Cancelled,
        _ => throw new DomainException($"Неизвестный статус: {value}")
    };
}