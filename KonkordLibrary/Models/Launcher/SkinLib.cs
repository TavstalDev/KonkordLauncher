using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace KonkordLibrary.Models.Launcher
{
    public class SkinLib
    {
        [JsonPropertyName("id"), JsonProperty("id")]
        public string Id { get; set; }
        [JsonPropertyName("name"), JsonProperty("name")]
        public string Name { get; set; }
        [JsonPropertyName("model"), JsonProperty("model")]
        public string Model { get; set; }
        [JsonPropertyName("modelImage"), JsonProperty("modelImage")]
        public string ModelImage { get; set; }
        [JsonPropertyName("textureImage"), JsonProperty("textureImage")]
        public string TextureImage { get; set; }
        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
        /// <summary>
        /// Functions as a IsReadonly field 
        /// </summary>
        public Visibility Visibility { get; set; }

        public SkinLib() { }

        public SkinLib(string id, string name, string model, string modelImage, string textureImage, Visibility visibility)
        {
            Id = id;
            Name = name;
            Model = model;
            ModelImage = modelImage;
            TextureImage = textureImage;
            Visibility = visibility;
        }

        public static SkinLib GetSteve()
        {
            return new SkinLib()
            {
                Id = "2F287E70-B685-4630-92CC-49C3388B4250",
                Name = "Steve",
                TextureImage = "/assets/images/steve_texture.png",
                ModelImage = "/assets/images/steve_full.png",
                Visibility = Visibility.Collapsed,
                Model = "wide"
            };
        }

        public static SkinLib GetAlex()
        {
            return new SkinLib()
            {
                Id = "F9CF6973-ECAA-4895-84F3-BC3CE88B2C14",
                Name = "Alex",
                TextureImage = "/assets/images/alex_texture.png",
                ModelImage = "/assets/images/alex_full.png",
                Visibility = Visibility.Collapsed,
                Model = "slim"
            };
        }

        /// <summary>
        /// Removes readonly skins
        /// </summary>
        /// <param name="skinLibs"></param>
        /// <returns></returns>
        public static List<SkinLib> Filter(List<SkinLib> skinLibs)
        {
            List<SkinLib> local = new List<SkinLib>();

            foreach (var skinLib in skinLibs)
            {
                if (skinLib == GetSteve() || skinLib == GetAlex())
                    continue;

                local.Add(skinLib);
            }

            return local;
        }

        /// <summary>
        /// Adds readonly skins
        /// </summary>
        /// <param name="skinLibs"></param>
        /// <returns></returns>
        public static List<SkinLib> IncludeDefs(List<SkinLib> skinLibs)
        {
            List<SkinLib> local = new List<SkinLib>()
            {
                GetSteve(),
                GetAlex(),
            };

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
