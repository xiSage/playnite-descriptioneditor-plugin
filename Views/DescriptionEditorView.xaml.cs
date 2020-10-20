﻿using DescriptionEditor.PlayniteResources.Controls;
using Playnite.SDK;
using PluginCommon;
using PluginCommon.PlayniteResources;
using PluginCommon.PlayniteResources.Extensions.Markup;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DescriptionEditor.Views
{
    /// <summary>
    /// Logique d'interaction pour DescriptionEditorView.xaml
    /// </summary>
    public partial class DescriptionEditorView : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();
        private IPlayniteAPI _PlayniteApi;

        public string Description { get; set; }
        private TextBox _TextDescription;

        private HtmlTextView htmlTextView = new HtmlTextView();


        public string imgUrl { get; set; } = string.Empty;
        public bool imgCent { get; set; } = true;
        public string imgSize { get; set; } = string.Empty;
        public bool imgLeft { get; set; } = false;
        public bool imgCenter { get; set; } = true;
        public bool imgRight { get; set; } = false;


        public DescriptionEditorView(IPlayniteAPI PlayniteApi, TextBox TextDescription)
        {
            _PlayniteApi = PlayniteApi;
            _TextDescription = TextDescription;
            InitializeComponent();

            Description = TextDescription.Text;

            PlayniteTools.SetThemeInformation(_PlayniteApi);
            string DescriptionViewFile = ThemeFile.GetFilePath("DescriptionView.html"); ;
#if DEBUG
            logger.Debug($"DescriptionEditor - {DescriptionViewFile}");
#endif
            try
            {
                htmlTextView.Visibility = Visibility.Visible;
                htmlTextView.TemplatePath = DescriptionViewFile;
                htmlTextView.HtmlText = Description;

                htmlTextView.HtmlFontSize = (double)resources.GetResource("FontSize");
                htmlTextView.HtmlFontFamily = (FontFamily)resources.GetResource("FontFamily");
                htmlTextView.HtmlForeground = (Color)resources.GetResource("TextColor");
                htmlTextView.LinkForeground = (Color)resources.GetResource("GlyphColor");

                PART_HtmlDescription.Children.Add(htmlTextView);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "DescriptionEditor", "Error on creation HtmlTextView");
            }

            DataContext = this;
        }

        private void BtEditorCancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }

        private void BtEditorOK_Click(object sender, RoutedEventArgs e)
        {
            _TextDescription.Text = DescriptionActual.Text;
            ((Window)this.Parent).Close();
        }


        private void ImgSize_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ((TextBox)sender).Text = Regex.Replace(((TextBox)sender).Text, "[^0-9]+", string.Empty);
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            imgUrl = string.Empty;
            imgCent = true;
            imgSize = string.Empty;
            imgLeft = false;
            imgCenter = true;
            imgRight = false;

            tbImgUrl.Text = imgUrl;
            ckImgCent.IsChecked = imgCent;
            tbImgSize.Text = imgSize;
            rbImgLeft.IsChecked = imgLeft;
            rbImgCenter.IsChecked = imgCenter;
            rbImgRight.IsChecked = imgRight;

            foreach (var ui in Tools.FindVisualChildren<Border>((ContextMenu)((Grid)sender).Parent))
            {
                if (((FrameworkElement)ui).Name == "HoverBorder")
                {
                    ((Border)ui).Background = (System.Windows.Media.Brush)resources.GetResource("NormalBrush");
                    break;
                }
            }
        }


        #region Common formatter 
        private void BtHtmlFormat_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.HtmlFormat(DescriptionActual.Text);
        }

        private void BtInsertImg_Click(object sender, RoutedEventArgs e)
        {
            string imgAdded = "<img src=\"{0}\" style=\"{1}\">";
            string style = string.Empty;
            if (!string.IsNullOrEmpty(imgUrl))
            {
                if (imgCent)
                {
                    style += $"width: 100%;";
                }
                else
                {
                    if (!string.IsNullOrEmpty(imgSize))
                    {
                        style += $"width: {imgSize }px;";

                    }
                    if (imgLeft)
                    {
                        style += $"width: 100%;";
                        //style += $"float: left;";
                        imgAdded = "<table style=\"border: 0; width: 100 %;border-spacing: 10px;\"><tr><td>Your text here!</td>"
                             + $"<td style=\"width: {imgSize }px;vertical-align: top;\">"
                             + imgAdded
                             + "</td>"
                             + "</tr></table>";
                    }
                    if (imgCenter)
                    {
                        //style += $"margin-left: auto;margin-right: auto;";
                        imgAdded = "<div style=\"text-align: center;\">"
                            + imgAdded
                            + "</div>";
                    }
                    if (imgRight)
                    {
                        style += $"width: 100%;";
                        //style += $"float: right;";
                        imgAdded = "<table style=\"border: 0; width: 100 %;border-spacing: 10px;\"><tr>"
                             + $"<td style=\"width: {imgSize }px;vertical-align: top;\">"
                             + imgAdded
                             + "</td>"
                             + "<td>Your text here!</td>"
                             + "</tr></table>";
                    }
                }

                imgAdded = "\r\n" + HtmlHelper.HtmlFormat(string.Format(imgAdded, imgUrl, style)) + "\r\n";

                if (DescriptionActual.CaretIndex <= 0)
                {
                    DescriptionActual.Focus();
                    DescriptionActual.Text = DescriptionActual.Text.Insert(DescriptionActual.CaretIndex, imgAdded);
                }
                else
                {
                    DescriptionActual.Text = DescriptionActual.Text.Insert(DescriptionActual.CaretIndex, imgAdded);
                }
            }

            btAddImgContextMenu.Visibility = Visibility.Collapsed;
        }

        private void BtRemoveImg_Click(object sender, RoutedEventArgs e)
        {
            DescriptionActual.Text = HtmlHelper.RemoveTag(DescriptionActual.Text, "img");
        }
        #endregion

        #region Steam formatter 
        private void SteamRemoveAbout_click(object sender, RoutedEventArgs e)
        {
            List<string> AboutGame = new List<string>();
            AboutGame.Add("<h1>about the game</h1>".ToLower());
            AboutGame.Add("<h1>à propos du jeu</h1>".ToLower());
            AboutGame.Add("<h1>Относно играта</h1>".ToLower());
            AboutGame.Add("<h1>关于游戏</h1>".ToLower());
            AboutGame.Add("<h1>關於此遊戲</h1>".ToLower());
            AboutGame.Add("<h1>O hře</h1>".ToLower());
            AboutGame.Add("<h1>Om spillet</h1>".ToLower());
            AboutGame.Add("<h1>Info over het spel</h1>".ToLower());
            AboutGame.Add("<h1>Tietoja pelistä</h1>".ToLower());
            AboutGame.Add("<h1>Über das Spiel</h1>".ToLower());
            AboutGame.Add("<h1>Σχετικά με το παιχνίδι</h1>".ToLower());
            AboutGame.Add("<h1>A játékról:&nbsp;</h1>".ToLower());
            AboutGame.Add("<h1>Informazioni sul gioco</h1>".ToLower());
            AboutGame.Add("<h1>ゲームについて</h1>".ToLower());
            AboutGame.Add("<h1>게임 정보</h1>".ToLower());
            AboutGame.Add("<h1>Om spillet</h1>".ToLower());
            AboutGame.Add("<h1>Informacje o&nbsp;grze</h1>".ToLower());
            AboutGame.Add("<h1>Acerca do Jogo</h1>".ToLower());
            AboutGame.Add("<h1>Sobre o jogo</h1>".ToLower());
            AboutGame.Add("<h1>Despre joc</h1>".ToLower());
            AboutGame.Add("<h1>Об игре</h1>".ToLower());
            AboutGame.Add("<h1>Acerca del juego</h1>".ToLower());
            AboutGame.Add("<h1>Acerca del juego</h1>".ToLower());
            AboutGame.Add("<h1>Om spelet</h1>".ToLower());
            AboutGame.Add("<h1>ข้อมูลเกม</h1>".ToLower());
            AboutGame.Add("<h1>Oyun Açıklaması</h1>".ToLower());
            AboutGame.Add("<h1>Про гру</h1>".ToLower());
            AboutGame.Add("<h1>Về trò chơi này</h1>".ToLower());

            bool isFind = false;
            int index = -1;
            string aboutFind = string.Empty;

            DescriptionActual.Text = Regex.Replace(DescriptionActual.Text, @"\r\n", string.Empty);
            DescriptionActual.Text = HtmlHelper.HtmlFormatRemove(DescriptionActual.Text);

            foreach (string about in AboutGame)
            {
                index = DescriptionActual.Text.ToLower().IndexOf(about);
                if (index > -1)
                {
                    aboutFind = about;
                    isFind = true;
                    break;
                }
            }
            
            if (isFind)
            {
                DescriptionActual.Text = DescriptionActual.Text.Substring(index + aboutFind.Length);
            }
        }
        #endregion
        

        private void DescriptionActual_TextChanged(object sender, TextChangedEventArgs e)
        {
            htmlTextView.HtmlText = ((TextBox)sender).Text;
        }
    }
}
