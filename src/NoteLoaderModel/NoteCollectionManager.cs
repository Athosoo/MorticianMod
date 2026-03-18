using MorticianMod.Interface;
using StardewModdingAPI;
using StardewValley;

namespace MorticianMod.NoteLoaderModel
{
    /// <summary>
    /// 纸条收集管理器
    /// 负责管理纸条收集状态 in player.modData[NotesKey]
    /// </summary>
    public class NoteCollectionManager : INamedManager

    {
        public string GetManagerName => "NoteCollectionManager";
        

        public string GetManagerDescription => "在player.modData[NotesKey]中管理已收集列表";

        private IModHelper _helper;
        private IMonitor _monitor;
        private const string NotesKey = "MorticianMod_NotesSeen";
        private Dictionary<Farmer, List<int>> _notesCache = new Dictionary<Farmer, List<int>>();

        public void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _monitor.Log($"{GetManagerName}::初始化完成", LogLevel.Debug);
        }

        /// <summary>
        /// 添加纸条到收集列表player.modData[NotesKey]
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="noteId">纸条ID</param>
        public void AddNote(Farmer player, int noteId)
        {
            var notes = GetNotes(player);
            if (!notes.Contains(noteId))
            {
                notes.Add(noteId);
                player.modData[NotesKey] = string.Join(",", notes);
                _monitor.Log($"{GetManagerName}::纸条 ID {noteId} 已添加到收集品列表", LogLevel.Debug);
                // 清除缓存
                _notesCache.Remove(player);
            }
        }

        /// <summary>
        /// 检查玩家是否已收集纸条
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="noteId">纸条ID</param>
        /// <returns>是否已收集</returns>
        public bool HasNote(Farmer player, int noteId)
        {
            var notes = GetNotes(player);
            return notes.Contains(noteId);
        }

        /// <summary>
        /// 获取玩家已收集的所有纸条(使用缓存)
        /// </summary>
        /// <param name="player">玩家</param>
        /// <returns>已经收集的纸条ID列表</returns>
        public List<int> GetNotes(Farmer player)
        {
            // 检查缓存
            if (_notesCache.TryGetValue(player, out List<int> cachedNotes))
            {
                _monitor.Log($"{GetManagerName}::从缓存获取纸条列表", LogLevel.Trace);
                return cachedNotes;
            }

            // 缓存未命中，解析数据
            List<int> notes;
            if (player.modData.TryGetValue(NotesKey, out string value))
            {
                notes = value.Split(',')
                    .Where(s => !string.IsNullOrEmpty(s) && int.TryParse(s, out _))
                    .Select(int.Parse)
                    .ToList();
            }
            else
            {
                notes = new List<int>();
            }

            // 更新缓存
            _notesCache[player] = notes;
            _monitor.Log("纸条列表已缓存", LogLevel.Trace);
            return notes;
        }

        /// <summary>
        /// 从收集中移除纸条
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="noteId">纸条ID</param>
        public void RemoveNote(Farmer player, int noteId)
        {
            var notes = GetNotes(player);
            if (notes.Remove(noteId))
            {
                player.modData[NotesKey] = string.Join(",", notes);
                _monitor.Log($"{GetManagerName}::纸条 ID {noteId} 已从收集品列表中移除", LogLevel.Debug);
                // 清除缓存
                _notesCache.Remove(player);
            }
        }

        /// <summary>
        /// 获取玩家已收集的纸条数量
        /// </summary>
        /// <param name="player">玩家</param>
        /// <returns>收集数量</returns>
        public int GetNoteCount(Farmer player)
        {
            return GetNotes(player).Count;
        }
    }
}
