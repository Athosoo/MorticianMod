using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MorticianMod.Interface
{
    public interface IAssetLoader
    {
        /// <summary>
        /// 加载器的名称，用于标识
        /// </summary>
        string LoaderName { get; }

        /// <summary>
        /// 加载器的简短描述
        /// </summary>
        string LoaderDescription { get; }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="monitor"></param>
        void Register(IModHelper helper, IMonitor monitor);

        /// <summary>
        /// 实现资源请求时触发,提供待注册的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnAssetRequested(object sender, AssetRequestedEventArgs e);
    }
}
