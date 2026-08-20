using System.Text.Json;
using System.Text.RegularExpressions;

public sealed class ScriptInfo
{
    public string Name { get; set; } = "";
    public string Guid { get; set; } = "";
    public string Version { get; set; } = "";
    public string Author { get; set; } = "";
    public List<int> TerritoryIds { get; set; } = [];
    public string DownloadUrl { get; set; } = "";
    public string UpdateInfo { get; set; } = "";
}

internal static class Parser
{
    private static string GetValue(string attributes, string key)
    {
        var match = Regex.Match(attributes,
            $@"\b{Regex.Escape(key)}\s*:\s*(?:""(?<quoted>[^""]*)""|(?<bare>[^,\)]+))",
            RegexOptions.Singleline);
        return match.Success
            ? (match.Groups["quoted"].Success ? match.Groups["quoted"].Value : match.Groups["bare"].Value).Trim()
            : "";
    }

    private static List<int> GetTerritories(string attributes, string file)
    {
        var match = Regex.Match(attributes, @"territorys\s*:\s*\[(?<ids>[^\]]*)\]");
        if (!match.Success || string.IsNullOrWhiteSpace(match.Groups["ids"].Value)) return [];

        var result = new List<int>();
        foreach (var value in match.Groups["ids"].Value.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!int.TryParse(value.Trim(), out var id))
                throw new FormatException($"Invalid territory id '{value.Trim()}' in {file}.");
            result.Add(id);
        }
        return result;
    }

    public static int Main(string[] args)
    {
        var root = Path.GetFullPath(args.Length > 0 ? args[0] : Directory.GetCurrentDirectory());
        var scriptsPath = Path.Combine(root, "Scripts");
        var outputPath = Path.Combine(root, "OnlineRepo.json");
        var repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY") ?? "Sonnet46/KodakkuScript";
        var branch = Environment.GetEnvironmentVariable("KODAKKU_REPO_BRANCH") ?? "main";

        var previousUpdateInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (File.Exists(outputPath))
        {
            try
            {
                var previous = JsonSerializer.Deserialize<List<ScriptInfo>>(File.ReadAllText(outputPath)) ?? [];
                previousUpdateInfo = previous
                    .Where(x => !string.IsNullOrWhiteSpace(x.Guid))
                    .ToDictionary(x => x.Guid, x => x.UpdateInfo ?? "", StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                Console.WriteLine("Existing OnlineRepo.json is invalid; UpdateInfo values will not be preserved.");
            }
        }

        if (!Directory.Exists(scriptsPath))
        {
            Console.Error.WriteLine($"Scripts directory not found: {scriptsPath}");
            return 1;
        }

        var infos = new List<ScriptInfo>();
        foreach (var file in Directory.EnumerateFiles(scriptsPath, "*.cs", SearchOption.AllDirectories).OrderBy(x => x))
        {
            var content = File.ReadAllText(file);
            var match = Regex.Match(content, @"\[ScriptType\((?<attributes>.*?)\)\]", RegexOptions.Singleline);
            if (!match.Success)
            {
                Console.WriteLine($"Skipping {Path.GetRelativePath(root, file)}: no ScriptType.");
                continue;
            }

            var attributes = match.Groups["attributes"].Value;
            var relativePath = Path.GetRelativePath(root, file).Replace('\\', '/');
            var info = new ScriptInfo
            {
                Name = GetValue(attributes, "name"),
                Guid = GetValue(attributes, "guid"),
                Version = GetValue(attributes, "version"),
                Author = GetValue(attributes, "author"),
                TerritoryIds = GetTerritories(attributes, relativePath),
                UpdateInfo = GetValue(attributes, "updateInfo"),
                DownloadUrl = $"https://raw.githubusercontent.com/{repository}/{branch}/{Uri.EscapeDataString(relativePath).Replace("%2F", "/")}"
            };

            if (string.IsNullOrWhiteSpace(info.UpdateInfo) && previousUpdateInfo.TryGetValue(info.Guid, out var oldUpdateInfo))
                info.UpdateInfo = oldUpdateInfo;

            if (string.IsNullOrWhiteSpace(info.Name) || string.IsNullOrWhiteSpace(info.Guid))
                throw new FormatException($"Missing name or guid in {relativePath}.");
            if (infos.Any(x => x.Guid.Equals(info.Guid, StringComparison.OrdinalIgnoreCase)))
                throw new FormatException($"Duplicate guid '{info.Guid}' in {relativePath}.");

            infos.Add(info);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        File.WriteAllText(outputPath, JsonSerializer.Serialize(infos, options));
        Console.WriteLine($"Generated {outputPath} with {infos.Count} entries.");
        return 0;
    }
}
