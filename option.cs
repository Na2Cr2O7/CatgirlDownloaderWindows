using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;



namespace CatGirlDownloaderWindows2
{
    public enum NSFWOption
    {
        SHOW_EVERYTHING,
        ONLY_NSFW,
        BLOCK_NSFW,
    }
    public class OptionItems
    {
       public NSFWOption Nsfw_mode { get; set; }
       public int Auto_reload_interval { get; set; }
       public bool Auto_reload_enabled { get; set; }
    }

    public class Option
    {
        private Windows.Storage.StorageFolder storageFolder =
Windows.Storage.ApplicationData.Current.LocalFolder;
        private string CONFIG = "";

        public OptionItems? Items { get; set; }
        public Option() {
            CONFIG = Path.Combine(storageFolder.Path, "config.json");
            try
            {
                string Text = File.ReadAllText(CONFIG);
                Items = new OptionItems();
                Items = JsonSerializer.Deserialize<OptionItems>(Text);
                if (Items == null)
                {
                    throw new Exception();
                }
            }
            catch
            {
                this.Items = new OptionItems
                {
                    Nsfw_mode = NSFWOption.BLOCK_NSFW,
                    Auto_reload_interval = 5,
                    Auto_reload_enabled = false
                };
            }

        }
        public void Save()
        {
            string text = JsonSerializer.Serialize<OptionItems>(Items!);
            File.WriteAllText(CONFIG, text);
        }
         ~Option()
        {
            Save();
        }
    }
}
