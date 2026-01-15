using UnityEngine;
using UnityEditor;

namespace VahTyah
{
    public enum ResizeDirection
    {
        Horizontal,
        Vertical
    }

    public class ResizableSeparator
    {
        private int currentSize;
        private Rect separatorRect;
        private bool separatorIsDragged;
        private readonly string prefsKey;
        private readonly int defaultSize;
        private readonly int minSize;
        private readonly int maxSize;
        private readonly ResizeDirection direction;

        public ResizableSeparator(
            string prefsKey,
            int defaultSize,
            ResizeDirection direction = ResizeDirection.Horizontal,
            int minSize = 200,
            int maxSize = 500)
        {
            this.prefsKey = prefsKey;
            this.defaultSize = defaultSize;
            this.minSize = minSize;
            this.maxSize = maxSize;
            this.direction = direction;
            currentSize = PlayerPrefs.GetInt(prefsKey, defaultSize);
        }

        public int CurrentSize => currentSize;
        public int CurrentWidth => currentSize;
        public int CurrentHeight => currentSize;

        public void DrawResizeSeparator()
        {
            if (direction == ResizeDirection.Horizontal)
            {
                separatorRect = EditorGUILayout.BeginHorizontal(
                    GUI.skin.box,
                    GUILayout.MinWidth(8),
                    GUILayout.ExpandHeight(true));
            }
            else
            {
                separatorRect = EditorGUILayout.BeginHorizontal(
                    GUI.skin.box,
                    GUILayout.MinHeight(8),
                    GUILayout.ExpandWidth(true));
            }

            EditorGUILayout.EndHorizontal();

            MouseCursor cursor = direction == ResizeDirection.Horizontal
                ? MouseCursor.ResizeHorizontal
                : MouseCursor.ResizeVertical;
            EditorGUIUtility.AddCursorRect(separatorRect, cursor);

            HandleMouseEvents();
        }

        private void HandleMouseEvents()
        {
            if (separatorRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    separatorIsDragged = true;
                    Event.current.Use();
                }
            }

            if (separatorIsDragged)
            {
                if (Event.current.type == EventType.MouseUp)
                {
                    separatorIsDragged = false;
                    SaveSize();
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    int delta = direction == ResizeDirection.Horizontal
                        ? Mathf.RoundToInt(Event.current.delta.x)
                        : Mathf.RoundToInt(Event.current.delta.y);

                    currentSize = Mathf.Clamp(currentSize + delta, minSize, maxSize);
                    Event.current.Use();
                }
            }
        }

        private void SaveSize()
        {
            PlayerPrefs.SetInt(prefsKey, currentSize);
            PlayerPrefs.Save();
        }

        public void ResetSize()
        {
            currentSize = defaultSize;
            SaveSize();
        }

        public void ResetWidth() => ResetSize();
        public void ResetHeight() => ResetSize();
    }
}