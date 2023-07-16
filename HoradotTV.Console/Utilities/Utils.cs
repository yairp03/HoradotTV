namespace HoradotTV.Console.Utilities;

internal static class Utils
{
    public static string GetFullDownloadLocation(string downloadLocation, MediaInformation media)
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
}
