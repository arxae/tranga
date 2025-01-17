﻿using System.Globalization;
using System.Text.RegularExpressions;
using Logging;
using Newtonsoft.Json;
using Tranga.LibraryConnectors;
using Tranga.NotificationConnectors;

namespace Tranga;

public abstract class GlobalBase
{
    protected Logger? logger { get; init; }
    protected TrangaSettings settings { get; init; }
    protected HashSet<NotificationConnector> notificationConnectors { get; init; }
    protected HashSet<LibraryConnector> libraryConnectors { get; init; }
    protected List<Manga> cachedPublications { get; init; }
    public static readonly NumberFormatInfo numberFormatDecimalPoint = new (){ NumberDecimalSeparator = "." };
    protected static readonly Regex baseUrlRex = new(@"https?:\/\/[0-9A-z\.-]+(:[0-9]+)?");

    protected GlobalBase(GlobalBase clone)
    {
        this.logger = clone.logger;
        this.settings = clone.settings;
        this.notificationConnectors = clone.notificationConnectors;
        this.libraryConnectors = clone.libraryConnectors;
        this.cachedPublications = clone.cachedPublications;
    }

    protected GlobalBase(Logger? logger, TrangaSettings settings)
    {
        this.logger = logger;
        this.settings = settings;
        this.notificationConnectors = settings.LoadNotificationConnectors(this);
        this.libraryConnectors = settings.LoadLibraryConnectors(this);
        this.cachedPublications = new();
    }

    protected void Log(string message)
    {
        logger?.WriteLine(this.GetType().Name, message);
    }

    protected void Log(string fStr, params object?[] replace)
    {
        Log(string.Format(fStr, replace));
    }

    protected void SendNotifications(string title, string text)
    {
        foreach (NotificationConnector nc in notificationConnectors)
            nc.SendNotification(title, text);
    }

    protected void AddNotificationConnector(NotificationConnector notificationConnector)
    {
        Log($"Adding {notificationConnector}");
        notificationConnectors.RemoveWhere(nc => nc.notificationConnectorType == notificationConnector.notificationConnectorType);
        notificationConnectors.Add(notificationConnector);
        
        while(IsFileInUse(settings.notificationConnectorsFilePath))
            Thread.Sleep(100);
        Log("Exporting notificationConnectors");
        File.WriteAllText(settings.notificationConnectorsFilePath, JsonConvert.SerializeObject(notificationConnectors));
    }

    protected void DeleteNotificationConnector(NotificationConnector.NotificationConnectorType notificationConnectorType)
    {
        Log($"Removing {notificationConnectorType}");
        notificationConnectors.RemoveWhere(nc => nc.notificationConnectorType == notificationConnectorType);
        while(IsFileInUse(settings.notificationConnectorsFilePath))
            Thread.Sleep(100);
        Log("Exporting notificationConnectors");
        File.WriteAllText(settings.notificationConnectorsFilePath, JsonConvert.SerializeObject(notificationConnectors));
    }

    protected void UpdateLibraries()
    {
        foreach(LibraryConnector lc in libraryConnectors)
            lc.UpdateLibrary();
    }

    protected void AddLibraryConnector(LibraryConnector libraryConnector)
    {
        Log($"Adding {libraryConnector}");
        libraryConnectors.RemoveWhere(lc => lc.libraryType == libraryConnector.libraryType);
        libraryConnectors.Add(libraryConnector);
        
        while(IsFileInUse(settings.libraryConnectorsFilePath))
            Thread.Sleep(100);
        Log("Exporting libraryConnectors");
        File.WriteAllText(settings.libraryConnectorsFilePath, JsonConvert.SerializeObject(libraryConnectors));
    }

    protected void DeleteLibraryConnector(LibraryConnector.LibraryType libraryType)
    {
        Log($"Removing {libraryType}");
        libraryConnectors.RemoveWhere(lc => lc.libraryType == libraryType);
        while(IsFileInUse(settings.libraryConnectorsFilePath))
            Thread.Sleep(100);
        Log("Exporting libraryConnectors");
        File.WriteAllText(settings.libraryConnectorsFilePath, JsonConvert.SerializeObject(libraryConnectors));
    }

    protected bool IsFileInUse(string filePath)
    {
        if (!File.Exists(filePath))
            return false;
        try
        {
            using FileStream stream = new (filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            stream.Close();
            return false;
        }
        catch (IOException)
        {
            Log($"File is in use {filePath}");
            return true;
        }
    }
}