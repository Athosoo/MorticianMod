using MorticianMod.Models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;
using System.Reflection;

namespace MorticianMod.AssetLoader
{
    /// <summary>
    /// 纸条展示管理器
    /// 负责显示秘密纸条的内容
    /// </summary>
    public class NoteDisplayManager
    {
        private IModHelper _helper;
        private IMonitor _monitor;

        public void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            
            _monitor.Log("纸条展示管理器初始化完成", LogLevel.Debug);
        }

        /// <summary>
        /// 显示秘密纸条
        /// </summary>
        /// <param name="noteData">纸条数据</param>
        public void ShowSecretNote(CustomSecretNoteData noteData)
        {
            try
            {
                if (noteData == null)
                {
                    _monitor.Log("纸条数据为null，无法显示", LogLevel.Warn);
                    return;
                }

                _monitor.Log($"显示秘密纸条: ID={noteData.Id}, IntId={noteData.IntId}", LogLevel.Debug);

                // 检查条件
                if (!CheckConditions(noteData))
                {
                    _monitor.Log($"纸条 {noteData.Id} 条件不满足，无法显示", LogLevel.Debug);
                    return;
                }

                // 使用星露谷原生的信件消息显示机制
                if (noteData.IntId > 0)
                {
                    // 标记为已读
                    MarkNoteAsRead(noteData.IntId);
                    
                    // 显示纸条 - 使用Game1.drawLetterMessage方法
                    try
                    {
                        _monitor.Log("尝试使用drawLetterMessage显示纸条...", LogLevel.Debug);
                        
                        // 使用drawLetterMessage方法显示纸条内容
                        Game1.drawLetterMessage(noteData.ContentText);
                        _monitor.Log($"成功显示纸条: ID={noteData.IntId}", LogLevel.Debug);
                    }
                    catch (Exception ex)
                    {
                        _monitor.Log($"显示纸条时出错: {ex.Message}", LogLevel.Error);
                        _monitor.Log($"堆栈跟踪: {ex.StackTrace}", LogLevel.Error);
                        
                        // 备用方案：显示提示信息
                        Game1.addHUDMessage(new HUDMessage($"秘密纸条 #{noteData.IntId}: {noteData.ContentText}"));
                        _monitor.Log($"使用备用方案显示纸条内容: {noteData.ContentText}", LogLevel.Debug);
                    }
                }
                else
                {
                    _monitor.Log($"纸条 IntId 无效: {noteData.IntId}", LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"显示纸条时出错: {ex.Message}", LogLevel.Error);
                _monitor.Log($"堆栈跟踪: {ex.StackTrace}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 检查纸条条件
        /// </summary>
        /// <param name="noteData">纸条数据</param>
        /// <returns>是否满足条件</returns>
        private bool CheckConditions(CustomSecretNoteData noteData)
        {
            if (noteData.Condition == null)
                return true;

            try
            {
                switch (noteData.Condition.Type)
                {
                    case "GameStateQuery":
                        // 这里可以实现游戏状态查询逻辑
                        // 例如检查玩家是否完成了某个任务等
                        return true;
                    default:
                        return true;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"检查纸条条件时出错: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// 标记纸条为已读
        /// </summary>
        /// <param name="noteId">纸条ID</param>
        private void MarkNoteAsRead(int noteId)
        {
            try
            {
                // 在星露谷中，秘密纸条的已读状态通常存储在游戏存档中
                // 尝试使用反射来访问和修改 secretNotesSeen
                try
                {
                    var playerType = Game1.player.GetType();
                    var secretNotesSeenField = playerType.GetField("secretNotesSeen", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    
                    if (secretNotesSeenField != null)
                    {
                        var secretNotesSeen = secretNotesSeenField.GetValue(Game1.player) as System.Collections.Generic.HashSet<int>;
                        if (secretNotesSeen == null)
                        {
                            secretNotesSeen = new System.Collections.Generic.HashSet<int>();
                            secretNotesSeenField.SetValue(Game1.player, secretNotesSeen);
                        }
                        secretNotesSeen.Add(noteId);
                        _monitor.Log($"标记纸条为已读: ID={noteId}", LogLevel.Debug);
                    }
                    else
                    {
                        _monitor.Log("未找到 secretNotesSeen 字段", LogLevel.Warn);
                    }
                }
                catch (Exception ex)
                {
                    _monitor.Log($"使用反射标记纸条已读时出错: {ex.Message}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"标记纸条已读时出错: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
