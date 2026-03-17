using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace MorticianMod.NoteLoaderModel;

public class ModData
{
    private const string NotesKey = "MorticianMod_NotesSeen";

    public static void AddNote(Farmer player, int noteId)
    {
        var notes = GetNotes(player);
        if (!notes.Contains(noteId))
        {
            notes.Add(noteId);
            player.modData[NotesKey] = string.Join(",", notes);
        }
    }

    public static bool HasNote(Farmer player, int noteId)
    {
        var notes = GetNotes(player);
        return notes.Contains(noteId);
    }

    public static List<int> GetNotes(Farmer player)
    {
        if (player.modData.TryGetValue(NotesKey, out string value))
        {
            return value.Split(',')
                .Where(s => !string.IsNullOrEmpty(s) && int.TryParse(s, out _))
                .Select(int.Parse)
                .ToList();
        }
        return new List<int>();
    }

    public static void RemoveNote(Farmer player, int noteId)
    {
        var notes = GetNotes(player);
        if (notes.Remove(noteId))
        {
            player.modData[NotesKey] = string.Join(",", notes);
        }
    }
}
