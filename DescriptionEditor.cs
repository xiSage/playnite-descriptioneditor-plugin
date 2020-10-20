﻿using DescriptionEditor.Views;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

// TODO Integrate control HtmlTextView
namespace DescriptionEditor
{
    public class DescriptionEditor : Plugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private DescriptionEditorSettings settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("7600a469-4616-4547-94b8-0c330db02b8f");

        public DescriptionEditor(IPlayniteAPI api) : base(api)
        {
            settings = new DescriptionEditorSettings(this);

            // Get plugin's location 
            string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Add plugin localization in application ressource.
            PluginCommon.Localization.SetPluginLanguage(pluginFolder, api.ApplicationSettings.Language);
            // Add common in application ressource.
            PluginCommon.Common.Load(pluginFolder);

            // Check version
            if (settings.EnableCheckVersion)
            {
                CheckVersion cv = new CheckVersion();

                if (cv.Check("DescriptionEditor", pluginFolder))
                {
                    cv.ShowNotification(api, "DescriptionEditor - " + resources.GetString("LOCUpdaterWindowTitle"));
                }
            }

            // Add Event for WindowBase for get the "WindowGameEdit".
            EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler(WindowBase_LoadedEvent));
        }

        public override IEnumerable<ExtensionFunction> GetFunctions()
        {
            List<ExtensionFunction> listFunctions = new List<ExtensionFunction>();

#if DEBUG
            listFunctions.Add(
                new ExtensionFunction(
                    "DescriptionEditor Test",
                    () =>
                    {

                    })
                );
#endif

            return listFunctions;
        }

        private TextBox TextDescription;

        private void WindowBase_LoadedEvent(object sender, System.EventArgs e)
        {
            string WinName = String.Empty;
            try
            {
                WinName = ((Window)sender).Name;

                if (WinName == "mainWindow")
                {
                    Window mainWindow = (Window)sender;
                    DockPanel ElementParent = (DockPanel)((Button)mainWindow.FindName("ButtonDownload")).Parent;

                    if (ElementParent != null)
                    {
                        // Game Description
                        TextDescription = (TextBox)mainWindow.FindName("TextDescription");

                        // Add new button
                        Button bt = new Button();
                        bt.Content = resources.GetString("LOCDescriptionEditorButton");
                        Style style = Application.Current.FindResource("BottomButton") as Style;
                        bt.Style = style;
                        bt.Click += OnButtonClick;

                        ElementParent.Children.Add(bt);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "DescriptionEditor", $"Error on WindowBase_LoadedEvent for {WinName}");
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var ViewExtension = new DescriptionEditorView(PlayniteApi ,TextDescription);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi,resources.GetString("LOCDescriptionEditor"), ViewExtension);
            windowExtension.ShowDialog();
        }


        public override void OnGameInstalled(Game game)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(Game game)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(Game game)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(Game game, long elapsedSeconds)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(Game game)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted()
        {
            // Add code to be executed when Playnite is initialized.
        }

        public override void OnApplicationStopped()
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated()
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        //public override UserControl GetSettingsView(bool firstRunSettings)
        //{
        //    return new DescriptionEditorSettingsView();
        //}
    }
}