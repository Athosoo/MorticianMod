using MorticianMod.ItemUseSystem;
using MorticianMod.NoteLoaderModel.Events;
using MorticianMod.src;
using StardewModdingAPI;
using StardewValley;

namespace MorticianMod.NoteLoaderModel.Strategies
{
  /// <summary>
  /// 纸条使用策略
  /// 只负责路由到事件处理器
  /// </summary>
  public class NoteUseStrategy : IItemUseStrategy
  {
    private NoteEventHandler _eventHandler;

    public string StrategyName => "NoteUseStrategy";
    public int Priority => 100;

    public void Initialize(IModHelper helper, IMonitor monitor)
    {
    }

    /// <summary>
    /// 注入事件处理器
    /// </summary>
    public void InjectEventHandler(NoteEventHandler eventHandler)
    {
      _eventHandler = eventHandler;
    }

    public bool CanHandle(Item item)
    {
      return item?.Name?.StartsWith($"{ModInfo._UniqueID}_") == true;
    }

    public void Handle(ItemUseContext context)
    {
      _eventHandler?.HandleNoteUsed(context.Player, context.Item);
      context.SuppressOriginalBehavior = true;
    }
  }
}
