using MorticianMod.NoteLoaderModel;
using MorticianMod.AssetLoader;
using MorticianMod.Interface;
using StardewModdingAPI;
using StardewValley.Delegates;

namespace MorticianMod.src
{
  public class ModInfo
  {
    public static string _UniqueID = "Athos.MorticianMod";
    public static string _ShortID = "MorticianMod";
    public static string _Author = "Athos";
  }
  public class MorticianMod : Mod
  {
    public override void Entry(IModHelper helper)
    {
      Monitor.Log("入殓师模组访问成功!", LogLevel.Debug);
      // 初始化所有加载器
      var assetLoaders = new List<IAssetLoader>
            {
                new FurnitureLoader(),
                new SecretNoteCore(), // 新的纸条系统核心
                // 以后添加其他加载器
            };

      // 注册所有加载器
      foreach (var loader in assetLoaders)
      {
        try
        {
          try
          {
            loader.Register(helper, Monitor);
            Monitor.Log($"加载器{loader.LoaderName}调用Register注册成功", LogLevel.Debug);
          }
          catch (Exception e)
          {
            Monitor.Log($"加载器{loader.LoaderName}调用Register注册异常:{e.Message}", LogLevel.Warn);
          }
        }
        catch
        {
          Monitor.Log($"尝试加载所有加载器异常", LogLevel.Debug);
        }

      }


    }
  }
}
