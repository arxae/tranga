﻿using Logging;
using Newtonsoft.Json;
using Tranga.LibraryManagers;

namespace Tranga;

public class TrangaSettings
{
    public string downloadLocation { get; set; }
    public string workingDirectory { get; set; }
    [JsonIgnore]public string settingsFilePath => Path.Join(workingDirectory, "settings.json");
    [JsonIgnore]public string tasksFilePath => Path.Join(workingDirectory, "tasks.json");
    [JsonIgnore]public string knownPublicationsPath => Path.Join(workingDirectory, "knownPublications.json");
    [JsonIgnore] public string coverImageCache => Path.Join(workingDirectory, "imageCache");
    public Komga? komga { get; set; }

    public TrangaSettings(string downloadLocation, string workingDirectory, Komga? komga)
    {
        if (downloadLocation.Length < 1 || workingDirectory.Length < 1)
            throw new ArgumentException("Download-location and working-directory paths can not be empty!");
        this.workingDirectory = workingDirectory;
        this.downloadLocation = downloadLocation;
        this.komga = komga;
    }

    public static TrangaSettings LoadSettings(string importFilePath, Logger? logger)
    {
        if (!File.Exists(importFilePath))
            return new TrangaSettings(Path.Join(Directory.GetCurrentDirectory(), "Downloads"), Directory.GetCurrentDirectory(), null);

        string toRead = File.ReadAllText(importFilePath);
        TrangaSettings settings = JsonConvert.DeserializeObject<TrangaSettings>(toRead)!;
        if(settings.komga is not null && logger is not null)
            settings.komga.AddLogger(logger);

        return settings;
    }
}