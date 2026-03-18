using StardewModdingAPI;

namespace MorticianMod.Interface;

public interface INamedManager
{
    string GetManagerName { get; }
    string GetManagerDescription { get; }
    public void Initialize(IModHelper helper, IMonitor monitor){}
}