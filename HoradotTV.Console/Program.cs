namespace HoradotTV.Console;

internal class Program
{

    static SdarotDriver? driver;

    static async Task Main()
    {
        IOHelpers.Print("Welcome to HoradotTV!");
        IOHelpers.Print("Initializing...");

        ExitHelper.Initialize(Shutdown);
        System.Console.InputEncoding = Encoding.Unicode;
        System.Console.OutputEncoding = Encoding.Unicode;

        driver = new SdarotDriver();
        await driver.Initialize();

        while (true)
        {
            var series = await SearchSeries(driver);
            if (series == null)
                continue;

            var downloadLocation = GetDownloadLocation();
            IOHelpers.Print("Full download path: " + Path.GetFullPath(downloadLocation));

            var mode = GetMode();
            if (mode == Modes.None)
                continue;
            if (mode == Modes.Series)
            {
                await DownloadSeries(driver, series, downloadLocation);
                continue;
            }

            var season = await GetSeason(driver, series);
            if (mode == Modes.Season)
            {
                await DownloadSeason(driver, season, downloadLocation);
                continue;
            }

            var episode = await GetEpisode(driver, season);
            if (mode == Modes.Episode)
            {
                await DownloadEpisode(driver, episode, downloadLocation);
                continue;
            }

            var episodesAmount = GetEpisodesAmount();
            await DownloadEpisodes(driver, episode, episodesAmount, downloadLocation);
        }
    }

    static async Task<SeriesInformation?> SearchSeries(SdarotDriver driver)
    {
        var searchResult = Array.Empty<SeriesInformation>();
        do
        {
            var query = IOHelpers.Input("\nEnter series name or part of it: ");
            if (query.Length < Constants.QUERY_MIN_LENGTH)
            {
                IOHelpers.Print($"Please enter at least {Constants.QUERY_MIN_LENGTH} characters.");
                continue;
            }

            searchResult = await driver.SearchSeries(query);
            if (searchResult.Length == 0)
                IOHelpers.Print("Series not found.");
        } while (searchResult.Length == 0);

        IOHelpers.Print("\nResults:");
        IOHelpers.Print("[0] Back to start");
        for (var i = 0; i < searchResult.Length; i++)
        {
            IOHelpers.Print($"[{i + 1}] {searchResult[i].SeriesNameEn} - {searchResult[i].SeriesNameHe}");
        }

        var selection = IOHelpers.ChooseOptionRange(searchResult.Length, "Choose a series");
        return selection == 0 ? null : searchResult[selection - 1];
    }

    static string GetDownloadLocation()
    {
        string path;
        do
        {
            path = IOHelpers.Input($"\nEnter path for download (empty - {Constants.DEFAULT_DOWNLOAD_LOCATION}): ").Trim();
            if (string.IsNullOrWhiteSpace(path))
                path = Constants.DEFAULT_DOWNLOAD_LOCATION;

            try
            {
                var fullPath = Path.GetFullPath(path);
                if (!File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory))
                {
                    IOHelpers.Print("Please enter a path to a directory.");
                    path = "";
                }
            }
            catch
            {
                IOHelpers.Print("Please enter a valid path.");
                path = "";
            }
        } while (string.IsNullOrWhiteSpace(path));

        return path;
    }

    static Modes GetMode()
    {
        IOHelpers.Print("\n" + Menus.MODES_MENU);
        return (Modes)IOHelpers.ChooseOptionRange(Enum.GetNames(typeof(Modes)).Length - 1, "Choose a mode");
    }

    static async Task<SeasonInformation> GetSeason(SdarotDriver driver, SeriesInformation series)
    {
        var seasons = await driver.GetSeasonsAsync(series);
        var seasonName = IOHelpers.ChooseOption(seasons.Select(s => s.SeasonName), "season", "Choose a season");
        var season = seasons.Where(s => s.SeasonName == seasonName).First();
        return season;
    }
    static async Task<EpisodeInformation> GetEpisode(SdarotDriver driver, SeasonInformation season)
    {
        var episodes = await driver.GetEpisodesAsync(season);
        var episodeName = IOHelpers.ChooseOption(episodes.Select(e => e.EpisodeName), "episode", "Choose an episode");
        var episode = episodes.Where(e => e.EpisodeName == episodeName).First();
        return episode;
    }

    static int GetEpisodesAmount()
    {
        var amount = IOHelpers.InputInt("\nEnter episodes amount: ");
        while (amount <= 0)
        {
            IOHelpers.Print("Please enter a positive amount.");
            amount = IOHelpers.InputInt("Enter episodes amount: ");
        }

        return amount;
    }

    static async Task DownloadSeries(SdarotDriver driver, SeriesInformation series, string downloadLocation) => await DownloadEpisodes(driver, await driver.GetEpisodesAsync(series), downloadLocation);
    static async Task DownloadSeason(SdarotDriver driver, SeasonInformation season, string downloadLocation) => await DownloadEpisodes(driver, await driver.GetEpisodesAsync(season), downloadLocation);
    static async Task DownloadEpisode(SdarotDriver driver, EpisodeInformation episode, string downloadLocation) => await DownloadEpisodes(driver, new EpisodeInformation[] { episode }, downloadLocation);
    static async Task DownloadEpisodes(SdarotDriver driver, EpisodeInformation episode, int episodesAmount, string downloadLocation) => await DownloadEpisodes(driver, await driver.GetEpisodesAsync(episode, episodesAmount), downloadLocation);
    static async Task<IEnumerable<EpisodeInformation>?> DownloadEpisodes(SdarotDriver driver, IEnumerable<EpisodeInformation> episodes, string downloadLocation, bool retryFailed = true)
    {
        try
        {
            var failedEpisodes = new List<EpisodeInformation>();

            var i = 0;
            foreach (var episode in episodes)
            {
                i++;
                IOHelpers.Print($"\n({i}/{episodes.Count()})");
                IOHelpers.Print($"Loading {episode.Season.SeasonString} {episode.EpisodeString}...");
                var episodeMedia = await GetEpisodeMediaDetails(driver, episode);
                if (episodeMedia == null)
                {
                    failedEpisodes.Add(episode);
                    IOHelpers.Print("Failed. Proceeding to next episode.");
                    continue;
                }

                IOHelpers.Print($"Downloading {episode.Season.SeasonString} {episode.EpisodeString}...");
                var cleanSeriesName = string.Concat(episode.Series.SeriesNameEn.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
                var finalLocation = Path.Combine(downloadLocation, cleanSeriesName, episode.Season.SeasonString, episode.EpisodeString + ".mp4");
                Directory.CreateDirectory(Path.GetDirectoryName(finalLocation)!);
                await SdarotHelper.DownloadEpisode(episodeMedia, finalLocation);
            }

            if (retryFailed)
            {
                if (failedEpisodes.Count > 0)
                {
                    IOHelpers.Print($"\nFinished. Trying again {failedEpisodes.Count} failed episodes");
                    failedEpisodes = (await DownloadEpisodes(driver, failedEpisodes, downloadLocation, false) ?? Enumerable.Empty<EpisodeInformation>()).ToList();
                }

                SummarizeDownload(episodes.Count(), failedEpisodes);
                IOHelpers.Print("\nDone. Returning to start.");
            }

            return failedEpisodes;
        }
        catch { }

        return null;
    }

    static void SummarizeDownload(int total, IEnumerable<EpisodeInformation>? failed = null)
    {
        failed ??= Enumerable.Empty<EpisodeInformation>();
        var success = total - failed.Count();
        var successPercentage = (int)((double)success / total * 100);
        IOHelpers.Print("\nDownload Summary:");
        IOHelpers.Print($"Total   = {total}");
        IOHelpers.Print($"Success = {success}\t({successPercentage}%)");
        IOHelpers.Print($"Fail    = {failed.Count()}\t({100 - successPercentage}%)");
        if (failed.Any())
        {
            IOHelpers.Print($"Failed episodes:");
            foreach (var episode in failed)
            {
                IOHelpers.Print($"\t{episode.Season.SeasonString} {episode.EpisodeString}");
            }
        }
    }

    static async Task<EpisodeMediaDetails?> GetEpisodeMediaDetails(SdarotDriver driver, EpisodeInformation episode, int retries = 2)
    {
        do
        {
            try
            {
                return await driver.GetEpisodeMediaDetailsAsync(episode);
            }
            catch (Error2Exception)
            {
                IOHelpers.Print("Error 2, Skipping.");
                return null;
            }
            catch
            {
                if (retries > 0)
                    IOHelpers.Print($"Failed. Trying again... ({retries} tries left)");
                retries--;

            }
        } while (retries > -1);

        return null;
    }

    static void Shutdown()
    {
        driver?.Shutdown();
        Environment.Exit(0);
    }
}
