using System;
using System.Collections.Generic;
using UnityEngine;

namespace VahTyah
{
    // [CreateAssetMenu(fileName = "StylesDatabase", menuName = "Custom List/List Styles Database", order = 1)]
    public class LevelEditorStylesDatabase : ScriptableObject
    {
        [SerializeField, BoxGroup("Settings")] private int defaultStyleIndex = 0;

        [SerializeField, BoxGroup("Styles")]
        private List<LevelEditorStyleData> styles = new List<LevelEditorStyleData>();

        [Button]
        public void AddDefaultStyle()
        {
            styles.Add(GetDefaultStyle());
        }

        public LevelEditorStyleData GetStyle()
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

        public static LevelEditorStyleData GetDefaultStyle()
        {
            LevelEditorStyleData defaultStyleData = new LevelEditorStyleData
            {
                globalBackground = LevelEditorStyleData.CreateDefaultStyleData().globalBackground,
                customListStyle = LevelEditorStyleData.CreateDefaultStyleData().customListStyle,
                panelNavigatorStyles = LevelEditorStyleData.CreateDefaultStyleData().panelNavigatorStyles
            };
            return defaultStyleData;
        }
    }

    [Serializable]
    public class LevelEditorStyleData
    {
        public GlobalBackground globalBackground;
        public CustomListStyle customListStyle;
        public PanelNavigatorStyles panelNavigatorStyles;

        public static LevelEditorStyleData CreateDefaultStyleData()
        {
            LevelEditorStyleData styleData = new LevelEditorStyleData
            {
                globalBackground = GlobalBackground.CreateDefaultStyles(),
                customListStyle = CustomListStyle.CreateDefaultStyles(),
                panelNavigatorStyles = PanelNavigatorStyles.CreateDefaultStyles()
            };
            return styleData;
        }


        [Serializable]
        public class GlobalBackground
        {
            public LayerConfiguration backgroundConfig;
            
            public static GlobalBackground CreateDefaultStyles()
            {
                GlobalBackground globalBackground = new GlobalBackground
                {
                    backgroundConfig = new LayerConfiguration(2)
                };

                globalBackground.backgroundConfig.layers[0] = new Layer
                {
                    type = LayerType.RoundedRect,
                    color = new Color(0.302f, 0.302f, 0.302f, 1f),
                    borderWidth = Vector4.one * 100,
                    borderRadius = Vector4.one * 4,
                    padding = new Padding(1, 1, 1, 1)
                };

                globalBackground.backgroundConfig.layers[1] = new Layer
                {
                    type = LayerType.Border,
                    color = new Color(0.141f, 0.141f, 0.141f, 1f),
                    borderWidth = Vector4.one * 1,
                    borderRadius = Vector4.one * 4,
                    padding = new Padding(1, 1, 1, 1)
                };

                return globalBackground;
            }
        }

        [Serializable]
        public class CustomListStyle
        {
            [Header("Features")] public bool enableHeader = false;
            public bool enableSearch = false;
            public bool enableFooterAddButton = true;
            public bool enableFooterRemoveButton = true;
            public bool enableElementRemoveButton = false;
            public bool ignoreDragEvents = false;

            [Header("Dimensions")] public float minHeight = 200f;
            public float minWidth = 150f;
            public bool stretchHeight = true;
            public bool stretchWidth = true;

            [Header("Components")] public Header header;
            public SearchField searchField;
            public ElementList list;
            public Element element;
            public DragHandle dragHandle;
            public RemoveElementButton removeElementButton;
            public Pagination pagination;
            public FooterButtons footerButtons;

            [Header("Messages")] public string emptyListMessage = "List is empty";
            public string noResultsMessage = "No results found";

            public static CustomListStyle CreateDefaultStyles()
            {
                CustomListStyle styles = new CustomListStyle
                {
                    enableHeader = false,
                    enableSearch = false,
                    enableFooterAddButton = true,
                    enableFooterRemoveButton = true,
                    enableElementRemoveButton = false,
                    ignoreDragEvents = false,

                    minHeight = 200f,
                    minWidth = 150f,
                    stretchHeight = true,
                    stretchWidth = true,

                    emptyListMessage = "List is empty",
                    noResultsMessage = "No results found",

                    header = CreateHeaderStyle(),
                    searchField = CreateSearchFieldStyle(),
                    list = CreateElementListStyle(),
                    element = CreateElementStyle(),
                    dragHandle = CreateDragHandleStyle(),
                    removeElementButton = CreateRemoveElementButtonStyle(),
                    pagination = CreatePaginationStyle(),
                    footerButtons = CreateFooterButtonsStyle(),
                };

                return styles;
            }

            private static Header CreateHeaderStyle()
            {
                Header header = new Header
                {
                    height = 20f,
                    contentPaddingLeft = 6f,
                    contentPaddingRight = 6f,
                    contentPaddingTop = 2f,
                    contentPaddingBottom = 2f,
                    textColor = Color.white,
                    backgroundConfig = new LayerConfiguration(1)
                };

                header.backgroundConfig.layers[0] = new Layer
                {
                    type = LayerType.Border,
                    color = new Color(0.22f, 0.22f, 0.22f, 1f),
                    borderWidth = new Vector4(0, 0, 0, 1),
                    borderRadius = Vector4.zero,
                    padding = new Padding()
                };

                return header;
            }

            private static SearchField CreateSearchFieldStyle()
            {
                SearchField searchField = new SearchField
                {
                    height = 22f,
                    contentPaddingLeft = 6f,
                    contentPaddingRight = 6f,
                    contentPaddingTop = 2f,
                    contentPaddingBottom = 2f,
                    clearButtonWidth = 16f,
                    clearButtonText = "×",
                    clearButtonTextColor = new Color(0.5f, 0.5f, 0.5f, 0.8f),
                    clearButtonHoverColor = new Color(1f, 0.3f, 0.3f, 1f),
                    backgroundConfig = new LayerConfiguration(1)
                };

                searchField.backgroundConfig.layers[0] = new Layer
                {
                    type = LayerType.Border,
                    color = new Color(0.22f, 0.22f, 0.22f, 1f),
                    borderWidth = new Vector4(0, 0, 0, 1),
                    borderRadius = Vector4.zero,
                    padding = new Padding()
                };

                return searchField;
            }

            private static ElementList CreateElementListStyle()
            {
                return new ElementList
                {
                    contentPaddingLeft = 6f,
                    contentPaddingRight = 6f,
                    contentPaddingTop = 2f,
                    contentPaddingBottom = 2f,
                    backgroundConfig = new LayerConfiguration(0)
                };
            }

            private static Element CreateElementStyle()
            {
                Element element = new Element
                {
                    collapsedElementHeight = 20f,
                    headerPaddingLeft = 0f,
                    headerPaddingRight = 6f,
                    headerPaddingTop = 0f,
                    headerPaddingBottom = 0f,
                    textColor = Color.white,
                    selectedBackgroundConfig = new LayerConfiguration(1),
                    unselectedBackgroundConfig = new LayerConfiguration(0),
                    hoverBackgroundConfig = new LayerConfiguration(1)
                };

                element.selectedBackgroundConfig.layers[0] = Layer.CreateSolidColor(
                    new Color(0.172549f, 0.3647059f, 0.5294118f, 1f)
                );

                element.hoverBackgroundConfig.layers[0] = Layer.CreateSolidColor(
                    new Color(0.3f, 0.3f, 0.3f, 0.5f)
                );

                return element;
            }

            private static DragHandle CreateDragHandleStyle()
            {
                return new DragHandle
                {
                    paddingLeft = 5f,
                    paddingBottom = 6f,
                    width = 10f,
                    height = 6f,
                    allocatedHorizontalSpace = 20f
                };
            }

            private static RemoveElementButton CreateRemoveElementButtonStyle()
            {
                return new RemoveElementButton
                {
                    width = 20f,
                    height = 20f,
                    paddingRight = 0f,
                    paddingLeft = 6f,
                    allocatedHorizontalSpace = 26f,
                    text = "X",
                    fontSize = 16,
                    textColor = Color.white
                };
            }

            private static Pagination CreatePaginationStyle()
            {
                Pagination pagination = new Pagination
                {
                    height = 20f,
                    contentPaddingLeft = 6f,
                    contentPaddingRight = 6f,
                    contentPaddingTop = 2f,
                    contentPaddingBottom = 2f,
                    buttonsWidth = 25f,
                    buttonsHeight = 16f,
                    backgroundConfig = new LayerConfiguration(1)
                };

                pagination.backgroundConfig.layers[0] = new Layer
                {
                    type = LayerType.Border,
                    color = new Color(0.22f, 0.22f, 0.22f, 1f),
                    borderWidth = new Vector4(0, 1, 0, 0),
                    borderRadius = Vector4.zero,
                    padding = new Padding()
                };

                return pagination;
            }

            private static FooterButtons CreateFooterButtonsStyle()
            {
                FooterButtons footerButtons = new FooterButtons
                {
                    height = 20f,
                    marginRight = 10f,
                    spaceBetweenButtons = 0f,
                    paddingTop = 4f,
                    paddingLeft = 4f,
                    paddingRight = 4f,
                    buttonsWidth = 25f,
                    buttonsHeight = 16f,
                    backgroundConfig = new LayerConfiguration(2)
                };

                footerButtons.backgroundConfig.layers[0] = new Layer
                {
                    type = LayerType.RoundedRect,
                    color = new Color(0.302f, 0.302f, 0.302f, 1f),
                    borderWidth = Vector4.one * 100,
                    borderRadius = new Vector4(0, 0, 4, 4),
                    padding = new Padding(0, 0, 0, 0)
                };

                footerButtons.backgroundConfig.layers[1] = new Layer
                {
                    type = LayerType.Border,
                    color = new Color(0.141f, 0.141f, 0.141f, 1f),
                    borderWidth = new Vector4(1, 0, 1, 1),
                    borderRadius = new Vector4(0, 0, 4, 4),
                    padding = new Padding(0, 0, 0, 0)
                };

                return footerButtons;
            }

            [Serializable]
            public class Header
            {
                public float height;
                public float contentPaddingLeft;
                public float contentPaddingRight;
                public float contentPaddingTop;
                public float contentPaddingBottom;
                public Color textColor = Color.white;
                public LayerConfiguration backgroundConfig;
            }

            [Serializable]
            public class SearchField
            {
                public float height;
                public float contentPaddingLeft;
                public float contentPaddingRight;
                public float contentPaddingTop;
                public float contentPaddingBottom;
                public float clearButtonWidth;
                public string clearButtonText;
                public Color clearButtonTextColor;
                public Color clearButtonHoverColor;
                public LayerConfiguration backgroundConfig;
            }

            [Serializable]
            public class ElementList
            {
                public float contentPaddingLeft;
                public float contentPaddingRight;
                public float contentPaddingTop;
                public float contentPaddingBottom;
                public LayerConfiguration backgroundConfig;
            }

            [Serializable]
            public class Element
            {
                public float collapsedElementHeight;
                public float headerPaddingLeft;
                public float headerPaddingRight;
                public float headerPaddingTop;
                public float headerPaddingBottom;
                public Color textColor = Color.white;
                public LayerConfiguration selectedBackgroundConfig;
                public LayerConfiguration unselectedBackgroundConfig;
                public LayerConfiguration hoverBackgroundConfig;
            }

            [Serializable]
            public class DragHandle
            {
                public float paddingLeft;
                public float paddingBottom;
                public float width;
                public float height;
                public float allocatedHorizontalSpace;
            }

            [Serializable]
            public class RemoveElementButton
            {
                public float width;
                public float height;
                public float paddingRight;
                public float paddingLeft;
                public float allocatedHorizontalSpace;
                public string text;
                public int fontSize;
                public Color textColor = Color.white;
            }

            [Serializable]
            public class Pagination
            {
                public float height;
                public float contentPaddingLeft;
                public float contentPaddingRight;
                public float contentPaddingTop;
                public float contentPaddingBottom;
                public float buttonsWidth;
                public float buttonsHeight;
                public LayerConfiguration backgroundConfig;
            }

            [Serializable]
            public class FooterButtons
            {
                public float height;
                public float marginRight;
                public float spaceBetweenButtons;
                public float paddingTop;
                public float paddingLeft;
                public float paddingRight;
                public float buttonsWidth;
                public float buttonsHeight;
                public LayerConfiguration backgroundConfig;
            }
        }

        [Serializable]
        public class PanelNavigatorStyles
        {
            public float menuBarHeight;
            public LayerConfiguration activeTabStyle;
            public LayerConfiguration inactiveTabStyle;

            public static PanelNavigatorStyles CreateDefaultStyles()
            {
                PanelNavigatorStyles styles = new PanelNavigatorStyles
                {
                    menuBarHeight = 28f,
                    activeTabStyle = CreateActiveTabStyle(),
                    inactiveTabStyle = CreateInactiveTabStyle()
                };
                return styles;
            }

            private static LayerConfiguration CreateActiveTabStyle()
            {
                LayerConfiguration config = new LayerConfiguration(2);
                config.layers[0] = new Layer();
                config.layers[0].type = LayerType.RoundedRect;
                config.layers[0].color = new Color(0.302f, 0.302f, 0.302f, 1f);
                config.layers[0].borderWidth = Vector4.one * 100;
                config.layers[0].borderRadius = new Vector4(4, 4, 0, 0);
                config.layers[0].padding = new Padding(2, 2, 2, 0);

                config.layers[1] = new Layer();
                config.layers[1].type = LayerType.Border;
                config.layers[1].color = new Color(0.141f, 0.141f, 0.141f, 1f);
                config.layers[1].borderWidth = new Vector4(1, 1, 1, 0);
                config.layers[1].borderRadius = new Vector4(4, 4, 0, 0);
                config.layers[1].padding = new Padding(2, 2, 2, 0);
                return config;
            }

            private static LayerConfiguration CreateInactiveTabStyle()
            {
                LayerConfiguration config = new LayerConfiguration(1);
                config.layers[0] = new Layer();
                config.layers[0].type = LayerType.RoundedRect;
                config.layers[0].color = new Color(0.302f, 0.302f, 0.302f, .5f);
                config.layers[0].borderWidth = Vector4.one * 100;
                config.layers[0].borderRadius = new Vector4(4, 4, 0, 0);
                config.layers[0].padding = new Padding(2, 2, 2, 0);
                return config;
            }
        }
    }
}