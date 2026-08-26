# Befriender

> *Your friends are worth it.*

Befriender is an improved, responsive, and feature-rich friend list plugin for Final Fantasy XIV, powered by Dalamud. It aims to completely replace the vanilla friend list by offering instant loading, background synchronization, and a plethora of quality-of-life features to manage your social circle in Eorzea.

## ✨ Key Features

* **Instant & Background Sync:** Your friend list loads instantly without any delay. It automatically refreshes in the background (or on zone changes/login) so your data is always up-to-date.
* **Rich Profiles & Metadata:** Automatically tracks when and where you met a friend. Add custom text notes to any profile.
* **Name History & Missing Characters:** Automatically detects when a friend changes their name and keeps a history. It also highlights friends who have deleted their characters or disappeared from your vanilla list.
* **Smart Archiving:** Archive friends to safely remove them from your in-game vanilla list (freeing up slots) while preserving their profile, notes, and metadata locally in Befriender. You can restore them anytime!
* **Quick Actions:** Send tells, invite to party, view adventurer plates, or teleport directly to a friend's estate or current region with a single click.
* **Login Alerts:** Track specific friends to receive an unobtrusive chat notification the moment they log in.
* **Customizable & Themed:** Fully compatible with custom color themes (Dark/Light included). Bind a custom hotkey to open the interface instantly.
* **Localization:** Fully translated into English, French, German, and Japanese based on your client language.
* **Privacy First:** All data is stored locally on your machine.

## 📦 Installation

Befriender is available via a custom Dalamud repository.

1. Open the Dalamud Settings in-game by typing `/xlsettings` in the chat.
2. Navigate to the **Experimental** tab.
3. Scroll down to the **Custom Plugin Repositories** section.
4. Add the following URL to an empty text box:
   `https://ffxiv.plugins.almeris.net/repo.json`
5. Click the **+** button, then click **Save and Close**.
6. Open the Plugin Installer (`/xlplugins`), search for **Befriender**, and click Install.

## 🚀 Usage

* Type `/befriender` or `/fl` in the game chat to open the friend list.
* Type `/befriender config` to directly access the plugin settings.
* You can also assign a custom hotkey (e.g., `Ctrl + F`) in the plugin settings to toggle the interface.

## 🛠️ For Developers

Befriender is built with a strong emphasis on Vertical Slicing (Feature Modules), SOLID principles, and Test-Driven Development (TDD). 

To build from source:
1. Clone the repository.
2. Ensure you have the latest .NET SDK installed.
3. Build the solution using Visual Studio or `dotnet build`.

## 📄 License

This project is licensed under the AGPL-3.0-or-later License.