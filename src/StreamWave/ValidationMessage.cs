namespace StreamWave;

/// <summary>
/// Represents a validation message indicating an issue or status related to the validation of an aggregate's state.
/// </summary>
/// <param name="Message">The message describing the validation issue or status.</param>
public record struct ValidationMessage(string Message);
