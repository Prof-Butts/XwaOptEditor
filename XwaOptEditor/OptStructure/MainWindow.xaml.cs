﻿using JeremyAnsel.Xwa.Opt.Nodes;
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

namespace OptStructure
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

        public OptFileNodes OptFile
        {
            get
            {
                return (OptFileNodes)((ObjectDataProvider)Resources["OptFile"]).ObjectInstance;
            }
            set
            {
                ((ObjectDataProvider)Resources["OptFile"]).ObjectInstance = value;
            }
        }

        private void Execute_Open(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Xwa OPT files|*.opt";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    OptFile = null;
                    OptFile = OptFileNodes.FromFile(dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
