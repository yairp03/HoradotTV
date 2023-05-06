namespace HoradotTV.Console;

internal static class Program
{
    private static SdarotDriver? _driver;

    private static async Task Main()
    {
        IOHelper.Print($"Welcome to HoradotTV {Constants.SoftwareVersion}!");
        IOHelper.Print("Initializing...");
        Initialize();
        if (_driver is null)
        {
            return;
        }

        MainMenuOption option;
        do
        {
            option = MainMenuChoose();
            switch (option)
            {
                case MainMenuOption.Quit:
                    break;
                case MainMenuOption.Search:
                    await DownloadFromSearch();
                    break;
                case MainMenuOption.DownloadFromFile:
                    await DownloadFromFile();
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected value option={option}");
            }
        } while (option != MainMenuOption.Quit);
    }

    private static void Initialize()
    {
        System.Console.InputEncoding = Encoding.Unicode;
        System.Console.OutputEncoding = Encoding.Unicode;

        try
        {
            _driver = new SdarotDriver();
        }
        catch (SdarotBlockedException)
        {
            IOHelper.Print(
                $"\nSdarotTV is blocked. Please follow this guide to unblock it:\n{Constants.SdarotConnectionProblemGuide}");
        }
    }

    private static async Task DownloadFromSearch()
    {
        if (_driver is null)
        {
            return;
        }

        var show = await SelectShow();
        if (show is null)
        {
            return;
        }

        string mediaLibraryLocation = await SelectMediaLibraryLocationAsync();
        IOHelper.Print("Full download path: " + mediaLibraryLocation);

        var mode = ChooseMode();
        if (mode == Mode.None)
        {
            return;
        }

        if (mode == Mode.Show)
        {
            await DownloadShow(show, mediaLibraryLocation);
            return;
        }

        var season = await ChooseSeason(show);
        if (season is null)
        {
            return;
        }

        if (mode == Mode.Season)
        {
            await DownloadSeason(season, mediaLibraryLocation);
            return;
        }

        var episode = await ChooseEpisode(season);
        if (episode is null)
        {
            return;
        }

        if (mode == Mode.Episode)
        {
            await DownloadEpisode(episode, mediaLibraryLocation);
            return;
        }

        if (mode == Mode.Episodes)
        {
            int episodesAmount = IOHelper.InputPositiveInt("\nEnter episodes amount (0 - cancel): ");
            if (episodesAmount == 0)
            {
                return;
            }

            await DownloadEpisodes(episode, episodesAmount, mediaLibraryLocation);
        }
    }

    private static async Task DownloadFromFile()
    {
        string mediaLibraryLocation = await SelectMediaLibraryLocationAsync();

        var episodesFiles = Directory.GetFiles(mediaLibraryLocation, $"*.{Constants.EpisodesFileExtension}")
            .Select(p => new FileInfo(p)).ToList();

        if (episodesFiles.Count == 0)
        {
            IOHelper.Print($"Didn't found any .{Constants.EpisodesFileExtension} files.");
            return;
        }

        episodesFiles.Sort((f1, f2) => f2.LastWriteTime.CompareTo(f1.LastWriteTime));

        IOHelper.Print("\nAvailable files (from new to old):");
        int selection = IOHelper.ChooseOptionIndex(episodesFiles.Select(f => $"({f.LastWriteTime:G}) - {f.Name}"),
            "Choose as file");
        if (selection == 0)
        {
            return;
        }

        var episodesList =
            await JsonSerializer.DeserializeAsync<List<EpisodeInformation>>(episodesFiles[selection - 1].OpenRead());
        if (episodesList is null)
        {
            IOHelper.Print("Bad file format.");
            return;
        }

        _ = await DownloadEpisodes(episodesList, mediaLibraryLocation);
    }

    private static async Task<ShowInformation?> SelectShow()
    {
        var searchResult = new List<ShowInformation>();
        do
        {
            string query = IOHelper.Input($"\nEnter show name or part of it ({Constants.Commands.Cancel} - cancel): ");
            if (query == Constants.Commands.Cancel)
            {
                return null;
            }

            if (query.Length < Constants.QueryMinLength)
            {
                IOHelper.Print($"Please enter at least {Constants.QueryMinLength} characters.");
                continue;
            }

            searchResult = (await _driver!.SearchShow(query)).ToList();
            if (searchResult.Count == 0)
            {
                IOHelper.Print("Show not found.");
            }
        } while (searchResult.Count == 0);

        IOHelper.Print("\nResults:");
        int selection =
            IOHelper.ChooseOptionIndex(searchResult.Select(s => $"{s.NameEn} - {s.NameHe}"), "Choose as show");

        return selection == 0 ? null : searchResult[selection - 1];
    }

    private static async Task<string> SelectMediaLibraryLocationAsync()
    {
        string path;
        do
        {
            var settings = AppSettings.Default;
            string? defaultPath = !string.IsNullOrWhiteSpace(settings.LastPath)
                ? settings.LastPath
                : Constants.DefaultDownloadLocation;
            path = IOHelper.Input($"\nEnter path for media library (empty - {defaultPath}): ").Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                path = defaultPath;
            }

            try
            {
                string fullPath = Path.GetFullPath(path);
                if (!File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory))
                {
                    IOHelper.Print("Please enter a path to a directory.");
                    path = "";
                }
                else
                {
                    path = fullPath;
                    settings.LastPath = path;
                    await settings.SaveAsync();
                }
            }

            catch
            {
                IOHelper.Print("Please enter a valid path.");
                path = "";
            }
        } while (string.IsNullOrWhiteSpace(path));

        return path;
    }

    private static MainMenuOption MainMenuChoose()
    {
        IOHelper.Print("\n" + Menus.MainMenu);
        return (MainMenuOption)IOHelper.ChooseOptionRange(Enum.GetNames(typeof(MainMenuOption)).Length - 1);
    }

    private static Mode ChooseMode()
    {
        IOHelper.Print("\n" + Menus.ModesMenu);
        return (Mode)IOHelper.ChooseOptionRange(Enum.GetNames(typeof(Mode)).Length - 1, "Choose a mode");
    }

    private static async Task<SeasonInformation?> ChooseSeason(ShowInformation show)
    {
        var seasons = (await _driver!.GetSeasonsAsync(show)).ToList();
        string seasonName = IOHelper.ChooseOption(seasons.Select(s => s.Name), "season", "Choose a season");
        if (seasonName == Constants.Commands.Cancel)
        {
            return null;
        }

        var season = seasons.First(s => s.Name == seasonName);
        return season;
    }

    private static async Task<EpisodeInformation?> ChooseEpisode(SeasonInformation season)
    {
        var episodes = (await _driver!.GetEpisodesAsync(season)).ToList();
        string episodeName = IOHelper.ChooseOption(episodes.Select(e => e.Name), "episode", "Choose an episode");
        if (episodeName == Constants.Commands.Cancel)
        {
            return null;
        }

        var episode = episodes.First(e => e.Name == episodeName);
        return episode;
    }

    private static async Task DownloadShow(ShowInformation show, string downloadLocation) =>
        await DownloadEpisodes(await _driver!.GetEpisodesAsync(show), downloadLocation);

    private static async Task DownloadSeason(SeasonInformation season, string downloadLocation) =>
        await DownloadEpisodes(await _driver!.GetEpisodesAsync(season), downloadLocation);

    private static async Task DownloadEpisode(EpisodeInformation episode, string downloadLocation) =>
        await DownloadEpisodes(new[] { episode }, downloadLocation);

    private static async Task
        DownloadEpisodes(EpisodeInformation episode, int episodesAmount, string downloadLocation) =>
        await DownloadEpisodes(await _driver!.GetEpisodesAsync(episode, episodesAmount), downloadLocation);

    private static async Task<IEnumerable<EpisodeInformation>?> DownloadEpisodes(
        IEnumerable<EpisodeInformation> episodes, string downloadLocation, bool rootRun = true)
    {
        if (_driver is null)
        {
            return null;
        }

        try
        {
            await LoginToWebsite();

            var failedEpisodes = new List<EpisodeInformation>();

            var episodesList = episodes.ToList();

            if (!AppSettings.Default.ForceDownload)
            {
                var filteredList = episodesList
                    .Where(episode => !File.Exists(GetFullDownloadLocation(downloadLocation, episode))).ToList();

                int exists = episodesList.Count - filteredList.Count;
                if (exists > 0)
                {
                    IOHelper.Print($"\nFound {exists} existing episodes. Ignoring them.");
                    episodesList = filteredList;
                }
            }

            foreach (var (episode, i) in episodesList.Select((value, i) => (value, i)))
            {
                IOHelper.Print($"\n({i + 1}/{episodesList.Count})");
                string finalLocation = GetFullDownloadLocation(downloadLocation, episode);
                if (File.Exists(finalLocation) && !AppSettings.Default.ForceDownload)
                {
                    IOHelper.Print($"{episode.Season} {episode} already downloaded. Skipping.");
                    continue;
                }

                IOHelper.Log($"Loading {episode.Season} {episode}...");
                string? episodeMediaUrl = await GetEpisodeMediaUrl(episode);
                if (episodeMediaUrl is null)
                {
                    failedEpisodes.Add(episode);
                    IOHelper.Log("Failed. Proceeding to next episode.");
                    continue;
                }

                IOHelper.Log($"Downloading {episode.Season} {episode}...");
                _ = Directory.CreateDirectory(Path.GetDirectoryName(finalLocation)!);
                await using var file = new FileStream(finalLocation, FileMode.Create, FileAccess.Write, FileShare.None);
                await _driver.DownloadEpisodeAsync($"https:{episodeMediaUrl}", file);
                IOHelper.Log("Download completed.");
            }

            IOHelper.Log("Finished.");

            if (!rootRun)
            {
                return failedEpisodes;
            }

            int tryAgainAmount = 1;
            while (failedEpisodes.Count > 0)
            {
                if (tryAgainAmount == 0)
                {
                    tryAgainAmount += await HandleFailedEpisodes(failedEpisodes, downloadLocation);

                    if (tryAgainAmount == 0)
                    {
                        break;
                    }
                }

                tryAgainAmount--;
                IOHelper.Print($"\nTrying again {failedEpisodes.Count} failed episodes");
                failedEpisodes = (await DownloadEpisodes(failedEpisodes, downloadLocation, false) ??
                                  Enumerable.Empty<EpisodeInformation>()).ToList();
            }

            SummarizeDownload(episodesList.Count, failedEpisodes);
            IOHelper.Print("\nDone. Returning to start.");

            return failedEpisodes;
        }
        catch
        {
            /* Every exception thrown */
        }

        return null;
    }

    private static async Task<int> HandleFailedEpisodes(List<EpisodeInformation> episodes, string downloadLocation)
    {
        IOHelper.Print("\nThere are still some failed episodes.\nWhat do you want to do with them?");
        IOHelper.Print(Menus.FailedMenu);
        var option = (FailedOption)IOHelper.ChooseOptionRange(Enum.GetNames(typeof(FailedOption)).Length - 1);

        if (option == FailedOption.None)
        {
            return 0;
        }

        if (option == FailedOption.TryAgain)
        {
            return IOHelper.InputPositiveInt("\nEnter retries amount (0 - cancel): ");
        }

        if (option != FailedOption.Export)
        {
            return 0;
        }

        string finalLocation = Path.Combine(downloadLocation,
            $"FailedEpisodes_{DateTime.Now:HH_mm_ss_dd_MM_yyyy}.{Constants.EpisodesFileExtension}");
        string a = JsonSerializer.Serialize(episodes, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(finalLocation, a);
        IOHelper.Print($"Exported to {finalLocation}");

        return 0;
    }

    private static string GetFullDownloadLocation(string downloadLocation, EpisodeInformation episode)
    {
        string cleanShowName =
            string.Concat(episode.Show.NameEn.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
        string finalLocation =
            Path.Combine(downloadLocation, cleanShowName, episode.Season.ToString(), episode + ".mp4");
        return finalLocation;
    }

    private static void SummarizeDownload(int total, IEnumerable<EpisodeInformation>? failed = null)
    {
        var failedList = failed?.ToList() ?? new List<EpisodeInformation>();
        int success = total - failedList.Count;
        int successPercentage = (int)((double)success / total * 100);
        IOHelper.Print("\nDownload Summary:");
        IOHelper.Print($"Total   = {total}");
        IOHelper.Print($"Success = {success}\t({successPercentage}%)");
        IOHelper.Print($"Fail    = {failedList.Count}\t({100 - successPercentage}%)");
        if (!failedList.Any())
        {
            return;
        }

        IOHelper.Print("Failed episodes:");
        foreach (var episode in failedList)
        {
            IOHelper.Print($"\t{episode.Season} {episode}");
        }
    }

    private static async Task LoginToWebsite()
    {
        if (_driver is null)
        {
            return;
        }

        if (await _driver.IsLoggedIn())
        {
            return;
        }

        IOHelper.Print("\nYou need to log in to download episodes.");

        var settings = AppSettings.Default;
        if (!string.IsNullOrWhiteSpace(settings.SdarotUsername) && !string.IsNullOrWhiteSpace(settings.SdarotPassword))
        {
            IOHelper.Print("\nSaved credentials detected. Trying to log in...");
            if (await _driver.Login(settings.SdarotUsername, settings.SdarotPassword))
            {
                IOHelper.Print("Logged in successfully, proceeding.");
                return;
            }

            await settings.ResetCredentialsAsync();
            IOHelper.Print("Bad credentials, proceeding to manual login.");
        }

        while (true)
        {
            string username = IOHelper.Input("\nUsername or email: ");
            string password = IOHelper.Input("Password: ");
            if (await _driver.Login(username, password))
            {
                IOHelper.Print("Logged in successfully, proceeding.");
                await settings.SaveCredentialsAsync(username, password);
                return;
            }

            IOHelper.Print("Bad credentials, please try again.");
        }
    }

    private static async Task<string?> GetEpisodeMediaUrl(EpisodeInformation episode, int retries = 2)
    {
        if (_driver is null)
        {
            return null;
        }

        do
        {
            try
            {
                return await _driver.GetEpisodeMediaUrlAsync(episode);
            }
            catch (WebsiteErrorException)
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
