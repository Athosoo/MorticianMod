using MorticianMod.Models;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MorticianMod.NoteLoaderModel
{
    /// <summary>
    /// 纸条动作管理器
    /// 负责处理秘密纸条的动作触发
    /// </summary>
    public class NoteActionManager
    {
        private IModHelper _helper;
        private IMonitor _monitor;

        public void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            
            _monitor.Log("纸条动作管理器初始化完成", LogLevel.Debug);
        }

        /// <summary>
        /// 触发纸条动作
        /// </summary>
        /// <param name="noteData">纸条数据</param>
        public void TriggerNoteActions(CustomSecretNoteData noteData)
        {
            try
            {
                // 处理附件
                if (noteData != null && noteData.Attachments != null && noteData.Attachments.Count > 0)
                {
                    _monitor.Log($"处理纸条附件: ID={noteData.Id}, 附件数量={noteData.Attachments.Count}", LogLevel.Debug);
                    HandleAttachments(noteData.Attachments);
                }

                // 处理触发动作
                if (noteData == null || noteData.TriggerActions == null || noteData.TriggerActions.Count == 0)
                {
                    _monitor.Log("没有动作需要触发", LogLevel.Debug);
                    return;
                }

                _monitor.Log($"触发纸条动作: ID={noteData.Id}, 动作数量={noteData.TriggerActions.Count}", LogLevel.Debug);

                foreach (var action in noteData.TriggerActions)
                {
                    try
                    {
                        TriggerAction(action, noteData);
                    }
                    catch (Exception ex)
                    {
                        _monitor.Log($"触发动作为 {action.Type} 时出错: {ex.Message}", LogLevel.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"触发纸条动作时出错: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 处理附件
        /// </summary>
        /// <param name="attachments">附件列表</param>
        private void HandleAttachments(List<Attachment> attachments)
        {
            try
            {
                foreach (var attachment in attachments)
                {
                    try
                    {
                        if (attachment.Type == "Item")
                        {
                            // 检查概率
                            if (attachment.Chance < 1.0f && new Random().NextDouble() > attachment.Chance)
                            {
                                _monitor.Log($"附件 {attachment.ItemId} 概率检查失败，跳过", LogLevel.Debug);
                                continue;
                            }

                            // 给予物品
                            Item item = ItemRegistry.Create(attachment.ItemId, attachment.Quantity);
                            if (item != null)
                            {
                                Game1.player.addItemByMenuIfNecessary(item);
                                _monitor.Log($"给予附件物品: {attachment.ItemId} x {attachment.Quantity}", LogLevel.Debug);
                            }
                            else
                            {
                                _monitor.Log($"无法创建附件物品: {attachment.ItemId}", LogLevel.Error);
                            }
                        }
                        else
                        {
                            _monitor.Log($"未知的附件类型: {attachment.Type}", LogLevel.Warn);
                        }
                    }
                    catch (Exception ex)
                    {
                        _monitor.Log($"处理附件时出错: {ex.Message}", LogLevel.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"处理附件时出错: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 触发单个动作
        /// </summary>
        /// <param name="action">动作数据</param>
        /// <param name="noteData">纸条数据</param>
        private void TriggerAction(TriggerAction action, CustomSecretNoteData noteData)
        {
            switch (action.Type)
            {
                case "ShowMessage":
                    ShowMessage(action);
                    break;
                case "UnlockContent":
                    UnlockContent(action);
                    break;
                case "GiveItem":
                    GiveItem(action);
                    break;
                case "ShowMap":
                    ShowMap(action);
                    break;
                case "AddQuest":
                    AddQuest(action);
                    break;
                default:
                    _monitor.Log($"未知的动作类型: {action.Type}", LogLevel.Warn);
                    break;
            }
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="action">动作数据</param>
        private void ShowMessage(TriggerAction action)
        {
            if (action.Parameters.TryGetValue("Message", out object messageObj) && messageObj is string message)
            {
                // 简化版 HUDMessage
                Game1.addHUDMessage(new HUDMessage(message));
                _monitor.Log($"显示消息: {message}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 解锁内容
        /// </summary>
        /// <param name="action">动作数据</param>
        private void UnlockContent(TriggerAction action)
        {
            if (action.Parameters.TryGetValue("ContentId", out object contentIdObj) && contentIdObj is string contentId)
            {
                // 这里可以实现解锁内容的逻辑
                // 例如解锁新的区域、配方等
                _monitor.Log($"解锁内容: {contentId}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 给予物品
        /// </summary>
        /// <param name="action">动作数据</param>
        private void GiveItem(TriggerAction action)
        {
            if (action.Parameters.TryGetValue("ItemId", out object itemIdObj) && itemIdObj is string itemId &&
                action.Parameters.TryGetValue("Quantity", out object quantityObj) && quantityObj is int quantity)
            {
                Item item = ItemRegistry.Create(itemId, quantity);
                if (item != null)
                {
                    Game1.player.addItemByMenuIfNecessary(item);
                    _monitor.Log($"给予物品: {itemId} x {quantity}", LogLevel.Debug);
                }
            }
        }

        /// <summary>
        /// 显示地图
        /// </summary>
        /// <param name="action">动作数据</param>
        private void ShowMap(TriggerAction action)
        {
            if (action.Parameters.TryGetValue("MapName", out object mapNameObj) && mapNameObj is string mapName)
            {
                // 这里可以实现显示地图的逻辑
                _monitor.Log($"显示地图: {mapName}", LogLevel.Debug);
            }
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="action">动作数据</param>
        private void AddQuest(TriggerAction action)
        {
            if (action.Parameters.TryGetValue("QuestId", out object questIdObj) && questIdObj is string questId)
            {
                // 这里可以实现添加任务的逻辑
                _monitor.Log($"添加任务: {questId}", LogLevel.Debug);
            }
        }
    }
}
