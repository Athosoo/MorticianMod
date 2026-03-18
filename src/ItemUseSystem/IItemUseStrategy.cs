using StardewModdingAPI;
using StardewValley;

namespace MorticianMod.ItemUseSystem
{
  /// <summary>
  /// 物品使用策略接口
  /// 所有物品使用处理器都需要实现此接口
  /// </summary>
  public interface IItemUseStrategy
  {
    /// <summary>
    /// 策略名称
    /// </summary>
    string StrategyName { get; }

    /// <summary>
    /// 策略优先级（数字越小优先级越高）
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// 判断是否能处理该物品
    /// </summary>
    bool CanHandle(Item item);

    /// <summary>
    /// 处理物品使用
    /// </summary>
    void Handle(ItemUseContext context);

    /// <summary>
    /// 初始化策略
    /// </summary>
    void Initialize(IModHelper helper, IMonitor monitor);
  }
}
