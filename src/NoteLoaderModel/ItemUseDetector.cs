using MorticianMod.Interface;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.IO;

namespace MorticianMod.NoteLoaderModel
{
  /// <summary>
  /// 物品使用检测器实现
  /// 用于检测玩家右键点击秘密纸条等物品
  /// </summary>
  public class ItemUseDetector : IItemUseDetector
  {
    private IModHelper _helper;
    private IMonitor _monitor;
    private List<ItemUseRule> _useRules = new List<ItemUseRule>();
    private ItemUseEventHandler _eventHandler;
    private Dictionary<string, string> _itemEventMap = new Dictionary<string, string>();

    public void Initialize(IModHelper helper, IMonitor monitor)
    {
      _helper = helper;
      _monitor = monitor;

      // 初始化事件处理器
      _eventHandler = new ItemUseEventHandler();
      _eventHandler.Initialize(helper, monitor);

      // 加载配置
      LoadConfig();

      // 注册右键点击事件
      _helper.Events.Input.ButtonPressed += OnButtonPressed;

      _monitor.Log($"{nameof(ItemUseDetector)}::物品使用检测器初始化完成", LogLevel.Info);
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    private void LoadConfig()
    {
      try
      {
        string configPath = Path.Combine(_helper.DirectoryPath, "assets", "config", "item_use_config.json");
        _eventHandler.LoadConfig(configPath);

        // 这里可以加载物品事件映射
        // 例如从JSON配置中读取物品ID到事件名称的映射
        _itemEventMap["Athos.MorticianMod_note1"] = "SecretNote";
        // 可以添加更多映射
      }
      catch (Exception ex)
      {
        _monitor.Log($"{nameof(ItemUseDetector)}::加载配置时出错: {ex.Message}", LogLevel.Error);
      }
    }

    public void RegisterUseRule(ItemUseRule rule)
    {
      _useRules.Add(rule);
      _monitor.Log($"{nameof(ItemUseDetector)}::注册物品使用规则: 物品ID={rule.TargetItemId}, 触发方式={rule.TriggerType}", LogLevel.Info);
    }

    public void UnregisterUseRule(int itemId)
    {
      _useRules.RemoveAll(rule => rule.TargetItemId == itemId);
      _monitor.Log($"{nameof(ItemUseDetector)}::注销物品使用规则: 物品ID={itemId}", LogLevel.Info);
    }

    public void ClearAllRules()
    {
      _useRules.Clear();
      _monitor.Log($"{nameof(ItemUseDetector)}::清空所有物品使用规则", LogLevel.Info);
    }

    private void OnButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
    {
      // 确保游戏世界已加载
      if (!Context.IsWorldReady)
        return;

      // 确保玩家持有物品
      if (Game1.player.CurrentItem == null)
        return;

      // 检查是否为右键点击
      if (e.Button == SButton.MouseRight)
      {
        Item item = Game1.player.CurrentItem;
        string itemName = item.Name;

        // 检查是否为秘密纸条物品
        if (itemName != null && itemName.StartsWith("Athos.MorticianMod_"))
        {
          _monitor.Log($"{nameof(ItemUseDetector)}::检测到秘密纸条物品使用: {itemName}", LogLevel.Info);

          // 执行处理逻辑
          foreach (var rule in _useRules)
          {
            // 这里使用名称匹配而不是ID匹配
            if (rule.OnItemUsed != null)
            {
              rule.OnItemUsed.Invoke(Game1.player, item, UseTrigger.RightClick);
              break;
            }
          }

          // 使用事件处理器执行事件
          if (_itemEventMap.TryGetValue(itemName, out var eventName))
          {
            _eventHandler.ExecuteEvent(eventName, Game1.player, item, UseTrigger.RightClick);
          }
          else
          {
            _monitor.Log($"{nameof(ItemUseDetector)}::未找到物品 {itemName} 对应的事件", LogLevel.Warn);
          }

          // 从玩家背包中删除该物品实例
          if (Game1.player.CurrentItem != null && Game1.player.CurrentItem.Name == itemName)
          {
            Item currentItem = Game1.player.CurrentItem;
            if (currentItem.Stack > 1)
            {
              currentItem.Stack--;
              _monitor.Log($"{nameof(ItemUseDetector)}::减少秘密纸条物品数量: {itemName}, 剩余数量: {currentItem.Stack}", LogLevel.Info);
            }
            else
            {
              // 使用 removeItemFromInventory 方法移除物品
              Game1.player.removeItemFromInventory(currentItem);
              _monitor.Log($"{nameof(ItemUseDetector)}::从背包中删除秘密纸条物品: {itemName}", LogLevel.Info);
            }
          }
        }
      }
    }
  }
}
