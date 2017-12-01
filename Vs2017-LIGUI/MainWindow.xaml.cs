using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;

namespace Vs2017LIGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string[] languages = new string[]
        {
            "cs-CZ",
            "de-DE",
            "en-US",
            "es-ES",
            "fr-FR",
            "it-IT",
            "ja-JP",
            "ko-KR",
            "pl-PL",
            "pt-BR",
            "ru-RU",
            "tr-TR",
            "zh-CN",
            "zh-TW",
        };

        public VSEdition[] editions = new VSEdition[]
        {
            new VSEdition() {
                Name = "Community",
                DownloadExe = "https://aka.ms/vs/15/release/vs_community.exe",
                WorkloadDoc = "https://docs.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-community",
                WorkloadRaw = "https://raw.githubusercontent.com/MicrosoftDocs/visualstudio-docs/master/docs/install/workload-component-id-vs-community.md",
            },

             new VSEdition() {
                Name = "Professional",
                DownloadExe = "https://aka.ms/vs/15/release/vs_professional.exe",
                WorkloadDoc = "https://docs.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-professional",
                WorkloadRaw = "https://raw.githubusercontent.com/MicrosoftDocs/visualstudio-docs/master/docs/install/workload-component-id-vs-professional.md",
            },

            new VSEdition()  {
                Name = "Enterprise",
                DownloadExe = "https://aka.ms/vs/15/release/vs_enterprise.exe",
                WorkloadDoc = "https://docs.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-enterprise",
                WorkloadRaw = "https://raw.githubusercontent.com/MicrosoftDocs/visualstudio-docs/master/docs/install/workload-component-id-vs-enterprise.md",
            },
        };

        public VSEdition ActiveEdition;

        public DocProcesser Processor = new DocProcesser();

        public MainWindow()
        {
            InitializeComponent();
            ActiveEdition = editions[0];
            ActiveEdition.LoadJSON();
            RebuildWorkload();
            PrintFetchStamp();
            _ed0.IsChecked = true;
            _lang.ItemsSource = languages;
            _lang.SelectedItem = "en-US";
        }

        private void _fetch_Click(object sender, RoutedEventArgs e)
        {
            string x = null;
            try
            {
                x = new WebClient().DownloadString(ActiveEdition.WorkloadRaw);

            }
            catch (Exception ex)
            {
                MessageBox.Show("An " + ex.GetType().ToString() + " When fetching URL. Is Internet OK?", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Processor.Process(x, ActiveEdition.metadata.Workloads);
            RebuildWorkload();
            ActiveEdition.SaveJSON();
            PrintFetchStamp();
        }

        private void _param_change(object sender, RoutedEventArgs e)
        {
            RebuildWorkload();


            BuildCLIOut();
        }

        private void _pkg_Changed(object sender, RoutedEventArgs e)
        {
            ComponentSettings.UseOptional = _pkgopt.IsChecked ?? false;
            ComponentSettings.UseRecommended = _pkgrec.IsChecked ?? false;

            RebuildWorkload();
            BuildCLIOut();
        }

        private void _lang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComponentSettings.Lang = (string)_lang.SelectedValue;
            BuildCLIOut();
        }

        public void RebuildWorkload()
        {
            // well, must be a better way to do this ...

            _workloads.SetBinding(ItemsControl.ItemsSourceProperty, "");
            _workloads.SetBinding(ItemsControl.ItemsSourceProperty, new Binding()
            {
                Source = ActiveEdition.metadata.Workloads,
                Mode = BindingMode.Default,

                UpdateSourceTrigger = UpdateSourceTrigger.Default
            });
        }

        public void BuildCLIOut()
        {
            ActiveEdition.GenerateCLIs();
            _clidown.Text = ActiveEdition.GeneratedFetch;
            _cliinst.Text = ActiveEdition.GeneratedInstall;

        }

        public void PrintFetchStamp()
        {
            _fetch.Content = ActiveEdition.metadata.Workloads.Count == 0 ? "Fetch Layout Info" : "Fetch Layout Info ( last update: " + ActiveEdition.metadata.FetchTime.ToShortDateString() + " )";

        }

        private void _edition_change(object sender, RoutedEventArgs e)
        {
            if (_ed0.IsChecked == true)
                ActiveEdition = editions[0];
            if (_ed1.IsChecked == true)
                ActiveEdition = editions[1];
            if (_ed2.IsChecked == true)
                ActiveEdition = editions[2];
            ActiveEdition.LoadJSON();
            RebuildWorkload();
            PrintFetchStamp();
            BuildCLIOut();
        }

        private void _manual_click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://docs.microsoft.com/en-us/visualstudio/install/install-vs-inconsistent-quality-network");
        }

        private void _opwiz_Click(object sender, RoutedEventArgs e)
        {
            new IsoWizard().Show(ActiveEdition, this);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }



    public class VSEdition
    {
        public string Name;
        public string WorkloadRaw;
        public string WorkloadDoc;
        public string DownloadExe;

        public Metadata metadata = new Metadata();

        public string GeneratedFetch, GeneratedInstall;

        public void LoadJSON()
        {
            var path = Environment.CurrentDirectory + "/" + Name + ".json";
            if (File.Exists(path))
                metadata = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText(path));

            // sync existing tree
            foreach (var load in metadata.Workloads)
                foreach (var comp in load.Components)
                    comp.TheWorkload = load;

        }

        public void SaveJSON()
        {
            metadata.FetchTime = DateTime.Now;
            var path = Environment.CurrentDirectory + "/" + Name + ".json";
            File.WriteAllText(path, JsonConvert.SerializeObject(metadata, Formatting.Indented));
        }

        public void GenerateCLIs()
        {
            var exe = "vs_" + Name + ".exe ";
            var layout = "--layout C:\\vs2017Layout ";
            var body = "";
            var foot = "";
            var lang = "--lang " + ComponentSettings.Lang;
            foreach (var loads in metadata.Workloads)
            {
                if (!string.IsNullOrEmpty(loads.ID) && loads.Selected)
                    body += "--add " + loads.ID + " ";

                foreach (var comp in loads.Components)
                    if (comp.SelfSelected && !comp.SelectedFromWorkload())
                        body += "--add " + comp.ID + " ";
            }

            if (ComponentSettings.UseRecommended)
                foot = "--includeRecommended " + foot;

            if (ComponentSettings.UseOptional)
                foot = "--includeOptional " + foot;

            GeneratedFetch = exe + layout + body + foot + lang;
            GeneratedInstall = exe + body + foot + (Name == "Enterprise" ? "--noWeb" : "");
        }


    }

    public class Metadata
    {
        public DateTime FetchTime;
        public List<Workload> Workloads = new List<Workload>();
    }
}
