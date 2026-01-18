using UnityEditor;
using UnityEngine;

namespace VahTyah
{
    public class BoxGroupDrawer : IGroupDrawer
    {
        private InspectorStyleData.GroupStyles groupStyles;

        private void EnsureStyles()
        {
            if (groupStyles == null)
            {
                groupStyles = InspectorStyle.GetStyle()?.groupStyles
                    ?? InspectorStyleData.GroupStyles.CreateDefaultStyles();
            }
        }

        public void Draw(PropertyGroup propertyGroup)
        {
            if (propertyGroup.Properties.Count == 0) return;

            EnsureStyles();

            var totalHeight = CalculateTotalGroupHeight(propertyGroup);
            Rect groupRect = GUILayoutUtility.GetRect(0, totalHeight, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
            {
                LayerDrawingSystem.DrawLayers(groupRect, groupStyles.backgroundConfig);

                Rect headerLayerRect = new Rect(
                    groupRect.x,
                    groupRect.y,
                    groupRect.width,
                    groupStyles.headerHeight
                );
                LayerDrawingSystem.DrawLayers(headerLayerRect, groupStyles.headerConfig);
            }

            Rect headerContentRect = new Rect(
                groupRect.x + groupStyles.headerPadding.left,
                groupRect.y,
                groupRect.width - groupStyles.headerPadding.left * 2,
                groupStyles.headerHeight
            );

            GUI.Label(headerContentRect, propertyGroup.Attribute.Label, groupStyles.labelStyle);

            var currentY = groupRect.y + groupStyles.headerHeight + groupStyles.contentPadding.top;
            var contentX = groupRect.x + groupStyles.contentPadding.left;
            var contentWidth = groupRect.width - groupStyles.contentPadding.left - groupStyles.contentPadding.right;

            EditorGUI.indentLevel++;

            foreach (var property in propertyGroup.Properties)
            {
                float propertyHeight = EditorGUI.GetPropertyHeight(property, true);
                Rect propertyRect = new Rect(contentX, currentY, contentWidth, propertyHeight);

                DrawProperty(propertyGroup, property, propertyRect);

                currentY += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;

            GUILayout.Space(groupStyles.groupSpacing);
        }

        private void DrawProperty(PropertyGroup propertyGroup, SerializedProperty property, Rect rect)
        {
            // Check AutoRef
            var autoRefInfo = propertyGroup.GetAutoRefAttribute(property);
            if (autoRefInfo.HasValue && propertyGroup.AutoRefDrawer != null)
            {
                propertyGroup.AutoRefDrawer.DrawProperty(rect, property, autoRefInfo.Value.attr, autoRefInfo.Value.field, propertyGroup.Target);
                return;
            }

            // Check AssetRef
            var assetRefInfo = propertyGroup.GetAssetRefAttribute(property);
            if (assetRefInfo.HasValue && propertyGroup.AssetRefDrawer != null)
            {
                propertyGroup.AssetRefDrawer.DrawProperty(rect, property, assetRefInfo.Value.attr, assetRefInfo.Value.field, propertyGroup.Target);
                return;
            }

            // Default
            EditorGUI.PropertyField(rect, property, true);
        }

        private float CalculateTotalGroupHeight(PropertyGroup propertyGroup)
        {
            EnsureStyles();

            // Fixed: was using contentPadding.top twice instead of top + bottom
            var totalHeight = groupStyles.headerHeight + groupStyles.contentPadding.top + groupStyles.contentPadding.bottom;

            foreach (var property in propertyGroup.Properties)
            {
                totalHeight += EditorGUI.GetPropertyHeight(property, true);
                totalHeight += EditorGUIUtility.standardVerticalSpacing;
            }

            return totalHeight;
        }
    }
}
