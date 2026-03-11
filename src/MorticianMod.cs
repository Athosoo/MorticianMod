using MorticianMod.AssetLoader;
using MorticianMod.Interface;
using StardewModdingAPI;
using StardewValley.Delegates;

namespace MorticianMod.src
{
    public class ModDebug
    {
        public static string _UniqueID { get; } = "Athos.MorticianMod";
        public static string _ShortID { get; } = "MorticianMod";
        public static string _Author { get; } = "Athos";
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
                        Monitor.Log($"加载器{loader.LoaderName}调用Register注册异常", LogLevel.Debug);
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
