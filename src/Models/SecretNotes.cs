using Newtonsoft.Json;

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

public class Attachment
{
    [JsonProperty("Type")]
    public string Type { get; set; }

    [JsonProperty("ItemId")]
    public string ItemId { get; set; }

    [JsonProperty("Quantity")]
    public int Quantity { get; set; }

    [JsonProperty("Chance")]
    public float Chance { get; set; } = 1.0f; // 默认100%概率
}

public class TextureData
{
    [JsonProperty("Path")]
    public string Path { get; set; }

    [JsonProperty("Width")]
    public int Width { get; set; }

    [JsonProperty("Height")]
    public int Height { get; set; }

    [JsonProperty("X")]
    public int X { get; set; }

    [JsonProperty("Y")]
    public int Y { get; set; }
}

