using SettlementNotification.Models;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.IO;

namespace SettlementNotification.Services
{
    public interface ContentService
    {
        string Content(NotificationModel model);
    }

    public class ContentServiceImp : ContentService
    {
        public string Content(NotificationModel model)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates\\AlertTemplate.cshtml");
            return Engine.Razor.RunCompile(File.ReadAllText(path), "Alert Template", model: model);
        }
    }
}