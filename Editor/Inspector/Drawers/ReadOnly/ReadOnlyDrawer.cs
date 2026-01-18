using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VahTyah
{
    public class ReadOnlyDrawer
    {
        // Track unlock state per property path
        private Dictionary<string, bool> unlockedStates = new Dictionary<string, bool>();

        public void DrawLockButton(Rect buttonRect, SerializedProperty property)
        {
            bool isUnlocked = IsUnlocked(property.propertyPath);

            // Use icons that work across Unity versions
            string iconName = isUnlocked ? "IN LockButton" : "IN LockButton on";
            string tooltip = isUnlocked ? "Click to lock (read-only)" : "Click to unlock (editable)";

            GUIContent buttonContent = EditorGUIUtility.IconContent(iconName);
            buttonContent.tooltip = tooltip;

            GUIStyle centeredButton = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0)
            };

            if (GUI.Button(buttonRect, buttonContent, centeredButton))
            {
                ToggleLock(property.propertyPath);
            }
        }

        public bool IsUnlocked(string propertyPath)
        {
            if (unlockedStates.TryGetValue(propertyPath, out bool unlocked))
            {
                return unlocked;
            }
            return false; // Default: locked
        }

        public bool IsLocked(string propertyPath)
        {
            return !IsUnlocked(propertyPath);
        }

        private void ToggleLock(string propertyPath)
        {
            if (unlockedStates.TryGetValue(propertyPath, out bool unlocked))
            {
                unlockedStates[propertyPath] = !unlocked;
            }
            else
            {
                unlockedStates[propertyPath] = true; // First click: unlock
            }
        }
    }
}
