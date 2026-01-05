using VahTyah;
using UnityEditor;
using UnityEngine;

namespace VahTyah
{
    public class BoxGroupDrawer : IGroupDrawer
    {
        private EditorCustomStyle.GroupStyles groupStyles = EditorStyles.Style.groupStyles;

        public void Draw(PropertyGroup propertyGroup)
        {
            if (propertyGroup.Properties.Count == 0) return;

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

                EditorGUI.PropertyField(propertyRect, property, true);

                currentY += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;

            GUILayout.Space(groupStyles.groupSpacing);
        }
        

        private float CalculateTotalGroupHeight(PropertyGroup propertyGroup)
        {
            var totalHeight = groupStyles.headerHeight + groupStyles.contentPadding.top + groupStyles.contentPadding.top;

            foreach (var property in propertyGroup.Properties)
            {
                totalHeight += EditorGUI.GetPropertyHeight(property, true);
                totalHeight += EditorGUIUtility.standardVerticalSpacing;
            }

            return totalHeight;
        }
    }
}