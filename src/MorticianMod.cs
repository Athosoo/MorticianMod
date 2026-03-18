using MorticianMod.AssetLoader;
using MorticianMod.Interface;
using MorticianMod.NoteLoaderModel.Core;
using StardewModdingAPI;

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
          loader.Register(helper, Monitor);
          Monitor.Log($"成功注册加载器: {loader.GetType().Name}", LogLevel.Debug);
        }
        catch (System.Exception ex)
        {
          Monitor.Log($"注册加载器失败: {ex.Message}", LogLevel.Error);
        }
      }
    }
  }
}
