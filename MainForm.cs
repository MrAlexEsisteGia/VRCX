﻿// Copyright(c) 2019 pypy. All rights reserved.
//
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT>.

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace VRCX
{
    public partial class MainForm : Form
    {
        public static MainForm Instance { get; private set; }
        public static ChromiumWebBrowser Browser { get; private set; }
        private int LastLocationX;
        private int LastLocationY;
        private int LastSizeWidth;
        private int LastSizeHeight;

        public MainForm()
        {
            Instance = this;
            InitializeComponent();
            try
            {
                var location = Assembly.GetExecutingAssembly().Location;
                var icon = Icon.ExtractAssociatedIcon(location);
                Icon = icon;
                TrayIcon.Icon = icon;
            }
            catch
            {
            }
            // Application.StartupPath + "/html/index.html"
            Browser = new ChromiumWebBrowser(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "html/index.html"))
            {
                BrowserSettings =
                {
                    // UniversalAccessFromFileUrls = CefState.Enabled,
                    DefaultEncoding = "UTF-8",
                },
                Dock = DockStyle.Fill,
            };
            var options = new BindingOptions()
            {
                CamelCaseJavascriptNames = false
            };
            Browser.JavascriptObjectRepository.Register("VRCX", VRCX.Instance, true, options);
            Browser.JavascriptObjectRepository.Register("VRCXStorage", VRCXStorage.Instance, false, options);
            Browser.JavascriptObjectRepository.Register("SQLite", SQLite.Instance, true, options);
            Browser.JavascriptObjectRepository.Register("LogWatcher", LogWatcher.Instance, true, options);
            Browser.JavascriptObjectRepository.Register("Discord", Discord.Instance, true, options);
            Browser.IsBrowserInitializedChanged += (A, B) =>
            {
                // Browser.ShowDevTools();
            };
            Controls.Add(Browser);
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            try
            {
                int.TryParse(VRCXStorage.Instance.Get("VRCX_LocationX"), out LastLocationX);
                int.TryParse(VRCXStorage.Instance.Get("VRCX_LocationY"), out LastLocationY);
                int.TryParse(VRCXStorage.Instance.Get("VRCX_SizeWidth"), out LastSizeWidth);
                int.TryParse(VRCXStorage.Instance.Get("VRCX_SizeHeight"), out LastSizeHeight);
                var location = new Point(LastLocationX, LastLocationY);
                var size = new Size(LastSizeWidth, LastSizeHeight);
                var screen = Screen.FromPoint(location);
                if (screen.Bounds.Contains(location.X, location.Y))
                {
                    Location = location;
                }
                if (size.Width > 0 && size.Height > 0)
                {
                    Size = size;
                }
            }
            catch
            {
            }
            try
            {
                var state = WindowState;
                if (int.TryParse(VRCXStorage.Instance.Get("VRCX_WindowState"), out int v))
                {
                    state = (FormWindowState)v;
                }
                if (state == FormWindowState.Minimized)
                {
                    state = FormWindowState.Normal;
                }
                if ("true".Equals(VRCXStorage.Instance.Get("VRCX_StartAsMinimizedState")))
                {
                    state = FormWindowState.Minimized;
                }
                WindowState = state;
            }
            catch
            {
            }
        }

        private void MainForm_Resize(object sender, System.EventArgs e)
        {
            if (WindowState != FormWindowState.Normal)
            {
                return;
            }
            LastSizeWidth = Size.Width;
            LastSizeHeight = Size.Height;
        }

        private void MainForm_Move(object sender, System.EventArgs e)
        {
            if (WindowState != FormWindowState.Normal)
            {
                return;
            }
            LastLocationX = Location.X;
            LastLocationY = Location.Y;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing &&
                "true".Equals(VRCXStorage.Instance.Get("VRCX_CloseToTray")))
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            VRCXStorage.Instance.Set("VRCX_LocationX", LastLocationX.ToString());
            VRCXStorage.Instance.Set("VRCX_LocationY", LastLocationY.ToString());
            VRCXStorage.Instance.Set("VRCX_SizeWidth", LastSizeWidth.ToString());
            VRCXStorage.Instance.Set("VRCX_SizeHeight", LastSizeHeight.ToString());
            VRCXStorage.Instance.Set("VRCX_WindowState", ((int)WindowState).ToString());
        }

        private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TrayMenu_Open_Click(sender, e);
        }

        private void TrayMenu_Open_Click(object sender, System.EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
            Show();
            Focus();
        }

        private void TrayMenu_Quit_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

    }
}