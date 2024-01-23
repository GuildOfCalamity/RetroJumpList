using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using wyDay.Controls;

namespace RetroJumpList
{
    /// <summary>
    /// Notify Icon example using the VistaMenu control.
    /// </summary>
    /// <remarks>
    /// This class will not make a main window form, but will only create a notify icon in the task tray.
    /// </remarks>
    public class AppContext : ApplicationContext
    {
        bool _newWay = true;
        bool _randomIcons = false;
        NotifyIcon _notifyIcon;
        List<string> _paths = new List<string>();
        List<MenuItem> _items = new List<MenuItem>();
        MenuItem[] _menu;
        Random _rnd = new Random();

        public AppContext(string[] options) : this()
        {
            foreach (var arg in options)
            { 
                Debug.WriteLine($"Received argument: '{arg}'");
            }
        }


        public AppContext() 
        {
            try
            {
                this.ThreadExit += AppContext_ThreadExit;

                VistaMenu vistaMenu = new VistaMenu();

                if (_newWay)
                {
                    Bitmap[,] _bitmaps = GenerateBitmapTiles();
                    var x_max = _bitmaps.GetLength(0);
                    var y_max = _bitmaps.GetLength(1);

                    var lines = ReadConfig(Path.Combine($"{Environment.CurrentDirectory}", "Config.txt"));
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrEmpty(line.Trim()))
                            _paths.Add($"{line.Trim()}");
                    }
                    // We could merge these two, but the first for loop is for pre-processing/filtering.
                    foreach (var path in _paths)
                    {
                        if (path.StartsWith("-"))
                        {
                            Debug.WriteLine($"[INFO] Separator");
                            _items.Add(new MenuItem($"-"));
                        }
                        else
                        {
                            Debug.WriteLine($"[INFO] {path.Split(',')[0]},{path.Split(',')[1]}");
                            _items.Add(new MenuItem($"{path.Split(',')[0]}", new EventHandler(mnuItem_Click)));
                        }
                    }

                    // Add +2 for last separator and exit option.
                    _menu = new MenuItem[_items.Count+2];
                    
                    // Add regular items to the MenuItem array.
                    for (int i = 0; i < _items.Count; i++)
                    {
                        try
                        {
                            _menu[i] = _items[i];
                            var isSep = _paths[i];
                            if (!isSep.StartsWith("-"))
                            {
                                if (_randomIcons)
                                {
                                    vistaMenu.SetImage(_menu[i], _bitmaps[_rnd.Next(0, x_max), _rnd.Next(0, y_max)]);
                                }
                                else
                                {
                                    if (File.Exists(isSep.Split(',')[1]))
                                        vistaMenu.SetImage(_menu[i], Properties.Resources.FileRun24x24b);
                                    else
                                        vistaMenu.SetImage(_menu[i], Properties.Resources.FolderOpen24x24);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            frmMessage.Show($"{ex.Message}", $"Exception", MessageLevel.Error, true, TimeSpan.Zero);
                        }
                    }
                    MenuItem mnuSeparator = new MenuItem("-");
                    MenuItem mnuExit = new MenuItem("Exit", new EventHandler(mnuExit_Click));

                    _menu[_items.Count] = mnuSeparator;
                    _menu[_items.Count + 1] = mnuExit;
                    vistaMenu.SetImage(_menu[_items.Count + 1], Properties.Resources.Exit24x24XP);
                    ContextMenu ctmNotifyIcon = new ContextMenu(_menu);
                    ctmNotifyIcon.Popup += (s, e) => { Debug.WriteLine($"[INFO] Popup Event"); };

                    // EndInit() is called by the designer on forms, but since
                    // this is an ApplicationContext we need to call it manually.
                    ((System.ComponentModel.ISupportInitialize)(vistaMenu)).EndInit();

                    // Creates a new instance for the notify icon
                    _notifyIcon = new NotifyIcon()
                    {
                        Icon = Properties.Resources.Notes,
                        ContextMenu = ctmNotifyIcon,
                        Text = "Right-click to show",
                        Visible = true,
                    };
                    
                    // We have no parent, so we cannot use the click event directly.
                    //_notifyIcon.Click += (s, e) => { ctmNotifyIcon.Show(new Control(), new Point(100,100)); };
                }
                else
                {
                    //MessageBox.Show("Creating old-style example.", "Init", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Creates menu items for the notify icon context menu
                    MenuItem mnuShow = new MenuItem("Show", new EventHandler(mnuShow_Click));
                    MenuItem mnuSeparator1 = new MenuItem("-");
                    MenuItem mnuSettings = new MenuItem("Settings", new EventHandler(mnuSettings_Click));
                    MenuItem mnuSeparator2 = new MenuItem("-");
                    MenuItem mnuFolder = new MenuItem(@"C:\", new EventHandler(mnuFolder_Click));
                    MenuItem mnuSeparator3 = new MenuItem("-");
                    MenuItem mnuAbout = new MenuItem("About...", new EventHandler(mnuAbout_Click));
                    MenuItem mnuSeparator4 = new MenuItem("-");
                    MenuItem mnuExit = new MenuItem("Exit", new EventHandler(mnuExit_Click));

                    // Adds the menu items to the notify icon context menu
                    ContextMenu ctmNotifyIcon = new ContextMenu(new MenuItem[]
                    {
                        mnuShow, mnuSeparator1, mnuSettings, mnuSeparator2, mnuFolder, mnuSeparator3, mnuAbout, mnuSeparator4, mnuExit
                    });

                    // Setup the menu item icons.
                    vistaMenu.SetImage(mnuShow, Properties.Resources.DotOff);
                    vistaMenu.SetImage(mnuSettings, Properties.Resources.Settings);
                    vistaMenu.SetImage(mnuFolder, Properties.Resources.Folder);
                    vistaMenu.SetImage(mnuAbout, Properties.Resources.About);
                    vistaMenu.SetImage(mnuExit, Properties.Resources.Exit);

                    // EndInit() is called by the designer on forms, but since
                    // this is an ApplicationContext you need to call it manually.
                    ((System.ComponentModel.ISupportInitialize)(vistaMenu)).EndInit();

                    // Creates a new instance for the notify icon
                    _notifyIcon = new NotifyIcon()
                    {
                        Icon = Properties.Resources.Notes,
                        ContextMenu = ctmNotifyIcon,
                        Visible = true,
                    };
                }
            }
            catch (Exception ex)
            {
                frmMessage.Show($"{ex.Message}", $"Exception", MessageLevel.Error, true, TimeSpan.Zero);
            }
        }

        string[] ReadConfig(string path)
        {
            if (File.Exists(path))
                return File.ReadAllLines(path);
            else
                return new string[0] { };
        }

        void mnuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Launching folder...", "Open", MessageBoxButtons.OK, MessageBoxIcon.Information);
            try
            {
                var mi = sender as MenuItem;
                var idx = mi.Index;
                if (_paths[idx].StartsWith("-"))
                {
                    // We should never get here.
                    Debug.WriteLine($"Ignoring separator.");
                }
                else
                {
                    var target = _paths[idx].Split(',')[1];
                    if (!string.IsNullOrEmpty(target))
                    {
                        //frmMessage.Show($"Starting \"{target}\"", $"JumpList", MessageLevel.Info, true, TimeSpan.FromSeconds(1.5));
                        _ = Process.Start(target);
                    }
                    else
                        frmMessage.Show($"Empty path for index {idx}", $"Warning", MessageLevel.Warning, true, TimeSpan.Zero);
                }
            }
            catch (Exception ex)
            {
                frmMessage.Show($"{ex.Message}", $"Error", MessageLevel.Error, true, TimeSpan.Zero);
            }
        }


        void AppContext_ThreadExit(object sender, EventArgs e)
        {
            Debug.WriteLine($"[INFO] Thread Exit");
        }

        void mnuShow_Click(object sender, EventArgs e) 
        {
            frmMessage.Show($"Do something here.", $"mnuShow_Click", MessageLevel.Success, true, TimeSpan.FromSeconds(5));
        }

        void mnuFolder_Click(object sender, EventArgs e)
        {
            _ = Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        void mnuAbout_Click(object sender, EventArgs e)
        {
            frmMessage.Show($"Do something here.", $"mnuShow_Click", MessageLevel.Success, true, TimeSpan.FromSeconds(5));
        }

        void mnuSettings_Click(object sender, EventArgs e)
        {
            frmMessage.Show($"Do something here.", $"mnuShow_Click", MessageLevel.Success, true, TimeSpan.FromSeconds(5));
        }

        void mnuExit_Click(object sender, EventArgs e) 
        {
            _notifyIcon?.Dispose();
            Application.Exit();
        }

        Image LoadResourceImageAsset(string imgName)
        {
            Image img = null;
            try
            {
                FileStream fs = new System.IO.FileStream($"Resources\\{imgName}", FileMode.Open, FileAccess.Read);
                img = Image.FromStream(fs);
                fs.Close();
            }
            catch (Exception ex)
            {
                frmMessage.Show($"{ex.Message}", $"LoadResourceImageAsset", MessageLevel.Error, true, TimeSpan.Zero);
            }
            return img;
        }

        /// <summary>
        /// NOTE: The width and height are specific to the "Windows_XP_Icon_Set.png"
        /// </summary>
        Bitmap[,] GenerateBitmapTiles(int singleWidth = 27, int singleHeight = 27)
        {
            Bitmap bitmap = new Bitmap(Properties.Resources.WindowsXP_Icon_Set);
            Bitmap[,] bitmaps = new Bitmap[Properties.Resources.WindowsXP_Icon_Set.Width / singleWidth, Properties.Resources.WindowsXP_Icon_Set.Height / singleHeight];
            for (int x = 0; x < Properties.Resources.WindowsXP_Icon_Set.Width / singleWidth; x++)
            {
                for (int y = 0; y < Properties.Resources.WindowsXP_Icon_Set.Height / singleHeight; y++)
                {
                    bitmaps[x, y] = bitmap.Clone(new Rectangle(x * singleWidth, y * singleHeight, singleWidth, singleHeight), bitmap.PixelFormat);
                }
            }
            bitmap.Dispose();
            return bitmaps;
        }
    }
}
