using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

namespace Tavstal.KonkordLauncher.Core.Models.Launcher
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
        [JsonProperty("capeId"), JsonPropertyName("capeId")]
        public string? CapeId {  get; set; }
        [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
        /// <summary>
        /// Functions as a IsReadonly field 
        /// </summary>
        public Visibility Visibility { get; set; }

        public SkinLib() { }

        public SkinLib(Skin skin, string? capeId = null) 
        {
            Id = skin.Id;
            Name = skin.Alias ?? "unnamed";
            TextureImage = Path.Combine(IOHelper.CacheDir, "skins", skin.Id, "texture.png");
            ModelImage = Path.Combine(IOHelper.CacheDir, "skins", skin.Id, "model.png");
            Model = skin.Variant == "SLIM" ? "slim" : "wide";
            CapeId = capeId;
            Visibility = Visibility.Visible;
        }

        public SkinLib(string id, string name, string model, string modelImage, string textureImage, string capeId, Visibility visibility)
        {
            Id = id;
            Name = name;
            Model = model;
            ModelImage = modelImage;
            TextureImage = textureImage;
            CapeId = capeId;
            Visibility = visibility;
        }

        public int GetModelAsIndex()
        {
            switch (Model)
            {
                case "wide":
                case "classic":
                    return 0;
                case "slim":
                    return 1;
                default:
                    return -1;
            }
        }

        public static string GetIndexAsModel(int index)
        {
            switch (index)
            {
                default:
                case 0:
                    return "wide";
                case 1:
                    return "slim";
            }
        }

        /// <summary>
        /// Asynchronously downloads skin files associated with the specified Mojang profile and skin.
        /// </summary>
        /// <param name="profile">The Mojang profile to download files for.</param>
        /// <param name="skin">The skin to download files for.</param>
        /// <param name="progressWindow">Optional: An instance of a progress window to show download progress.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        public async Task DownloadFilesAsync(MojangProfile profile, Skin skin, IProgressWindow? progressWindow = null)
        {
            // Download Texture
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (sender, e) =>
            {
                if (progressWindow != null)
                {
                    if (!progressWindow.GetIsVisible())
                        progressWindow.ShowWindow();
                    progressWindow.UpdateProgressBarTranslated(e, "ui_downloading_skin_texture", new object[] { e.ToString("0.00") });
                }
            };

            string dir = IOHelper.GetDirectory(TextureImage);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            dir = IOHelper.GetDirectory(ModelImage);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(TextureImage))
            {
                HttpClient client = HttpHelper.GetHttpClient();
               
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:125.0) Gecko/20100101 Firefox/125.0");
                client.DefaultRequestHeaders.Add("Accept", "*/*");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");

                try
                {
                    byte[]? fileBytes = await client.GetByteArrayAsync(skin.Url);
                    if (fileBytes != null)
                    {
                        await File.WriteAllBytesAsync(TextureImage, fileBytes);
                    }
                }
                catch { }
            }

            // Download Full Model
            // https://starlightskins.lunareclipse.studio/render/default//full
            progress = new Progress<double>();
            progress.ProgressChanged += (sender, e) =>
            {
                if (progressWindow != null)
                {
                    if (!progressWindow.GetIsVisible())
                        progressWindow.ShowWindow();
                    progressWindow.UpdateProgressBarTranslated(e, "ui_downloading_skin_model", new object[] { e.ToString("0.00") });
                }
            };

            if (!File.Exists(ModelImage))
            {
                byte[]? fileBytes = await HttpHelper.GetByteArrayAsync($"https://starlightskins.lunareclipse.studio/render/default/{profile.Id}/full?skinUrl={skin.Url}&skinType={Model}&capeEnabled=true", progress);
                if (fileBytes != null)
                {
                    await File.WriteAllBytesAsync(ModelImage, fileBytes);
                }
            }

            // Download No Cape Model
            /*progress = new Progress<double>();
            progress.ProgressChanged += (sender, e) =>
            {
                if (progressWindow != null)
                {
                    if (!progressWindow.GetIsVisible())
                        progressWindow.ShowWindow();
                    progressWindow.UpdateProgressBarTranslated(e, "ui_downloading_skin_cape", new object[] { e.ToString("0.00"), "hidden" });
                }
            };

            string noCapeModel = Path.Combine(IOHelper.CacheDir, "skins", skin.Id, "model_cape_none.png");
            if (!File.Exists(noCapeModel))
            {
                byte[]? fileBytes = await HttpHelper.GetByteArrayAsync($"https://starlightskins.lunareclipse.studio/render/default/{profile.Id}/full?skinUrl={skin.Url}&skinType={Model}&cameraPosition={{%22x%22:%220%22,%22y%22:%2216%22,%22z%22:%2232%22}}&cameraFocalPoint={{%22x%22:%223.67%22,%22y%22:%2216.31%22,%22z%22:%223.35%22}}&capeEnabled=false&capeTexture=https://laby.net/texture/download/5b37a01fde6a3e075f3bc5694c18e667.png", progress);
                if (fileBytes != null)
                {
                    await File.WriteAllBytesAsync(noCapeModel, fileBytes);
                }
            }*/

            // Download Cape Models
            // https://starlightskins.lunareclipse.studio/render/default/Gabenosz/full?cameraPosition={%22x%22:%220%22,%22y%22:%2216%22,%22z%22:%2232%22}&cameraFocalPoint={%22x%22:%223.67%22,%22y%22:%2216.31%22,%22z%22:%223.35%22}&capeEnabled=false&capeTexture=https://laby.net/texture/download/5b37a01fde6a3e075f3bc5694c18e667.png
            string capeDir = Path.Combine(IOHelper.CacheDir, "capes");
            if (!Directory.Exists(capeDir))
                Directory.CreateDirectory(capeDir);
            
            foreach (Cape cape in profile.Capes)
            {
                progress = new Progress<double>();
                progress.ProgressChanged += (sender, e) =>
                {
                    if (progressWindow != null)
                    {
                        if (!progressWindow.GetIsVisible())
                            progressWindow.ShowWindow();
                        progressWindow.UpdateProgressBarTranslated(e, "ui_downloading_skin_cape", new object[] { e.ToString("0.00"), cape.Alias });
                    }
                };

                string capeModel = Path.Combine(capeDir, $"{cape.Alias}.png");
                if (File.Exists(capeModel))
                    continue;

                byte[]? fileBytes = await HttpHelper.GetByteArrayAsync($"https://starlightskins.lunareclipse.studio/render/default/{profile.Id}/full?skinUrl=https://raw.githubusercontent.com/TavstalDev/KonkordLauncher/master/KonkordLauncher/assets/images/steve_texture.png&skinType=slim&cameraPosition={{%22x%22:%220%22,%22y%22:%2216%22,%22z%22:%2232%22}}&cameraFocalPoint={{%22x%22:%223.67%22,%22y%22:%2216.31%22,%22z%22:%223.35%22}}&capeEnabled=true&capeTexture={cape.Url}", progress);
                if (fileBytes != null)
                {
                    await File.WriteAllBytesAsync(capeModel, fileBytes);
                }
            }
        }

        /// <summary>
        /// Retrieves the skin library entry for Steve.
        /// </summary>
        /// <returns>
        /// The skin library entry for Steve.
        /// </returns>
        public static SkinLib GetSteve()
        {
            return new SkinLib()
            {
                Id = "2F287E70-B685-4630-92CC-49C3388B4250",
                Name = "Steve",
                TextureImage = "/assets/images/steve_texture.png",
                ModelImage = "/assets/images/steve_full.png",
                Visibility = Visibility.Collapsed,
                CapeId = null,
                Model = "wide"
            };
        }

        /// <summary>
        /// Retrieves the skin library entry for Alex.
        /// </summary>
        /// <returns>
        /// The skin library entry for Alex.
        /// </returns>
        public static SkinLib GetAlex()
        {
            return new SkinLib()
            {
                Id = "F9CF6973-ECAA-4895-84F3-BC3CE88B2C14",
                Name = "Alex",
                TextureImage = "/assets/images/alex_texture.png",
                ModelImage = "/assets/images/alex_full.png",
                Visibility = Visibility.Collapsed,
                CapeId = null,
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
