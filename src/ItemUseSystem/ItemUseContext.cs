using StardewValley;

namespace MorticianMod.ItemUseSystem
{
    /// <summary>
    /// 物品使用触发类型
    /// </summary>
    public enum UseTrigger
    {
        LeftClick,
        RightClick,
        UseButton
    }

    /// <summary>
    /// 物品使用上下文
    /// 包含物品使用时的所有相关信息
    /// </summary>
    public class ItemUseContext
    {
        /// <summary>
        /// 使用物品的玩家
        /// </summary>
        public Farmer Player { get; set; }

        /// <summary>
        /// 被使用的物品
        /// </summary>
        public Item Item { get; set; }

        /// <summary>
        /// 触发类型
        /// </summary>
        public UseTrigger TriggerType { get; set; }

        /// <summary>
        /// 是否阻止原始行为
        /// </summary>
        public bool SuppressOriginalBehavior { get; set; }

        /// <summary>
        /// 自定义数据
        /// </summary>
        public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();
    }
}
