using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows;

namespace KonkordLibrary.Models.Launcher
{
    public class SkinLibData
    {
        [JsonPropertyName("name"), JsonProperty("name")]
        public string Name { get; set; }
        [JsonPropertyName("model"), JsonProperty("model")]
        public string Model { get; set; }
        [JsonPropertyName("image"), JsonProperty("image")]
        public string Image { get; set; }
        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
        /// <summary>
        /// Functions as a IsReadonly field 
        /// </summary>
        public Visibility Visibility { get; set; }

        public SkinLibData() { }

        public SkinLibData(string name, string model, string image, Visibility visibility)
        {
            Name = name;
            Model = model;
            Image = image;
            Visibility = visibility;
        }

        public static SkinLibData GetSteve()
        {
            return new SkinLibData()
            {
                Name = "Steve",
                Image = "/assets/images/steve_texture.png",
                Visibility = Visibility.Hidden,
                Model = "wide"
            };
        }

        public static SkinLibData GetAlex()
        {
            return new SkinLibData()
            {
                Name = "Alex",
                Image = "/assets/images/alex_texture.png",
                Visibility = Visibility.Hidden,
                Model = "slim"
            };
        }

        /// <summary>
        /// Removes readonly skins
        /// </summary>
        /// <param name="skinLibs"></param>
        /// <returns></returns>
        public static List<SkinLibData> Filter(List<SkinLibData> skinLibs)
        {
            List<SkinLibData> local = new List<SkinLibData>();

            foreach (var skinLib in skinLibs)
            {
                if (skinLib == GetSteve() || skinLib == GetAlex())
                    continue;

                local.Add(skinLib);
            }

            return local;
        }
    }
}
