using MorticianMod.Interface;
using MorticianMod.ItemUseSystem;
using MorticianMod.NoteLoaderModel.Core;
using MorticianMod.NoteLoaderModel.Events;
using MorticianMod.NoteLoaderModel.Managers;
using MorticianMod.NoteLoaderModel.Strategies;
using StardewModdingAPI;

namespace MorticianMod.NoteLoaderModel.Core
{
  /// <summary>
  /// 纸条系统核心管理类
  /// 统一管理所有纸条相关功能
  /// </summary>
  public class SecretNoteCore : IAssetLoader
  {
    public string LoaderName => "SecretNoteCore";
    public string LoaderDescription => "纸条系统统一核心管理类";

    private IModHelper _helper;
    private IMonitor _monitor;

    // 子模块
    private NoteDataManager _noteDataManager;
    private NoteItemManager _noteItemManager;
    private NoteActionExecutor _noteActionExecutor;
    private NoteCollectionManager _noteCollectionManager;
    private NoteUIManager _noteUIManager;

    // 物品使用系统
    private ItemUseManager _itemUseManager;
    private NoteUseStrategy _noteUseStrategy;
    private NoteEventHandler _noteEventHandler;

    public void Register(IModHelper helper, IMonitor monitor)
    {
      _helper = helper;
      _monitor = monitor;

      // 初始化静态引用
      ModEntry.Initialize(helper, monitor);

      _monitor.Log("初始化纸条系统核心...", LogLevel.Debug);

      // 初始化子模块
      _noteDataManager = new NoteDataManager();
      _noteActionExecutor = new NoteActionExecutor();
      _noteCollectionManager = new NoteCollectionManager();
      _noteItemManager = new NoteItemManager();
      _noteUIManager = new NoteUIManager();

      // 依赖注入
      _noteDataManager.Initialize(helper, monitor);
      _noteActionExecutor.Initialize(helper, monitor);
      _noteCollectionManager.Initialize(helper, monitor);
      _noteItemManager.Initialize(helper, monitor, _noteDataManager);
      _noteUIManager.Initialize(helper, monitor, _noteDataManager, _noteCollectionManager);

      // 初始化物品使用系统
      InitializeItemUseSystem();

      // 注册事件
      _helper.Events.Content.AssetRequested += OnAssetRequested;

      // 应用补丁
      _noteUIManager.ApplyPatches();

      _monitor.Log("纸条系统核心初始化完成", LogLevel.Debug);
    }

    /// <summary>
    /// 初始化物品使用系统
    /// </summary>
    private void InitializeItemUseSystem()
    {
      _monitor.Log("初始化物品使用系统...", LogLevel.Debug);

      // 创建物品使用管理器
      _itemUseManager = new ItemUseManager();
      _itemUseManager.Initialize(_helper, _monitor);

      // 创建纸条使用策略
      _noteUseStrategy = new NoteUseStrategy();
      _noteUseStrategy.Initialize(_helper, _monitor);

      // 创建事件处理器
      _noteEventHandler = new NoteEventHandler();
      _noteEventHandler.Initialize(
          _noteDataManager,
          _noteActionExecutor,
          _noteCollectionManager,
          _noteUIManager.GetNoteDisplayManager());

      // 注入事件处理器到策略
      _noteUseStrategy.InjectEventHandler(_noteEventHandler);

      // 注册策略
      _itemUseManager.RegisterStrategy(_noteUseStrategy);

      _monitor.Log("物品使用系统初始化完成", LogLevel.Debug);
    }

    public void OnAssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
    {
      // 委托给相应的管理器处理
      _noteDataManager.HandleAssetRequest(e);
      _noteItemManager.HandleAssetRequest(e);
    }

    /// <summary>
    /// 获取纸条数据管理器
    /// </summary>
    public NoteDataManager GetNoteDataManager() => _noteDataManager;

    /// <summary>
    /// 获取纸条物品管理器
    /// </summary>
    public NoteItemManager GetNoteItemManager() => _noteItemManager;

    /// <summary>
    /// 获取纸条动作执行器
    /// </summary>
    public NoteActionExecutor GetNoteActionExecutor() => _noteActionExecutor;

    /// <summary>
    /// 获取纸条收集管理器
    /// </summary>
    public NoteCollectionManager GetNoteCollectionManager() => _noteCollectionManager;

    /// <summary>
    /// 获取纸条界面管理器
    /// </summary>
    public NoteUIManager GetNoteUIManager() => _noteUIManager;
  }
}
