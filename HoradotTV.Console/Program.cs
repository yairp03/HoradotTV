namespace HoradotTV.Console;

internal static class Program
{
    private static readonly HoradotService HoradotService = new();
    private static readonly HoradotHelper HoradotHelper = new(HoradotService);

    private static async Task Main()
    {
        IOUtils.Print($"Welcome to HoradotTV {Constants.SoftwareVersion}!");
        IOUtils.Print("Initializing...");
        if (!await InitializeAsync())
        {
            return;
        }

        MainMenuOption option;
        do
        {
            option = IOHelper.MainMenuChoose();
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
        if (!result.success)
        {
            System.Console.WriteLine(result.errorMessage);
        }

        return result.success;
    }

    private static async Task DownloadFromSearch()
    {
        var media = await SelectMedia();

        if (media is null)
        {
            return;
        }

        string mediaLibraryLocation = await IOHelper.SelectMediaLibraryLocationAsync();
        IOUtils.Print("Full download path: " + mediaLibraryLocation);

        if (media is MovieInformation movie)
        {
            await DownloadMedia(movie, mediaLibraryLocation);
            return;
        }

        if (media is not ShowInformation show)
        {
            throw new BadMediaTypeException();
        }

        var mode = IOHelper.ChooseMode();
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
            int episodesAmount = IOUtils.InputPositiveInt("\nEnter episodes amount (0 - cancel): ");
            if (episodesAmount == 0)
            {
                return;
            }

            await DownloadEpisodes(episode, episodesAmount, mediaLibraryLocation);
        }
    }

    private static async Task DownloadFromFile()
    {
        string mediaLibraryLocation = await IOHelper.SelectMediaLibraryLocationAsync();

        var mediaFiles = Directory.GetFiles(mediaLibraryLocation, $"*.{Constants.MediaFileExtension}")
            .Select(p => new FileInfo(p)).ToList();

        if (mediaFiles.Count == 0)
        {
            IOUtils.Print($"Didn't found any .{Constants.MediaFileExtension} files.");
            return;
        }

        mediaFiles.Sort((f1, f2) => f2.LastWriteTime.CompareTo(f1.LastWriteTime));

        IOUtils.Print("\nAvailable files (from new to old):");
        int selection = IOUtils.ChooseOptionIndex(mediaFiles.Select(f => $"({f.LastWriteTime:G}) - {f.Name}"),
            "Choose as file");
        if (selection == 0)
        {
            return;
        }

        var mediaList =
            await JsonSerializer.DeserializeAsync<List<MediaInformation>>(mediaFiles[selection - 1].OpenRead());
        if (mediaList is null)
        {
            IOUtils.Print("Bad file format.");
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
                IOUtils.Input($"\nEnter show/movie name or part of it ({Constants.Commands.Cancel} - cancel): ");
            if (query == Constants.Commands.Cancel)
            {
                return null;
            }

            if (query.Length < Constants.QueryMinLength)
            {
                IOUtils.Print($"Please enter at least {Constants.QueryMinLength} characters.");
                continue;
            }

            searchResult = (await HoradotService.SearchAsync(query)).ToList();
            if (searchResult.Count == 0)
            {
                IOUtils.Print("Not found.");
            }
        } while (searchResult.Count == 0);

        IOUtils.Print("\nResults:");
        int selection = IOUtils.ChooseOptionIndex(searchResult.Select(s => $"({s.ProviderName}) {s.Name} - {s.NameHe}"),
            "Choose a show/movie");

        return selection == 0 ? null : searchResult[selection - 1];
    }

    private static async Task<SeasonInformation?> ChooseSeason(ShowInformation show)
    {
        var seasons = (await HoradotService.GetSeasonsAsync(show)).ToList();
        string seasonName = IOUtils.ChooseOption(seasons.Select(s => s.Name), "season", "Choose a season");
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
        string episodeName = IOUtils.ChooseOption(episodes.Select(e => e.Name), "episode", "Choose an episode");
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
                    .Where(episode => !File.Exists(Utils.GetFullDownloadLocation(downloadLocation, episode))).ToList();

                int exists = mediaList.Count - filteredList.Count;
                if (exists > 0)
                {
                    IOUtils.Print($"\nFound {exists} existing media files. Ignoring them.");
                    mediaList = filteredList;
                }
            }

            foreach (var (media, i) in mediaList.Select((value, i) => (value, i)))
            {
                IOUtils.Print($"\n({i + 1}/{mediaList.Count})");
                string finalLocation = Utils.GetFullDownloadLocation(downloadLocation, media);

                IOUtils.Log($"Loading {media}...");
                var mediaDownloadInformation = await HoradotHelper.LoadMedia(media);
                if (mediaDownloadInformation is null)
                {
                    failedMedia.Add(media);
                    IOUtils.Log("Failed. Proceeding to next media.");
                    continue;
                }

                IOUtils.Log($"Downloading {media}...");
                _ = Directory.CreateDirectory(Path.GetDirectoryName(finalLocation)!);
                await using var file = new FileStream(finalLocation, FileMode.Create, FileAccess.Write, FileShare.None);
                await HoradotService.DownloadAsync(mediaDownloadInformation,
                    mediaDownloadInformation.Resolutions.Max(r => r.Key), file);
                IOUtils.Log("Download completed.");
            }

            IOUtils.Log("Finished.");
            IOHelper.SummarizeDownload(mediaList.Count, failedMedia);

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
                IOUtils.Print($"\nTrying again {failedMedia.Count} failed media");
                failedMedia = (await DownloadMedia(failedMedia, downloadLocation, false) ??
                               Enumerable.Empty<MediaInformation>()).ToList();
            }

            IOUtils.Print("\nDone. Returning to start.");

            return failedMedia;
        }
        catch (Exception e)
        {
            IOUtils.Print($"Caught exception: {e}");
            /* Every exception thrown */
        }

        return null;
    }

    private static async Task<int> HandleFailedMedia(List<MediaInformation> media, string downloadLocation)
    {
        IOUtils.Print("\nThere are still some failed media.\nWhat do you want to do with them?");
        IOUtils.Print(Menus.FailedMenu);
        var option = (FailedOption)IOUtils.ChooseOptionRange(Enum.GetNames(typeof(FailedOption)).Length - 1);

        switch (option)
        {
            case FailedOption.None:
                return 0;
            case FailedOption.TryAgain:
                return IOUtils.InputPositiveInt("\nEnter retries amount (0 - cancel): ");
            case FailedOption.Export:
            {
                string finalLocation = await HoradotHelper.ExportToFile(media, downloadLocation);
                IOUtils.Print($"Exported to {finalLocation}");
                break;
            }
            default:
                throw new InvalidOperationException("Bad option.");
        }

        return 0;
    }

    private static async Task LoginToProviders(IEnumerable<string> providersNames)
    {
        foreach (string providerName in providersNames)
        {
            if (await HoradotService.IsLoggedIn(providerName))
            {
                continue;
            }

            IOUtils.Print($"\nYou need to log in to {providerName}.");

            var settings = AppSettings.Default;
            var saved = settings.Credentials.GetValueOrDefault(providerName);
            if (!string.IsNullOrWhiteSpace(saved.username) && !string.IsNullOrWhiteSpace(saved.password))
            {
                IOUtils.Print("\nSaved credentials detected. Trying to log in...");
                if (await HoradotService.Login(providerName, saved.username, saved.password))
                {
                    IOUtils.Print("Logged in successfully, proceeding.");
                    continue;
                }

                await settings.ResetCredentialsAsync(providerName);
                IOUtils.Print("Bad credentials, proceeding to manual login.");
            }

            while (true)
            {
                string username = IOUtils.Input("\nUsername or email: ");
                string password = IOUtils.Input("Password: ");
                if (await HoradotService.Login(providerName, username, password))
                {
                    IOUtils.Print("Logged in successfully, proceeding.");
                    await settings.SaveCredentialsAsync(providerName, username, password);
                    break;
                }

                IOUtils.Print("Bad credentials, please try again.");
            }
        }
    }
}
