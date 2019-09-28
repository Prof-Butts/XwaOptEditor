﻿using HelixToolkit.Wpf;
using JeremyAnsel.Xwa.HooksConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XwaHangarMapEditor
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ViewModel ViewModel
        {
            get
            {
                return (ViewModel)this.Resources["viewModel"];
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetWorkingDirectory();

            bool error = false;

            if (System.IO.Directory.Exists(AppSettings.WorkingDirectory))
            {
                try
                {
                    AppSettings.SetData();
                    this.workingDirectoryText.Text = AppSettings.WorkingDirectory;

                    var viewModel = this.Resources["viewModel"] as ViewModel;
                    if (viewModel != null)
                    {
                        viewModel.Update();
                        this.viewport3D.ResetCamera();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    error = true;
                }
            }
            else
            {
                error = true;
            }

            if (error)
            {
                this.Close();
                return;
            }
        }

        private void SetWorkingDirectory()
        {
            var dlg = new WPFFolderBrowser.WPFFolderBrowserDialog();
            dlg.Title = "Choose a working directory containing " + AppSettings.XwaExeFileName + " or a child directory";

            if (dlg.ShowDialog(this) == true)
            {
                string fileName = dlg.FileName;

                if (!System.IO.File.Exists(System.IO.Path.Combine(fileName, "XWingAlliance.exe")))
                {
                    fileName = System.IO.Path.GetDirectoryName(fileName);

                    if (!System.IO.File.Exists(System.IO.Path.Combine(fileName, "XWingAlliance.exe")))
                    {
                        return;
                    }
                }

                AppSettings.WorkingDirectory = fileName + System.IO.Path.DirectorySeparatorChar;
            }
        }

        private void viewport3D_CameraChanged(object sender, RoutedEventArgs e)
        {
            var viewport = (HelixViewport3D)sender;

            viewport.Camera.NearPlaneDistance = 10;
            viewport.Camera.FarPlaneDistance = 4000000;
        }

        private void ExecuteHelp(object sender, ExecutedRoutedEventArgs e)
        {
            var help = new HelpWindow(this);
            help.ShowDialog();
        }

        private void ExecuteNew(object sender, ExecutedRoutedEventArgs e)
        {
            this.ViewModel.TextFileName = null;
            this.ViewModel.Text = ViewModel.DefaultText;
            this.viewport3D.ResetCamera();
        }

        private void ExecuteOpen(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".ini";
            dialog.CheckFileExists = true;
            dialog.Filter = "Hangar Map files (*.ini, *.txt)|*.ini;*HangarMap.txt";
            dialog.InitialDirectory = AppSettings.WorkingDirectory + "FLIGHTMODELS";

            string fileName;

            if (dialog.ShowDialog(this) == true)
            {
                fileName = dialog.FileName;
            }
            else
            {
                return;
            }

            try
            {
                if (fileName.EndsWith(".ini", StringComparison.OrdinalIgnoreCase))
                {
                    string ship = XwaHooksConfig.GetStringWithoutExtension(fileName);
                    var iniFile = new XwaIniFile(ship);
                    iniFile.ParseIni();
                    iniFile.Read("HangarMap", "HangarMap");

                    this.ViewModel.Text = string.Join("\n", iniFile.Sections["HangarMap"].Lines);
                }
                else
                {
                    this.ViewModel.Text = System.IO.File.ReadAllText(fileName, Encoding.ASCII);
                }

                this.ViewModel.TextFileName = fileName;

                string name = System.IO.Path.GetFileNameWithoutExtension(fileName);

                int index = name.LastIndexOf("Map");
                if (index != -1)
                {
                    this.ViewModel.HangarModel = name.Substring(0, index);
                }

                this.viewport3D.ResetCamera();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, fileName + "\n" + ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.ViewModel.TextFileName))
            {
                this.ExecuteSaveAs(null, null);
                return;
            }

            try
            {
                this.WriteText(this.ViewModel.TextFileName, this.ViewModel.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".ini";
            dialog.Filter = "Hangar Map files (*.ini, *.txt)|*.ini;*HangarMap.txt";
            dialog.FileName = System.IO.Path.GetFileName(this.ViewModel.TextFileName);
            dialog.InitialDirectory = AppSettings.WorkingDirectory + "FLIGHTMODELS";

            string fileName;

            if (dialog.ShowDialog(this) == true)
            {
                fileName = dialog.FileName;
            }
            else
            {
                return;
            }

            try
            {
                this.WriteText(fileName, this.ViewModel.Text);
                this.ViewModel.TextFileName = fileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WriteText(string fileName, string text)
        {
            if (fileName.EndsWith(".ini", StringComparison.OrdinalIgnoreCase))
            {
                string ship = XwaHooksConfig.GetStringWithoutExtension(fileName);
                var iniFile = new XwaIniFile(ship);
                iniFile.ParseIni();
                iniFile.Read("HangarMap", "HangarMap");

                var iniList = iniFile.RetrieveLinesList("HangarMap");

                foreach (string line in text.SplitLines(false))
                {
                    iniList.Add(line);
                }

                iniFile.Save();
            }
            else
            {
                System.IO.File.WriteAllText(fileName, text, Encoding.ASCII);
            }

            MessageBox.Show(this, "Saved.", this.Title);
        }
    }
}
