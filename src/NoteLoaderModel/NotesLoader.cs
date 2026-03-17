
using MorticianMod.Interface;
using MorticianMod.Models;
using MorticianMod.src;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;
using System.Text.Json;
using System.Threading;
using System.Xml;
using System.IO;

namespace MorticianMod.NoteLoaderModel;

public class NotesLoader : IAssetLoader
{
    public string LoaderName => "NotesLoader";
    public string LoaderDescription => "用于加载秘密纸条数据的加载器类";
    private string _UniqueID = ModInfo._UniqueID;
    private IModHelper _helper;
    private IMonitor _monitor;

    // 新添加的组件
    private ItemUseDetector _itemUseDetector;
    private NoteDisplayManager _noteDisplayManager;
    private NoteActionManager _noteActionManager;

    private Dictionary<string, int> _idMappings;
    public void Register(IModHelper helper, IMonitor monitor)
    {
        _helper = helper;
        _monitor = monitor;

        // 初始化新组件
        _itemUseDetector = new ItemUseDetector();
        _noteDisplayManager = new NoteDisplayManager();
        _noteActionManager = new NoteActionManager();

        _itemUseDetector.Initialize(helper, monitor);
        _noteDisplayManager.Initialize(helper, monitor);
        _noteActionManager.Initialize(helper, monitor);

        loadCustomSecretNoteDatasFromJson();
        string mapPath = Path.Combine(_helper.DirectoryPath, "assets", "notes", "ID_mapping.json");
        if (File.Exists(mapPath))
        {
            string mapJson = File.ReadAllText(mapPath);
            _idMappings = JsonSerializer.Deserialize<Dictionary<string, int>>(mapJson) ?? new Dictionary<string, int>();
            _monitor.Log($"[{LoaderName}]::已加载 {_idMappings.Count} 个现有 ID 映射。", LogLevel.Alert);
        }
        else
        {
            _idMappings = new Dictionary<string, int>();
            _monitor.Log($"[{LoaderName}]::未找到 ID 映射文件，将创建新映射。", LogLevel.Alert);
        }

        _helper.Events.Content.AssetRequested += OnAssetRequested;
        
        _helper.Events.Input.ButtonPressed += (sender, args) =>
        {
            // 确保游戏世界已加载
            if (!Context.IsWorldReady)
                return;

            // 按下 M 键给予第一个自定义纸条的物品
            if (args.Button == SButton.M)
            {
                if (cachedCustomSecretNoteDatas != null && cachedCustomSecretNoteDatas.Count > 0)
                {
                    var firstNote = cachedCustomSecretNoteDatas[0]; // 获取第一个纸条
                    string itemId = $"{_UniqueID}_{firstNote.Id}";   // 物品 ID 必须与 Data/Objects 中注册的一致
                    Item item = ItemRegistry.Create(itemId, 1);      // 创建一个
                    Game1.player.addItemByMenuIfNecessary(item);     // 添加到背包
                    _monitor.Log($"已给予物品: {itemId}", LogLevel.Alert);
                }
                else
                {
                    _monitor.Log("没有可用的秘密纸条数据", LogLevel.Warn);
                }
            }
        };

        // 注册秘密纸条物品使用规则
        var noteUseRule = new ItemUseRule
        {
            TargetItemId = 0, // 由于我们使用名称匹配，这里设为0
            TriggerType = UseTrigger.RightClick,
            SuppressOriginalBehavior = true,
            OnItemUsed = HandleNoteItemUse
        };
        _itemUseDetector.RegisterUseRule(noteUseRule);

        _monitor.Log($"[{_UniqueID}]::->{LoaderName}<-已注册", LogLevel.Alert);
    }

    private List<CustomSecretNoteData> cachedCustomSecretNoteDatas;
    private List<ObjectData> cachedObjectDatas;
   
    private List<CustomSecretNoteData> loadCustomSecretNoteDatasFromJson()
        {
            _monitor.Log($"[{LoaderName}]::loadCustomSecretNoteDatasFromJson方法被调用",LogLevel.Alert);
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
                _monitor.Log($"[{LoaderName}]::成功读取JSON文件，内容长度: {json.Length} 字符", LogLevel.Alert);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var wrapper = JsonSerializer.Deserialize<NotesDatasWrapper>(json, options);

                if (wrapper == null)
                {
                    _monitor.Log($"[{LoaderName}]::反序列化后 wrapper 为 null", LogLevel.Alert);
                    return null;
                }

                _monitor.Log($"[{LoaderName}]::已知反序列化wrapper非空,其NoteDatas属性是否为空:{wrapper.NoteDatas == null}",LogLevel.Alert);

                cachedCustomSecretNoteDatas = wrapper?.NoteDatas;

                _monitor.Log($"[{LoaderName}]::成功加载 {cachedCustomSecretNoteDatas?.Count ?? 0} 个秘密纸条", LogLevel.Alert);
                _monitor.Log($"[{LoaderName}]::调用LoadCustomSecretNoteDatasFromJson完毕", LogLevel.Alert);
                return cachedCustomSecretNoteDatas;
            }
            catch (Exception e)
            {
                _monitor.Log($"[{_UniqueID}]::LoadCustomSecretNoteDatasFromJson进入异常分支!",LogLevel.Warn);
                throw;
            }
            return cachedCustomSecretNoteDatas;
        }

        /// <summary>
        /// 处理秘密纸条物品的使用
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="item">物品</param>
        /// <param name="trigger">触发方式</param>
        private void HandleNoteItemUse(Farmer player, Item item, UseTrigger trigger)
        {
            try
            {
                if (item == null)
                    return;

                string itemName = item.Name;
                _monitor.Log($"处理纸条物品使用: {itemName}", LogLevel.Debug);

                // 提取纸条ID
                string noteId = itemName.Replace($"{_UniqueID}_", "");
                _monitor.Log($"提取纸条ID: {noteId}", LogLevel.Debug);

                // 查找对应的纸条数据
                CustomSecretNoteData noteData = cachedCustomSecretNoteDatas?.Find(note => note.Id == noteId);
                if (noteData == null)
                {
                    _monitor.Log($"未找到纸条数据: {noteId}", LogLevel.Warn);
                    return;
                }

                _monitor.Log($"找到纸条数据: {noteData.Id}, IntId={noteData.IntId}", LogLevel.Debug);

                // 显示纸条
                _noteDisplayManager.ShowSecretNote(noteData);

                // 触发纸条动作
                _noteActionManager.TriggerNoteActions(noteData);

            } catch (Exception ex)
            {
                _monitor.Log($"处理纸条物品使用时出错: {ex.Message}", LogLevel.Error);
            }
        }

    public void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Data/SecretNotes"))
        {
            e.Edit(asset =>
            {
                var data = asset.AsDictionary<int, string>().Data;
                var usedIds = new HashSet<int>(data.Keys);
                var allocatedIds = new HashSet<int>(_idMappings.Values);

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
                        // 检查现有 ID 是否已被占用
                        if (usedIds.Contains(existingId))
                        {
                            // 如果被占用，检查内容是否匹配（判断是否仍是我们自己的纸条）
                            if (data.TryGetValue(existingId, out string existingContent) && existingContent == note.ContentText)
                            {
                                // 内容匹配，说明是我们自己的，直接复用
                                _monitor.Log($"获取到映射内已保存ID[{note.IntId}]",LogLevel.Alert);
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

                    // 记录已使用 ID（无论是否复用都要加入集合，防止循环内重复）
                    usedIds.Add(assignedId);
                    allocatedIds.Add(assignedId);

                    // 将纸条加入游戏字典
                    data[assignedId] = note.ContentText;
                    _monitor.Log($"纸条 {note.Id} 分配 ID {assignedId}", LogLevel.Alert);

                    note.IntId = assignedId;
                }

                // 若映射有变化，保存到文件（确保目录存在）
                if (mappingChanged)
                {
                    string mapPath = Path.Combine(_helper.DirectoryPath, "assets", "notes", "ID_mapping.json");
                    string directory = Path.GetDirectoryName(mapPath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    string json = JsonSerializer.Serialize(_idMappings, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(mapPath, json);
                    _monitor.Log($"映射已保存到 {mapPath}", LogLevel.Alert);
                }
            });
        }
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
        {
            _monitor.Log($"[{LoaderName}]::请求资源:[{e.NameWithoutLocale}]",LogLevel.Alert);
            e.Edit(asset =>
            {
                var objectsData = asset.AsDictionary<string, ObjectData>().Data;

                foreach (var note in cachedCustomSecretNoteDatas)
                {
                    _monitor.Log($"[{LoaderName}]::进入遍历:[{note.IntId}]", LogLevel.Alert);
                    string itemId = $"{_UniqueID}_{note.Id}";
                    if (!objectsData.ContainsKey(itemId))
                    {
                        var newObjectData = new StardewValley.GameData.Objects.ObjectData
                        {
                            Name = itemId,
                            DisplayName = $"神秘纸条", 
                            Description = "上面写着神秘的信息",
                            Type = "Quest",
                            Price = 0,
                            Category = -1,
                            Edibility = -300,
                            Texture = "Maps/springobjects",
                            SpriteIndex = 79,
                        };
                        objectsData[itemId] = newObjectData;
                    }
                    _monitor.Log($"[{LoaderName}]::添加检查:[{itemId}:{objectsData[itemId]}]", LogLevel.Alert);
                }
            });
        }
    }

    /// <summary>查找下一个可用的整数 ID，从 10000 开始递增。</summary>
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

