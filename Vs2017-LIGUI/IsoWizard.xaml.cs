using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Path = System.IO.Path;
using MessageBox = System.Windows.MessageBox;
using IsoCreatorLib;
using BER.CDCat.Export;
using System.Threading;

namespace Vs2017LIGUI
{
    /// <summary>
    /// Interaction logic for IsoWizard.xaml
    /// </summary>
    public partial class IsoWizard : Window
    {
        public IsoWizard()
        {
            InitializeComponent();
        }

        string batchtemplate;
        string batchproduct;

        Thread hotthread = null;

        public void Show(VSEdition template, Window parent)
        {
            _label.Text = "VS2017 " + template.Name;
            _batch.Text = "vs_" + template.Name + ".bat";
            template.GenerateCLIs();
            batchproduct = template.Name;
            batchtemplate = template.GeneratedInstall;
            Owner = parent;
            ShowDialog();
        }

        private void _hotbtn_Click(object sender, RoutedEventArgs e)
        {
            if (hotthread != null)
            {
                if (MessageBox.Show("Are you sure want to abort the operation?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    hotthread?.Abort();
            }
            else
                StartJob(_layout.Text, _dest.Text, _label.Text);
        }

        private void _batchbtn_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckValidity()) return;

            var path = Path.Combine(_layout.Text, _batch.Text);
            if (Path.GetExtension(path).ToLower() != ".bat") path += ".bat";
            File.WriteAllText(path, string.Format(BatContent, batchproduct, batchtemplate));
            MessageBox.Show("Batchfile has been written to " + path, "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void _destbtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog()
            {
                DefaultExt = "iso",
                FileName = _dest.Text,
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _dest.Text = dlg.FileName;
            }

        }

        private void _layoutbtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog()
            {
                SelectedPath = _layout.Text,
                ShowNewFolderButton = true,
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _layout.Text = dlg.SelectedPath;
            }
            dlg.Dispose();
        }

        bool CheckValidity()
        {
            if (!IsValid(_dest.Text)) MessageBox.Show("Output ISO is not valid!", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else if (!IsValid(_batch.Text)) MessageBox.Show("Output Batchfile is not valid!", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else if (!IsValid(_layout.Text)) MessageBox.Show("Input Layout Directory is not valid!", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else return true;
            return false;
        }

        static bool IsValid(string path)
        {
            FileInfo fi = null;
            try
            {
                fi = new FileInfo(path);
            }
            catch (Exception) { }

            return fi != null;
        }

        void StartJob(string input, string output, string label)
        {
            var builder = new IsoCreator();

            builder.Progress += delegate (object sender, ProgressEventArgs e)
            {
                Dispatcher.Invoke(() =>
                {
                    if (e.Maximum < 0)
                    {
                        if (e.Current >= _hotprog.Maximum)
                        {
                            // undetermined?
                            _hottxt.Content = e.Action + " " + (e.Current).ToString("0");
                            _hotprog.IsIndeterminate = true;
                        } else
                        {
                            // use cache (weird?)
                            _hottxt.Content = e.Action + " " + (e.Current / _hotprog.Maximum).ToString("P1");
                            _hotprog.Value = e.Current;
                            _hotprog.IsIndeterminate = false;
                        }
                    }
                    else
                    {
                        _hottxt.Content = e.Action + " " + (e.Current / (double)e.Maximum).ToString("P1");
                        _hotprog.Value = e.Current;
                        _hotprog.Maximum = e.Maximum;
                        _hotprog.IsIndeterminate = false;
                    }
                });
            };

            builder.Abort += delegate (object sender, AbortEventArgs e)
            {
                Dispatcher.Invoke(() =>
                {
                    _hottxt.Content = e.Message;
                    _hotprog.Value = 0;
                    _hotprog.Maximum = 1;
                    _hotbtn.Content = "Start";
                    _hotprog.IsIndeterminate = false;
                    hotthread = null;
                });
            };

            builder.Finish += delegate (object sender, FinishEventArgs e)
            {
                Dispatcher.Invoke(() =>
                {
                    _hottxt.Content = e.Message;
                    _hotprog.Value = 1;
                    _hotprog.Maximum = 1;
                    _hotprog.IsIndeterminate = false;
                    _hotbtn.Content = "Start";
                    hotthread = null;
                });
            };

            _hotbtn.Content = "Abort";

            hotthread = new Thread(new ParameterizedThreadStart(builder.Folder2Iso));
            hotthread.IsBackground = true;
            hotthread.Start(new IsoCreator.IsoCreatorFolderArgs(input, output, label));
        }

        const string BatContent = @"@echo off
REM Generated using github.com/willnode/Vs2017-LayoutInGUI
REM This batch file will install certificates then launch VS installer with appopriate offline arguments
echo:
echo Welcome to Visual Studio {0} Offline Installation
echo:
echo Before installation begin, we need to install certificates first.
echo:
echo The process is not automatic. By pressing enter, we'll show three certificates, 
echo and it's your job is to next-clicking until wizards finished.
pause>nul
echo Launching certificates ...
forfiles /S /M *.p12 /C ""cmd /c explorer @file""
echo Do not press enter until all certificates has been installed.
pause>nul
echo Enter again to start installation..
pause>nul
echo Launching Vs 2017 Offline Installation....
{1}
";
    }
}
