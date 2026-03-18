using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Text.Json;
using MorticianMod.Interface;

namespace MorticianMod.NoteLoaderModel
{
  /// <summary>
  /// 纸条数据管理器
  /// 负责纸条数据的加载和管理
  /// </summary>
  public class NoteDataManager : INamedManager
  {
    public string GetManagerName => "NoteDataManager";
    public string GetManagerDescription => "用于";
        private IModHelper _helper;
    private IMonitor _monitor;
    private Dictionary<string, int> _idMappings;
    private List<CustomSecretNoteData> _noteDatas;

    public void Initialize(IModHelper helper, IMonitor monitor)
    {
      _helper = helper;
      _monitor = monitor;
      _idMappings = new Dictionary<string, int>();
      _noteDatas = new List<CustomSecretNoteData>();

      LoadNoteDatas();
      LoadIdMappings();

      _monitor.Log($"纸条数据管理器初始化完成，加载了 {_noteDatas.Count} 个纸条", LogLevel.Debug);
    }

    /// <summary>
    /// 从Json文件(路径为硬编码)加载纸条数据
    /// </summary>
    private void LoadNoteDatas()
    {
      try
      {
        string fullPath = Path.Combine(_helper.DirectoryPath, "assets", "notes", "secret_notes.json");
        if (!File.Exists(fullPath))
        {
          _monitor.Log($"秘密纸条数据文件不存在: {fullPath}", LogLevel.Error);
          return;
        }

        string json = File.ReadAllText(fullPath);
        var options = new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true,
          AllowTrailingCommas = true,
          ReadCommentHandling = JsonCommentHandling.Skip
        };

        var wrapper = JsonSerializer.Deserialize<NotesDatasWrapper>(json, options);
        if (wrapper != null && wrapper.NoteDatas != null)
        {
          _noteDatas = wrapper.NoteDatas;
          _monitor.Log($"成功加载 {_noteDatas.Count} 个秘密纸条", LogLevel.Debug);
        }
      }
      catch (System.Exception ex)
      {
        _monitor.Log($"加载纸条数据时出错: {ex.Message}", LogLevel.Error);
      }
    }

    /// <summary>
    /// 从指定文件(路径硬编码)加载ID映射(纸条独有ID)
    /// </summary>
    private void LoadIdMappings()
    {
      try
      {
        string mapPath = Path.Combine(_helper.DirectoryPath, "assets", "notes", "ID_mapping.json");
        if (File.Exists(mapPath))
        {
          string mapJson = File.ReadAllText(mapPath);
          _idMappings = JsonSerializer.Deserialize<Dictionary<string, int>>(mapJson) ?? new Dictionary<string, int>();
          _monitor.Log($"已加载 {_idMappings.Count} 个现有 ID 映射", LogLevel.Debug);
        }
        else
        {
          _idMappings = new Dictionary<string, int>();
          _monitor.Log("未找到 ID 映射文件，将创建新映射", LogLevel.Debug);
        }
      }
      catch (System.Exception ex)
      {
        _monitor.Log($"加载ID映射时出错: {ex.Message}", LogLevel.Error);
        _idMappings = new Dictionary<string, int>();
      }
    }

    /// <summary>
    /// 处理资产请求
    /// </summary>
    public void HandleAssetRequest(AssetRequestedEventArgs e)
    {
      if (e.NameWithoutLocale.IsEquivalentTo("Data/SecretNotes"))
      {
        e.Edit(asset =>
        {
          var data = asset.AsDictionary<int, string>().Data;
          var usedIds = new System.Collections.Generic.HashSet<int>(data.Keys);
          var allocatedIds = new System.Collections.Generic.HashSet<int>(_idMappings.Values);

          if (_noteDatas.Count == 0)
          {
            _monitor.Log("没有自定义纸条可加载", LogLevel.Debug);
            return;
          }

          bool mappingChanged = false;

          foreach (var note in _noteDatas)
          {
            int assignedId = GetOrAssignNoteId(note, usedIds, allocatedIds, ref mappingChanged, data);
            data[assignedId] = note.ContentText;
            note.IntId = assignedId;
          }

          if (mappingChanged)
          {
            SaveIdMappings();
          }
        });
      }
    }

    /// <summary>
    /// 获取或分配纸条ID
    /// </summary>
    private int GetOrAssignNoteId(CustomSecretNoteData note, System.Collections.Generic.HashSet<int> usedIds, System.Collections.Generic.HashSet<int> allocatedIds, ref bool mappingChanged, System.Collections.Generic.IDictionary<int, string> data)
    {
      int assignedId;

      if (_idMappings.TryGetValue(note.Id, out int existingId))
      {
        if (usedIds.Contains(existingId))
        {
          // 检查内容是否匹配（判断是否仍是我们自己的纸条）
          if (data.TryGetValue(existingId, out string existingContent) && existingContent == note.ContentText)
          {
            // 内容匹配，说明是我们自己的，直接复用
            assignedId = existingId;
          }
          else
          {
            // 内容不匹配，被其他模组占用，重新分配
            _monitor.Log($"ID {existingId} 已被其他模组占用，为纸条 {note.Id} 重新分配...", LogLevel.Warn);
            assignedId = FindNextAvailableId(usedIds, allocatedIds);
            _idMappings[note.Id] = assignedId;
            mappingChanged = true;
          }
        }
        else
        {
          // ID 未被占用，直接使用
          assignedId = existingId;
        }
      }
      else
      {
        // 新纸条，分配新 ID
        assignedId = FindNextAvailableId(usedIds, allocatedIds);
        _idMappings[note.Id] = assignedId;
        mappingChanged = true;
      }

      usedIds.Add(assignedId);
      allocatedIds.Add(assignedId);
      _monitor.Log($"纸条 {note.Id} 分配 ID {assignedId}", LogLevel.Debug);

      return assignedId;
    }

    /// <summary>
    /// 查找下一个可用的整数 ID
    /// </summary>
    private int FindNextAvailableId(System.Collections.Generic.HashSet<int> usedIds, System.Collections.Generic.HashSet<int> allocatedIds)
    {
      const int baseId = 2000;
      int candidate = baseId;
      while (usedIds.Contains(candidate) || allocatedIds.Contains(candidate))
      {
        candidate++;
        if (candidate > baseId + 2000)
          throw new System.Exception("无法找到可用 ID，尝试次数过多。");
      }
      return candidate;
    }

    /// <summary>
    /// 保存ID映射
    /// </summary>
    private void SaveIdMappings()
    {
      try
      {
        string mapPath = Path.Combine(_helper.DirectoryPath, "assets", "notes", "ID_mapping.json");
        string directory = Path.GetDirectoryName(mapPath);
        if (!Directory.Exists(directory))
          Directory.CreateDirectory(directory);

        string json = JsonSerializer.Serialize(_idMappings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(mapPath, json);
        _monitor.Log($"映射已保存到 {mapPath}", LogLevel.Debug);
      }
      catch (System.Exception ex)
      {
        _monitor.Log($"保存ID映射时出错: {ex.Message}", LogLevel.Error);
      }
    }

    /// <summary>
    /// 获取所有纸条数据
    /// </summary>
    public List<CustomSecretNoteData> GetAllNoteDatas() => _noteDatas;

    /// <summary>
    /// 根据ID获取纸条数据
    /// </summary>
    public CustomSecretNoteData GetNoteDataById(string noteId) => _noteDatas.Find(note => note.Id == noteId);

    /// <summary>
    /// 根据整数ID获取纸条数据
    /// </summary>
    public CustomSecretNoteData GetNoteDataByIntId(int intId) => _noteDatas.Find(note => note.IntId == intId);
  }
}
