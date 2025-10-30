# Language Packs Checker and Setter

A WPF utility for Windows that helps administrators **check, install, and remove language packs, with optional .NET 3.5 feature activation** using DISM.  
The tool provides a simple UI with logging and color‑coded status messages so users can easily follow the process.

---

## ✨ Features

- **Install Lancguage Packs from a folder**

  Select a folder from the Browse Folder Dialog, choose the correct *.cab files and run the installation with the implemented Install Selected button

- **Check installed language packs**

  Select from a dropdown or type a language code (e.g. `en-US`, `de-DE`) and view all installed packages.

- **Delete language packs**

  Remove selected language packs with confirmation and detailed logging.

- **Update the Language Settings List under the Language and Region Control Panel Element**

   On the Refresh Language Settings Tab, clicking the Refresh Language Settings button will add six, hardcoded languages to the user UI depending of the installed language pack files (*.cab):
   - en-US
   - de-DE
   - fr-FR
   - it-IT
   - ja-JP
   - zn-CH

  This settings comes from the LanguagePackConfig class. Change this accordingly to your needs. (Feature release is in planning to be able to import the wanted languages via an *.xml file.)

- **Enable .NET 3.5 and it's child elements**

  After selecting the correct *.cab file for the .NET 3.5 Package (version of the OS and the version of this *.cab file have to match), it will enable this feature and it's child elements

- **Restart PC**

  This button implementation will check if any processes are still running related to the features above and inform the user about it. Confirmation for the restart is needed.

- **Color‑coded log window**
  - ⬜ Gray: command information
  - 🟦 Blue: process started  
  - 🟧 Orange: removing package  
  - 🟩 Green: success  
  - 🟥 Red: error  

- **Future roadmap**  

  - XML based Language Settings Update

---

## 🛠️ Tech Stack

- **.NET 8** (WPF)  
- **MVVM Design-Pattern**  
- **DISM.exe** - integration for package management
- **Powershell SDK as a dependency**

---

## 🚀 Getting Started

1. Clone the repository with:
   **git clone https://github.com/C0ntrl08/LanguagePacksCheckerAndSetter.git**

   **cd** to the cloned repository folder
2. Open the solution in Visual Studio 2022 (or later)
3. Build and Run the Project

## 📝 Repository Structure
 ```bash
 LanguagePacksCheckerAndSetter/
│
├── Configuration/ # Package settings (e.g. LanguagePackConfig)
├── Models/ # Data models (e.g. LogEntry, LanguagePackModel)
├── ViewModels/ # MVVM ViewModels (CheckViewModel, DeleteViewModel, etc.)
├── Views/  # WPF XAML views
├── Utilities/  # Helper classes (RelayCommand, etc.)
├── LanguagePacksCheckerAndSetter.csproj
├── README.md
└── .gitignore
 ```

 ## ✅ Distribution

 Compiled binaries will **not** be committed to the repository. The release will be published in a ZIP format.

 ## 📜 License