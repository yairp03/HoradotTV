# HoradotTV

Downloader for Shows/Movies providers.

Currently supported providers:
- SdarotTV
- SratimTV (Coming soon)

## HoradotAPI

The HoradotAPI is a library allowing you t to access metadata and download shows/movies from content providers.

The library is very extensible and allows you to add support for new providers easily.

The library also has built-in support for the following providers:
- SdarotTV
  - Access SdarotTV's search engine.
  - Access metadata of shows, seasons and episodes.
  - Access SdarotTV's media servers.
  - Login to SdarotTV.
  - Signup to SdarotTV (Coming soon)
- SratimTV (Coming soon)

## Console App

The console app is a command-line application that utilizes the HoradotAPI library, with the power to Download in 4 different download modes:

- Single episode download.
- Multiple episode download.
- Complete season download.
- Complete show download.
- Movie download.

With some more features like retries on server errors, and division to folders.

### Running The Console App

The easiest way to use the console app is through the release on GitHub:

1. Go to the [releases](https://github.com/yairp03/HoradotTV/releases) page.

2. Download the latest `zip` file:
   - If you have `.NET` installed, you can download the `HoradotTV.Console.zip`.
   - If you don't know what `.NET` is, download the `HoradotTV.Console.WithRuntime.zip`.

3. Extract the zip.

4. Run the `HoradotTV.Console.exe` file.

#### Running With Visual Studio

If you want to build the project from source, you can do that by using Visual Studio:

1. Open the `HoradotTV.sln` file with Visual Studio.
2. Select the console app (`HoradotTV.Console`) as a startup project.
3. Press `F5` to run.

## GUI App

The GUI app is currently on hold.  
Contributors who are willing to help utilize the `HoradotAPI` library to build the GUI app are welcome to contact me.

## Future Development

- Support for SratimTV.
- Release updater.
- GUI app.
- HordotAPI in Python. (In the meanwhile, check out [Xiddoc's python API for SdarotTV](https://github.com/Xiddoc/PySdarot))

## Contributing

If the URL for SdarotTV changes, please fork the repository, change the [SdarotTV-domain.txt](/Resources/SdarotTV-domain.txt) file, and open a PR.

If you find any bugs or have any suggestions, please open an issue on [GitHub](https://github.com/yairp03/HoradotTV/issues).

Contributors can check the [issues](https://github.com/yairp03/HoradotTV/issues) page for open issues and help improve this project.
