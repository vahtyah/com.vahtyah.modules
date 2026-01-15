using System;
using UnityEngine;

namespace VahTyah
{
    [Serializable]
    public class CustomListStyle
    {
        [SerializeField] public string name = "Default Style";

        [Header("Features")]
        public bool enableHeader = false;
        public bool enableSearch = false;
        public bool enableFooterAddButton = true;
        public bool enableFooterRemoveButton = true;
        public bool enableElementRemoveButton = false;
        public bool ignoreDragEvents = false;

        [Header("Dimensions")]
        public float minHeight = 200f;
        public float minWidth = 150f;
        public bool stretchHeight = true;
        public bool stretchWidth = true;

        [Header("Components")]
        public Header header;
        public SearchField searchField;
        public ElementList list;
        public Element element;
        public DragHandle dragHandle;
        public RemoveElementButton removeElementButton;
        public Pagination pagination;
        public FooterButtons footerButtons;
        public GlobalBackground globalBackground;

        [Header("Messages")]
        public string emptyListMessage = "List is empty";
        public string noResultsMessage = "No results found";

        public void SetDefaultStyleValues()
        {
            name = "Default Style";

            // Features
            enableHeader = false;
            enableSearch = false;
            enableFooterAddButton = true;
            enableFooterRemoveButton = true;
            enableElementRemoveButton = false;
            ignoreDragEvents = false;

            // Dimensions
            minHeight = 200f;
            minWidth = 150f;
            stretchHeight = true;
            stretchWidth = true;

            // Messages
            emptyListMessage = "List is empty";
            noResultsMessage = "No results found";

            // Header
            header = new Header();
            header.height = 20f;
            header.contentPaddingLeft = 6f;
            header.contentPaddingRight = 6f;
            header.contentPaddingTop = 2f;
            header.contentPaddingBottom = 2f;
            header. textColor = Color.white;
            
            // ✅ Dùng trực tiếp LayerConfiguration
            header.backgroundConfig = new LayerConfiguration(1);
            header.backgroundConfig.layers[0] = new Layer();
            header. backgroundConfig.layers[0].type = LayerType.Border;
            header.backgroundConfig. layers[0].color = new Color(0.22f, 0.22f, 0.22f, 1f);
            header.backgroundConfig.layers[0].borderWidth = new Vector4(0, 0, 0, 1);
            header.backgroundConfig. layers[0].borderRadius = Vector4.zero;
            header. backgroundConfig.layers[0].padding = new Padding();

            // Search
            searchField = new SearchField();
            searchField.height = 22f;
            searchField.contentPaddingLeft = 6f;
            searchField.contentPaddingRight = 6f;
            searchField.contentPaddingTop = 2f;
            searchField.contentPaddingBottom = 2f;
            searchField.clearButtonWidth = 16f;
            searchField.clearButtonText = "×";
            searchField.clearButtonTextColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            searchField.clearButtonHoverColor = new Color(1f, 0.3f, 0.3f, 1f);
            
            searchField.backgroundConfig = new LayerConfiguration(1);
            searchField.backgroundConfig.layers[0] = new Layer();
            searchField.backgroundConfig.layers[0]. type = LayerType.Border;
            searchField.backgroundConfig. layers[0].color = new Color(0.22f, 0.22f, 0.22f, 1f);
            searchField.backgroundConfig.layers[0].borderWidth = new Vector4(0, 0, 0, 1);
            searchField.backgroundConfig.layers[0]. borderRadius = Vector4.zero;
            searchField.backgroundConfig. layers[0].padding = new Padding();

            // List
            list = new ElementList();
            list.contentPaddingLeft = 6f;
            list.contentPaddingRight = 6f;
            list.contentPaddingTop = 2f;
            list.contentPaddingBottom = 2f;
            list.backgroundConfig = new LayerConfiguration(0); // Transparent

            // Element
            element = new Element();
            element.collapsedElementHeight = 20f;
            element.headerPaddingLeft = 0f;
            element.headerPaddingRight = 6f;
            element.headerPaddingTop = 0f;
            element.headerPaddingBottom = 0f;
            element.textColor = Color.white;
            
            element.selectedBackgroundConfig = new LayerConfiguration(1);
            element.selectedBackgroundConfig.layers[0] = Layer.CreateSolidColor(
                new Color(0.172549f, 0.3647059f, 0.5294118f, 1f)
            );
            
            element.unselectedBackgroundConfig = new LayerConfiguration(0); // Transparent
            element.hoverBackgroundConfig = new LayerConfiguration(1);
            element.hoverBackgroundConfig.layers[0] = Layer.CreateSolidColor(
                new Color(0.3f, 0.3f, 0.3f, .5f)
            );

            // Drag Handle
            dragHandle = new DragHandle();
            dragHandle.paddingLeft = 5f;
            dragHandle.paddingBottom = 6f;
            dragHandle.width = 10f;
            dragHandle.height = 6f;
            dragHandle.allocatedHorizontalSpace = 20f;

            // Remove Element Button
            removeElementButton = new RemoveElementButton();
            removeElementButton.width = 20f;
            removeElementButton.height = 20f;
            removeElementButton.paddingRight = 0f;
            removeElementButton. paddingLeft = 6f;
            removeElementButton. allocatedHorizontalSpace = 26f;
            removeElementButton. text = "X";
            removeElementButton.fontSize = 16;
            removeElementButton.textColor = Color.white;

            // Pagination
            pagination = new Pagination();
            pagination.height = 20f;
            pagination.contentPaddingLeft = 6f;
            pagination.contentPaddingRight = 6f;
            pagination.contentPaddingTop = 2f;
            pagination. contentPaddingBottom = 2f;
            pagination.buttonsWidth = 25f;
            pagination.buttonsHeight = 16f;
            
            pagination.backgroundConfig = new LayerConfiguration(1);
            pagination.backgroundConfig. layers[0] = new Layer();
            pagination.backgroundConfig.layers[0].type = LayerType.Border;
            pagination.backgroundConfig.layers[0].color = new Color(0.22f, 0.22f, 0.22f, 1f);
            pagination.backgroundConfig.layers[0].borderWidth = new Vector4(0, 1, 0, 0);
            pagination.backgroundConfig. layers[0].borderRadius = Vector4.zero;
            pagination. backgroundConfig.layers[0].padding = new Padding();

            // Footer Buttons
            footerButtons = new FooterButtons();
            footerButtons.height = 20f;
            footerButtons.marginRight = 10f;
            footerButtons. spaceBetweenButtons = 0f;
            footerButtons. paddingTop = 4f;
            footerButtons. paddingLeft = 4f;
            footerButtons. paddingRight = 4f;
            footerButtons.buttonsWidth = 25f;
            footerButtons.buttonsHeight = 16f;
            
            footerButtons.backgroundConfig = new LayerConfiguration(2);
            
            footerButtons.backgroundConfig. layers[0] = new Layer();
            footerButtons.backgroundConfig.layers[0].type = LayerType.RoundedRect;
            footerButtons.backgroundConfig.layers[0].color = new Color(0.302f, 0.302f, 0.302f, 1f);
            footerButtons. backgroundConfig.layers[0].borderWidth = Vector4.one * 100;
            footerButtons.backgroundConfig.layers[0].borderRadius = new Vector4(0, 0, 4, 4);
            footerButtons.backgroundConfig.layers[0].padding = new Padding(0, 0, 0, 0);
            
            footerButtons.backgroundConfig. layers[1] = new Layer();
            footerButtons.backgroundConfig.layers[1].type = LayerType.Border;
            footerButtons.backgroundConfig.layers[1].color = new Color(0.141f, 0.141f, 0.141f, 1f);
            footerButtons. backgroundConfig.layers[1].borderWidth = new Vector4(1, 0, 1, 1);
            footerButtons.backgroundConfig.layers[1].borderRadius = new Vector4(0, 0, 4, 4);
            footerButtons.backgroundConfig.layers[1].padding = new Padding(0, 0, 0, 0);

            // Global Background
            globalBackground = new GlobalBackground();
            globalBackground.backgroundConfig = new LayerConfiguration(2);
            
            globalBackground.backgroundConfig.layers[0] = new Layer();
            globalBackground.backgroundConfig.layers[0].type = LayerType.RoundedRect;
            globalBackground.backgroundConfig.layers[0]. color = new Color(0.302f, 0.302f, 0.302f, 1f);
            globalBackground.backgroundConfig.layers[0].borderWidth = Vector4.one * 100;
            globalBackground.backgroundConfig.layers[0].borderRadius = Vector4.one * 4;
            globalBackground.backgroundConfig.layers[0]. padding = new Padding(0, 0, 1, 21);
            
            globalBackground. backgroundConfig.layers[1] = new Layer();
            globalBackground. backgroundConfig.layers[1].type = LayerType.Border;
            globalBackground.backgroundConfig.layers[1].color = new Color(0.141f, 0.141f, 0.141f, 1f);
            globalBackground.backgroundConfig.layers[1].borderWidth = Vector4.one * 1;
            globalBackground.backgroundConfig.layers[1].borderRadius = Vector4.one * 4;
            globalBackground.backgroundConfig. layers[1].padding = new Padding(0, 0, 0, 20);
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
            public LayerConfiguration backgroundConfig; // ← Dùng trực tiếp
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
            public LayerConfiguration backgroundConfig; // ← Dùng trực tiếp
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
            public LayerConfiguration backgroundConfig; // ← Dùng trực tiếp
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
            public LayerConfiguration backgroundConfig; // ← Dùng trực tiếp
        }

        [Serializable]
        public class GlobalBackground
        {
            public LayerConfiguration backgroundConfig; // ← Dùng trực tiếp
        }
    }
}