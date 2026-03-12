using System.Text.Json.Serialization;

namespace MorticianMod.Models
{
    /// <summary>
    /// 表示一个自定义家具的数据，对应游戏中的家具定义。
    /// </summary>
    public class FurnitureData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        /// <summary>
        /// 家具内部名称（也是默认英文显示名称）。
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// 家具类型，参见 <see cref="FurnitureType"/> 枚举。
        /// </summary>
        [JsonPropertyName("furniture_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FurnitureType FurnitureType { get; set; }

        /// <summary>
        /// 家具在图集中的尺寸（以瓦片为单位），格式如 "1 2" 或 "-1"（使用类型默认值）。
        /// </summary>
        [JsonPropertyName("tilesheet_size")]
        public string TilesheetSize { get; set; }

        /// <summary>
        /// 家具放置后的碰撞箱尺寸（以瓦片为单位），格式同 <see cref="TilesheetSize"/>。
        /// </summary>
        [JsonPropertyName("bounding_box_size")]
        public string BoundingBoxSize { get; set; }

        /// <summary>
        /// 家具可旋转的方向数（1、2 或 4）。
        /// </summary>
        [JsonPropertyName("rotations")]
        public int Rotations { get; set; }

        /// <summary>
        /// 商店购买价格。
        /// </summary>
        [JsonPropertyName("price")]
        public int Price { get; set; }

        /// <summary>
        /// 放置限制：-1（默认，按类型）、0（仅室内）、1（仅室外）、2（室内外均可）。
        /// </summary>
        [JsonPropertyName("placement_restriction")]
        public int PlacementRestriction { get; set; }

        /// <summary>
        /// 本地化显示名称（可使用令牌字符串）。
        /// </summary>
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        /// <summary>
        /// 在图集中的瓦片索引。
        /// </summary>
        [JsonPropertyName("sprite_index")]
        public int SpriteIndex { get; set; }

        /// <summary>
        /// 可选，自定义纹理资产名称，默认使用 "TileSheets/furniture"。
        /// </summary>
        [JsonPropertyName("texture")]
        public string? Texture { get; set; }

        /// <summary>
        /// 可选，是否禁止在随机商店库存和家具目录中出现，默认 false。
        /// </summary>
        [JsonPropertyName("off_limits_for_random_sale")]
        public bool? OffLimitsForRandomSale { get; set; }

        /// <summary>
        /// 可选，空格分隔的上下文标签列表。
        /// </summary>
        [JsonPropertyName("context_tags")]
        public string? ContextTags { get; set; }
    }

    /// <summary>
    /// 家具类型枚举，与游戏中的类型字符串对应。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FurnitureType
    {
        [JsonPropertyName("chair")] chair,
        [JsonPropertyName("bench")] bench,
        [JsonPropertyName("couch")] couch,
        [JsonPropertyName("armchair")] armchair,
        [JsonPropertyName("dresser")] dresser,
        [JsonPropertyName("long table")] long_table,
        [JsonPropertyName("painting")] painting,
        [JsonPropertyName("lamp")] lamp,
        [JsonPropertyName("decor")] decor,
        [JsonPropertyName("other")] other,
        [JsonPropertyName("bookcase")] bookcase,
        [JsonPropertyName("table")] table,
        [JsonPropertyName("rug")] rug,
        [JsonPropertyName("window")] window,
        [JsonPropertyName("fireplace")] fireplace,
        [JsonPropertyName("bed")] bed,
        [JsonPropertyName("bed double")] bed_double,
        [JsonPropertyName("bed child")] bed_child,
        [JsonPropertyName("torch")] torch,
        [JsonPropertyName("sconce")] sconce,
        [JsonPropertyName("randomized_plant")] randomized_plant,
        [JsonPropertyName("fishtank")] fishtank,
    }

    public class FurnitureDataWrapper
    {
        public List<FurnitureData> furnitureDatas { get; set; }
    }
}