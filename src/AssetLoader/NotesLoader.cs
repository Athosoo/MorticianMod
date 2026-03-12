using MorticianMod.Interface;
using MorticianMod.src;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Text.Json;
using System.Xml;

namespace MorticianMod.AssetLoader;

public class NotesLoader:IAssetLoader
{
    public string LoaderName { get; } = "NotesLoader";
    public string LoaderDescription { get; } = "用于加载assets/notes的加载器类";

    private string _UniqueID = ModInfo._UniqueID;
    private IModHelper _helper;
    private IMonitor _monitor;

    private Dictionary<int, string> cachedNotesData;

    public void Register(IModHelper helper, IMonitor monitor)
    {
        _helper = helper;
        _monitor = monitor;
        _helper.Events.Content.AssetRequested += OnAssetRequested;
        _monitor.Log($"[{_UniqueID}]::->{LoaderName}<-已注册", LogLevel.Debug);
    }

    /// <summary>
    /// 从 JSON 加载秘密纸条数据到缓存数据
    /// </summary>
    private Dictionary<int, string> LoadNotesFromJson()
    {
        if (cachedNotesData != null)
            return cachedNotesData;
        try
        {
            string fullPath = Path.Combine(_helper.DirectoryPath, "assets", "notes", "secret_notes.json");
            if (!File.Exists(fullPath))
            {
                _monitor.Log($"[{_UniqueID}]::秘密纸条文件不存在: {fullPath}", LogLevel.Debug);
                return null;
            }

            string json = File.ReadAllText(fullPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            /// 加载到缓存字段中
            cachedNotesData = JsonSerializer.Deserialize<Dictionary<int, string>>(json, options);

            _monitor.Log($"[{_UniqueID}]::成功加载 {cachedNotesData?.Count ?? 0} 个秘密纸条", LogLevel.Debug);
            return cachedNotesData;
        }
        catch (Exception ex)
        {
            _monitor.Log($"[{_UniqueID}]::读取秘密纸条 JSON 失败: {ex.Message}", LogLevel.Error);
            return null;
        }
    }
    public void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        throw new NotImplementedException();
    }
}