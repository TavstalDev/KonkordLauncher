using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Platform;
using MinecraftSkinRender;
using MinecraftSkinRender.Image;
using SkiaSharp;
using Path = System.IO.Path;

namespace Tavstal.KonkordLauncher.Desktop;

public partial class TestWindow : Window
{
    public TestWindow()
    {
        InitializeComponent();
        using var skinStream = AssetLoader.Open(
            new Uri("avares://Desktop/Assets/Images/placeholders/steve_texture.png")
        );
        using var skin = SKBitmap.Decode(skinStream);
        
        using var capeStream = AssetLoader.Open(
            new Uri("avares://Desktop/Assets/Images/placeholders/test_cape.png")
        );
        using var cape = SKBitmap.Decode(capeStream);
        
        string projectDir = Environment.CurrentDirectory;
        Debug.WriteLine(projectDir);
        // Logged in as image
        Skin3DHeadTypeA.MakeHeadImage(skin).SavePng(Path.Combine(projectDir, "tempa.png"));
        Skin3DHeadTypeB.MakeHeadImage(skin, 15, 65).SavePng(Path.Combine(projectDir, "tempb.png"));
        // Cape preview
        Cape2DTypaA.MakeCapeImage(cape).SavePng("tempi.png");
        // Skin 2D preview
        Skin2DTypeB.MakeSkinImage(skin).SavePng("tempf.png");
        Skin2DTypeB.MakeSkinImage(skin, SkinType.NewSlim).SavePng("temph.png");
    }
}