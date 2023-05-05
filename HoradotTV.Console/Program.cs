namespace HoradotTV.Console;

internal static class Program
{
    private static SdarotDriver? driver;

    private static async Task Main()
    {
        IOHelper.Print($"Welcome to HoradotTV {Constants.SoftwareVersion}!");
        IOHelper.Print("Initializing...");
        Initialize();
        if (driver is null)
        {
            Environment.Exit(0);
        }

        while (true)
        {
            var show = await SelectShow(driver);
            if (show is null)
            {
                continue;
            }

            var downloadLocation = SelectDownloadLocation();
            IOHelper.Print("Full download path: " + Path.GetFullPath(downloadLocation));

            var mode = ChooseMode();
            if (mode == Mode.None)
            {
                continue;
            }

            if (mode == Mode.Show)
            {
                await DownloadShow(driver, show, downloadLocation);
                continue;
            }

            var season = await ChooseSeason(driver, show);
            if (season is null)
            {
                continue;
            }

            if (mode == Mode.Season)
            {
                await DownloadSeason(driver, season, downloadLocation);
                continue;
            }

            var episode = await ChooseEpisode(driver, season);
            if (episode is null)
            {
                continue;
            }

            if (mode == Mode.Episode)
            {
                await DownloadEpisode(driver, episode, downloadLocation);
                continue;
            }

            var episodesAmount = IOHelper.InputPositiveInt("\nEnter episodes amount (0 - cancel): ");
            if (episodesAmount == 0)
            {
                continue;
            }

            await DownloadEpisodes(driver, episode, episodesAmount, downloadLocation);
        }
    }

    private static void Initialize()
    {
        System.Console.InputEncoding = Encoding.Unicode;
        System.Console.OutputEncoding = Encoding.Unicode;

        try
        {
            driver = new SdarotDriver();
        }
        catch (SdarotBlockedException)
        {
            IOHelper.Print($"\nSdarotTV is blocked. Please follow this guide to unblock it:\n{Constants.SdarotTVConnectionProblemGuide}");
        }
    }

    private static async Task<ShowInformation?> SelectShow(SdarotDriver driver)
    {
        var searchResult = new List<ShowInformation>();
        do
        {
            var query = IOHelper.Input($"\nEnter show name or part of it ({Constants.Commands.Quit} - quit): ");
            if (query == Constants.Commands.Quit)
            {
                Environment.Exit(0);
            }

            if (query.Length < Constants.QueryMinLength)
            {
                IOHelper.Print($"Please enter at least {Constants.QueryMinLength} characters.");
                continue;
            }

            searchResult = (await driver.SearchShow(query)).ToList();
            if (searchResult.Count == 0)
            {
                IOHelper.Print("Show not found.");
            }
        } while (searchResult.Count == 0);

        IOHelper.Print("\nResults:");
        IOHelper.Print("[0] Back to start");
        for (var i = 0; i < searchResult.Count; i++)
        {
            IOHelper.Print($"[{i + 1}] {searchResult[i].NameEn} - {searchResult[i].NameHe}");
        }

        var selection = IOHelper.ChooseOptionRange(searchResult.Count, "Choose a show");
        return selection == 0 ? null : searchResult[selection - 1];
    }

    private static string SelectDownloadLocation()
    {
        string path;
        do
        {
            var settings = AppSettings.Default;
            var defaultPath = !string.IsNullOrWhiteSpace(settings.LastPath) ? settings.LastPath : Constants.DefaultDownloadLocation;
            path = IOHelper.Input($"\nEnter path for download (empty - {defaultPath}): ").Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                path = defaultPath;
            }

            try
            {
                var fullPath = Path.GetFullPath(path);
                if (!File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory))
                {
                    IOHelper.Print("Please enter a path to a directory.");
                    path = "";
                }

                settings.LastPath = path;
                settings.Save();
            }
            catch
            {
                IOHelper.Print("Please enter a valid path.");
                path = "";
            }
        } while (string.IsNullOrWhiteSpace(path));

        return path;
    }

    private static Mode ChooseMode()
    {
        IOHelper.Print("\n" + Menus.ModesMenu);
        return (Mode)IOHelper.ChooseOptionRange(Enum.GetNames(typeof(Mode)).Length - 1, "Choose a mode");
    }

    private static async Task<SeasonInformation?> ChooseSeason(SdarotDriver driver, ShowInformation show)
    {
        var seasons = await driver.GetSeasonsAsync(show);
        var seasonName = IOHelper.ChooseOption(seasons.Select(s => s.Name), "season", "Choose a season");
        if (seasonName == Constants.Commands.Cancel)
        {
            return null;
        }

        var season = seasons.Where(s => s.Name == seasonName).First();
        return season;
    }

    private static async Task<EpisodeInformation?> ChooseEpisode(SdarotDriver driver, SeasonInformation season)
    {
        var episodes = await driver.GetEpisodesAsync(season);
        var episodeName = IOHelper.ChooseOption(episodes.Select(e => e.Name), "episode", "Choose an episode");
        if (episodeName == Constants.Commands.Cancel)
        {
            return null;
        }

        var episode = episodes.Where(e => e.Name == episodeName).First();
        return episode;
    }

    private static async Task DownloadShow(SdarotDriver driver, ShowInformation show, string downloadLocation) => await DownloadEpisodes(driver, await driver.GetEpisodesAsync(show), downloadLocation);
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
                    IOHelper.Print($"\nFound {exists} existing episodes. Ignoring them.");
                    episodesList = filteredList;
                }
            }

            foreach (var (episode, i) in episodesList.Select((value, i) => (value, i)))
            {
                IOHelper.Print($"\n({i + 1}/{episodesList.Count})");
                var finalLocation = GetFullDownloadLocation(downloadLocation, episode);
                if (File.Exists(finalLocation) && !AppSettings.Default.ForceDownload)
                {
                    IOHelper.Print($"{episode.Season} {episode} already downloaded. Skipping.");
                    continue;
                }

                IOHelper.Log($"Loading {episode.Season} {episode}...");
                var episodeMediaUrl = await GetEpisodeMediaUrl(driver, episode);
                if (episodeMediaUrl is null)
                {
                    failedEpisodes.Add(episode);
                    IOHelper.Log("Failed. Proceeding to next episode.");
                    continue;
                }

                IOHelper.Log($"Downloading {episode.Season} {episode}...");
                _ = Directory.CreateDirectory(Path.GetDirectoryName(finalLocation)!);
                using var file = new FileStream(finalLocation, FileMode.Create, FileAccess.Write, FileShare.None);
                await driver.DownloadEpisode($"https:{episodeMediaUrl}", file);
                IOHelper.Log("Download completed.");
            }

            IOHelper.Log("Finished.");

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
                    IOHelper.Print($"\nTrying again {failedEpisodes.Count} failed episodes");
                    failedEpisodes = (await DownloadEpisodes(driver, failedEpisodes, downloadLocation, false) ?? Enumerable.Empty<EpisodeInformation>()).ToList();
                }

                SummarizeDownload(episodesList.Count, failedEpisodes);
                IOHelper.Print("\nDone. Returning to start.");
            }

            return failedEpisodes;
        }
        catch { /* Every exception thrown */ }

        return null;
    }

    private static int HandleFailedEpisodes(List<EpisodeInformation> episodes, string downloadLocation)
    {
        IOHelper.Print("\nThere are still some failed episodes.\nWhat do you want to do with them?");
        IOHelper.Print(Menus.FailedMenu);
        var option = (FailedOption)IOHelper.ChooseOptionRange(Enum.GetNames(typeof(FailedOption)).Length - 1, "Choose an option");

        if (option == FailedOption.None)
        {
            return 0;
        }

        if (option == FailedOption.TryAgain)
        {
            return IOHelper.InputPositiveInt("\nEnter retries amount (0 - cancel): ");
        }

        if (option == FailedOption.Export)
        {
            var finalLocation = Path.Combine(downloadLocation, $"FailedEpisodes_{DateTime.Now:HH_mm_ss_dd_MM_yyyy}.epis");
            File.WriteAllText(finalLocation, JsonSerializer.Serialize(episodes, options: new JsonSerializerOptions { WriteIndented = true }));
        }

        return 0;
    }

    private static string GetFullDownloadLocation(string downloadLocation, EpisodeInformation episode)
    {
        var cleanShowName = string.Concat(episode.Show.NameEn.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
        var finalLocation = Path.Combine(downloadLocation, cleanShowName, episode.Season.ToString(), episode.ToString() + ".mp4");
        return finalLocation;
    }

    private static void SummarizeDownload(int total, IEnumerable<EpisodeInformation>? failed = null)
    {
        failed ??= Enumerable.Empty<EpisodeInformation>();
        var success = total - failed.Count();
        var successPercentage = (int)((double)success / total * 100);
        IOHelper.Print("\nDownload Summary:");
        IOHelper.Print($"Total   = {total}");
        IOHelper.Print($"Success = {success}\t({successPercentage}%)");
        IOHelper.Print($"Fail    = {failed.Count()}\t({100 - successPercentage}%)");
        if (failed.Any())
        {
            IOHelper.Print($"Failed episodes:");
            foreach (var episode in failed)
            {
                IOHelper.Print($"\t{episode.Season} {episode}");
            }
        }
    }

    private static async Task LoginToWebsite(SdarotDriver driver)
    {
        if (await driver.IsLoggedIn())
        {
            return;
        }

        IOHelper.Print("\nYou need to log in to download episodes.");

        var settings = AppSettings.Default;
        if (!string.IsNullOrWhiteSpace(settings.SdarotUsername) && !string.IsNullOrWhiteSpace(settings.SdarotPassword))
        {
            IOHelper.Print("\nSaved credentials detected. Trying to log in...");
            if (await driver.Login(settings.SdarotUsername, settings.SdarotPassword))
            {
                IOHelper.Print("Logged in successfully, proceeding.");
                return;
            }

            settings.ResetCredentials();
            IOHelper.Print("Bad credentials, proceeding to manual login.");
        }

        while (true)
        {
            var username = IOHelper.Input("\nUsername or email: ");
            var password = IOHelper.Input("Password: ");
            if (await driver.Login(username, password))
            {
                IOHelper.Print("Logged in successfully, proceeding.");
                settings.SaveCredentials(username, password);
                return;
            }

            IOHelper.Print("Bad credentials, please try again.");
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
                IOHelper.Log("Error 2, Skipping.");
                return null;
            }
            catch
            {
                if (retries > 0)
                {
                    IOHelper.Log($"Failed. Trying again... ({retries} tries left)");
                }

                retries--;

            }
        } while (retries > -1);

        return null;
    }
}
