using StardewModdingAPI;
using StardewValley;

namespace MorticianMod.Interface
{
    /// <summary>
    /// 物品使用检测的核心接口
    /// </summary>
    public interface IItemUseDetector
    {
        /// <summary>
        /// 初始化检测（注册SMAPI事件）
        /// </summary>
        /// <param name="helper">SMAPI的Helper实例</param>
        /// <param name="monitor">日志输出实例</param>
        void Initialize(IModHelper helper, IMonitor monitor);

        /// <summary>
        /// 注册物品使用检测规则
        /// </summary>
        /// <param name="rule">检测规则（包含物品筛选+处理逻辑）</param>
        void RegisterUseRule(ItemUseRule rule);

        /// <summary>
        /// 注销指定物品的检测规则
        /// </summary>
        /// <param name="itemId">物品ID（如木头=1）</param>
        void UnregisterUseRule(int itemId);

        /// <summary>
        /// 清空所有检测规则
        /// </summary>
        void ClearAllRules();
    }

    /// <summary>
    /// 物品使用检测规则（封装筛选条件+处理逻辑）
    /// </summary>
    public class ItemUseRule
    {
        /// <summary>
        /// 物品ID（筛选条件：仅检测该ID的物品）
        /// </summary>
        public int TargetItemId { get; set; }

        /// <summary>
        /// 触发方式（右键/左键/任意）
        /// </summary>
        public UseTrigger TriggerType { get; set; }

        /// <summary>
        /// 是否阻止原生使用行为（如禁用物品默认右键功能）
        /// </summary>
        public bool SuppressOriginalBehavior { get; set; } = false;

        /// <summary>
        /// 物品使用时的处理逻辑
        /// </summary>
        /// <param name="player">使用物品的玩家</param>
        /// <param name="item">被使用的物品</param>
        /// <param name="trigger">触发方式</param>
        public delegate void ItemUseHandler(Farmer player, Item item, UseTrigger trigger);

        /// <summary>
        /// 处理逻辑实例
        /// </summary>
        public ItemUseHandler OnItemUsed { get; set; }
    }

    /// <summary>
    /// 物品使用触发方式（枚举）
    /// </summary>
    public enum UseTrigger
    {
        /// <summary>右键使用</summary>
        RightClick,
        /// <summary>左键使用</summary>
        LeftClick,
        /// <summary>任意方式</summary>
        Any
    }
}