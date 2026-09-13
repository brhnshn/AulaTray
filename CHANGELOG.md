# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.1.0] - 2026-09-13

### Added
- **Multi-Model Support**: Native catalog support for **Aula F75, F87, F99, and F68** mechanical keyboards.
- **Dynamic Model Header**: Status flyout card and system tray context menus now dynamically display the active keyboard model name.
- **External `models.json` Support**: Users can add new keyboard models or override hardware IDs without recompiling the source code.
- **Generic Fallback**: Intelligent fallback naming (`"Aula Wireless Keyboard"`) for any Compx/Aula dongle (`VID_3554`) with unknown PIDs.
- **3-Layer Design Token System (`ThemeTokens.cs`)**: Structured design hierarchy (Primitives $\rightarrow$ Semantics $\rightarrow$ Components) for dark slate and emerald styling.
- **Asynchronous Event-Driven Architecture (`KeyboardMonitor.cs`)**: Non-blocking background worker with `PeriodicTimer` ensuring zero taskbar/tray UI stuttering during device timeouts.
- **Decoupled Transports (`ITransport`)**: Modular hardware communication split into `WiredTransport`, `DongleTransport`, and `BleTransport`.
- **Pure Battery State Machine (`BatteryFilter.cs`)**: Lithium voltage sag rebound filter isolated into a 100% unit-testable domain class.
- **Working Set Memory Optimizer (`MemoryOptimizer.cs`)**: Automatic working set trimming, reducing RAM footprint from 75.8 MB to ~12.8 MB (83% reduction).
- **Automated xUnit Test Suite (`AulaTray.Tests`)**: 12 comprehensive unit tests covering battery filtering, RF packet checksum calculation, and model registry matching.
- **Architectural Decision Records (ADRs)**: Added ADR 0003 (Glass "A" Logo & Card Design), ADR 0004 (Multi-Model Architecture), and ADR 0005 (Design Tokens & Performance Optimization).
- **AutoStart Migration (Expand-Contract)**: Seamless migration from legacy registry key `AulaF75Tray` to `AulaTray`.

### Changed
- **Tray Icon**: Replaced legacy percentage font icon with the metallic glass "A" brand icon. Active when connected, loş/grayscale when sleeping or disconnected.
- **Zero-Allocation Icon Caching**: Pre-rendered active and dimmed icons in memory, completely eliminating 3.5-second GDI allocation churn and handle leaks.
- **Flyout Window Dismissal**: Replaced low-level global mouse hook (`WH_MOUSE_LL`) with clean local WinForms `WM_ACTIVATE` / `Deactivate` handling.
- **Polling Ordering**: Reordered hardware polling to check 2.4G Dongle before Bluetooth, eliminating unnecessary BLE device scans.
- **Fast Sleep Detection**: Polling interval set to 3500ms with a 2-period threshold (~7 seconds) for immediate sleep state feedback.

### Removed
- Removed obsolete `aulatray_badge.png` asset and deprecated `CreateBatteryIcon` API.

---

## [1.0.0] - 2026-09-12

### Added
- Initial release with support for AULA F75 wireless mechanical keyboard.
- Native Win32 HID reverse-engineered protocol support for Compx 2.4G dongles (`0x3554:0xFA09`).
- Native USB wired mode support via Sinowealth Feature Reports (`0x258A:0x010C`).
- Bluetooth Low Energy (BLE) GATT battery service (`0x180F`) integration.
- Windows 11 dark mode status flyout card.
- Single-click "Start with Windows" tray menu option.
