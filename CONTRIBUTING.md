# Contributing to AulaTray

Thank you for your interest in contributing to **AulaTray**! We welcome bug reports, model additions, documentation updates, and code contributions from the community.

---

## Code of Conduct

Please be respectful, constructive, and kind in all discussions and contributions.

---

## How Can I Contribute?

### 1. Adding Support for a New Keyboard Model
If you have an AULA keyboard model (such as F87 Pro, F99, F68, S99, etc.) that is not currently detected:
1. Find your keyboard's hardware IDs:
   - Open **Device Manager** on Windows.
   - Look under **Human Interface Devices** or **Universal Serial Bus controllers**.
   - Note the **Vendor ID (VID)** and **Product ID (PID)** for 2.4 GHz Dongle, Wired USB, and Bluetooth device name.
2. Test your model locally by adding it to `models.json`:
   ```json
   {
     "modelName": "Aula MyModel",
     "dongleVid": "0x3554",
     "donglePids": ["0xFAXX"],
     "wiredVid": "0x258A",
     "wiredPids": ["0x01XX"],
     "btKeywords": ["MyModel", "AULA MyModel"]
   }
   ```
3. Once verified, submit a Pull Request adding the definition to `ModelRegistry.cs` and `models.json`.

### 2. Reporting Bugs
- Use the [Bug Report Template](.github/ISSUE_TEMPLATE/bug_report.md).
- Include your Windows version, keyboard model, and connection mode (2.4G, Wired, Bluetooth).

### 3. Submitting Pull Requests
1. Fork the repository and create your branch from `main`:
   ```bash
   git checkout -b feat/your-feature-name
   ```
2. Follow existing code patterns and naming conventions.
3. Make sure all unit tests pass:
   ```powershell
   dotnet test tests/AulaTray.Tests/AulaTray.Tests.csproj
   ```
4. If you added new logic (like parsing or filtering), write accompanying xUnit tests in `AulaTray.Tests`.
5. Submit your Pull Request using the [PR Template](.github/pull_request_template.md).

---

## Development Setup

- **IDE**: Visual Studio 2022 (v17.12+), VS Code with C# Dev Kit, or JetBrains Rider.
- **SDK**: [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).
- **Target OS**: Windows 10 (19041+) or Windows 11.

```powershell
# Build the project
dotnet build AulaTray/AulaTray.csproj

# Run tests
dotnet test tests/AulaTray.Tests/AulaTray.Tests.csproj

# Publish release single-file
dotnet publish AulaTray/AulaTray.csproj -c Release -r win-x64 -o dist --self-contained false
```
