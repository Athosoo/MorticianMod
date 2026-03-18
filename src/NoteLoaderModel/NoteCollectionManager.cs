using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace MorticianMod.NoteLoaderModel
{
    /// <summary>
    /// 纸条收集管理器
    /// 负责管理纸条收集状态
    /// </summary>
    public class NoteCollectionManager
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private const string NotesKey = "MorticianMod_NotesSeen";

        public void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            _monitor.Log("纸条收集管理器初始化完成", LogLevel.Debug);
        }

        /// <summary>
        /// 添加纸条到收集
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
                _monitor.Log($"纸条 ID {noteId} 已添加到收集品列表", LogLevel.Debug);
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
        /// 获取玩家已收集的所有纸条
        /// </summary>
        /// <param name="player">玩家</param>
        /// <returns>纸条ID列表</returns>
        public List<int> GetNotes(Farmer player)
        {
            if (player.modData.TryGetValue(NotesKey, out string value))
            {
                return value.Split(',')
                    .Where(s => !string.IsNullOrEmpty(s) && int.TryParse(s, out _))
                    .Select(int.Parse)
                    .ToList();
            }
            return new List<int>();
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
                _monitor.Log($"纸条 ID {noteId} 已从收集品列表中移除", LogLevel.Debug);
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
