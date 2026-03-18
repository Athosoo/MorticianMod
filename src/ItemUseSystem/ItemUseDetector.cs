using StardewModdingAPI;
using StardewValley;

namespace MorticianMod.ItemUseSystem
{
    /// <summary>
    /// 物品使用检测器
    /// 负责检测玩家使用物品的事件并触发回调
    /// </summary>
    public class ItemUseDetector
    {
        private IModHelper _helper;
        private IMonitor _monitor;

        /// <summary>
        /// 物品使用事件
        /// </summary>
        public event Action<Farmer, Item, UseTrigger> OnItemUsed;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            // 注册右键点击事件
            _helper.Events.Input.ButtonPressed += OnButtonPressed;

            _monitor.Log("物品使用检测器初始化完成", LogLevel.Debug);
        }

        /// <summary>
        /// 按钮按下事件处理
        /// </summary>
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

                // 触发物品使用事件
                OnItemUsed?.Invoke(Game1.player, item, UseTrigger.RightClick);
            }
        }
    }
}
