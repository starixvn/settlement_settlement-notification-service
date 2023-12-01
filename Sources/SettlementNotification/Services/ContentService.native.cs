using RazorEngine;
using RazorEngine.Templating;
using SettlementNotification.Models;
using System.IO;

namespace SettlementNotification.Services;

public interface ContentService
{
    string Content(NotificationModel model);
}

public class ContentServiceImp : ContentService
{
    public string Content(NotificationModel model)
    {
        var path = $"{AppDomain.CurrentDomain.BaseDirectory}Templates{Path.DirectorySeparatorChar}AlertTemplate.cshtml";
        return Engine.Razor.RunCompile(File.ReadAllText(path), "Alert Template", model: model);
    }
}