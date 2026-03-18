using MorticianMod.Interface;
using MorticianMod.Models;
using MorticianMod.src;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;

namespace MorticianMod.NoteLoaderModel
{
    /// <summary>
    /// 纸条物品管理器
    /// 负责纸条物品的创建和使用
    /// </summary>
    public class NoteItemManager
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private NoteDataManager _noteDataManager;
        private ItemUseDetector _itemUseDetector;
        private NoteActionExecutor _noteActionExecutor;
        private NoteCollectionManager _noteCollectionManager;

        public void Initialize(IModHelper helper, IMonitor monitor, NoteDataManager noteDataManager)
        {
            _helper = helper;
            _monitor = monitor;
            _noteDataManager = noteDataManager;

            _itemUseDetector = new ItemUseDetector();
            _itemUseDetector.Initialize(helper, monitor);

            _noteActionExecutor = new NoteActionExecutor();
            _noteActionExecutor.Initialize(helper, monitor);

            _noteCollectionManager = new NoteCollectionManager();
            _noteCollectionManager.Initialize(helper, monitor);

            // 注册纸条物品使用规则
            RegisterNoteUseRule();

            // 注册物品给予测试命令
            RegisterTestCommand();

            _monitor.Log("纸条物品管理器初始化完成", LogLevel.Debug);
        }

        /// <summary>
        /// 注册纸条使用规则
        /// </summary>
        private void RegisterNoteUseRule()
        {
            var noteUseRule = new ItemUseRule
            {
                TargetItemId = 0, // 由于我们使用名称匹配，这里设为0
                TriggerType = UseTrigger.RightClick,
                SuppressOriginalBehavior = true,
                OnItemUsed = HandleNoteItemUse
            };
            _itemUseDetector.RegisterUseRule(noteUseRule);
        }

        /// <summary>
        /// 注册测试命令
        /// </summary>
        private void RegisterTestCommand()
        {
            _helper.Events.Input.ButtonPressed += (sender, args) =>
            {
                // 确保游戏世界已加载
                if (!Context.IsWorldReady)
                    return;

                // 按下 M 键给予第一个自定义纸条的物品
                if (args.Button == SButton.M)
                {
                    var noteDatas = _noteDataManager.GetAllNoteDatas();
                    if (noteDatas != null && noteDatas.Count > 0)
                    {
                        var firstNote = noteDatas[0]; // 获取第一个纸条
                        string itemId = $"{ModInfo._UniqueID}_{firstNote.Id}";   // 物品 ID
                        Item item = ItemRegistry.Create(itemId, 1);      // 创建一个
                        item.Stack = 1;                                 // 确保不可堆叠
                        Game1.player.addItemByMenuIfNecessary(item);     // 添加到背包
                        _monitor.Log($"已给予物品: {itemId}", LogLevel.Alert);
                    }
                    else
                    {
                        _monitor.Log("没有可用的秘密纸条数据", LogLevel.Warn);
                    }
                }
            };
        }

        /// <summary>
        /// 处理秘密纸条物品的使用
        /// </summary>
        private void HandleNoteItemUse(Farmer player, Item item, UseTrigger trigger)
        {
            try
            {
                if (item == null)
                    return;

                string itemName = item.Name;
                _monitor.Log($"处理纸条物品使用: {itemName}", LogLevel.Debug);

                // 提取纸条ID
                string noteId = itemName.Replace($"{ModInfo._UniqueID}_", "");
                _monitor.Log($"提取纸条ID: {noteId}", LogLevel.Debug);

                // 查找对应的纸条数据
                CustomSecretNoteData noteData = _noteDataManager.GetNoteDataById(noteId);
                if (noteData == null)
                {
                    _monitor.Log($"未找到纸条数据: {noteId}", LogLevel.Warn);
                    return;
                }

                _monitor.Log($"找到纸条数据: {noteData.Id}, IntId={noteData.IntId}", LogLevel.Debug);

                // 显示纸条
                var noteDisplayManager = new NoteDisplayManager();
                noteDisplayManager.Initialize(_helper, _monitor);
                noteDisplayManager.ShowSecretNote(noteData);

                // 触发纸条动作
                _noteActionExecutor.TriggerNoteActions(noteData);

                // 将纸条添加到收集品列表
                _noteCollectionManager.AddNote(player, noteData.IntId);
                _monitor.Log($"纸条 {noteData.Id} 已添加到收集品列表", LogLevel.Debug);

            } catch (System.Exception ex)
            {
                _monitor.Log($"处理纸条物品使用时出错: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 处理资产请求
        /// </summary>
        public void HandleAssetRequest(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                {
                    var objectsData = asset.AsDictionary<string, ObjectData>().Data;
                    var noteDatas = _noteDataManager.GetAllNoteDatas();

                    foreach (var note in noteDatas)
                    {
                        string itemId = $"{ModInfo._UniqueID}_{note.Id}";
                        if (!objectsData.ContainsKey(itemId))
                        {
                            var newObjectData = new ObjectData
                            {
                                Name = itemId,
                                DisplayName = "神秘纸条",
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
                    }
                });
            }
        }
    }
}
