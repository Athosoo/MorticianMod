using MorticianMod.Interface;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace MorticianMod.AssetLoader
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

        public void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            
            // 注册右键点击事件
            _helper.Events.Input.ButtonPressed += OnButtonPressed;
            
            _monitor.Log("物品使用检测器初始化完成", LogLevel.Debug);
        }

        public void RegisterUseRule(ItemUseRule rule)
        {
            _useRules.Add(rule);
            _monitor.Log($"注册物品使用规则: 物品ID={rule.TargetItemId}, 触发方式={rule.TriggerType}", LogLevel.Debug);
        }

        public void UnregisterUseRule(int itemId)
        {
            _useRules.RemoveAll(rule => rule.TargetItemId == itemId);
            _monitor.Log($"注销物品使用规则: 物品ID={itemId}", LogLevel.Debug);
        }

        public void ClearAllRules()
        {
            _useRules.Clear();
            _monitor.Log("清空所有物品使用规则", LogLevel.Debug);
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
                
                // 检查是否为秘密纸条物品
                if (item.Name != null && item.Name.StartsWith("Athos.MorticianMod_"))
                {
                    _monitor.Log($"检测到秘密纸条物品使用: {item.Name}", LogLevel.Debug);
                    
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
                }
            }
        }
    }
}
