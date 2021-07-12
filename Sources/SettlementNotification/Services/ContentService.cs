using SettlementNotification.Models;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace SettlementNotification.Services
{
    public interface ContentService
    {
        string Content(NotificationModel model);
    }

    public class ContentServiceImp : ContentService
    {
        private readonly IHostingEnvironment env;

        public ContentServiceImp(IHostingEnvironment env)
        {
            this.env = env;
        }

        public string Content(NotificationModel model)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}Templates{Path.DirectorySeparatorChar}AlertTemplate.cshtml";
            return Engine.Razor.RunCompile(File.ReadAllText(path), "Alert Template", model: model);
        }
    }
}