namespace HoradotAPI.Interfaces;

public abstract class BaseShowProvider : BaseContentProvider, IShowProvider
{
    public abstract Task<IEnumerable<SeasonInformation>> GetSeasonsAsync(ShowInformation show);
    public abstract Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(SeasonInformation season);

    public async Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(EpisodeInformation firstEpisode,
        int maxEpisodeAmount)
    {
        var episodesBuffer =
            new Queue<EpisodeInformation>(
                (await GetEpisodesAsync(firstEpisode.Season)).ToArray()[firstEpisode.Index..]);
        var seasonBuffer =
            new Queue<SeasonInformation>(
                (await GetSeasonsAsync(firstEpisode.Season.Show)).ToArray()[(firstEpisode.Season.Index + 1)..]);

        List<EpisodeInformation> episodes = new();
        while (episodes.Count < maxEpisodeAmount)
        {
            if (episodesBuffer.Count == 0)
            {
                if (seasonBuffer.Count == 0)
                {
                    break;
                }

                episodesBuffer = new Queue<EpisodeInformation>(await GetEpisodesAsync(seasonBuffer.Dequeue()));
                continue;
            }

            episodes.Add(episodesBuffer.Dequeue());
        }

        return episodes;
    }

    public async Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(ShowInformation show)
    {
        var seasons = await GetSeasonsAsync(show);

        List<EpisodeInformation> episodes = new();
        foreach (var season in seasons)
        {
            episodes.AddRange(await GetEpisodesAsync(season));
        }

        return episodes;
    }
}
