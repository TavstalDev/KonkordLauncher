using System.Text.Json.Serialization;

namespace KonkordLibrary.Models
{
    [Serializable]
    public class ProfileIcon
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("path")]
        public string Path { get; set; }

        public ProfileIcon() { }
        public ProfileIcon(string name, string path) 
        { 
            Name = name;
            Path = path;
        }


        #region Static Values
        [JsonIgnore]
        public static readonly List<ProfileIcon> Icons = new List<ProfileIcon>()
        {
            new ProfileIcon("Stone", "/assets/images/blocks/1_Stone.png"),
            new ProfileIcon("Grass", "/assets/images/blocks/2_Grass.png"),
            new ProfileIcon("Dirt", "/assets/images/blocks/3_Dirt.png"),
            new ProfileIcon("Cobblestone", "/assets/images/blocks/4_Cobblestone.png"),
            new ProfileIcon("Oak Wood Plank", "/assets/images/blocks/5_Oak Wood Plank.png"),
            new ProfileIcon("Bedrock", "/assets/images/blocks/7_Bedrock.png"),
            new ProfileIcon("Sand", "/assets/images/blocks/12_Sand.png"),
            new ProfileIcon("Gravel", "/assets/images/blocks/13_Gravel.png"),
            new ProfileIcon("Gold Ore", "/assets/images/blocks/14_Gold Ore.png"),
            new ProfileIcon("Iron Ore", "/assets/images/blocks/15_Iron Ore.png"),
            new ProfileIcon("Coal Ore", "/assets/images/blocks/16_Coal Ore.png"),
            new ProfileIcon("Oak Wood", "/assets/images/blocks/17_Oak Wood.png"),
            new ProfileIcon("Sponge", "/assets/images/blocks/19_Sponge.png"),
            new ProfileIcon("Glass", "/assets/images/blocks/20_Glass.png"),
            new ProfileIcon("Lapis Lazuli Ore", "/assets/images/blocks/21_Lapis Lazuli Ore.png"),
            new ProfileIcon("Lapis Lazuli Block", "/assets/images/blocks/22_Lapis Lazuli Block.png"),
            new ProfileIcon("Dispenser", "/assets/images/blocks/23_Dispenser.png"),
            new ProfileIcon("Sandstone", "/assets/images/blocks/24_Sandstone.png"),
            new ProfileIcon("Note Block", "/assets/images/blocks/25_Note Block.png"),
            new ProfileIcon("Piston", "/assets/images/blocks/33_Piston.png"),
            new ProfileIcon("Gold Block", "/assets/images/blocks/41_Gold Block.png"),
            new ProfileIcon("Iron Block", "/assets/images/blocks/42_Iron Block.png"),
            new ProfileIcon("Bricks", "/assets/images/blocks/45_Bricks.png"),
            new ProfileIcon("TNT", "/assets/images/blocks/46_TNT.png"),
            new ProfileIcon("Bookshelf", "/assets/images/blocks/47_Bookshelf.png"),
            new ProfileIcon("Moss Stone", "/assets/images/blocks/48_Moss Stone.png"),
            new ProfileIcon("Obsidian", "/assets/images/blocks/49_Obsidian.png"),
            new ProfileIcon("Chest", "/assets/images/blocks/54_Chest.png"),
            new ProfileIcon("Diamond Ore", "/assets/images/blocks/56_Diamond Ore.png"),
            new ProfileIcon("Diamond Block", "/assets/images/blocks/57_Diamond Block.png"),
            new ProfileIcon("Crafting Table", "/assets/images/blocks/58_Crafting Table.png"),
            new ProfileIcon("Furnace", "/assets/images/blocks/61_Furnace.png"),
            new ProfileIcon("Redstone Ore", "/assets/images/blocks/73_Redstone Ore.png"),
            new ProfileIcon("Snow", "/assets/images/blocks/78_Snow.png"),
            new ProfileIcon("Ice", "/assets/images/blocks/79_Ice.png"),
            new ProfileIcon("Clay", "/assets/images/blocks/82_Clay.png"),
            new ProfileIcon("Jukebox", "/assets/images/blocks/84_Jukebox.png"),
            new ProfileIcon("Pumpkin", "/assets/images/blocks/86_Pumpkin.png"),
        };
        #endregion
    }
}
