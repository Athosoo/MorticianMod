using MorticianMod.Models;
using Newtonsoft.Json;
using System.Net.Mail;
using StardewValley.GameData.Objects;

public class CustomSecretNoteData
{
    [JsonProperty("Id")]
    public string Id { get; set; }

    [JsonProperty("Condition")]
    public ConditionData Condition { get; set; } 

    [JsonProperty("BackgroundImagePath")]
    public string BackgroundImagePath { get; set; }

    [JsonProperty("ContentImagePath")]
    public string ContentImagePath { get; set; }

    [JsonProperty("ContentText")]     
    public string ContentText { get; set; }

    [JsonProperty("Attachments")]
    public List<Attachment> Attachments { get; set; } = new();

    [JsonProperty("TriggerActions")]
    public List<TriggerAction> TriggerActions { get; set; } = new();

    [JsonIgnore]
    public int IntId { get; set; }
}

public class ConditionData
{
    [JsonProperty("Type")]
    public string Type { get; set; }

    [JsonProperty("Query")]      // 仅当 Type == "GameStateQuery" 时使用
    public string Query { get; set; }

    // 可根据需要添加其他条件类型的参数
}

public class TriggerAction
{
    [JsonProperty("Type")]
    public string Type { get; set; }

    [JsonProperty("Parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class NotesDatasWrapper
{
    public List<CustomSecretNoteData>? NoteDatas { get; set; }
}

