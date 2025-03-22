using RRecord;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Helpers
{
    public static class SettingsManager
    {
        private static readonly string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.xml");

        [Serializable]
        public class UserSettings
        {

            public int HotKeyModifiers { get; set; } = (int)(HotKeyManager.ModifierKeys.Shift | HotKeyManager.ModifierKeys.Windows);
            public int HotKeyKey { get; set; } = (int)System.Windows.Forms.Keys.D;
            public string LastOpenedFile { get; set; } = "";


        }
        public static UserSettings CurrentSettings { get; private set; }

        public static event EventHandler SettingsChanged;


        static SettingsManager()
        {
            Load();
        }

        public static void Load()
        {
            if (File.Exists(settingsFilePath))
            {
                try
                {
                    using (var stream = File.OpenRead(settingsFilePath))
                    {
                        var serializer = new XmlSerializer(typeof(UserSettings));
                        CurrentSettings = (UserSettings)serializer.Deserialize(stream);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Fel vid laddning av inställningar: " + ex.Message);
                    CurrentSettings = new UserSettings();
                }
            }
            else
            {
                CurrentSettings = new UserSettings();
            }
        }


        public static void Save()
        {
            try
            {
                using (var stream = File.Create(settingsFilePath))
                {
                    var serializer = new XmlSerializer(typeof(UserSettings));
                    serializer.Serialize(stream, CurrentSettings);
                }

                SettingsChanged?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fel vid sparning av inställningar: " + ex.Message);
            }
        }
    }
}
