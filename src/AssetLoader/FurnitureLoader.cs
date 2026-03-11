using MorticianMod.Interface;
using MorticianMod.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Text.Json;
using Microsoft.Xna.Framework.Graphics; // 或者使用 Newtonsoft.Json，这里用 System.Text.Json
using MorticianMod.src;

namespace MorticianMod.AssetLoader
{
    public class FurnitureLoader : IAssetLoader
    {
        public string LoaderName => "FurnitureLoader";
        public string LoaderDescription => "用于加载assets/furniture和assets/notes的加载器";

        private string _UniqueID = ModDebug._UniqueID; 
        private IModHelper _helper;
        private IMonitor _monitor;
        

        public void Register(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _helper.Events.Content.AssetRequested += OnAssetRequested;
            _monitor.Log($"[{_UniqueID}]::->{LoaderName}<-已注册", LogLevel.Debug);
        }

        // 缓存解析后的数据，避免多次读取
        private List<FurnitureData> cachedFurnitureData;
        private Dictionary<int, string> cachedNotesData;

        private class FurnitureDataWrapper
        {
            public List<FurnitureData> furnitureDatas { get; set; }
        }

        /// <summary>
        /// 从 JSON 加载家具数据列表到缓存数据
        /// </summary>
        private List<FurnitureData> LoadFurnitureFromJson()
        {
            if (cachedFurnitureData != null) return cachedFurnitureData;
            try
            {
                string fullPath = Path.Combine(_helper.DirectoryPath, "assets", "furniture", "furniture_data.json");
                if (!File.Exists(fullPath))
                {
                    _monitor.Log($"[{_UniqueID}]::家具数据文件不存在: {fullPath}", LogLevel.Debug);
                    return null;
                }

                string json = File.ReadAllText(fullPath);
                _monitor.Log($"[{_UniqueID}]::成功读取JSON文件，内容长度: {json.Length} 字符", LogLevel.Debug);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                // 解析 JSON 为包含 FurnitureData 列表的包装对象
                var wrapper = JsonSerializer.Deserialize<FurnitureDataWrapper>(json, options);

                if (wrapper == null)
                {
                    _monitor.Log($"[{_UniqueID}]::反序列化后 wrapper 为 null", LogLevel.Debug);
                    return null;
                }
                _monitor.Log($"[{_UniqueID}]::wrapper 不为 null，furnitureDatas 属性是否为 null？ {wrapper.furnitureDatas == null}", LogLevel.Debug);
                cachedFurnitureData = wrapper?.furnitureDatas;

                _monitor.Log($"[{_UniqueID}]::成功加载 {cachedFurnitureData?.Count ?? 0} 个家具数据", LogLevel.Debug);
                _monitor.Log($"[{_UniqueID}]::调用LoadFurnitureFromJson完毕", LogLevel.Debug);
                return cachedFurnitureData;
            }
            catch (Exception ex)
            {
                _monitor.Log($"[{_UniqueID}]::读取家具 JSON 失败: {ex.Message}", LogLevel.Error);
                return null;
            }
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
                _monitor.Log($"[{_UniqueID}]::调用LoadNotesFromJson完毕");
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
            // 处理家具数据
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Furniture"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    var furnitureList = LoadFurnitureFromJson();
                    if (furnitureList != null)
                    {
                        foreach (var furn in furnitureList)
                        {
                            string entry = string.Join("/",
                                furn.Name,                    // 索引0：名称
                                furn.FurnitureType.ToString(), // 索引1：类型
                                furn.TilesheetSize,            // 索引2：地块表尺寸
                                furn.BoundingBoxSize,          // 索引3：碰撞箱尺寸
                                furn.Rotations,                 // 索引4：旋转
                                furn.Price,                     // 索引5：价格
                                furn.PlacementRestriction,      // 索引6：位置限制
                                furn.DisplayName,                // 索引7：显示名称
                                furn.SpriteIndex,                // 索引8：图集索引
                                $"{_UniqueID}|{furn.Texture}",         // 索引9：图集路径
                                furn.OffLimitsForRandomSale ?? false,    // 索引10：禁止随机出售
                                furn.ContextTags ?? ""                     // 索引11：上下文标签
                            );
                            _monitor.Log($"[{_UniqueID}]::注册家具: ID:{furn.Id}Name:{furn.Name}", LogLevel.Debug);
                            data[furn.Id] = entry;
                        }
                    }
                    else
                    {
                        _monitor.Log($"[{_UniqueID}]::调用LoadFurnitureFromJson后的返回值为null.", LogLevel.Debug);
                    }
                });
            }
            else if (e.NameWithoutLocale.StartsWith($"{_UniqueID}"))
            {
                _monitor.Log($"[{_UniqueID}]::{e.NameWithoutLocale}",LogLevel.Debug);
                if (cachedFurnitureData == null)
                {
                    _monitor.Log($"[{_UniqueID}]::缓存列表为空",LogLevel.Debug);
                }
                foreach (var furn in cachedFurnitureData)
                {
                    _monitor.Log($"[{_UniqueID}]::缓存列表中元素数量:{cachedFurnitureData.Count}",LogLevel.Debug);
                    if (e.NameWithoutLocale.IsEquivalentTo($"{_UniqueID}|{furn.Texture}"))
                    {
                        string path_now = furn.Texture.Replace("|", "/");
                        _monitor.Log($"[{_UniqueID}]::{path_now}为遍历取出的相对路径", LogLevel.Debug);
                        e.LoadFromModFile<Texture2D>($"{path_now}", AssetLoadPriority.Medium);
                    }
                }
            }
            // 处理秘密纸条数据
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/SecretNotes"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<int, string>().Data;
                    var notes = LoadNotesFromJson();
                    if (notes != null)
                    {
                        foreach (var kvp in notes)
                        {
                            if (!data.ContainsKey(kvp.Key))
                            {
                                data[kvp.Key] = kvp.Value;
                                _monitor.Log($"[{_UniqueID}]::已添加秘密纸条 ID: {kvp.Key}", LogLevel.Debug);
                            }
                            else
                            {
                                _monitor.Log($"[{_UniqueID}]::纸条 ID {kvp.Key} 已存在，跳过", LogLevel.Debug);
                            }
                        }
                    }
                    else
                    {
                        _monitor.Log($"[{_UniqueID}]::调用LoadNoteFromJson后的返回值notes为null.", LogLevel.Debug);
                    }
                });
            }
        }
    }
}
