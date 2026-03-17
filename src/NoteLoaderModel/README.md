# NoteLoaderModel 使用说明

## 功能介绍

NoteLoaderModel 是一个用于星露谷物语模组开发的秘密纸条加载和管理模块，提供了以下功能：

- 从 JSON 配置文件加载自定义秘密纸条
- 显示秘密纸条内容
- 处理秘密纸条的触发动作
- 将秘密纸条添加到游戏收集品列表
- 检测物品使用并触发相应事件

## 核心类

### SecretNoteAdder

主要入口类，封装了所有秘密纸条相关的功能，提供统一的接口。

#### 方法说明

- **Initialize(IModHelper helper, IMonitor monitor, string configPath = null)**
  - 初始化秘密纸条添加器
  - 参数：
    - `helper`: SMAPI 的 Helper 实例
    - `monitor`: 日志输出实例
    - `configPath`: 配置文件路径（可选）

- **LoadConfig(string path = null)**
  - 加载秘密纸条配置
  - 参数：
    - `path`: 配置文件路径（可选）

- **AddSecretNote(CustomSecretNoteData noteData)**
  - 添加自定义秘密纸条
  - 参数：
    - `noteData`: 秘密纸条数据

- **ShowSecretNote(CustomSecretNoteData noteData)**
  - 显示秘密纸条
  - 参数：
    - `noteData`: 秘密纸条数据

- **TriggerNoteActions(CustomSecretNoteData noteData)**
  - 触发纸条动作
  - 参数：
    - `noteData`: 秘密纸条数据

- **AddNoteToCollection(object noteId)**
  - 将秘密纸条添加到游戏收集品列表
  - 参数：
    - `noteId`: 纸条 ID（支持 int 或 string 类型）

- **RegisterUseRule(ItemUseRule rule)**
  - 注册物品使用规则
  - 参数：
    - `rule`: 物品使用规则

- **UnregisterUseRule(int itemId)**
  - 注销物品使用规则
  - 参数：
    - `itemId`: 物品 ID

- **ClearAllRules()**
  - 清空所有物品使用规则

- **ExploreCollectionSystem()**
  - 探索游戏收集品系统（用于调试）

## 配置文件格式

配置文件为 JSON 格式，默认路径为 `assets/notes/secret_notes.json`。

### 示例配置

```json
{
  "NoteDatas": [
    {
      "Id": "example_note",
      "Condition": null,
      "BackgroundImagePath": "",
      "ContentImagePath": "",
      "ContentText": "这是一个示例秘密纸条",
      "Attachments": [],
      "TriggerActions": []
    }
  ]
}
```

### 配置字段说明

- **Id**: 秘密纸条的唯一标识符
- **Condition**: 触发条件（可选）
- **BackgroundImagePath**: 背景图片路径（可选）
- **ContentImagePath**: 内容图片路径（可选）
- **ContentText**: 纸条内容文本
- **Attachments**: 附件列表（可选）
- **TriggerActions**: 触发动作列表（可选）

## 使用示例

### 基本用法

```csharp
using MorticianMod.NoteLoaderModel;
using StardewModdingAPI;

public class MyMod : Mod
{
    private SecretNoteAdder _noteAdder;

    public override void Entry(IModHelper helper)
    {
        // 初始化秘密纸条添加器
        _noteAdder = new SecretNoteAdder();
        _noteAdder.Initialize(helper, Monitor);

        // 加载配置
        _noteAdder.LoadConfig();

        // 注册物品使用规则
        var rule = new ItemUseRule
        {
            ItemId = 123, // 物品ID
            OnUse = (player, item) =>
            {
                // 当物品被使用时的处理逻辑
                Monitor.Log("物品被使用了！", LogLevel.Info);
            }
        };
        _noteAdder.RegisterUseRule(rule);
    }
}
```

### 添加秘密纸条到收集品列表

```csharp
// 添加整数ID的纸条
_noteAdder.AddNoteToCollection(1);

// 添加字符串ID的纸条
_noteAdder.AddNoteToCollection("custom_note");
```

## 注意事项

1. **配置文件路径**：默认配置文件路径为 `assets/notes/secret_notes.json`，如果文件不存在，会自动创建默认配置。

2. **纸条ID类型**：
   - 如果游戏的 `secretNotesSeen` 字段是 `HashSet<int>` 类型，字符串ID会尝试转换为整数
   - 如果游戏的 `secretNotesSeen` 字段是 `HashSet<string>` 类型，整数ID会转换为字符串

3. **错误处理**：模块包含详细的错误处理和日志记录，便于调试和问题排查。

4. **兼容性**：通过反射访问游戏内部字段，可能会受到游戏版本更新的影响。

## 日志级别

模块使用以下日志级别：

- **Info**：信息性日志，如初始化完成、加载配置等
- **Warn**：警告性日志，如配置文件不存在、无法添加到收集品列表等
- **Error**：错误性日志，如加载配置失败、反射访问出错等

## 依赖项

- SMAPI (Stardew Valley Modding API)
- Stardew Valley
- System.Text.Json

## 故障排除

1. **配置文件加载失败**：检查配置文件格式是否正确，确保 JSON 格式合法。

2. **秘密纸条不显示**：检查纸条数据是否正确，确保 `ContentText` 或 `ContentImagePath` 已设置。

3. **无法添加到收集品列表**：检查游戏版本，可能需要更新反射逻辑以适应游戏变化。

4. **物品使用检测不工作**：确保已正确注册物品使用规则，且物品ID匹配。

## 版本历史

- **1.0.0**：初始版本，实现基本功能
- **1.0.1**：添加收集品列表集成
- **1.0.2**：优化错误处理和日志记录
