namespace StreamWave;

public abstract record Event
{
    public DateTimeOffset OccouredOn { get; set; } = SystemTime.Now();
}
