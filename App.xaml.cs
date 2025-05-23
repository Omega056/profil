// App.xaml.cs
using System;
using System.IO;
using System.Windows;
using WpfApp14.Services;

namespace WpfApp14
{
    public partial class App : Application
    {
        public object CurrentUser { get; internal set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Инициализация БД один раз при старте
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "quiz.db");
            DatabaseService.Initialize(dbPath);
        }
    }
}
