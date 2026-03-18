using StardewModdingAPI;

namespace MorticianMod.NoteLoaderModel
{
    /// <summary>
    /// 模组入口静态引用
    /// </summary>
    public static class ModEntry
    {
        public static IModHelper Helper { get; set; }
        public static IMonitor Monitor { get; set; }

        /// <summary>
        /// 初始化静态引用
        /// </summary>
        /// <param name="helper">ModHelper实例</param>
        /// <param name="monitor">Monitor实例</param>
        public static void Initialize(IModHelper helper, IMonitor monitor)
        {
            Helper = helper;
            Monitor = monitor;
        }
    }
}
