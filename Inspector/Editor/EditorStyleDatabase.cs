using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VahTyah
{
    // [CreateAssetMenu(fileName = "EditorStyles", menuName = "CustomEditor/EditorStyles", order = 1)]
    public class EditorStyleDatabase : ScriptableObject
    {
        [SerializeField, BoxGroup("Components", "Components") ] private int defaultStyleIndex = 0;
        [SerializeField, BoxGroup("Components", "Components") ] private List<EditorCustomStyle> styles = new List<EditorCustomStyle>();

        public void AddDefaultStyle()
        {
            EditorCustomStyle defaultStyle = new EditorCustomStyle
            {
                groupStyles = EditorCustomStyle.GroupStyles.CreateDefaultStyles()
            };
            styles.Add(defaultStyle);
        }

        public EditorCustomStyle GetStyle()
        {
            if (styles.Count == 0)
            {
                AddDefaultStyle();
            }

            if (defaultStyleIndex < 0 || defaultStyleIndex >= styles.Count)
            {
                defaultStyleIndex = 0;
            }

            return styles[defaultStyleIndex];
        }
    }

    [Serializable]
    public class EditorCustomStyle
    {
        public GroupStyles groupStyles;

        [Serializable]
        public class GroupStyles
        {
            public float headerHeight;
            public Padding headerPadding;
            public Padding contentPadding;
            public float groupSpacing;
            public LayerConfiguration backgroundConfig;
            public LayerConfiguration headerConfig;
            public GUIStyle labelStyle;

            public static GroupStyles CreateDefaultStyles()
            {
                GroupStyles styles = new GroupStyles
                {
                    headerHeight = 28f,
                    headerPadding = new Padding(8f, 0f, 0f, 0f),
                    contentPadding = new Padding(8f, 8f, 8f, 8f),
                    groupSpacing = 6f,
                    headerConfig = CreateDarkHeaderStyle(),
                    backgroundConfig = CreateDarkBackgroundStyle(),
                    labelStyle = CreateLabelStyle()
                };
                return styles;
            }

            private static LayerConfiguration CreateDarkHeaderStyle()
            {
                LayerConfiguration config = new LayerConfiguration(3);
                config.layers[0] = Layer.CreateRoundedRect(new Color(0f, 0f, 0f, 0.15f), 4f, new Padding(0, 1, 1, 0));
                config.layers[1] = Layer.CreateRoundedRect(new Color(0f, 0f, 0f, 0.15f), 4f);
                config.layers[2] = Layer.CreateBorder(new Color(0.35f, 0.35f, 0.35f, 1f), 1f, 4f);
                return config;
            }

            private static LayerConfiguration CreateDarkBackgroundStyle()
            {
                LayerConfiguration config = new LayerConfiguration(3);
                config.layers[0] = Layer.CreateRoundedRect(new Color(0.15f, 0.15f, 0.15f, 1f), 4f, new Padding(0, 1, 1, 0));
                config.layers[1] = Layer.CreateRoundedRect(new Color(0.22f, 0.22f, 0.22f, 1f), 4f);
                config.layers[2] = Layer.CreateBorder(new Color(0.35f, 0.35f, 0.35f, 1f), 1f, 4f);
                return config;
            }

            private static GUIStyle CreateLabelStyle()
            {
                GUIStyle labelStyle = new GUIStyle(UnityEditor.EditorStyles.boldLabel);
                labelStyle.fontSize = 12;
                labelStyle.alignment = TextAnchor.MiddleLeft;
                labelStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                labelStyle.padding = new RectOffset(0, 0, 0, 0);
                return labelStyle;
            }
        }
    }
}