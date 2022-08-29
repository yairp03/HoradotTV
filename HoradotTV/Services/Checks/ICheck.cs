namespace HoradotTV.Services.Checks;

public interface ICheck
{
    public string LoadingText { get; }
    public string FixProblemUrl { get; }

    public Task<bool> RunCheckAsync();
}
