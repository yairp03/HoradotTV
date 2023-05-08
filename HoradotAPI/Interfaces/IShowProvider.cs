namespace HoradotAPI.Interfaces;

public interface IShowProvider : IContentProvider
{
    public Task<IEnumerable<SeasonInformation>> GetSeasonsAsync(ShowInformation show);
    public Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(SeasonInformation season);

    public Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(EpisodeInformation firstEpisode,
        int maxEpisodeAmount);

    public Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(ShowInformation show);
}
