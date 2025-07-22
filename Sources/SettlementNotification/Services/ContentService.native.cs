using RazorEngine;
using RazorEngine.Templating;
using SettlementNotification.Constants;
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
        var temp = model.MessageGroupId == MessageGroup.MatchReadyToMove
            ? "MatchReadyToMoveTemplate.cshtml"
            : "AlertTemplate.cshtml";
        var path = $"{AppDomain.CurrentDomain.BaseDirectory}Templates{Path.DirectorySeparatorChar}{temp}";
        return Engine.Razor.RunCompile(File.ReadAllText(path), "Alert Template", model: model);
    }
}