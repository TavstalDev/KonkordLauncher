using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Tavstal.KonkordLibrary.Helpers;
using Tavstal.KonkordLibrary.Models.Minecraft.API;
using System.IO;

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

        public async Task DownloadFiles(MojangProfile profile, Skin skin)
        {
            // Download Texture
            // TODO, add progress window
            if (!File.Exists(TextureImage))
            {
                byte[]? fileBytes = await HttpHelper.GetByteArrayAsync(skin.Url);
                if (fileBytes != null)
                {
                    await File.WriteAllBytesAsync(TextureImage, fileBytes);
                }
            }

            // Download Full Model
            // https://starlightskins.lunareclipse.studio/render/default//full

            if (!File.Exists(ModelImage))
            {
                byte[]? fileBytes = await HttpHelper.GetByteArrayAsync($"https://starlightskins.lunareclipse.studio/render/default/{profile.Id}/full?skinUrl={skin.Url}&skinType={Model}&capeEnabled=true");
                if (fileBytes != null)
                {
                    await File.WriteAllBytesAsync(ModelImage, fileBytes);
                }
            }

            // Download No Cape Model
            string noCapeModel = Path.Combine(IOHelper.CacheDir, "skins", skin.Id, "model_cape_none.png");
            if (!File.Exists(noCapeModel))
            {
                byte[]? fileBytes = await HttpHelper.GetByteArrayAsync($"https://starlightskins.lunareclipse.studio/render/default/{profile.Id}/full?skinUrl={skin.Url}&skinType={Model}&cameraPosition={{%22x%22:%220%22,%22y%22:%2216%22,%22z%22:%2232%22}}&cameraFocalPoint={{%22x%22:%223.67%22,%22y%22:%2216.31%22,%22z%22:%223.35%22}}&capeEnabled=false&capeTexture=https://laby.net/texture/download/5b37a01fde6a3e075f3bc5694c18e667.png");
                if (fileBytes != null)
                {
                    await File.WriteAllBytesAsync(noCapeModel, fileBytes);
                }
            }

            // Download Cape Models
            // https://starlightskins.lunareclipse.studio/render/default/Gabenosz/full?cameraPosition={%22x%22:%220%22,%22y%22:%2216%22,%22z%22:%2232%22}&cameraFocalPoint={%22x%22:%223.67%22,%22y%22:%2216.31%22,%22z%22:%223.35%22}&capeEnabled=false&capeTexture=https://laby.net/texture/download/5b37a01fde6a3e075f3bc5694c18e667.png
            foreach (Cape cape in profile.Capes)
            {
                string capeModel = Path.Combine(IOHelper.CacheDir, "skins", skin.Id, $"model_cape_{cape.Alias}.png");
                if (File.Exists(capeModel))
                    continue;

                byte[]? fileBytes = await HttpHelper.GetByteArrayAsync($"https://starlightskins.lunareclipse.studio/render/default/{profile.Id}/full?skinUrl={skin.Url}&skinType={Model}&cameraPosition={{%22x%22:%220%22,%22y%22:%2216%22,%22z%22:%2232%22}}&cameraFocalPoint={{%22x%22:%223.67%22,%22y%22:%2216.31%22,%22z%22:%223.35%22}}&capeEnabled=true&capeTexture={cape.Url}");
                if (fileBytes != null)
                {
                    await File.WriteAllBytesAsync(capeModel, fileBytes);
                }
            }
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
