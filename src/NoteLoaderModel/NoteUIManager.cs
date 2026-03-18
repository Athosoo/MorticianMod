using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MorticianMod.NoteLoaderModel
{
    /// <summary>
    /// 纸条界面管理器
    /// 负责管理纸条相关界面
    /// </summary>
    public class NoteUIManager
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private NoteDataManager _noteDataManager;
        private NoteCollectionManager _noteCollectionManager;

        public void Initialize(IModHelper helper, IMonitor monitor, NoteDataManager noteDataManager, NoteCollectionManager noteCollectionManager)
        {
            _helper = helper;
            _monitor = monitor;
            _noteDataManager = noteDataManager;
            _noteCollectionManager = noteCollectionManager;

            _monitor.Log("纸条界面管理器初始化完成", LogLevel.Debug);
        }

        /// <summary>
        /// 应用补丁
        /// </summary>
        public void ApplyPatches()
        {
            try
            {
                // 设置补丁类的依赖
                Patches.SetDependencies(_noteDataManager, _noteCollectionManager);

                Harmony harmony = new("MorticianMod");

                // 修补CollectionsPage构造函数，添加我们的纸条到收集品页面
                PatchMethod(harmony, typeof(CollectionsPage),
                        nameof(CollectionsPage.receiveLeftClick), null,
                        nameof(Patches.CollectionsPage_receiveLeftClick_Postfix));

                string ctorFullName = "<unresolved>";
                try
                {
                    ConstructorInfo collectionspage_ctor = typeof(CollectionsPage)
                            .GetConstructor(new[]{typeof(int),typeof(int),typeof(int),typeof(int)});
                    ctorFullName = collectionspage_ctor.DeclaringType.FullName + "." +
                            collectionspage_ctor.Name;
                    harmony.Patch(original: collectionspage_ctor,
                            postfix: new HarmonyMethod(typeof(Patches),
                                nameof(Patches.CollectionsPage_ctor_Postfix)));
                    _monitor.Log($"Patched (Postfix) {ctorFullName}", LogLevel.Debug);
                }
                catch (Exception e)
                {
                    _monitor.Log($"Patch failed ({ctorFullName}): {e}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"应用补丁时出错: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 修补方法的辅助函数
        /// </summary>
        private void PatchMethod(Harmony harmony, Type t, string name,
                Type[] argTypes, string patch)
        {
            string[] parts = patch.Split("_");
            string last = parts[parts.Length-1];
            if (last != "Prefix" && last != "Postfix" && last != "Transpiler")
            {
                _monitor.Log($"Skipping patch method '{patch}': bad type '{last}'", LogLevel.Error);
                return;
            }
            try
            {
                MethodInfo m;
                if (argTypes is null)
                {
                    m = t.GetMethod(name,
                            BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.Static);
                }
                else
                {
                    m = t.GetMethod(name,
                            BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.Static,
                            null, argTypes, null);
                }
                HarmonyMethod func = new(typeof(Patches), patch);
                if (last == "Prefix")
                {
                    harmony.Patch(original: m, prefix: func);
                }
                else if (last == "Postfix")
                {
                    harmony.Patch(original: m, postfix: func);
                }
                else if (last == "Transpiler")
                {
                    harmony.Patch(original: m, transpiler: func);
                }
                _monitor.Log($"Patched ({last}) {m.DeclaringType.FullName + "." + m.Name}", LogLevel.Debug);
            }
            catch (Exception e)
            {
                _monitor.Log($"Patch failed ({patch}): {e}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 显示纸条内容
        /// </summary>
        /// <param name="noteData">纸条数据</param>
        public void ShowSecretNote(CustomSecretNoteData noteData)
        {
            // 委托给NoteDisplayManager处理
            var noteDisplayManager = new NoteDisplayManager();
            noteDisplayManager.Initialize(_helper, _monitor);
            noteDisplayManager.ShowSecretNote(noteData);
        }

        /// <summary>
        /// 内部补丁类
        /// </summary>
        private class Patches
        {
            private static NoteDataManager _noteDataManager;
            private static NoteCollectionManager _noteCollectionManager;

            /// <summary>
            /// 设置依赖
            /// </summary>
            public static void SetDependencies(NoteDataManager noteDataManager, NoteCollectionManager noteCollectionManager)
            {
                _noteDataManager = noteDataManager;
                _noteCollectionManager = noteCollectionManager;
            }

            /// <summary>
            /// 修补CollectionsPage构造函数，添加我们的纸条到收集品页面
            /// </summary>
            public static void CollectionsPage_ctor_Postfix(CollectionsPage __instance)
            {
                try
                {
                    // 确保收集品页面有秘密纸条部分
                    if (!__instance.collections.ContainsKey(6))
                    {
                        return;
                    }

                    int rowItems = 10;
                    int index = 0;
                    int baseX = __instance.xPositionOnScreen + IClickableMenu.borderWidth +
                            IClickableMenu.spaceToClearSideBorder;
                    int baseY = __instance.yPositionOnScreen + IClickableMenu.borderWidth +
                            IClickableMenu.spaceToClearTopBorder - 16;
                    var list = __instance.collections[6].Last();
                    if (list.Count > 0)
                    {
                        index = (list.Count / rowItems + 1) * rowItems;
                    }

                    // 获取我们的纸条数据
                    var noteDatas = _noteDataManager?.GetAllNoteDatas();
                    if (noteDatas == null)
                    {
                        return;
                    }

                    foreach (var noteData in noteDatas)
                    {
                        int noteId = noteData.IntId;
                        bool hasNote = _noteCollectionManager?.HasNote(Game1.player, noteId) ?? false;

                        int xPos = baseX + (index % rowItems) * 68;
                        int yPos = baseY + (index / rowItems) * 68;

                        if (yPos > __instance.yPositionOnScreen + __instance.height - 128)
                        {
                            var nl = new List<ClickableTextureComponent>();
                            __instance.collections[6].Add(nl);
                            list = __instance.collections[6].Last();
                            index = 0;
                            xPos = baseX;
                            yPos = baseY;
                        }

                        // 创建纸条物品
                        string itemId = $"MorticianMod_{noteData.Id}";
                        var itemData = ItemRegistry.GetDataOrErrorItem(itemId);

                        // 添加到收集品页面
                        list.Add(new ClickableTextureComponent(
                            name: $"{noteId} {hasNote}",
                            bounds: new Rectangle(xPos, yPos, 64, 64),
                            label: null,
                            hoverText: makeHoverText(noteData),
                            texture: itemData.GetTexture(),
                            sourceRect: itemData.GetSourceRect(),
                            scale: 4f,
                            drawShadow: hasNote));
                        ++index;
                    }
                }
                catch (Exception e)
                {
                    // 静默处理异常，避免影响游戏
                }
            }

            /// <summary>
            /// 修补CollectionsPage的点击事件，让玩家可以查看已收集的纸条
            /// </summary>
            public static void CollectionsPage_receiveLeftClick_Postfix(
                    CollectionsPage __instance,
                    int x, int y)
            {
                try
                {
                    if (__instance.currentTab != 6 ||
                            __instance.letterviewerSubMenu != null)
                    {
                        return;
                    }

                    var clicked = __instance.collections[6][__instance.currentPage]
                            .Where(c => c.containsPoint(x, y)).ToList();
                    if (clicked.Count == 0)
                    {
                        return;
                    }

                    string[] parts = clicked[0].name.Split(' ');
                    if (parts.Length < 2 || !bool.TryParse(parts[1], out bool hasNote))
                    {
                        return;
                    }

                    if (!hasNote || !int.TryParse(parts[0], out int noteId))
                    {
                        return;
                    }

                    // 获取纸条数据
                    var noteData = _noteDataManager?.GetNoteDataByIntId(noteId);
                    if (noteData == null)
                    {
                        return;
                    }

                    // 显示纸条
                    var noteDisplayManager = new NoteDisplayManager();
                    noteDisplayManager.Initialize(ModEntry.Helper, ModEntry.Monitor);
                    noteDisplayManager.ShowSecretNote(noteData);

                }
                catch (Exception e)
                {
                    // 静默处理异常，避免影响游戏
                }
            }

            /// <summary>
            /// 创建纸条的悬停文本
            /// </summary>
            private static string makeHoverText(CustomSecretNoteData noteData)
            {
                string title = "神秘纸条";
                string content = noteData.ContentText?.Trim() ?? "";
                if (!string.IsNullOrEmpty(content))
                {
                    // 处理内容格式
                    content = content.Replace("^", Environment.NewLine);
                    content = Game1.parseText(content, Game1.smallFont, 512);
                    string[] split = content.Split(Environment.NewLine);
                    int max = 10;
                    if (split.Length > max)
                    {
                        content = string.Join(Environment.NewLine,
                                split.Take(max)) + Environment.NewLine + "(...)";
                    }
                    return title + Environment.NewLine + Environment.NewLine + content;
                }
                return title;
            }
        }
    }
}
