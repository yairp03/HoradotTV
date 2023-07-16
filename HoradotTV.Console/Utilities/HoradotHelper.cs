namespace HoradotTV.Console.Utilities;

internal class HoradotHelper
{
    private readonly HoradotService horadotService;

    public HoradotHelper(HoradotService horadotService)
    {
        this.horadotService = horadotService;
    }

    public Task<MediaDownloadInformation?> LoadMedia(MediaInformation media, int retries = 2)
    {
        do
        {
            try
            {
                return horadotService.PrepareDownloadAsync(media);
            }
            catch (WebsiteErrorException)
            {
                if (retries > 0)
                {
                    IOUtils.Log($"Failed. Trying again... ({retries} tries left)");
                }

                retries--;
            }
        } while (retries > -1);

        return Task.FromResult<MediaDownloadInformation?>(null);
    }

    public static async Task<string> ExportToFile(List<MediaInformation> media, string downloadLocation)
    {
        string finalLocation = Path.Combine(downloadLocation,
            $"FailedEpisodes_{DateTime.Now:HH_mm_ss_dd_MM_yyyy}.{Constants.MediaFileExtension}");
        string mediaDetails = JsonSerializer.Serialize(media, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(finalLocation, mediaDetails);
        return finalLocation;
    }
}
