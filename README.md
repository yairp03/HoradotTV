# HoradotTV
Downloader for SdarotTV

## SdarotAPI
The SdarotAPI is a library allowing:
- Access metadata of shows, seasons and episodes.
- Get access to SdarotTV's media servers.
- Download episodes from SdarotTV.
- Retrieve the most updated SdarotTV domain.

## Console App
The console app is a command line application that utilizes the SdarotAPI library, with the power to Download in 4 different download modes:
- Single episode download.
- Multiple episode download.
- Complete season download.
- Complete show download.

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
2. Select the console app (`HoradotTV.Console) as a startup project.
3. Press `F5` to run.


## GUI App
The GUI app is currently on hold.  
Contributors who are willing to help use the SdarotAPI library to build the GUI app are welcome to contact me.

## Future Development
- Release updater.
- Support for SratimTV.
- GUI app.
- SdarotAPI in python.

## Contributing
If the URL for SdarotTV changes, please fork the repo, change the [sdarot-url.txt](https://github.com/yairp03/HoradotTV/blob/master/Resources/sdarot-url.txt) file, and open a PR.  
If you find any bugs or have any suggestions, please open an issue on [Github](https://github.com/yairp03/HoradotTV/issues).  
Contributors can check the [issues](https://github.com/yairp03/HoradotTV/issues) page for open issues and help make this project the best.
