
using MorticianMod.Interface;
using MorticianMod.Models;
using MorticianMod.src;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Text.Json;
using System.Threading;
using System.Xml;

namespace MorticianMod.AssetLoader;

public class NotesLoader : IAssetLoader
{
    public string LoaderName => "NotesLoader";
    public string LoaderDescription => "用于加载秘密纸条数据的加载器类";
    private string _UniqueID = ModInfo._UniqueID;
    private IModHelper _helper;
    private IMonitor _monitor;

    private Dictionary<string, int> _idMappings;
    public void Register(IModHelper helper, IMonitor monitor)
    {
        _helper = helper;
        _monitor = monitor;

        loadCustomSecretNoteDatasFromJson();
        string mapPath = Path.Combine(_helper.DirectoryPath, "assets", "notes", "ID_mapping.json");
        if (File.Exists(mapPath))
        {
            string mapJson = File.ReadAllText(mapPath);
            _idMappings = JsonSerializer.Deserialize<Dictionary<string, int>>(mapJson) ?? new Dictionary<string, int>();
            _monitor.Log($"[{LoaderName}]::已加载 {_idMappings.Count} 个现有 ID 映射。", LogLevel.Info);
        }
        else
        {
            _idMappings = new Dictionary<string, int>();
            _monitor.Log($"[{LoaderName}]::未找到 ID 映射文件，将创建新映射。", LogLevel.Info);
        }

        _helper.Events.Content.AssetRequested += OnAssetRequested;
        _monitor.Log($"[{_UniqueID}]::->{LoaderName}<-已注册", LogLevel.Debug);
    }

    private List<CustomSecretNoteData> cachedCustomSecretNoteDatas;
   
    private List<CustomSecretNoteData> loadCustomSecretNoteDatasFromJson()
    {
        _monitor.Log($"[{LoaderName}]::loadCustomSecretNoteDatasFromJson方法被调用",LogLevel.Info);
        if (cachedCustomSecretNoteDatas != null)
            return cachedCustomSecretNoteDatas;
        try
        {
            string fullPath = Path.Combine(_helper.DirectoryPath, "assets", "notes", "secret_notes.json");
            if (!File.Exists(fullPath))
            {
                _monitor.Log($"[{LoaderName}]::秘密纸条数据文件不存在: {fullPath}", LogLevel.Error);
                return null;
            }

            string json = File.ReadAllText(fullPath);
            _monitor.Log($"[{LoaderName}]::成功读取JSON文件，内容长度: {json.Length} 字符", LogLevel.Info);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var wrapper = JsonSerializer.Deserialize<NotesDatasWrapper>(json, options);

            if (wrapper == null)
            {
                _monitor.Log($"[{LoaderName}]::反序列化后 wrapper 为 null", LogLevel.Info);
                return null;
            }

            _monitor.Log($"[{LoaderName}]::已知反序列化wrapper非空,其NoteDatas属性是否为空:{wrapper.NoteDatas == null}",LogLevel.Info);

            cachedCustomSecretNoteDatas = wrapper?.NoteDatas;

            _monitor.Log($"[{LoaderName}]::成功加载 {cachedCustomSecretNoteDatas?.Count ?? 0} 个秘密纸条", LogLevel.Debug);
            _monitor.Log($"[{LoaderName}]::调用LoadCustomSecretNoteDatasFromJson完毕", LogLevel.Info);
            return cachedCustomSecretNoteDatas;
        }
        catch (Exception e)
        {
            _monitor.Log($"[{_UniqueID}]::LoadCustomSecretNoteDatasFromJson进入异常分支!",LogLevel.Warn);
            throw;
        }
        return cachedCustomSecretNoteDatas;
    }
    public void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Data/SecretNotes"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<int, string>().Data;
                
                var usedIds = new HashSet<int>(data.Keys);                 // 当前所有已用 ID
                var allocatedIds = new HashSet<int>(_idMappings.Values);   // 本模组已分配的 ID

                //string allEntries = string.Join(", ", data.Select(kv => $"{kv.Key}:{kv.Value}"));
                //_monitor.Log($"[{LoaderName}]::现有秘密纸条数据: {allEntries}", LogLevel.Info);

                if (cachedCustomSecretNoteDatas == null || cachedCustomSecretNoteDatas.Count == 0)
                {
                    _monitor.Log($"[{LoaderName}]::没有自定义纸条可加载", LogLevel.Warn);
                    return;
                }

                bool mappingChanged = false;

                foreach (var note in cachedCustomSecretNoteDatas)
                {
                    int assignedId;
                    if (_idMappings.TryGetValue(note.Id, out int existingId))
                    {
                        // 已有映射，检查是否可用
                        if (!usedIds.Contains(existingId) && !allocatedIds.Contains(existingId))
                        {
                            assignedId = existingId;
                        }
                        else
                        {
                            // 冲突，重新分配
                            _monitor.Log($"原映射中的 ID {existingId} 已被占用，为纸条 {note.Id} 重新分配...", LogLevel.Warn);
                            assignedId = FindNextAvailableId(usedIds, allocatedIds);
                            _idMappings[note.Id] = assignedId;
                            mappingChanged = true;
                        }
                    }
                    else
                    {
                        // 新纸条，分配新 ID
                        assignedId = FindNextAvailableId(usedIds, allocatedIds);
                        _idMappings[note.Id] = assignedId;
                        mappingChanged = true;
                    }

                    // 记录已使用 ID
                    usedIds.Add(assignedId);
                    allocatedIds.Add(assignedId);

                    // 将纸条加入游戏字典（此处仅用 ContentText，可按需扩展）
                    data[assignedId] = note.ContentText;
                    _monitor.Log($"{assignedId}被加入游戏data字典引用",LogLevel.Info);


                    // 保存分配的 ID 到纸条对象，便于后续逻辑使用
                    note.IntId = assignedId;
                }

                // 若映射有变化，保存到文件
                if (mappingChanged)
                {
                    string mapPath = Path.Combine(_helper.DirectoryPath, "assets", "notes", "ID_mappings.json");
                    string json = JsonSerializer.Serialize(_idMappings, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(mapPath, json);
                    _monitor.Log("ID 映射已保存。", LogLevel.Info);
                }
                //allEntries = string.Join(", ", data.Select(kv => $"{kv.Key}:{kv.Value}"));
                //_monitor.Log("============================================================",LogLevel.Info);
                //_monitor.Log($"[{LoaderName}]::新的秘密纸条数据: {allEntries}", LogLevel.Info);
                //_monitor.Log("============================================================",LogLevel.Info);
            });
        }
    }

    /// <summary>查找下一个可用的整数 ID，从 1,000,000 开始递增。</summary>
    private int FindNextAvailableId(HashSet<int> usedIds, HashSet<int> allocatedIds)
    {
        const int baseId = 10000;
        int candidate = baseId;
        while (usedIds.Contains(candidate) || allocatedIds.Contains(candidate))
        {
            candidate++;
            if (candidate > baseId + 10000)
                throw new Exception("无法找到可用 ID，尝试次数过多。");
        }
        return candidate;
    }


}

