using System.Threading.Tasks;

namespace HoradotTV.Services.Checks;

internal interface IDependencyCheck
{
    public string LoadingText { get; }
    public string FixProblemUrl { get; }

    public Task<bool> RunCheckAsync();
}
