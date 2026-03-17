using MorticianMod.Interface;
using MorticianMod.Models;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MorticianMod.NoteLoaderModel
{
    /// <summary>
    /// SecretNoteAdder::自定义秘密纸条添加器
    /// 封装现有的秘密纸条相关功能，提供统一的接口
    /// </summary>
    public class SecretNoteAdder
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private NotesLoader _notesLoader;
        private ItemUseDetector _itemUseDetector;
        private NoteDisplayManager _noteDisplayManager;
        private NoteActionManager _noteActionManager;
        private string _configPath;

        /// <summary>
        /// SecretNoteAdder::初始化秘密纸条添加器
        /// </summary>
        /// <param name="helper">SMAPI的Helper实例</param>
        /// <param name="monitor">日志输出实例</param>
        /// <param name="configPath">配置文件路径</param>
        public void Initialize(IModHelper helper, IMonitor monitor, string configPath = null)
        {
            _helper = helper;
            _monitor = monitor;
            _configPath = configPath ?? Path.Combine(_helper.DirectoryPath, "assets", "notes", "secret_notes.json");

            // 初始化各个组件
            _notesLoader = new NotesLoader();
            _itemUseDetector = new ItemUseDetector();
            _noteDisplayManager = new NoteDisplayManager();
            _noteActionManager = new NoteActionManager();

            // 注册组件
            _notesLoader.Register(_helper, _monitor);
            _itemUseDetector.Initialize(_helper, _monitor);
            _noteDisplayManager.Initialize(_helper, _monitor);
            _noteActionManager.Initialize(_helper, _monitor);

            _monitor.Log($"{nameof(SecretNoteAdder)}::秘密纸条添加器初始化完成", LogLevel.Info);
        }

        /// <summary>
        /// SecretNoteAdder::加载秘密纸条配置
        /// </summary>
        /// <param name="path">配置文件路径</param>
        public void LoadConfig(string path = null)
        {
            try
            {
                string configPath = path ?? _configPath;
                if (!File.Exists(configPath))
                {
                    _monitor.Log($"{nameof(SecretNoteAdder)}::配置文件不存在: {configPath}", LogLevel.Warn);
                    // 创建默认配置文件
                    CreateDefaultConfig(configPath);
                    return;
                }

                _monitor.Log($"{nameof(SecretNoteAdder)}::加载秘密纸条配置: {configPath}", LogLevel.Info);
                
                // 读取配置文件
                string json = File.ReadAllText(configPath);
                
                // 反序列化配置
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                
                var wrapper = JsonSerializer.Deserialize<NotesDatasWrapper>(json, options);
                
                if (wrapper == null || wrapper.NoteDatas == null)
                {
                    _monitor.Log($"{nameof(SecretNoteAdder)}::配置文件格式错误，未找到NoteDatas", LogLevel.Error);
                    return;
                }
                
                _monitor.Log($"{nameof(SecretNoteAdder)}::成功加载 {wrapper.NoteDatas.Count} 个秘密纸条", LogLevel.Info);
                
                // 配置加载由NotesLoader处理
            }
            catch (Exception ex)
            {
                _monitor.Log($"{nameof(SecretNoteAdder)}::加载配置时出错: {ex.Message}", LogLevel.Error);
                _monitor.Log($"{nameof(SecretNoteAdder)}::堆栈跟踪: {ex.StackTrace}", LogLevel.Error);
            }
        }

        /// <summary>
        /// SecretNoteAdder::创建默认配置文件
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        private void CreateDefaultConfig(string configPath)
        {
            try
            {
                // 确保目录存在
                string directory = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 创建默认配置
                var defaultConfig = new NotesDatasWrapper
                {
                    NoteDatas = new List<CustomSecretNoteData>
                    {
                        new CustomSecretNoteData
                        {
                            Id = "example_note",
                            Condition = null,
                            BackgroundImagePath = "",
                            ContentImagePath = "",
                            ContentText = "这是一个示例秘密纸条",
                            Attachments = new List<Attachment>(),
                            TriggerActions = new List<TriggerAction>()
                        }
                    }
                };
                
                // 序列化并保存
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(defaultConfig, options);
                File.WriteAllText(configPath, json);
                
                _monitor.Log($"{nameof(SecretNoteAdder)}::创建默认配置文件: {configPath}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"{nameof(SecretNoteAdder)}::创建默认配置文件时出错: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// SecretNoteAdder::添加自定义秘密纸条
        /// </summary>
        /// <param name="noteData">秘密纸条数据</param>
        public void AddSecretNote(CustomSecretNoteData noteData)
        {
            try
            {
                if (noteData == null)
                {
                    _monitor.Log($"{nameof(SecretNoteAdder)}::秘密纸条数据为null，无法添加", LogLevel.Warn);
                    return;
                }

                _monitor.Log($"{nameof(SecretNoteAdder)}::添加自定义秘密纸条: {noteData.Id}", LogLevel.Info);
                // 纸条添加由NotesLoader处理
            }
            catch (Exception ex)
            {
                _monitor.Log($"{nameof(SecretNoteAdder)}::添加秘密纸条时出错: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// SecretNoteAdder::显示秘密纸条
        /// </summary>
        /// <param name="noteData">秘密纸条数据</param>
        public void ShowSecretNote(CustomSecretNoteData noteData)
        {
            _noteDisplayManager.ShowSecretNote(noteData);
        }

        /// <summary>
        /// SecretNoteAdder::触发纸条动作
        /// </summary>
        /// <param name="noteData">秘密纸条数据</param>
        public void TriggerNoteActions(CustomSecretNoteData noteData)
        {
            _noteActionManager.TriggerNoteActions(noteData);
        }

        /// <summary>
        /// SecretNoteAdder::注册物品使用规则
        /// </summary>
        /// <param name="rule">物品使用规则</param>
        public void RegisterUseRule(ItemUseRule rule)
        {
            _itemUseDetector.RegisterUseRule(rule);
        }

        /// <summary>
        /// SecretNoteAdder::注销物品使用规则
        /// </summary>
        /// <param name="itemId">物品ID</param>
        public void UnregisterUseRule(int itemId)
        {
            _itemUseDetector.UnregisterUseRule(itemId);
        }

        /// <summary>
        /// SecretNoteAdder::清空所有物品使用规则
        /// </summary>
        public void ClearAllRules()
        {
            _itemUseDetector.ClearAllRules();
        }

        /// <summary>
        /// SecretNoteAdder::探索游戏收集品系统
        /// </summary>
        public void ExploreCollectionSystem()
        {
            try
            {
                _monitor.Log($"{nameof(SecretNoteAdder)}::开始探索游戏收集品系统...", LogLevel.Info);
                
                // 检查Player类中与收集品相关的字段
                var playerType = Game1.player.GetType();
                var fields = playerType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                _monitor.Log($"{nameof(SecretNoteAdder)}::Player类共有 {fields.Length} 个字段", LogLevel.Info);
                
                // 搜索与收集品相关的字段
                foreach (var field in fields)
                {
                    if (field.Name.Contains("collect") || field.Name.Contains("museum") || field.Name.Contains("secretNote"))
                    {
                        _monitor.Log($"{nameof(SecretNoteAdder)}::找到相关字段: {field.Name} - {field.FieldType.Name}", LogLevel.Info);
                    }
                }
                
                // 检查Player类中与收集品相关的方法
                var methods = playerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                _monitor.Log($"{nameof(SecretNoteAdder)}::Player类共有 {methods.Length} 个方法", LogLevel.Info);
                
                // 搜索与收集品相关的方法
                foreach (var method in methods)
                {
                    if (method.Name.Contains("collect") || method.Name.Contains("museum") || method.Name.Contains("secretNote"))
                    {
                        _monitor.Log($"{nameof(SecretNoteAdder)}::找到相关方法: {method.Name}", LogLevel.Info);
                    }
                }
                
                // 检查Game1类中与收集品相关的字段和方法
                var game1Type = typeof(Game1);
                var game1Fields = game1Type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                
                _monitor.Log($"{nameof(SecretNoteAdder)}::Game1类共有 {game1Fields.Length} 个静态字段", LogLevel.Info);
                
                // 搜索与收集品相关的静态字段
                foreach (var field in game1Fields)
                {
                    if (field.Name.Contains("collect") || field.Name.Contains("museum") || field.Name.Contains("secretNote"))
                    {
                        _monitor.Log($"{nameof(SecretNoteAdder)}::找到相关静态字段: {field.Name} - {field.FieldType.Name}", LogLevel.Info);
                    }
                }
                
                _monitor.Log($"{nameof(SecretNoteAdder)}::探索游戏收集品系统完成", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"{nameof(SecretNoteAdder)}::探索游戏收集品系统时出错: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// SecretNoteAdder::将秘密纸条添加到游戏收集品列表
        /// </summary>
        /// <param name="noteId">纸条ID</param>
        public void AddNoteToCollection(object noteId)
        {
            try
            {
                _monitor.Log($"{nameof(SecretNoteAdder)}::尝试将秘密纸条添加到收集品列表: ID={noteId}", LogLevel.Info);
                
                // 使用反射访问和修改 secretNotesSeen 字段
                var playerType = Game1.player.GetType();
                var secretNotesSeenField = playerType.GetField("secretNotesSeen", BindingFlags.Instance | BindingFlags.NonPublic);
                
                if (secretNotesSeenField != null)
                {
                    var secretNotesSeen = secretNotesSeenField.GetValue(Game1.player);
                    
                    // 检查 secretNotesSeen 的类型
                    if (secretNotesSeen is System.Collections.Generic.HashSet<int> intNotes)
                    {
                        // 处理整数类型的ID
                        if (noteId is int intId)
                        {
                            if (intNotes.Add(intId))
                            {
                                _monitor.Log($"{nameof(SecretNoteAdder)}::成功将秘密纸条添加到收集品列表: ID={intId}", LogLevel.Info);
                            }
                            else
                            {
                                _monitor.Log($"{nameof(SecretNoteAdder)}::秘密纸条已经在收集品列表中: ID={intId}", LogLevel.Info);
                            }
                        }
                        else if (noteId is string stringId)
                        {
                            // 尝试将字符串ID转换为整数
                            if (int.TryParse(stringId, out int parsedId))
                            {
                                if (intNotes.Add(parsedId))
                                {
                                    _monitor.Log($"{nameof(SecretNoteAdder)}::成功将秘密纸条添加到收集品列表: ID={parsedId}", LogLevel.Info);
                                }
                                else
                                {
                                    _monitor.Log($"{nameof(SecretNoteAdder)}::秘密纸条已经在收集品列表中: ID={parsedId}", LogLevel.Info);
                                }
                            }
                            else
                            {
                                _monitor.Log($"{nameof(SecretNoteAdder)}::无法将字符串ID {stringId} 转换为整数，无法添加到收集品列表", LogLevel.Warn);
                                // 对于非数字的字符串ID，需要特殊处理
                                // 这里可以考虑使用其他方式存储自定义纸条
                            }
                        }
                    }
                    else if (secretNotesSeen is System.Collections.Generic.HashSet<string> stringNotes)
                    {
                        // 处理字符串类型的ID
                        if (noteId is string stringId)
                        {
                            if (stringNotes.Add(stringId))
                            {
                                _monitor.Log($"{nameof(SecretNoteAdder)}::成功将秘密纸条添加到收集品列表: ID={stringId}", LogLevel.Info);
                            }
                            else
                            {
                                _monitor.Log($"{nameof(SecretNoteAdder)}::秘密纸条已经在收集品列表中: ID={stringId}", LogLevel.Info);
                            }
                        }
                        else if (noteId is int intId)
                        {
                            string idStr = intId.ToString();
                            if (stringNotes.Add(idStr))
                            {
                                _monitor.Log($"{nameof(SecretNoteAdder)}::成功将秘密纸条添加到收集品列表: ID={idStr}", LogLevel.Info);
                            }
                            else
                            {
                                _monitor.Log($"{nameof(SecretNoteAdder)}::秘密纸条已经在收集品列表中: ID={idStr}", LogLevel.Info);
                            }
                        }
                    }
                    else
                    {
                        _monitor.Log($"{nameof(SecretNoteAdder)}::secretNotesSeen 字段类型未知: {secretNotesSeen?.GetType().Name}", LogLevel.Warn);
                    }
                }
                else
                {
                    _monitor.Log($"{nameof(SecretNoteAdder)}::未找到 secretNotesSeen 字段，无法添加到收集品列表", LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"{nameof(SecretNoteAdder)}::将秘密纸条添加到收集品列表时出错: {ex.Message}", LogLevel.Error);
            }
        }
    }
}