using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Vs2017LIGUI
{

    public class Workload
    {
        public string Name { get; set; }

        [JsonIgnore]
        public string NameFull { get { return Name; } }

        public string ID { get; set; }
        public string Description { get; set; }
        
        public List<Component> Components { get; } = new List<Component>();

        [JsonIgnore]
        public bool Selectable { get { return !string.IsNullOrEmpty(ID); } }

        [JsonIgnore]
        public bool Selected { get; set; }

        [JsonIgnore]
        public bool Expanded { get; set; }
    }

    public static class ComponentSettings
    {
        public static bool UseRecommended;
        public static bool UseOptional;
        public static string Lang = "en-US";

    }

    public class Component
    {

        [JsonIgnore]
        public Workload TheWorkload;

        public string ID { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public string NameFull { get { return Name + " - " + Depedency.ToString(); } }

        public string Version { get; set; }

        public Depedency Depedency { get; set; }

        [JsonIgnore]
        public bool Selectable { get { return !(!SelfSelected & SelectedFromWorkload()); } }
        
        [JsonIgnore]
        public bool Selected { get { return SelfSelected | SelectedFromWorkload(); } set { if (Selectable) SelfSelected = value; } }

        [JsonIgnore]
        public bool SelfSelected { get; set; }

        public bool SelectedFromWorkload()
        {
            switch (Depedency)
            {
                case Depedency.Required:
                    return TheWorkload.Selected;
                case Depedency.Recommended:
                    return TheWorkload.Selected & ComponentSettings.UseRecommended;
                case Depedency.Optional:
                    return TheWorkload.Selected & ComponentSettings.UseOptional;
                case Depedency.Independent:
                default:
                    return false;
            }
        }
    }

    public enum Depedency
    {
        Independent = 0,
        Required = 1,
        Recommended = 2,
        Optional = 3,
    }

    public class DocProcesser
    {

        // Markdown parsing starts here
        public void Process(string doc, List<Workload> Workloads)
        {
            Workloads.Clear();

            Workload item = null;

            var lines = doc.Replace("\r", "").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length;)
            {
                var line = lines[i].Trim();
                if (line.Length > 3 && line.Substring(0, 3) == "## ")
                {
                    // title (H2)
                    item = new Workload() { Name = line.Substring(3) };

                    if (item.Name.ToLower() == "get support")
                        break; // must be not

                    var line2nd = lines[++i];

                    if (line2nd.Substring(0, 8) == "**ID:** ")
                    {
                        item.ID = line2nd.Substring(7);
                        item.Description = lines[++i].Replace("**Description:** ", "");
                    }
                    else
                        item.Description = line2nd; // Other non-workload options

                    while (i < lines.Length && lines[i++].Substring(0, 5) != "--- |")
                    {
                        // we're incrementing i
                    }

                    // begin fetching components
                    while (i < lines.Length && lines[i].Substring(0, 3) != "## ")
                    {
                        var line3rd = lines[i++].Split('|');

                        if (line3rd.Length < 3)
                            continue; // NOT-A-TABLE line :/

                        var component = new Component()
                        {
                            ID = line3rd[0].Trim(),
                            Name = line3rd[1],
                            Version = line3rd[2],
                            Depedency = line3rd.Length > 3 ? (Depedency)Enum.Parse(typeof(Depedency), line3rd[3], true) : Depedency.Independent,
                            TheWorkload = item,
                        };
                        item.Components.Add(component);
                    }

                    Workloads.Add(item);
                }
                else
                    i++;

            }
        }
    }
    

}
