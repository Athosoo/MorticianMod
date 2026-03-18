using MorticianMod.NoteLoaderModel.Core;
using MorticianMod.NoteLoaderModel.Managers;
using MorticianMod.src;
using StardewModdingAPI;
using StardewValley;

namespace MorticianMod.NoteLoaderModel.Events
{
  /// <summary>
  /// 纸条事件处理器
  /// 集中处理所有纸条相关事件
  /// </summary>
  public class NoteEventHandler
  {
    private NoteDataManager _dataManager;
    private NoteActionExecutor _actionExecutor;
    private NoteCollectionManager _collectionManager;
    private NoteDisplayManager _displayManager;

    public void Initialize(
        NoteDataManager dataManager,
        NoteActionExecutor actionExecutor,
        NoteCollectionManager collectionManager,
        NoteDisplayManager displayManager)
    {
      _dataManager = dataManager;
      _actionExecutor = actionExecutor;
      _collectionManager = collectionManager;
      _displayManager = displayManager;
    }

    /// <summary>
    /// 处理纸条使用事件
    /// </summary>
    public void HandleNoteUsed(Farmer player, Item item)
    {
      try
      {
        string noteId = ExtractNoteId(item);
        var noteData = _dataManager.GetNoteDataById(noteId);

        if (noteData == null)
        {
          ModEntry.Monitor.Log($"未找到纸条数据: {noteId}", LogLevel.Warn);
          return;
        }

        ModEntry.Monitor.Log($"处理纸条使用: ID={noteData.Id}, IntId={noteData.IntId}", LogLevel.Debug);

        _displayManager.ShowSecretNote(noteData);
        _actionExecutor.TriggerNoteActions(noteData);
        _collectionManager.AddNote(player, noteData.IntId);

        player.removeItemFromInventory(item);
        ModEntry.Monitor.Log($"纸条 {noteData.Id} 处理完成，已从背包移除", LogLevel.Debug);
      }
      catch (System.Exception ex)
      {
        ModEntry.Monitor.Log($"处理纸条事件时出错: {ex.Message}", LogLevel.Error);
      }
    }

    private string ExtractNoteId(Item item)
    {
      return item.Name.Replace($"{ModInfo._UniqueID}_", "");
    }
  }
}
