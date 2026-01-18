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
            var autoRefInfo = propertyGroup.GetAutoRefAttribute(property);
            var assetRefInfo = propertyGroup.GetAssetRefAttribute(property);
            var onValueChangedInfo = propertyGroup.GetOnValueChangedAttribute(property);
            var requiredInfo = propertyGroup.GetRequiredAttribute(property);
            bool showRequiredIcon = requiredInfo.HasValue && propertyGroup.RequiredDrawer != null && propertyGroup.RequiredDrawer.ShouldShowIcon(property);

            int buttonCount = (autoRefInfo.HasValue ? 1 : 0) + (assetRefInfo.HasValue ? 1 : 0) + (onValueChangedInfo.HasValue ? 1 : 0) + (showRequiredIcon ? 1 : 0);

            if (buttonCount == 0)
            {
                EditorGUI.PropertyField(rect, property, true);
                return;
            }

            const float BUTTON_WIDTH = 25f;
            const float BUTTON_SPACING = 2f;
            float totalButtonWidth = buttonCount * BUTTON_WIDTH + (buttonCount - 1) * BUTTON_SPACING;

            Rect fieldRect = new Rect(rect.x, rect.y, rect.width - totalButtonWidth - 2, rect.height);
            EditorGUI.PropertyField(fieldRect, property, new GUIContent(property.displayName), true);

            float buttonX = rect.xMax - totalButtonWidth;

            if (showRequiredIcon)
            {
                Rect iconRect = new Rect(buttonX, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);
                propertyGroup.RequiredDrawer.DrawIcon(iconRect, property, requiredInfo.Value.attr);
                buttonX += BUTTON_WIDTH + BUTTON_SPACING;
            }

            if (autoRefInfo.HasValue && propertyGroup.AutoRefDrawer != null)
            {
                Rect buttonRect = new Rect(buttonX, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);
                propertyGroup.AutoRefDrawer.DrawButton(buttonRect, property, autoRefInfo.Value.attr, autoRefInfo.Value.field, propertyGroup.Target);
                buttonX += BUTTON_WIDTH + BUTTON_SPACING;
            }

            if (assetRefInfo.HasValue && propertyGroup.AssetRefDrawer != null)
            {
                Rect buttonRect = new Rect(buttonX, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);
                propertyGroup.AssetRefDrawer.DrawButton(buttonRect, property, assetRefInfo.Value.attr, assetRefInfo.Value.field, propertyGroup.Target);
                buttonX += BUTTON_WIDTH + BUTTON_SPACING;
            }

            if (onValueChangedInfo.HasValue && propertyGroup.OnValueChangedDrawer != null)
            {
                Rect buttonRect = new Rect(buttonX, rect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);
                propertyGroup.OnValueChangedDrawer.DrawButton(buttonRect, property, onValueChangedInfo.Value.attrs, onValueChangedInfo.Value.methods, propertyGroup.Targets);
            }
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
