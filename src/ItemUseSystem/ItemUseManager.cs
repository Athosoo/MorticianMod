using StardewModdingAPI;
using StardewValley;

namespace MorticianMod.ItemUseSystem
{
    /// <summary>
    /// 物品使用管理器
    /// 统一管理所有物品使用策略
    /// </summary>
    public class ItemUseManager
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private ItemUseDetector _detector;
        private List<IItemUseStrategy> _strategies = new List<IItemUseStrategy>();

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            _detector = new ItemUseDetector();
            _detector.Initialize(helper, monitor);
            _detector.OnItemUsed += OnItemUsed;

            _monitor.Log("物品使用管理器初始化完成", LogLevel.Debug);
        }

        /// <summary>
        /// 注册使用策略
        /// </summary>
        public void RegisterStrategy(IItemUseStrategy strategy)
        {
            strategy.Initialize(_helper, _monitor);
            _strategies.Add(strategy);
            _strategies = _strategies.OrderBy(s => s.Priority).ToList();

            _monitor.Log($"注册物品使用策略: {strategy.StrategyName}", LogLevel.Debug);
        }

        /// <summary>
        /// 注销使用策略
        /// </summary>
        public void UnregisterStrategy(string strategyName)
        {
            _strategies.RemoveAll(s => s.StrategyName == strategyName);
            _monitor.Log($"注销物品使用策略: {strategyName}", LogLevel.Debug);
        }

        /// <summary>
        /// 获取已注册的策略列表
        /// </summary>
        public IReadOnlyList<IItemUseStrategy> GetRegisteredStrategies()
        {
            return _strategies.AsReadOnly();
        }

        /// <summary>
        /// 物品使用事件处理
        /// </summary>
        private void OnItemUsed(Farmer player, Item item, UseTrigger trigger)
        {
            var context = new ItemUseContext
            {
                Player = player,
                Item = item,
                TriggerType = trigger,
                SuppressOriginalBehavior = false
            };

            // 查找能处理该物品的策略
            var strategy = _strategies.FirstOrDefault(s => s.CanHandle(item));

            if (strategy != null)
            {
                _monitor.Log($"使用策略 {strategy.StrategyName} 处理物品: {item.Name}", LogLevel.Debug);
                strategy.Handle(context);

                if (context.SuppressOriginalBehavior)
                {
                    _monitor.Log("阻止原始物品行为", LogLevel.Trace);
                }
            }
            else
            {
                _monitor.Log($"未找到能处理物品 {item.Name} 的策略", LogLevel.Trace);
            }
        }
    }
}
