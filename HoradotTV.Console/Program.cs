﻿namespace HoradotTV.Console;

internal static class Program
{
    private static SdarotDriver? driver;

    private static async Task Main()
    {
        IOHelpers.Print($"Welcome to HoradotTV {Constants.SOFTWARE_VERSION}!");
        IOHelpers.Print("Initializing...");

        System.Console.InputEncoding = Encoding.Unicode;
        System.Console.OutputEncoding = Encoding.Unicode;

        try
        {
            driver = new SdarotDriver();
        }
        catch (SdarotBlockedException)
        {
            IOHelpers.Print($"\nSdarotTV is blocked. Please follow this guide to unblock it:\n{Constants.SdarotTVConnectionProblemGuide}");
            Environment.Exit(0);
        }

        while (true)
        {
            var series = await SearchSeries(driver);
            if (series is null)
            {
                continue;
            }

            var downloadLocation = GetDownloadLocation();
            IOHelpers.Print("Full download path: " + Path.GetFullPath(downloadLocation));

            var mode = GetMode();
            if (mode == Modes.None)
            {
                continue;
            }

            if (mode == Modes.Series)
            {
                await DownloadSeries(driver, series, downloadLocation);
                continue;
            }

            var season = await GetSeason(driver, series);
            if (season is null)
            {
                continue;
            }

            if (mode == Modes.Season)
            {
                await DownloadSeason(driver, season, downloadLocation);
                continue;
            }

            var episode = await GetEpisode(driver, season);
            if (episode is null)
            {
                continue;
            }

            if (mode == Modes.Episode)
            {
                await DownloadEpisode(driver, episode, downloadLocation);
                continue;
            }

            var episodesAmount = IOHelpers.InputPositiveInt("\nEnter episodes amount (0 - cancel): ");
            if (episodesAmount == 0)
            {
                continue;
            }

            await DownloadEpisodes(driver, episode, episodesAmount, downloadLocation);
        }
    }

    private static async Task<SeriesInformation?> SearchSeries(SdarotDriver driver)
    {
        var searchResult = new List<SeriesInformation>();
        do
        {
            var query = IOHelpers.Input("\nEnter series name or part of it (q - quit): ");
            if (query == "q")
            {
                Environment.Exit(0);
            }

            if (query.Length < Constants.QUERY_MIN_LENGTH)
            {
                IOHelpers.Print($"Please enter at least {Constants.QUERY_MIN_LENGTH} characters.");
                continue;
            }

            searchResult = (await driver.SearchSeries(query)).ToList();
            if (searchResult.Count == 0)
            {
                IOHelpers.Print("Series not found.");
            }
        } while (searchResult.Count == 0);

        IOHelpers.Print("\nResults:");
        IOHelpers.Print("[0] Back to start");
        for (var i = 0; i < searchResult.Count; i++)
        {
            IOHelpers.Print($"[{i + 1}] {searchResult[i].SeriesNameEn} - {searchResult[i].SeriesNameHe}");
        }

        var selection = IOHelpers.ChooseOptionRange(searchResult.Count, "Choose a series");
        return selection == 0 ? null : searchResult[selection - 1];
    }

    private static string GetDownloadLocation()
    {
        string path;
        do
        {
            var settings = AppSettings.Default;
            var defaultPath = !string.IsNullOrWhiteSpace(settings.LastPath) ? settings.LastPath : Constants.DEFAULT_DOWNLOAD_LOCATION;
            path = IOHelpers.Input($"\nEnter path for download (empty - {defaultPath}): ").Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                path = defaultPath;
            }

            try
            {
                var fullPath = Path.GetFullPath(path);
                if (!File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory))
                {
                    IOHelpers.Print("Please enter a path to a directory.");
                    path = "";
                }

                settings.LastPath = path;
                settings.Save();
            }
            catch
            {
                IOHelpers.Print("Please enter a valid path.");
                path = "";
            }
        } while (string.IsNullOrWhiteSpace(path));

        return path;
    }

    private static Modes GetMode()
    {
        IOHelpers.Print("\n" + Menus.MODES_MENU);
        return (Modes)IOHelpers.ChooseOptionRange(Enum.GetNames(typeof(Modes)).Length - 1, "Choose a mode");
    }

    private static async Task<SeasonInformation?> GetSeason(SdarotDriver driver, SeriesInformation series)
    {
        var seasons = await driver.GetSeasonsAsync(series);
        var seasonName = IOHelpers.ChooseOption(seasons.Select(s => s.SeasonName), "season", "Choose a season");
        if (seasonName == "c")
        {
            return null;
        }

        var season = seasons.Where(s => s.SeasonName == seasonName).First();
        return season;
    }

    private static async Task<EpisodeInformation?> GetEpisode(SdarotDriver driver, SeasonInformation season)
    {
        var episodes = await driver.GetEpisodesAsync(season);
        var episodeName = IOHelpers.ChooseOption(episodes.Select(e => e.EpisodeName), "episode", "Choose an episode");
        if (episodeName == "c")
        {
            return null;
        }

        var episode = episodes.Where(e => e.EpisodeName == episodeName).First();
        return episode;
    }

    private static async Task DownloadSeries(SdarotDriver driver, SeriesInformation series, string downloadLocation) => await DownloadEpisodes(driver, await driver.GetEpisodesAsync(series), downloadLocation);
    private static async Task DownloadSeason(SdarotDriver driver, SeasonInformation season, string downloadLocation) => await DownloadEpisodes(driver, await driver.GetEpisodesAsync(season), downloadLocation);
    private static async Task DownloadEpisode(SdarotDriver driver, EpisodeInformation episode, string downloadLocation) => await DownloadEpisodes(driver, new[] { episode }, downloadLocation);
    private static async Task DownloadEpisodes(SdarotDriver driver, EpisodeInformation episode, int episodesAmount, string downloadLocation) => await DownloadEpisodes(driver, await driver.GetEpisodesAsync(episode, episodesAmount), downloadLocation);

    private static async Task<IEnumerable<EpisodeInformation>?> DownloadEpisodes(SdarotDriver driver, IEnumerable<EpisodeInformation> episodes, string downloadLocation, bool rootRun = true)
    {
        try
        {
            await LoginToWebsite(driver);

            var failedEpisodes = new List<EpisodeInformation>();

            var episodesList = episodes.ToList();

            if (!AppSettings.Default.ForceDownload)
            {
                List<EpisodeInformation> filteredList = new();
                foreach (var episode in episodesList)
                {
                    if (!File.Exists(GetFullDownloadLocation(downloadLocation, episode)))
                    {
                        filteredList.Add(episode);
                    }
                }

                var exists = episodesList.Count - filteredList.Count;
                if (exists > 0)
                {
                    IOHelpers.Print($"\nFound {exists} existing episodes. Ignoring them.");
                    episodesList = filteredList;
                }
            }

            foreach (var (episode, i) in episodesList.Select((value, i) => (value, i)))
            {
                IOHelpers.Print($"\n({i + 1}/{episodesList.Count})");
                var finalLocation = GetFullDownloadLocation(downloadLocation, episode);
                if (File.Exists(finalLocation) && !AppSettings.Default.ForceDownload)
                {
                    IOHelpers.Print($"{episode.Season.SeasonString} {episode.EpisodeString} already downloaded. Skipping.");
                    continue;
                }

                IOHelpers.Log($"Loading {episode.Season.SeasonString} {episode.EpisodeString}...");
                var episodeMediaUrl = await GetEpisodeMediaUrl(driver, episode);
                if (episodeMediaUrl is null)
                {
                    failedEpisodes.Add(episode);
                    IOHelpers.Log("Failed. Proceeding to next episode.");
                    continue;
                }

                IOHelpers.Log($"Downloading {episode.Season.SeasonString} {episode.EpisodeString}...");
                _ = Directory.CreateDirectory(Path.GetDirectoryName(finalLocation)!);
                using var file = new FileStream(finalLocation, FileMode.Create, FileAccess.Write, FileShare.None);
                await driver.DownloadEpisode($"https:{episodeMediaUrl}", file);
                IOHelpers.Log("Download completed.");
            }

            IOHelpers.Log("Finished.");

            if (rootRun)
            {
                var tryAgainAmount = 1;
                while (failedEpisodes.Count > 0)
                {
                    if (tryAgainAmount == 0)
                    {
                        tryAgainAmount += HandleFailedEpisodes(failedEpisodes, downloadLocation);

                        if (tryAgainAmount == 0)
                        {
                            break;
                        }
                    }

                    tryAgainAmount--;
                    IOHelpers.Print($"\nTrying again {failedEpisodes.Count} failed episodes");
                    failedEpisodes = (await DownloadEpisodes(driver, failedEpisodes, downloadLocation, false) ?? Enumerable.Empty<EpisodeInformation>()).ToList();
                }

                SummarizeDownload(episodesList.Count, failedEpisodes);
                IOHelpers.Print("\nDone. Returning to start.");
            }

            return failedEpisodes;
        }
        catch { /* Every exception thrown */ }

        return null;
    }

    private static int HandleFailedEpisodes(List<EpisodeInformation> episodes, string downloadLocation)
    {
        IOHelpers.Print("\nThere are still some failed episodes.\nWhat do you want to do with them?");
        IOHelpers.Print(Menus.FAILED_MENU);
        var option = (FailedOptions)IOHelpers.ChooseOptionRange(Enum.GetNames(typeof(FailedOptions)).Length - 1, "Choose an option");

        if (option == FailedOptions.None)
        {
            return 0;
        }

        if (option == FailedOptions.TryAgain)
        {
            return IOHelpers.InputPositiveInt("\nEnter retries amount (0 - cancel): ");
        }

        if (option == FailedOptions.Export)
        {
            var finalLocation = Path.Combine(downloadLocation, $"FailedEpisodes_{DateTime.Now:HH_mm_ss_dd_MM_yyyy}.epis");
            File.WriteAllText(finalLocation, JsonSerializer.Serialize(episodes, options: new JsonSerializerOptions { WriteIndented = true }));
        }

        return 0;
    }

    private static string GetFullDownloadLocation(string downloadLocation, EpisodeInformation episode)
    {
        var cleanSeriesName = string.Concat(episode.Series.SeriesNameEn.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
        var finalLocation = Path.Combine(downloadLocation, cleanSeriesName, episode.Season.SeasonString, episode.EpisodeString + ".mp4");
        return finalLocation;
    }

    private static void SummarizeDownload(int total, IEnumerable<EpisodeInformation>? failed = null)
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

    private static async Task LoginToWebsite(SdarotDriver driver)
    {
        if (await driver.IsLoggedIn())
        {
            return;
        }

        IOHelpers.Print("\nYou need to log in to download episodes.");

        var settings = AppSettings.Default;
        if (!string.IsNullOrWhiteSpace(settings.SdarotUsername) && !string.IsNullOrWhiteSpace(settings.SdarotPassword))
        {
            IOHelpers.Print("\nSaved credentials detected. Trying to log in...");
            if (await driver.Login(settings.SdarotUsername, settings.SdarotPassword))
            {
                IOHelpers.Print("Logged in successfully, proceeding.");
                return;
            }

            settings.ResetCredentials();
            IOHelpers.Print("Bad credentials, proceeding to manual login.");
        }

        while (true)
        {
            var username = IOHelpers.Input("\nUsername or email: ");
            var password = IOHelpers.Input("Password: ");
            if (await driver.Login(username, password))
            {
                IOHelpers.Print("Logged in successfully, proceeding.");
                settings.SaveCredentials(username, password);
                return;
            }

            IOHelpers.Print("Bad credentials, please try again.");
        }
    }

    private static async Task<string?> GetEpisodeMediaUrl(SdarotDriver driver, EpisodeInformation episode, int retries = 2)
    {
        do
        {
            try
            {
                return await driver.GetEpisodeMediaUrlAsync(episode);
            }
            catch (Error2Exception)
            {
                IOHelpers.Log("Error 2, Skipping.");
                return null;
            }
            catch
            {
                if (retries > 0)
                {
                    IOHelpers.Log($"Failed. Trying again... ({retries} tries left)");
                }

                retries--;

            }
        } while (retries > -1);

        return null;
    }
}
