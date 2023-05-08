namespace HoradotTV.Console;

internal static class Program
{
    private static readonly HoradotService HoradotService = new();

    private static async Task Main()
    {
        IOHelper.Print($"Welcome to HoradotTV {Constants.SoftwareVersion}!");
        IOHelper.Print("Initializing...");
        if (!await InitializeAsync())
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

    private static async Task<bool> InitializeAsync()
    {
        System.Console.InputEncoding = Encoding.Unicode;
        System.Console.OutputEncoding = Encoding.Unicode;

        var result = await HoradotService.InitializeAsync();
        if (result.success)
        {
            return true;
        }

        System.Console.WriteLine(result.errorMessage);
        return false;
    }

    private static async Task DownloadFromSearch()
    {
        var media = await SelectMedia();

        if (media is null)
        {
            return;
        }

        string mediaLibraryLocation = await SelectMediaLibraryLocationAsync();
        IOHelper.Print("Full download path: " + mediaLibraryLocation);

        if (media is MovieInformation movie)
        {
            await DownloadMedia(movie, mediaLibraryLocation);
            return;
        }

        if (media is not ShowInformation show)
        {
            throw new BadMediaTypeException();
        }

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
            await DownloadMedia(episode, mediaLibraryLocation);
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

        var mediaFiles = Directory.GetFiles(mediaLibraryLocation, $"*.{Constants.MediaFileExtension}")
            .Select(p => new FileInfo(p)).ToList();

        if (mediaFiles.Count == 0)
        {
            IOHelper.Print($"Didn't found any .{Constants.MediaFileExtension} files.");
            return;
        }

        mediaFiles.Sort((f1, f2) => f2.LastWriteTime.CompareTo(f1.LastWriteTime));

        IOHelper.Print("\nAvailable files (from new to old):");
        int selection = IOHelper.ChooseOptionIndex(mediaFiles.Select(f => $"({f.LastWriteTime:G}) - {f.Name}"),
            "Choose as file");
        if (selection == 0)
        {
            return;
        }

        var mediaList =
            await JsonSerializer.DeserializeAsync<List<MediaInformation>>(mediaFiles[selection - 1].OpenRead());
        if (mediaList is null)
        {
            IOHelper.Print("Bad file format.");
            return;
        }

        _ = await DownloadMedia(mediaList, mediaLibraryLocation);
    }

    private static async Task<MediaInformation?> SelectMedia()
    {
        var searchResult = new List<MediaInformation>();
        do
        {
            string query =
                IOHelper.Input($"\nEnter show/movie name or part of it ({Constants.Commands.Cancel} - cancel): ");
            if (query == Constants.Commands.Cancel)
            {
                return null;
            }

            if (query.Length < Constants.QueryMinLength)
            {
                IOHelper.Print($"Please enter at least {Constants.QueryMinLength} characters.");
                continue;
            }

            searchResult = (await HoradotService.SearchAsync(query)).ToList();
            if (searchResult.Count == 0)
            {
                IOHelper.Print("Not found.");
            }
        } while (searchResult.Count == 0);

        IOHelper.Print("\nResults:");
        int selection =
            IOHelper.ChooseOptionIndex(searchResult.Select(s => $"({s.ProviderName}) {s.Name} - {s.NameHe}"), "Choose a show/movie");

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
        var seasons = (await HoradotService.GetSeasonsAsync(show)).ToList();
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
        var episodes = (await HoradotService.GetEpisodesAsync(season)).ToList();
        string episodeName = IOHelper.ChooseOption(episodes.Select(e => e.Name), "episode", "Choose an episode");
        if (episodeName == Constants.Commands.Cancel)
        {
            return null;
        }

        var episode = episodes.First(e => e.Name == episodeName);
        return episode;
    }

    private static async Task DownloadShow(ShowInformation media, string downloadLocation) =>
        await DownloadMedia(await HoradotService.GetEpisodesAsync(media), downloadLocation);

    private static async Task DownloadSeason(SeasonInformation season, string downloadLocation) =>
        await DownloadMedia(await HoradotService.GetEpisodesAsync(season), downloadLocation);

    private static Task DownloadMedia(MediaInformation media, string downloadLocation) =>
        DownloadMedia(media.Yield(), downloadLocation);

    private static async Task
        DownloadEpisodes(EpisodeInformation episode, int episodesAmount, string downloadLocation) =>
        await DownloadMedia(await HoradotService.GetEpisodesAsync(episode, episodesAmount), downloadLocation);

    private static async Task<IEnumerable<MediaInformation>?> DownloadMedia(
        IEnumerable<MediaInformation> mediaToDownload, string downloadLocation, bool rootRun = true)
    {
        try
        {
            List<MediaInformation> failedMedia = new();

            var mediaList = mediaToDownload.ToList();

            await LoginToProviders(mediaList.Select(m => m.ProviderName).Distinct());

            if (!AppSettings.Default.ForceDownload)
            {
                var filteredList = mediaList
                    .Where(episode => !File.Exists(GetFullDownloadLocation(downloadLocation, episode))).ToList();

                int exists = mediaList.Count - filteredList.Count;
                if (exists > 0)
                {
                    IOHelper.Print($"\nFound {exists} existing media files. Ignoring them.");
                    mediaList = filteredList;
                }
            }

            foreach (var (media, i) in mediaList.Select((value, i) => (value, i)))
            {
                IOHelper.Print($"\n({i + 1}/{mediaList.Count})");
                string finalLocation = GetFullDownloadLocation(downloadLocation, media);

                IOHelper.Log($"Loading {media}...");
                var mediaDownloadInformation = await LoadMedia(media);
                if (mediaDownloadInformation is null)
                {
                    failedMedia.Add(media);
                    IOHelper.Log("Failed. Proceeding to next media.");
                    continue;
                }

                IOHelper.Log($"Downloading {media}...");
                _ = Directory.CreateDirectory(Path.GetDirectoryName(finalLocation)!);
                await using var file = new FileStream(finalLocation, FileMode.Create, FileAccess.Write, FileShare.None);
                await HoradotService.DownloadAsync(mediaDownloadInformation,
                    mediaDownloadInformation.Resolutions.Max(r => r.Key), file);
                IOHelper.Log("Download completed.");
            }

            IOHelper.Log("Finished.");
            SummarizeDownload(mediaList.Count, failedMedia);

            if (!rootRun)
            {
                return failedMedia;
            }

            int tryAgainAmount = 1;
            while (failedMedia.Count > 0)
            {
                if (tryAgainAmount == 0)
                {
                    tryAgainAmount += await HandleFailedMedia(failedMedia, downloadLocation);

                    if (tryAgainAmount == 0)
                    {
                        break;
                    }
                }

                tryAgainAmount--;
                IOHelper.Print($"\nTrying again {failedMedia.Count} failed media");
                failedMedia = (await DownloadMedia(failedMedia, downloadLocation, false) ??
                               Enumerable.Empty<MediaInformation>()).ToList();
            }

            IOHelper.Print("\nDone. Returning to start.");

            return failedMedia;
        }
        catch (Exception e)
        {
            IOHelper.Print($"Caught exception: {e}");
            /* Every exception thrown */
        }

        return null;
    }

    private static async Task<int> HandleFailedMedia(List<MediaInformation> media, string downloadLocation)
    {
        IOHelper.Print("\nThere are still some failed media.\nWhat do you want to do with them?");
        IOHelper.Print(Menus.FailedMenu);
        var option = (FailedOption)IOHelper.ChooseOptionRange(Enum.GetNames(typeof(FailedOption)).Length - 1);

        switch (option)
        {
            case FailedOption.None:
                return 0;
            case FailedOption.TryAgain:
                return IOHelper.InputPositiveInt("\nEnter retries amount (0 - cancel): ");
            case FailedOption.Export:
            {
                string finalLocation = Path.Combine(downloadLocation,
                    $"FailedEpisodes_{DateTime.Now:HH_mm_ss_dd_MM_yyyy}.{Constants.MediaFileExtension}");
                string mediaDetails =
                    JsonSerializer.Serialize(media, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(finalLocation, mediaDetails);
                IOHelper.Print($"Exported to {finalLocation}");
                break;
            }
        }

        return 0;
    }

    private static string GetFullDownloadLocation(string downloadLocation, MediaInformation media)
    {
        return media switch
        {
            MovieInformation movie => Path.Combine(downloadLocation, CleanName(movie.Name),
                movie.Name + $".{Constants.DefaultMediaFormat}"),
            EpisodeInformation episode => Path.Combine(downloadLocation, CleanName(episode.Show.Name),
                episode.Season.RelativeName, episode.RelativeName + $".{Constants.DefaultMediaFormat}"),
            _ => throw new BadMediaTypeException()
        };
    }

    private static string CleanName(string name)
    {
        return string.Concat(name.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
    }

    private static void SummarizeDownload(int total, IEnumerable<MediaInformation>? failed = null)
    {
        var failedList = failed?.ToList() ?? new List<MediaInformation>();
        int success = total - failedList.Count;
        int successPercentage = total > 0 ? (int)((double)success / total * 100) : 0;
        IOHelper.Print("\nDownload Summary:");
        IOHelper.Print($"Total   = {total}");
        IOHelper.Print($"Success = {success}\t({successPercentage}%)");
        IOHelper.Print($"Fail    = {failedList.Count}\t({100 - successPercentage}%)");
        if (!failedList.Any())
        {
            return;
        }

        IOHelper.Print("Failed media:");
        foreach (var media in failedList)
        {
            IOHelper.Print($"\t{media}");
        }
    }

    private static async Task LoginToProviders(IEnumerable<string> providersNames)
    {
        foreach (string providerName in providersNames)
        {
            if (await HoradotService.IsLoggedIn(providerName))
            {
                continue;
            }

            IOHelper.Print($"\nYou need to log in to {providerName}.");

            var settings = AppSettings.Default;
            var saved = settings.Credentials.GetValueOrDefault(providerName);
            if (!string.IsNullOrWhiteSpace(saved.username) && !string.IsNullOrWhiteSpace(saved.password))
            {
                IOHelper.Print("\nSaved credentials detected. Trying to log in...");
                if (await HoradotService.Login(providerName, saved.username, saved.password))
                {
                    IOHelper.Print("Logged in successfully, proceeding.");
                    return;
                }

                await settings.ResetCredentialsAsync(providerName);
                IOHelper.Print("Bad credentials, proceeding to manual login.");
            }

            while (true)
            {
                string username = IOHelper.Input("\nUsername or email: ");
                string password = IOHelper.Input("Password: ");
                if (await HoradotService.Login(providerName, username, password))
                {
                    IOHelper.Print("Logged in successfully, proceeding.");
                    await settings.SaveCredentialsAsync(providerName, username, password);
                    return;
                }

                IOHelper.Print("Bad credentials, please try again.");
            }
        }
    }

    private static Task<MediaDownloadInformation?> LoadMedia(MediaInformation media, int retries = 2)
    {
        do
        {
            try
            {
                return HoradotService.PrepareDownloadAsync(media);
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

        return Task.FromResult<MediaDownloadInformation?>(null);
    }
}
