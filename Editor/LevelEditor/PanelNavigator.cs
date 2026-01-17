using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace VahTyah
{
    public interface IEditorPanel
    {
        void OnEnable();
        void OnDisable();
        void Draw(Rect rect);
    }
    public class PanelNavigator
    {
        private string currentPanel;
        private Dictionary<string, IEditorPanel> panels;

        private LevelEditorStyleData.PanelNavigatorStyles currentPanelStyle;
        private LevelEditorStyleData.GlobalBackground contentBackground;
        private LevelEditorStylesDatabase _levelEditorStylesDatabase;

        public PanelNavigator(Dictionary<string, IEditorPanel> panels)
        {
            this.panels = panels;

            if (panels != null && panels.Count > 0)
            {
                currentPanel = panels.Keys.First();

                foreach (var panel in panels.Values)
                {
                    panel.OnEnable();
                }
            }

            InitializeStyles();
        }

        private void InitializeStyles()
        {
            if (_levelEditorStylesDatabase == null)
            {
                _levelEditorStylesDatabase = EditorUtils.GetAsset<LevelEditorStylesDatabase>();
            }

            if (_levelEditorStylesDatabase != null)
            {
                var style = _levelEditorStylesDatabase.GetStyle();
                currentPanelStyle = style.panelNavigatorStyles;
                contentBackground = style.globalBackground;
            }
            else
            {
                currentPanelStyle = LevelEditorStyleData.PanelNavigatorStyles.CreateDefaultStyles();
                contentBackground = LevelEditorStyleData.GlobalBackground.CreateDefaultStyles();
            }
        }

        public void Cleanup()
        {
            if (panels != null)
            {
                foreach (var panel in panels.Values)
                {
                    panel.OnDisable();
                }
            }
        }

        public void Draw(Rect rect)
        {
            Rect menuBarRect = new Rect(rect.x + 5f, rect.y, rect.width - 10f, currentPanelStyle.menuBarHeight);
            Rect contentRect = new Rect(
                rect.x,
                rect.y + currentPanelStyle.menuBarHeight,
                rect.width,
                rect.height - currentPanelStyle.menuBarHeight - 10f
            );

            DrawContent(contentRect);
            DrawMenuBar(menuBarRect);
        }

        private void DrawMenuBar(Rect menuBarRect)
        {
            float tabWidth = menuBarRect.width / panels.Count;
            int index = 0;

            foreach (var kvp in panels)
            {
                Rect tabRect = new Rect(
                    menuBarRect.x + (index * tabWidth),
                    menuBarRect.y,
                    tabWidth,
                    menuBarRect.height
                );

                bool isActive = currentPanel == kvp.Key;

                var borderWidth = contentBackground?.backgroundConfig?.layers[1]?.borderWidth.y ?? 2;
                if (isActive) 
                    tabRect.height += borderWidth + 1f;

                LayerDrawingSystem.DrawLayers(
                    tabRect,
                    isActive ? currentPanelStyle.activeTabStyle : currentPanelStyle.inactiveTabStyle
                );

                GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = isActive ? Color.white : new Color(0.5f, 0.5f, 0.5f) },
                    fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal
                };

                GUI.Label(tabRect, kvp.Key, labelStyle);

                if (Event.current.type == EventType.MouseDown && tabRect.Contains(Event.current.mousePosition))
                {
                    currentPanel = kvp.Key;
                    Event.current.Use();
                }

                index++;
            }
        }

        private void DrawContent(Rect contentRect)
        {
            if (panels == null || !panels.ContainsKey(currentPanel))
            {
                return;
            }

            LayerDrawingSystem.DrawLayers(contentRect, contentBackground.backgroundConfig);
            
            float padding = 10f;
            Rect paddedRect = new Rect(
                contentRect.x + padding,
                contentRect.y + padding,
                contentRect.width - (padding * 2),
                contentRect.height - (padding * 2)
            );
            
            panels[currentPanel].Draw(paddedRect);
        }
    }
}