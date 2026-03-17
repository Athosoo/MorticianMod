using MorticianMod.Interface;
using MorticianMod.Models;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MorticianMod.NoteLoaderModel
{
    /// <summary>
    /// 物品使用事件处理器
    /// 负责根据JSON配置执行相应的事件和附件给予
    /// </summary>
    public class ItemUseEventHandler
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private Dictionary<string, Action<Farmer, Item, UseTrigger>> _eventHandlers;
        private Dictionary<string, List<Attachment>> _itemAttachments;

        public void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _eventHandlers = new Dictionary<string, Action<Farmer, Item, UseTrigger>>();
            _itemAttachments = new Dictionary<string, List<Attachment>>();

            // 注册默认事件处理器
            RegisterDefaultHandlers();

            _monitor.Log("物品使用事件处理器初始化完成", LogLevel.Debug);
        }

        /// <summary>
        /// 注册默认事件处理器
        /// </summary>
        private void RegisterDefaultHandlers()
        {
            // 注册秘密纸条处理
            _eventHandlers["SecretNote"] = HandleSecretNote;

            // 可以在这里添加其他默认事件处理器
        }

        /// <summary>
        /// 从JSON加载配置
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        public void LoadConfig(string configPath)
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    _monitor.Log($"配置文件不存在: {configPath}", LogLevel.Warn);
                    return;
                }

                string json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<ItemUseConfig>(json);

                if (config != null)
                {
                    // 加载附件配置
                    if (config.Attachments != null)
                    {
                        foreach (var attachmentConfig in config.Attachments)
                        {
                            if (!_itemAttachments.ContainsKey(attachmentConfig.ItemId))
                            {
                                _itemAttachments[attachmentConfig.ItemId] = new List<Attachment>();
                            }
                            _itemAttachments[attachmentConfig.ItemId].AddRange(attachmentConfig.Attachments);
                        }
                        _monitor.Log($"加载了 {config.Attachments.Count} 个物品的附件配置", LogLevel.Debug);
                    }

                    // 这里可以添加其他配置加载逻辑
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"加载配置时出错: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 执行事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="player">玩家</param>
        /// <param name="item">物品</param>
        /// <param name="trigger">触发方式</param>
        public void ExecuteEvent(string eventName, Farmer player, Item item, UseTrigger trigger)
        {
            try
            {
                if (_eventHandlers.TryGetValue(eventName, out var handler))
                {
                    handler(player, item, trigger);
                    _monitor.Log($"执行事件: {eventName}", LogLevel.Debug);
                }
                else
                {
                    _monitor.Log($"未找到事件处理器: {eventName}", LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"执行事件时出错: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 处理秘密纸条
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="item">物品</param>
        /// <param name="trigger">触发方式</param>
        private void HandleSecretNote(Farmer player, Item item, UseTrigger trigger)
        {
            try
            {
                _monitor.Log($"处理秘密纸条: {item.Name}", LogLevel.Debug);
                
                // 这里可以添加秘密纸条的处理逻辑
                // 例如显示纸条内容、给予附件等
                
                // 处理附件
                HandleItemAttachments(item.Name, player);
            }
            catch (Exception ex)
            {
                _monitor.Log($"处理秘密纸条时出错: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 处理物品附件
        /// </summary>
        /// <param name="itemName">物品名称</param>
        /// <param name="player">玩家</param>
        private void HandleItemAttachments(string itemName, Farmer player)
        {
            try
            {
                if (_itemAttachments.TryGetValue(itemName, out var attachments))
                {
                    foreach (var attachment in attachments)
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
                                player.addItemByMenuIfNecessary(item);
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
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"处理物品附件时出错: {ex.Message}", LogLevel.Error);
            }
        }
    }

    /// <summary>
    /// 物品使用配置
    /// </summary>
    public class ItemUseConfig
    {
        /// <summary>
        /// 物品附件配置
        /// </summary>
        public List<ItemAttachmentConfig> Attachments { get; set; } = new List<ItemAttachmentConfig>();

        /// <summary>
        /// 事件配置
        /// </summary>
        public List<EventConfig> Events { get; set; } = new List<EventConfig>();
    }

    /// <summary>
    /// 物品附件配置
    /// </summary>
    public class ItemAttachmentConfig
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// 附件列表
        /// </summary>
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
    }

    /// <summary>
    /// 事件配置
    /// </summary>
    public class EventConfig
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// 物品ID
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// 触发方式
        /// </summary>
        public string TriggerType { get; set; }
    }
}