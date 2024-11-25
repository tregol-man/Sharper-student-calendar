using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Calendar;
using Microsoft.Maui.Controls;
using System.Reflection;
using Microsoft.Extensions.Logging;
public static class FunctionsLib

{
    public static (List<Calendar.EventInfo> Events, List<SubjectData> Subjects, List<GroupData> Groups) LoadEvents()
    {
        string json = string.Empty;

    #if ANDROID
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("dummyData.json").Result;
            using var reader = new StreamReader(stream);
            json = reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading file from MauiAsset: {ex.Message}");
            return (new List<Calendar.EventInfo>(), new List<SubjectData>(), new List<GroupData>());
        }
    #else
        // Fallback for other platforms (if necessary)
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "dummyData.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: The file {filePath} was not found.");
            return (new List<Calendar.EventInfo>(), new List<SubjectData>(), new List<GroupData>());
        }
        json = File.ReadAllText(filePath);
    #endif

        if (json != null)
        {
            var calendarData = JsonConvert.DeserializeObject<CalendarData>(json);
            return (
                calendarData?.Events ?? new List<Calendar.EventInfo>(),
                calendarData?.Subjects ?? new List<SubjectData>(),
                calendarData?.Groups ?? new List<GroupData>()
            );
        }

        return (new List<Calendar.EventInfo>(), new List<SubjectData>(), new List<GroupData>());
    }
    public static void OnDayTapped(DateTime date)
    {
        var formattedDate = date.ToString("yyyy-MM-dd");
        Shell.Current.GoToAsync($"datepage?date={formattedDate}");
    }
    public static void OnEventTapped(int eventId)
    {
        Shell.Current.GoToAsync($"eventpage?eventId={eventId}");
    }
}
