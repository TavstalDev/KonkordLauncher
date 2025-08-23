# KonkordLauncher

![Release (latest by date)](https://img.shields.io/github/v/release/TavstalDev/KonkordLauncher?style=plastic-square)
![Workflow Status](https://img.shields.io/github/actions/workflow/status/TavstalDev/KonkordLauncher/release.yml?branch=stable&label=build&style=plastic-square)
![License](https://img.shields.io/github/license/TavstalDev/KonkordLauncher?style=plastic-square)
![Downloads](https://img.shields.io/github/downloads/TavstalDev/KonkordLauncher/total?style=plastic-square)
![Issues](https://img.shields.io/github/issues/TavstalDev/KonkordLauncher?style=plastic-square)

> Work in progress.

## Table of Contents
- [TODO](#todo)
- [Description](#description)
- [Features](#features)
- [Screenshots](#screenshots)
- [Installation](#installation)
- [Disclaimer](#disclaimer)
- [Credits](#credits)
- [License](#license)

## TODO
- [ ] Enchance README
- [ ] Cleanup translations
- [ ] Add Modrinth support
- [ ] Add CurseForge support
- - This requires a server-side API due to CurseForge API limitations, so since I have no resources to host a server 24/7 with good network connection, this will be implemented later.
- [ ] Implement instance creation with modpacks
- [ ] Implement instance import/export
- [ ] Implement better token encryption for Linux & MacOS
- [ ] Remove sensitive data from log files before 1.7
- [ ] Implement a more robust way to install old forge libraries
- [x] Implement automatic updates for the launcher
- - [ ] Test the updater on Windows, Linux and MacOS
- [x] Implement patch notes

## Description
KonkordLauncher is a free and open-source Minecraft launcher written in C# using .NET 9 and Avalonia UI. It is designed to be lightweight, fast, and user-friendly, providing a seamless experience for launching and managing Minecraft instances.

## Features
- **Multi-platform support**: Runs on Windows, Linux, and MacOS.
- **Lightweight**: Minimal resource usage, ensuring smooth performance.
- **User-friendly interface**: Intuitive design for easy navigation and management of Minecraft instances.
- **Instance management**: Create, edit, and delete multiple Minecraft instances with different configurations.
- **Mod support**: Easily add and manage mods for your Minecraft instances.
- **Mod loaders**: Support for popular mod loaders like Forge, Fabric, NeoForge and Quilt.
- **Custom Java versions**: Use different Java versions for different instances.
- **Automatic updates**: Keep the launcher up-to-date with the latest features and improvements.
- **Open-source**: Fully open-source, allowing for community contributions and transparency.
- **Secure token storage**: User tokens are securely stored using platform-specific secure storage solutions.

## Screenshots
> Work in progress.

## Installation
Please refer to the [Installation Guide](docs/Installation.md)

## Disclaimer

KonkordLauncher is an independent project and is **not affiliated with Mojang AB**, Microsoft, or the official Minecraft team in any way.

## Credits

Credits to the following projects and resources:

- **FontAwesome** icons
- **Minecraft Block Icons** (source: [https://minecraft.wiki/](https://minecraft.wiki/))
- **TextStudio's Minecraft 3D Text Creator** ([https://www.textstudio.com/logo/minecraft-3d-text-41](https://www.textstudio.com/logo/minecraft-3d-text-41))
- **Avalonia UI** ([https://avaloniaui.net/](https://avaloniaui.net/))
- **CommunityToolkit.Mvvm** ([https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/))
- **Newtonsoft.Json** ([https://www.newtonsoft.com/json](https://www.newtonsoft.com/json))
- **Markdown.Avalonia** 
- **CmlLib.Forge** for references regarding forge versions
- **Modrinth** for their API ([https://docs.modrinth.com/](https://docs.modrinth.com/))
- **CurseForge** for their API ([https://docs.curseforge.com/](https://docs.curseforge.com/))
- **Mojang** for making Minecraft
- **PrismLauncher** for inspiration ([https://prismlauncher.org/](https://prismlauncher.org/))

## License

The code of the project is licensed under the GNU General Public License v3.0.

Images and other media assets are licensed under their respective licenses and are not owned by the KonkordLauncher project.