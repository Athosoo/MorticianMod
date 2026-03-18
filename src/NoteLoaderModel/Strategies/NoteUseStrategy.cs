using MorticianMod.ItemUseSystem;
using MorticianMod.src;
using StardewModdingAPI;
using StardewValley;

namespace MorticianMod.NoteLoaderModel.Strategies
{
    /// <summary>
    /// 纸条使用策略
    /// 处理秘密纸条的使用逻辑
    /// </summary>
    public class NoteUseStrategy : IItemUseStrategy
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private NoteDataManager _noteDataManager;
        private NoteActionExecutor _noteActionExecutor;
        private NoteCollectionManager _noteCollectionManager;
        private NoteDisplayManager _noteDisplayManager;

        public string StrategyName => "NoteUseStrategy";
        public int Priority => 100;

        /// <summary>
        /// 初始化策略
        /// </summary>
        public void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _noteDisplayManager = new NoteDisplayManager();
            _noteDisplayManager.Initialize(helper, monitor);
        }

        /// <summary>
        /// 注入纸条相关管理器
        /// </summary>
        public void InjectManagers(
            NoteDataManager dataManager,
            NoteActionExecutor actionExecutor,
            NoteCollectionManager collectionManager)
        {
            _noteDataManager = dataManager;
            _noteActionExecutor = actionExecutor;
            _noteCollectionManager = collectionManager;
        }

        /// <summary>
        /// 判断是否能处理该物品
        /// </summary>
        public bool CanHandle(Item item)
        {
            return item?.Name?.StartsWith($"{ModInfo._UniqueID}_") == true;
        }

        /// <summary>
        /// 处理物品使用
        /// </summary>
        public void Handle(ItemUseContext context)
        {
            try
            {
                string itemName = context.Item.Name;
                _monitor.Log($"纸条策略处理物品: {itemName}", LogLevel.Debug);

                // 提取纸条ID
                string noteId = itemName.Replace($"{ModInfo._UniqueID}_", "");
                _monitor.Log($"提取纸条ID: {noteId}", LogLevel.Debug);

                // 查找纸条数据
                var noteData = _noteDataManager?.GetNoteDataById(noteId);
                if (noteData == null)
                {
                    _monitor.Log($"未找到纸条数据: {noteId}", LogLevel.Warn);
                    return;
                }

                _monitor.Log($"找到纸条数据: {noteData.Id}, IntId={noteData.IntId}", LogLevel.Debug);

                // 显示纸条
                _noteDisplayManager?.ShowSecretNote(noteData);

                // 触发纸条动作
                _noteActionExecutor?.TriggerNoteActions(noteData);

                // 添加到收集
                _noteCollectionManager?.AddNote(context.Player, noteData.IntId);

                // 阻止原始行为
                context.SuppressOriginalBehavior = true;

                // 从背包中移除物品
                context.Player.removeItemFromInventory(context.Item);
                _monitor.Log($"纸条 {noteData.Id} 处理完成，已从背包移除", LogLevel.Debug);
            }
            catch (System.Exception ex)
            {
                _monitor.Log($"处理纸条时出错: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
