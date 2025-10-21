# Language Packs Checker and Setter

A WPF utility for Windows that helps administrators **check, install, and remove language packs** using DISM.  
The tool provides a simple UI with logging and color‑coded status messages so users can easily follow the process.

---

## ✨ Features

- **Install Lancguage Packs from a folder**
  **Coming Soon**

- **Check installed language packs**  
  Select from a dropdown or type a language code (e.g. `en-US`, `de-DE`) and view all installed packages.

- **Delete language packs**  
  Remove selected language packs with confirmation and detailed logging.

- **Color‑coded log window**  
  - 🟦 Blue: process started  
  - 🟧 Orange: removing package  
  - 🟩 Green: success  
  - 🟥 Red: error  

- **Future roadmap**  
  - Install language packs directly from the tool  
  - Export results to file  
  - Improved package filtering

---

## 🛠️ Tech Stack

- **.NET 8** (WPF)  
- **MVVM pattern**  
- **DISM.exe** integration for package management

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
├── Models/ # Data models (e.g. LogEntry, LanguagePackModel)
├── ViewModels/ # MVVM ViewModels (CheckViewModel, DeleteViewModel, etc.)
├── Views/  # WPF XAML views
├── Utilities/  # Helper classes (RelayCommand, etc.)
├── LanguagePacksCheckerAndSetter.csproj
├── README.md
└── .gitignore
 ```

 ## ✅ Distribution

 Compiled binaries will **not** be committed to the repository. When the **Install Language Packs** feature is implemented, a release ZIP will be published on GitHub.

 ## 📜 License