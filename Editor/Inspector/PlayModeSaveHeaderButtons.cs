#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VahTyah
{
    [InitializeOnLoad]
    public static class PlayModeSaveHeaderButtons
    {
        private const BindingFlags MaxBindingFlags = (BindingFlags)62;

        static PlayModeSaveHeaderButtons()
        {
            Editor.finishedDefaultHeaderGUI += UpdateHeaderButtons;
        }

        private static void UpdateHeaderButtons(Editor _)
        {
            var buttons = typeof(EditorGUIUtility)
                .GetField("s_EditorHeaderItemsMethods", BindingFlags.Static | BindingFlags.NonPublic)?
                .GetValue(null) as IList;

            if (buttons == null) return;

            var delegateType = typeof(EditorGUIUtility).GetNestedType("HeaderItemDelegate", MaxBindingFlags);
            if (delegateType == null) return;

            playmodeSaveButton ??= typeof(PlayModeSaveHeaderButtons)
                .GetMethod(nameof(DrawPlayModeSaveButton), MaxBindingFlags)?
                .CreateDelegate(delegateType);

            if (playmodeSaveButton == null) return;

            buttons.Remove(playmodeSaveButton);
            buttons.Insert(0, playmodeSaveButton);

            Editor.finishedDefaultHeaderGUI -= UpdateHeaderButtons;
        }

        private static Delegate playmodeSaveButton;

        private static bool DrawPlayModeSaveButton(Rect buttonRect, Object[] targets)
        {
            // Only show in play mode
            if (!Application.isPlaying) return false;
            
            if (targets == null || targets.Length == 0) return false;
            if (targets[0] is not Component component) return false;

            bool isSaved = PlayModeSaveSystem.IsSaved(component);

            // Tooltip
            string tooltip = isSaved ? "Saved (click to unsave)" : "Save in play mode";
            GUI.Label(buttonRect, new GUIContent("", tooltip));

            // Colors - green when saved
            bool isDarkTheme = EditorGUIUtility.isProSkin;
            Color color, colorHovered, colorPressed;
            
            if (isSaved)
            {
                // Green color for saved state
                color = new Color(0.4f, 0.9f, 0.4f);
                colorHovered = new Color(0.5f, 1f, 0.5f);
                colorPressed = new Color(0.3f, 0.7f, 0.3f);
            }
            else
            {
                color = Greyscale(isDarkTheme ? .8f : .46f);
                colorHovered = Greyscale(isDarkTheme ? 1f : .2f);
                colorPressed = Greyscale(isDarkTheme ? .8f : .6f);
            }

            // Use same icon, color indicates state
            string iconName = "Save";

            if (IconButton(buttonRect, iconName, 16, color, colorHovered, colorPressed))
            {
                // Toggle save for all targets
                foreach (var target in targets)
                {
                    if (target is Component comp)
                    {
                        PlayModeSaveSystem.SaveComponent(comp);
                    }
                }
            }

            return true;
        }

        #region Icon Button Helper

        private static int pressedIconButtonId;

        private static bool IconButton(Rect rect, string iconName, float iconSize, Color color, Color colorHovered, Color colorPressed)
        {
            var id = EditorGUIUtility.GUIToScreenRect(rect).GetHashCode();
            var isPressed = id == pressedIconButtonId;
            var wasActivated = false;

            // Draw icon
            if (Event.current.type == EventType.Repaint)
            {
                var drawColor = color;
                
                if (rect.Contains(Event.current.mousePosition))
                    drawColor = colorHovered;
                
                if (isPressed)
                    drawColor = colorPressed;

                var iconRect = rect.SetSizeFromMid(iconSize);
                var prevColor = GUI.color;
                GUI.color = drawColor;
                GUI.DrawTexture(iconRect, GetIcon(iconName));
                GUI.color = prevColor;
            }

            // Mouse down
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                pressedIconButtonId = id;
                Event.current.Use();
            }

            // Mouse up
            if (Event.current.type == EventType.MouseUp && isPressed)
            {
                pressedIconButtonId = 0;
                if (rect.Contains(Event.current.mousePosition))
                    wasActivated = true;
                Event.current.Use();
            }

            // Mouse drag
            if (Event.current.type == EventType.MouseDrag && isPressed)
            {
                Event.current.Use();
            }

            return wasActivated;
        }

        private static Rect SetSizeFromMid(this Rect rect, float size)
        {
            return new Rect(
                rect.center.x - size / 2,
                rect.center.y - size / 2,
                size,
                size
            );
        }

        private static Color Greyscale(float brightness, float alpha = 1f)
        {
            return new Color(brightness, brightness, brightness, alpha);
        }

        #endregion

        #region Icon Helper

        private static Dictionary<string, Texture2D> iconCache = new();

        private static Texture2D GetIcon(string iconName)
        {
            if (iconCache.TryGetValue(iconName, out var cached) && cached != null)
                return cached;

            Texture2D icon = null;

            // Map to Unity built-in icons
            string builtinName = iconName switch
            {
                "Save" => "SaveAs",
                "Saved" => "SaveAs@2x", // Or use a checkmark icon
                _ => iconName
            };

            var content = EditorGUIUtility.IconContent(builtinName);
            icon = content?.image as Texture2D;

            // Empty fallback
            if (icon == null)
            {
                icon = Texture2D.whiteTexture;
            }

            iconCache[iconName] = icon;
            return icon;
        }

        #endregion
    }
}
#endif
