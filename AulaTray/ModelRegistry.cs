using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AulaTray;

public class KeyboardModelDefinition
{
    public string ModelName { get; set; } = "Aula Keyboard";
    public ushort DongleVid { get; set; } = 0x3554;
    public List<ushort> DonglePids { get; set; } = new();
    public ushort WiredVid { get; set; } = 0x258A;
    public List<ushort> WiredPids { get; set; } = new();
    public List<string> BtKeywords { get; set; } = new();

    public KeyboardModelDefinition() { }

    public KeyboardModelDefinition(
        string modelName,
        ushort dongleVid,
        IEnumerable<ushort> donglePids,
        ushort wiredVid,
        IEnumerable<ushort> wiredPids,
        IEnumerable<string> btKeywords)
    {
        ModelName = modelName;
        DongleVid = dongleVid;
        DonglePids = donglePids.ToList();
        WiredVid = wiredVid;
        WiredPids = wiredPids.ToList();
        BtKeywords = btKeywords.ToList();
    }
}

internal class JsonModelDto
{
    [JsonPropertyName("modelName")]
    public string? ModelName { get; set; }

    [JsonPropertyName("dongleVid")]
    public string? DongleVid { get; set; }

    [JsonPropertyName("donglePids")]
    public List<string>? DonglePids { get; set; }

    [JsonPropertyName("wiredVid")]
    public string? WiredVid { get; set; }

    [JsonPropertyName("wiredPids")]
    public List<string>? WiredPids { get; set; }

    [JsonPropertyName("btKeywords")]
    public List<string>? BtKeywords { get; set; }
}

public static class ModelRegistry
{
    public const string DefaultModelName = "Aula F75";
    public const string GenericDongleModelName = "Aula Wireless Keyboard";
    public const string GenericWiredModelName = "Aula Wired Keyboard";
    public const string GenericBluetoothModelName = "Aula Bluetooth Keyboard";

    private static readonly List<KeyboardModelDefinition> _models = new();

    static ModelRegistry()
    {
        InitializeModels();
    }

    private static void InitializeModels()
    {
        _models.Clear();

        // 1. Yerleşik Modeller (Built-in Catalog)
        _models.Add(new KeyboardModelDefinition(
            modelName: "Aula F75",
            dongleVid: 0x3554,
            donglePids: new ushort[] { 0xFA09 },
            wiredVid: 0x258A,
            wiredPids: new ushort[] { 0x010C },
            btKeywords: new[] { "F75", "AULA F75", "AULA-F75" }
        ));

        _models.Add(new KeyboardModelDefinition(
            modelName: "Aula F87",
            dongleVid: 0x3554,
            donglePids: new ushort[] { 0xFA0A, 0xFA10 },
            wiredVid: 0x258A,
            wiredPids: new ushort[] { 0x010D },
            btKeywords: new[] { "F87", "AULA F87", "AULA-F87" }
        ));

        _models.Add(new KeyboardModelDefinition(
            modelName: "Aula F99",
            dongleVid: 0x3554,
            donglePids: new ushort[] { 0xFA11 },
            wiredVid: 0x258A,
            wiredPids: new ushort[] { 0x010E },
            btKeywords: new[] { "F99", "AULA F99", "AULA-F99" }
        ));

        _models.Add(new KeyboardModelDefinition(
            modelName: "Aula F68",
            dongleVid: 0x3554,
            donglePids: new ushort[] { 0xFA08 },
            wiredVid: 0x258A,
            wiredPids: new ushort[] { 0x010B },
            btKeywords: new[] { "F68", "AULA F68", "AULA-F68" }
        ));

        // 2. Harici models.json dosyasından okuma (Kullanıcı yapılandırması)
        LoadExternalModels();
    }

    private static void LoadExternalModels()
    {
        try
        {
            string configPath = Path.Combine(AppContext.BaseDirectory, "models.json");
            if (!File.Exists(configPath))
            {
                return;
            }

            string json = File.ReadAllText(configPath);
            var parsedList = JsonSerializer.Deserialize<List<JsonModelDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsedList == null || parsedList.Count == 0) return;

            foreach (var item in parsedList)
            {
                if (string.IsNullOrWhiteSpace(item.ModelName)) continue;

                ushort dVid = ParseHexOrDec(item.DongleVid, 0x3554);
                ushort wVid = ParseHexOrDec(item.WiredVid, 0x258A);

                var dPids = (item.DonglePids ?? new List<string>())
                    .Select(p => ParseHexOrDec(p, 0))
                    .Where(p => p != 0)
                    .ToList();

                var wPids = (item.WiredPids ?? new List<string>())
                    .Select(p => ParseHexOrDec(p, 0))
                    .Where(p => p != 0)
                    .ToList();

                var btKeys = (item.BtKeywords ?? new List<string>())
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .ToList();

                // Eğer model zaten varsa güncelle, yoksa başa ekle (öncelikli)
                int existingIdx = _models.FindIndex(m => m.ModelName.Equals(item.ModelName, StringComparison.OrdinalIgnoreCase));
                var newDef = new KeyboardModelDefinition(item.ModelName, dVid, dPids, wVid, wPids, btKeys);

                if (existingIdx >= 0)
                {
                    _models[existingIdx] = newDef;
                }
                else
                {
                    _models.Insert(0, newDef);
                }
            }
        }
        catch
        {
            // models.json hatalıysa yerleşik katalog bozulmadan çalışmaya devam eder
        }
    }

    private static ushort ParseHexOrDec(string? value, ushort fallback)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        value = value.Trim();

        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (ushort.TryParse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort hexVal))
                return hexVal;
        }
        else if (ushort.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort decVal))
        {
            return decVal;
        }

        return fallback;
    }

    public static IReadOnlyList<KeyboardModelDefinition> GetAllModels() => _models.AsReadOnly();

    public static string MatchDonglePath(string path)
    {
        foreach (var model in _models)
        {
            string vidStr = $"VID_{model.DongleVid:X4}";
            if (!path.Contains(vidStr, StringComparison.OrdinalIgnoreCase))
                continue;

            foreach (var pid in model.DonglePids)
            {
                string pidStr = $"PID_{pid:X4}";
                if (path.Contains(pidStr, StringComparison.OrdinalIgnoreCase))
                {
                    return model.ModelName;
                }
            }
        }

        // Seçenek A Fallback: Compx dongle (VID_3554) ise generic isim
        if (path.Contains("VID_3554", StringComparison.OrdinalIgnoreCase))
        {
            return GenericDongleModelName;
        }

        return DefaultModelName;
    }

    public static string MatchWiredPath(string path)
    {
        foreach (var model in _models)
        {
            string vidStr = $"VID_{model.WiredVid:X4}";
            if (!path.Contains(vidStr, StringComparison.OrdinalIgnoreCase))
                continue;

            foreach (var pid in model.WiredPids)
            {
                string pidStr = $"PID_{pid:X4}";
                if (path.Contains(pidStr, StringComparison.OrdinalIgnoreCase))
                {
                    return model.ModelName;
                }
            }
        }

        if (path.Contains("VID_258A", StringComparison.OrdinalIgnoreCase))
        {
            return GenericWiredModelName;
        }

        return DefaultModelName;
    }

    public static string MatchBluetoothDevice(string deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
            return GenericBluetoothModelName;

        foreach (var model in _models)
        {
            foreach (var key in model.BtKeywords)
            {
                if (deviceName.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    return model.ModelName;
                }
            }
        }

        if (deviceName.Contains("AULA", StringComparison.OrdinalIgnoreCase))
        {
            return deviceName;
        }

        return GenericBluetoothModelName;
    }

    public static bool IsAnyKnownDongle(string path)
    {
        if (!path.Contains("Col01", StringComparison.OrdinalIgnoreCase))
            return false;

        // Compx dongle ailesi
        if (path.Contains("VID_3554", StringComparison.OrdinalIgnoreCase))
            return true;

        // Özel modellerin farklı VID'si varsa
        foreach (var model in _models)
        {
            if (path.Contains($"VID_{model.DongleVid:X4}", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static bool IsAnyKnownWired(string path)
    {
        if (path.Contains("VID_258A", StringComparison.OrdinalIgnoreCase))
            return true;

        foreach (var model in _models)
        {
            if (path.Contains($"VID_{model.WiredVid:X4}", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
