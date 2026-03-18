using MorticianMod.src;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;

namespace MorticianMod.NoteLoaderModel
{
  /// <summary>
  /// 纸条物品管理器
  /// 负责纸条物品的创建和资产注册
  /// </summary>
  public class NoteItemManager
  {
    private IModHelper _helper;
    private IMonitor _monitor;
    private NoteDataManager _noteDataManager;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize(IModHelper helper, IMonitor monitor, NoteDataManager noteDataManager)
    {
      _helper = helper;
      _monitor = monitor;
      _noteDataManager = noteDataManager;

      // 注册测试命令
      RegisterTestCommand();

      _monitor.Log("纸条物品管理器初始化完成", LogLevel.Debug);
    }

    /// <summary>
    /// 注册测试命令
    /// </summary>
    private void RegisterTestCommand()
    {
      _helper.Events.Input.ButtonPressed += (sender, args) =>
      {
        // 确保游戏世界已加载
        if (!Context.IsWorldReady)
          return;

        // 按下 M 键给予第一个自定义纸条的物品
        if (args.Button == SButton.M)
        {
          var noteDatas = _noteDataManager.GetAllNoteDatas();
          if (noteDatas != null && noteDatas.Count > 0)
          {
            var firstNote = noteDatas[0];
            string itemId = $"{ModInfo._UniqueID}_{firstNote.Id}";
            Item item = ItemRegistry.Create(itemId, 1);
            item.Stack = 1;
            Game1.player.addItemByMenuIfNecessary(item);
            _monitor.Log($"已给予物品: {itemId}", LogLevel.Alert);
          }
          else
          {
            _monitor.Log("没有可用的秘密纸条数据", LogLevel.Warn);
          }
        }
      };
    }

    /// <summary>
    /// 处理资产请求
    /// </summary>
    public void HandleAssetRequest(AssetRequestedEventArgs e)
    {
      if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
      {
        e.Edit(asset =>
        {
          var objectsData = asset.AsDictionary<string, ObjectData>().Data;
          var noteDatas = _noteDataManager.GetAllNoteDatas();

          foreach (var note in noteDatas)
          {
            string itemId = $"{ModInfo._UniqueID}_{note.Id}";
            if (!objectsData.ContainsKey(itemId))
            {
              var newObjectData = new ObjectData
              {
                Name = itemId,
                DisplayName = "神秘纸条",
                Description = "上面写着神秘的信息",
                Type = "Quest",
                Price = 0,
                Category = -1,
                Edibility = -300,
                Texture = "Maps/springobjects",
                SpriteIndex = 79,
              };
              objectsData[itemId] = newObjectData;
            }
          }
        });
      }
    }
  }
}
