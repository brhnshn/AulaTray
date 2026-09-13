---
name: Keyboard Model Support Request
about: Request support or submit VID/PID for an untested Aula / Epomaker wireless keyboard
title: '[MODEL] Support Aula '
labels: ['enhancement', 'hardware-support']
assignees: ''
---

### Keyboard Information
- **Model Name**: (e.g. Aula F68, Aula F87 Pro, Aula F99)
- **Manufacturer**: Aula / Epomaker / Other
- **Connection Type Tested**: (2.4 GHz USB Dongle / USB Cable / Bluetooth)

### Hardware Identification (Device Manager / PowerShell)
Please provide the Vendor ID (VID) and Product ID (PID).
You can find this in Windows Device Manager -> Human Interface Devices -> Properties -> Details -> Hardware Ids.

```text
VID: 0x3554 (or other)
PID: 0x...
MI: 01 (Interface number if visible)
```

### Protocol Validation (if tested)
Have you tested this model by creating or editing `models.json`?
- [ ] Yes, it works with `models.json`!
- [ ] Tested, but battery report not responding.
- [ ] Have not tested yet, requesting support.

If working, please share the `models.json` entry:
```json
{
  "id": "aula_...",
  "name": "Aula ...",
  "vendorId": 13652,
  "productId": 0,
  "interfaceNumber": 1,
  "batteryUsagePage": 65280,
  "batteryUsage": 1,
  "supportsDongleMode": true,
  "isWiredSupported": true
}
```
