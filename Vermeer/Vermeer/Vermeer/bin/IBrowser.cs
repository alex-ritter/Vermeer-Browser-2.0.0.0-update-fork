﻿using IndieGoat.MaterialFramework.Controls;
using Moonbyte.Vermeer.bin;
using Moonbyte.Vermeer.browser;
using System;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Vermeer.Vermeer.bin.Cefsharp;
using Vermeer.Vermeer.Controls;

namespace Vermeer.Vermeer.bin
{ 
    public static class IBrowser
    {

        #region MaterialTabPage

        public static void GenerateNewBrowserTab(MaterialTabPage browserPage)
        { generateCommon(browserPage, false); }
        public static void GenerateNewBrowserTab()
        { MaterialTabPage browserPage = new MaterialTabPage(); generateCommon(browserPage); }

        private static void generateCommon(MaterialTabPage browserPage, bool showTab = true)
        {
            browserPage.BackColor = Color.White;
            browserPage.Text = "New Tab";

            if (showTab) { vermeer.baseTabControl.TabPages.Add(browserPage); }

            RenderBrowserUI(browserPage);
            RenderHeaderUI(browserPage);
        }

        #endregion

        #region Header

        private static void RenderHeaderUI(MaterialTabPage mainPage)
        {
            VermeerBrowserInstance instance = BrowserInstance(mainPage);

            GC.WaitForPendingFinalizers();
            GC.Collect();

            // ** Back Button ** //
            BackButton backButton = new BackButton();
            backButton.Location = new Point(0, 0);
            backButton.Click += (obj, args) =>
            {
                instance.BrowserInterface.GoBack();
            };
            mainPage.Controls.Add(backButton);

            // ** Forward Button ** //
            ForwardButton forwardButton = new ForwardButton();
            forwardButton.Location = new Point(32, 0);
            forwardButton.Click += (obj, args) =>
            {
                instance.BrowserInterface.GoForward();
            };
            mainPage.Controls.Add(forwardButton);

            // ** Reload Button ** //
            ReloadButton reloadButton = new ReloadButton();
            reloadButton.Location = new Point(64, 0);
            reloadButton.Click += (obj, args) =>
            {
                instance.BrowserInterface.Reload();
            };
            mainPage.Controls.Add(reloadButton);

            // ** SearchBar ** //

            DefaultSearchBar searchBar = new DefaultSearchBar();
            searchBar.Anchor = (AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left);
            searchBar.Location = new Point(86 + 12, 4);
            searchBar.Font = new Font("Segoe UI", 12f);
            searchBar.GetURL = "";
            searchBar.Width = mainPage.Width - 86 - 64;

            searchBar.baseTextBox.KeyDown += (obj, Args) =>
            {
                KeyEventArgs args = (KeyEventArgs)Args;

                if (args.KeyCode == Keys.Enter)
                { instance.BrowserInterface.Navigate(searchBar.baseTextBox.Text); }
            };
            mainPage.Controls.Add(searchBar);

            // ** Design Timer ** //
            DesignTimer designTimer = new DesignTimer(mainPage, BrowserInstance(mainPage));
            instance.BrowserInterface.OnDocumentIconChange += (obj, args) =>
            {
                DocumentIconChange e = args;
                instance.BrowserInterface.getTabPage().ChangeTabIcon(e.icon); vermeer.ApplicationLogger.AddToLog("INFO", "Changed Tab Icon");
            };

            //Setting browser instance events
            instance.BrowserInterface.OnTitleChange += (obj, Args) =>
            {
                if (mainPage.InvokeRequired) { mainPage.Invoke((MethodInvoker)delegate 
                    {
                        try
                        {
                            DocumentTitleChange args = (DocumentTitleChange)Args;

                            if (args.DocumentTitle != "")
                            {
                                GetTabPage(args.VermeerVars.Instance).Text = args.DocumentTitle;
                                vermeer.ApplicationLogger.AddToLog("INFO", "Changed mainPage text from args.DocumentTitle, DocumentTitle : " + args.DocumentTitle);
                            }
                        } catch (Exception e){ vermeer.ApplicationLogger.AddToLog("ERROR", "Exception : " + e.Message); vermeer.ApplicationLogger.AddToLog("EROR", e.StackTrace); vermeer.ApplicationLogger.AddToLog("WARN", "Last two error's can be natural if the browser was closed by the header."); }
                    });
                }
            };

            instance.BrowserInterface.OnDocumentURLChange += (obj, Args) =>
            {
                if (mainPage.InvokeRequired)
                {
                    mainPage.Invoke((MethodInvoker)delegate
                    {
                        DocumentURLChange args = (DocumentURLChange)Args;
                        X509Certificate2 cert2 = Ssl.GetSSLCertificate(args.DocumentURL);

                        searchBar.secureButton.SecureLogo = Ssl.VerifySSLCertificate(cert2);
                        searchBar.baseTextBox.Text = args.DocumentURL;
                    });
                }
            };

            instance.BrowserInterface.OnDocumentIconChange += (obj, Args) =>
            {

            };

            instance.BrowserInterface.OnDocumentLoadChange += (obj, Args) =>
            {
                DocumentLoadingChange args = (DocumentLoadingChange)Args;


            };
        }

        #endregion

        #region Browser Comp

        private static void RenderBrowserUI(MaterialTabPage mainPage)
        {
            CefBrowserInterface browserEngine = new CefBrowserInterface();
            browserEngine.OnInit(mainPage, "https://google.com");

            VermeerBrowserInstance browserInstance = new VermeerBrowserInstance(browserEngine);

            browserInstance.Location = new Point(0, 32);
            browserInstance.Size = new Size(mainPage.Width, mainPage.Height - 32);

            mainPage.Controls.Add(browserInstance);
        }

        #endregion

        #region Getting Usercontrols

        
        /// <summary>
        /// Gets the Panel of a MaterialTabPage
        /// </summary>
        public static VermeerBrowserInstance BrowserInstance(MaterialTabPage mainPage) { return GetBrowserInstance(mainPage); }
        public static VermeerBrowserInstance GetBrowserInstance(MaterialTabPage mainPage)
        {
            //Initialize the return panel
            VermeerBrowserInstance returnPanel = null;

            //Gets Panel control on the TabPage
            foreach(Control userControl in mainPage.Controls)
            { if (userControl is VermeerBrowserInstance) { returnPanel = (VermeerBrowserInstance)userControl; } }

            //Returns the panel
            if (returnPanel == null) { vermeer.ApplicationLogger.AddToLog("WARN", "TabPanel was not found!"); return null; } else { return returnPanel; }
        }
        public static MaterialTabPage GetTabPage(VermeerBrowserInstance instance) { return (MaterialTabPage)instance.Parent; }


        #endregion

    }
}