using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VahTyah
{
    // [CreateAssetMenu(fileName = "EditorStyles", menuName = "CustomEditor/EditorStyles", order = 1)]
    public class EditorStyleDatabase : ScriptableObject
    {
        [SerializeField, BoxGroup("Settings")] private int defaultStyleIndex = 0;
        [SerializeField, BoxGroup("Styles")] private List<InspectorStyleData> styles = new List<InspectorStyleData>();

        [Button]
        public void AddDefaultStyle()
        {
            InspectorStyleData defaultStyleData = new InspectorStyleData
            {
                groupStyles = InspectorStyleData.GroupStyles.CreateDefaultStyles(),
                buttonStyles = InspectorStyleData.ButtonStyles.CreateDefaultStyles()
            };
            styles.Add(defaultStyleData);
        }

        public InspectorStyleData GetStyle()
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

        public static InspectorStyleData GetDefaultStyle()
        {
            InspectorStyleData defaultStyleData = new InspectorStyleData
            {
                groupStyles = InspectorStyleData.GroupStyles.CreateDefaultStyles(),
                buttonStyles = InspectorStyleData.ButtonStyles.CreateDefaultStyles()
            };
            return defaultStyleData;
        }
    }

    [Serializable]
    public class InspectorStyleData
    {
        public GroupStyles groupStyles;
        public ButtonStyles buttonStyles;

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
                config.layers[0] =
                    Layer.CreateRoundedRect(new Color(0.15f, 0.15f, 0.15f, 1f), 4f, new Padding(0, 1, 1, 0));
                config.layers[1] = Layer.CreateRoundedRect(new Color(0.22f, 0.22f, 0.22f, 1f), 4f);
                config.layers[2] = Layer.CreateBorder(new Color(0.35f, 0.35f, 0.35f, 1f), 1f, 4f);
                return config;
            }

            private static GUIStyle CreateLabelStyle()
            {
                GUIStyle labelStyle = new GUIStyle(); // Create from scratch
                labelStyle.fontSize = 12;
                labelStyle.fontStyle = FontStyle.Bold;
                labelStyle.alignment = TextAnchor.MiddleLeft;
                labelStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                labelStyle.padding = new RectOffset(0, 0, 0, 0);
                return labelStyle;
            }
        }

        [Serializable]
        public class ButtonStyles
        {
            public float buttonHeight;
            public float buttonSpacing;
            public Padding buttonPadding;
            public LayerConfiguration backgroundConfig;
            public LayerConfiguration normalConfig;
            public LayerConfiguration hoverConfig;
            public LayerConfiguration activeConfig;
            public GUIStyle labelStyle;

            public static ButtonStyles CreateDefaultStyles()
            {
                ButtonStyles styles = new ButtonStyles
                {
                    buttonHeight = 30f,
                    buttonSpacing = 5f,
                    buttonPadding = new Padding(12f, 8f, 12f, 8f),
                    backgroundConfig = CreateBackgroundStyle(),
                    normalConfig = CreateButtonNormalStyle(),
                    hoverConfig = CreateButtonHoverStyle(),
                    activeConfig = CreateButtonActiveStyle(),
                    labelStyle = CreateButtonLabelStyle()
                };
                return styles;
            }

            private static LayerConfiguration CreateBackgroundStyle()
            {
                LayerConfiguration config = new LayerConfiguration(2);
                config.layers[0] = Layer.CreateRoundedRect(new Color(0.22f, 0.22f, 0.22f, 1f), 4f);
                config.layers[1] = Layer.CreateBorder(new Color(0.4f, 0.4f, 0.4f, 1f), 1f, 4f);
                return config;
            }

            private static LayerConfiguration CreateButtonNormalStyle()
            {
                LayerConfiguration config = new LayerConfiguration(3);
                config.layers[0] =
                    Layer.CreateRoundedRect(new Color(0f, 0f, 0f, 0.15f), 4f, new Padding(0, 1, 1, 0));
                config.layers[1] = Layer.CreateRoundedRect(new Color(0f, 0f, 0f, 0.15f), 4f);
                config.layers[2] = Layer.CreateBorder(new Color(0.4f, 0.4f, 0.4f, 1f), 1f, 4f);
                return config;
            }

            private static LayerConfiguration CreateButtonHoverStyle()
            {
                LayerConfiguration config = new LayerConfiguration(3);
                config.layers[0] =
                    Layer.CreateRoundedRect(new Color(0.2f, 0.2f, 0.2f, 1f), 4f, new Padding(0, 1, 1, 0));
                config.layers[1] = Layer.CreateRoundedRect(new Color(0.3f, 0.3f, 0.3f, 1f), 4f);
                config.layers[2] = Layer.CreateBorder(new Color(0.5f, 0.5f, 0.5f, 1f), 1f, 4f);
                return config;
            }

            private static LayerConfiguration CreateButtonActiveStyle()
            {
                LayerConfiguration config = new LayerConfiguration(3);
                config.layers[0] =
                    Layer.CreateRoundedRect(new Color(0.1f, 0.1f, 0.1f, 1f), 4f, new Padding(0, 1, 1, 0));
                config.layers[1] = Layer.CreateRoundedRect(new Color(0.2f, 0.35f, 0.5f, 1f), 4f);
                config.layers[2] = Layer.CreateBorder(new Color(0.3f, 0.5f, 0.7f, 1f), 1f, 4f);
                return config;
            }

            private static GUIStyle CreateButtonLabelStyle()
            {
                GUIStyle labelStyle = new GUIStyle(); // Create from scratch
                labelStyle.fontSize = 12;
                labelStyle.fontStyle = FontStyle.Bold;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                labelStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                labelStyle.padding = new RectOffset(0, 0, 0, 0);
                labelStyle.margin = new RectOffset(0, 0, 0, 0);
                labelStyle.contentOffset = Vector2.zero;
                labelStyle.clipping = TextClipping.Overflow;
                return labelStyle;
            }
        }
    }
}