using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VahTyah
{
    public class CustomList
    {
        // Data
        private SerializedObject serializedObject;
        private SerializedProperty elementsProperty;
        private List<SerializedProperty> elementsList;
        private IList elements;

        // Data mode flags
        private bool usingPropertyList = false;
        private bool usingListInterface = false;

        // Display settings
        private float minHeight = 200;
        private float minWidth = 150;
        private bool stretchHeight = true;
        private bool stretchWidth = true;

        // Style
        private GUIStyle controlStyle;

        // Rects
        private Rect globalRect;
        private Rect headerRect;
        private Rect headerContentRect;
        private Rect searchRect;
        private Rect searchFieldRect;
        private Rect searchClearButtonRect;
        private Rect listRect;
        private Rect listContentRect;
        private Rect footerPaginationRect;
        private Rect footerButtonsRect;
        private Rect buttonsRect;
        private Rect footerButtonRect;
        private Rect elementRect;
        private Rect elementHeaderRect;
        private Rect labelRect;
        private Rect removeButtonRect;
        private Rect draggingHandleRect;
        private Rect filledElementsRect;

        // Pagination rects
        private Rect paginationContentRect;
        private Rect firstPageButtonRect;
        private Rect previousPageButtonRect;
        private Rect nextPageButtonRect;
        private Rect lastPageButtonRect;
        private Rect paginationLabelRect;

        // Layer Configurations
        // private LayerConfiguration globalBackgroundConfig;
        // private LayerConfiguration headerBackgroundConfig;
        // private LayerConfiguration listBackgroundConfig;
        // private LayerConfiguration paginationBackgroundConfig;
        // private LayerConfiguration selectedElementConfig;
        // private LayerConfiguration unselectedElementConfig;
        private LevelEditorStyleData currentCustomStyle;
        private LevelEditorStyleData.CustomListStyle customListStyle;
        private LevelEditorStyleData.GlobalBackground globalBackgroundStyle;

        // State
        private int selectedIndex = -1;
        private bool executedOnce = false;
        private Event currentEvent;
        private int prevIndent;
        private Rect calculatedGlobalRect;
        private EditorWindow parentWindow;

        // Drag & Drop
        private bool dragging = false;
        private int startDragIndex = -1;
        private int currentDragIndex = -1;
        private float dragOffset = 0;
        private float draggedElementY = 0;
        private float draggedElementHeight = 20;
        private Vector2 lastMouseDownPosition;
        private int lastMouseDownIndex = -1;
        private float lastMouseDownHeight = 20;
        private bool isSelected = false;

        //Double-click detection
        private double lastClickTime = 0;
        private int lastClickedIndex = -1;
        private const double DOUBLE_CLICK_TIME = 0.3;

        // Element display
        private float collapsedElementHeight = 20;
        private string labelPropertyName;
        private bool useLabelProperty;

        // Pagination
        private bool enablePagination = false;
        private int currentPage = 0;
        private int pagesCount = 0;
        private int pageBeginIndex = 0;
        private int pageElementCount = 0;
        private int maxElementCount = 0;

        // Delegates
        public delegate string GetLabelDelegate(SerializedProperty elementProperty, int elementIndex);

        public delegate string GetHeaderLabelCallbackDelegate();

        public delegate void SelectionChangedCallbackDelegate();

        public delegate void ListChangedCallbackDelegate();

        public delegate void AddElementCallbackDelegate();

        public delegate void RemoveElementCallbackDelegate();

        public delegate void ListReorderedCallbackDelegate();

        public delegate void ListReorderedCallbackWithDetailsDelegate(int srcIndex, int destIndex);

        public delegate void AddElementWithDropdownCallbackDelegate(Rect buttonRect);

        public delegate void DisplayContextMenuCallbackDelegate(int index);

        public delegate void ListUndoCallbackDelegate(string undoMessage);

        public delegate void ElementDoubleClickedDelegate(int index);

        public delegate bool SearchFilterDelegate(SerializedProperty elementProperty, int elementIndex,
            string searchQuery);


        private GetLabelDelegate getLabelCallback;
        public GetHeaderLabelCallbackDelegate getHeaderLabelCallback;
        public SelectionChangedCallbackDelegate selectionChangedCallback;
        public ListChangedCallbackDelegate listChangedCallback;
        public AddElementCallbackDelegate addElementCallback;
        public RemoveElementCallbackDelegate removeElementCallback;
        public AddElementWithDropdownCallbackDelegate addElementWithDropdownCallback;
        public ListReorderedCallbackDelegate listReorderedCallback;
        public ListReorderedCallbackWithDetailsDelegate listReorderedCallbackWithDetails;
        public DisplayContextMenuCallbackDelegate displayContextMenuCallback;
        public ListUndoCallbackDelegate listUndoCallback;
        public ElementDoubleClickedDelegate elementDoubleClickedCallback;
        public SearchFilterDelegate searchFilterCallback;

        // Settings
        public bool enableHeader = false;
        public bool enableFooterAddButton = true;
        public bool enableFooterRemoveButton = true;
        public bool enableElementRemoveButton = false;
        public bool ignoreDragEvents = false;
        public bool enableSearch = false;

        // Search state
        private string searchQuery = "";
        private List<int> filteredIndices = new List<int>();
        private bool isSearchActive = false;

        //mesages
        private string emptyListMessage = "List is empty";
        private string noResultsMessage = "No results found";

        // Properties
        public int SelectedIndex
        {
            get => selectedIndex;
            set => selectedIndex = value;
        }

        public float MinHeight
        {
            get => minHeight;
            set => minHeight = value;
        }

        public float MinWidth
        {
            get => minWidth;
            set => minWidth = value;
        }

        public bool StretchHeight
        {
            get => stretchHeight;
            set => stretchHeight = value;
        }

        public bool StretchWidth
        {
            get => stretchWidth;
            set => stretchWidth = value;
        }

        public bool IgnoreDragEvents
        {
            get => ignoreDragEvents;
            set => ignoreDragEvents = value;
        }

        public EditorWindow ParentWindow
        {
            get => parentWindow ?? LevelEditorBase.Window;
            set => parentWindow = value;
        }

        public LevelEditorStyleData CurrentCustomStyle
        {
            get => currentCustomStyle;
        }

        public void RequestRepaint()
        {
            if (ParentWindow != null)
            {
                ParentWindow.Repaint();
            }
        }

        // Constants
        private const float HEADER_HEIGHT = 20f;
        private const float SEARCH_HEIGHT = 22f;
        private const float PAGINATION_HEIGHT = 20f;
        private const float FOOTER_HEIGHT = 20f;
        private const float DRAG_HANDLE_WIDTH = 10f;
        private const float DRAG_HANDLE_ALLOCATED_SPACE = 20f;
        private const float REMOVE_BUTTON_WIDTH = 20f;
        private const float REMOVE_BUTTON_ALLOCATED_SPACE = 26f;


        #region Constructors

        public CustomList(SerializedObject serializedObject, SerializedProperty elements,
            string labelPropertyName)
        {
            this.serializedObject = serializedObject;
            this.labelPropertyName = labelPropertyName;
            useLabelProperty = true;
            elementsProperty = elements;
            usingPropertyList = false;
            LoadCustomStyle();
        }

        public CustomList(SerializedObject serializedObject, SerializedProperty elements,
            GetLabelDelegate getLabelCallback)
        {
            this.serializedObject = serializedObject;
            this.getLabelCallback = getLabelCallback;
            useLabelProperty = false;
            elementsProperty = elements;
            usingPropertyList = false;
            LoadCustomStyle();
        }

        public CustomList(SerializedObject serializedObject, List<SerializedProperty> propertyList,
            string labelPropertyName)
        {
            this.serializedObject = serializedObject;
            this.labelPropertyName = labelPropertyName;
            useLabelProperty = true;
            elementsList = propertyList;
            usingPropertyList = true;
            LoadCustomStyle();
        }

        public CustomList(SerializedObject serializedObject, List<SerializedProperty> propertyList,
            GetLabelDelegate getLabelCallback)
        {
            this.serializedObject = serializedObject;
            this.getLabelCallback = getLabelCallback;
            useLabelProperty = false;
            elementsList = propertyList;
            usingPropertyList = true;
            LoadCustomStyle();
        }

        public CustomList(IList elements, GetLabelDelegate getLabelCallback)
        {
            this.getLabelCallback = getLabelCallback;
            useLabelProperty = false;
            this.elements = elements;
            usingListInterface = true;
            LoadCustomStyle();
        }

        #endregion

        #region Initialization

        public void LoadCustomStyle(int index = 0)
        {
            LevelEditorStylesDatabase levelEditorStylesDatabase = EditorUtils.GetAsset<LevelEditorStylesDatabase>();

            if (levelEditorStylesDatabase == null)
            {
                currentCustomStyle = LevelEditorStylesDatabase.GetDefaultStyle();
            }
            else currentCustomStyle = levelEditorStylesDatabase.GetStyle();

            if (currentCustomStyle == null)
            {
                customListStyle = LevelEditorStyleData.CustomListStyle.CreateDefaultStyles();
                globalBackgroundStyle = LevelEditorStyleData.GlobalBackground.CreateDefaultStyles();
                return;
            }
            
            customListStyle = currentCustomStyle.customListStyle;
            globalBackgroundStyle = currentCustomStyle.globalBackground;

            var listStyle = currentCustomStyle.customListStyle;
            enableHeader = listStyle.enableHeader;
            enableSearch = listStyle.enableSearch;
            enableFooterAddButton = listStyle.enableFooterAddButton;
            enableFooterRemoveButton = listStyle.enableFooterRemoveButton;
            enableElementRemoveButton = listStyle.enableElementRemoveButton;
            ignoreDragEvents = listStyle.ignoreDragEvents;

            minHeight = listStyle.minHeight;
            minWidth = listStyle.minWidth;
            stretchHeight = listStyle.stretchHeight;
            stretchWidth = listStyle.stretchWidth;

            collapsedElementHeight = listStyle.element.collapsedElementHeight;

            emptyListMessage = listStyle.emptyListMessage;
            noResultsMessage = listStyle.noResultsMessage;

            RequestRepaint();
        }
        private void ExecuteOnce()
        {
            if (executedOnce) return;
            executedOnce = true;

            // Control style
            controlStyle = new GUIStyle();
            controlStyle.stretchHeight = stretchHeight;
            controlStyle.stretchWidth = stretchWidth;

            // Initialize rects
            globalRect = new Rect();
            calculatedGlobalRect = new Rect();
            headerRect = new Rect();
            headerContentRect = new Rect();
            listRect = new Rect();
            listContentRect = new Rect();
            footerPaginationRect = new Rect();
            footerButtonsRect = new Rect();
            buttonsRect = new Rect();
            footerButtonRect = new Rect();
            elementRect = new Rect();
            elementHeaderRect = new Rect();
            labelRect = new Rect();
            removeButtonRect = new Rect();
            draggingHandleRect = new Rect();
            paginationContentRect = new Rect();
            firstPageButtonRect = new Rect();
            previousPageButtonRect = new Rect();
            nextPageButtonRect = new Rect();
            lastPageButtonRect = new Rect();
            paginationLabelRect = new Rect();
            filledElementsRect = new Rect();
        }

        #endregion

        #region Data Methods

        public SerializedProperty GetElement(int index)
        {
            if (usingListInterface) return null;

            if (index >= ArraySize() || index < 0)
            {
                Debug.LogError("Index out of bounds:  " + index);
                return null;
            }

            if (usingPropertyList)
                return elementsList[index];
            else
                return elementsProperty.GetArrayElementAtIndex(index);
        }

        public int ArraySize()
        {
            if (usingListInterface)
                return elements.Count;
            else if (usingPropertyList)
                return elementsList.Count;
            else
                return elementsProperty.arraySize;
        }

        public void MoveElement(int srcIndex, int destIndex)
        {
            if (srcIndex == destIndex) return;

            string elementName = "Element";
            if (!usingListInterface)
            {
                SerializedProperty prop = GetElement(srcIndex);
                elementName = GetElementLabel(prop, srcIndex);
            }

            UndoCallback($"Reorder {elementName}");

            if (usingListInterface)
            {
                var item = elements[srcIndex];
                elements.RemoveAt(srcIndex);
                elements.Insert(destIndex, item);
            }
            else if (usingPropertyList)
            {
                SerializedProperty temp = elementsList[srcIndex];
                elementsList.RemoveAt(srcIndex);
                elementsList.Insert(destIndex, temp);
            }
            else
            {
                elementsProperty.MoveArrayElement(srcIndex, destIndex);
                serializedObject.ApplyModifiedProperties();
            }

            listChangedCallback?.Invoke();
        }

        #endregion

        #region Display

        public void Display()
        {
            ExecuteOnce();

            currentEvent = Event.current;
            prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            DoCalculations();

            // Draw global background
            LayerDrawingSystem.DrawLayers(globalRect, globalBackgroundStyle.backgroundConfig);

            if (enableHeader)
            {
                DrawHeader();
            }

            if (enableSearch)
            {
                DrawSearch();
            }

            DrawList();

            if (enablePagination)
            {
                DrawPagination();
            }

            if (enableFooterAddButton || enableFooterRemoveButton)
            {
                DrawFooterButtons();
            }

            // Handle drag & drop detection
            if (currentEvent.isMouse && ArraySize() > 0 &&
                currentEvent.type != EventType.Used && !ignoreDragEvents && !isSearchActive)
            {
                HandleDraggingDetection();
            }

            if (currentEvent.type == EventType.ScrollWheel && ArraySize() > 0)
            {
                HandleScrollWheel();
            }

            if (currentEvent.type == EventType.KeyDown && ArraySize() > 0)
            {
                HandleKeyboardNavigation();
            }

            EditorGUI.indentLevel = prevIndent;
        }

        private void DoCalculations()
        {
            // Get rect with stretch
            globalRect = GUILayoutUtility.GetRect(
                GUIContent.none,
                controlStyle,
                GUILayout.MinHeight(minHeight),
                GUILayout.MinWidth(minWidth)
            );

            if (globalRect.height < 5)
            {
                return;
            }

            // Skip recalculation if rect hasn't changed
            if (calculatedGlobalRect == globalRect &&
                (currentEvent.type != EventType.Layout && currentEvent.type != EventType.Repaint))
            {
                return;
            }

            calculatedGlobalRect = globalRect;

            // Calculate available height for list content
            float availableHeight = globalRect.height;

            // Subtract header
            if (enableHeader)
                availableHeight -= HEADER_HEIGHT;

            if (enableSearch)
                availableHeight -= SEARCH_HEIGHT;

            // Subtract footer
            availableHeight -= FOOTER_HEIGHT;

            // Subtract list padding
            availableHeight -= 4;

            // Calculate max elements that can fit WITHOUT pagination
            maxElementCount = Mathf.FloorToInt(availableHeight / collapsedElementHeight);

            int displayCount = isSearchActive ? filteredIndices.Count : ArraySize();

            // Check if pagination is needed
            enablePagination = (displayCount > maxElementCount);

            if (enablePagination)
            {
                // Subtract pagination height
                availableHeight -= PAGINATION_HEIGHT;
                pageElementCount = Mathf.FloorToInt(availableHeight / collapsedElementHeight);
            }
            else
            {
                pageElementCount = maxElementCount;
            }

            // Calculate pagination values
            if (enablePagination)
            {
                pagesCount = Mathf.CeilToInt((displayCount + 0f) / pageElementCount);

                if (pagesCount > 1)
                {
                    currentPage = Mathf.Clamp(currentPage, 0, pagesCount - 1);

                    // Keep selected element in view
                    if (selectedIndex != -1)
                    {
                        int displayIndex = isSearchActive ? filteredIndices.IndexOf(selectedIndex) : selectedIndex;
                        if (displayIndex >= 0)
                        {
                            currentPage = Mathf.FloorToInt((displayIndex + 0f) / pageElementCount);
                        }
                    }
                }

                pageBeginIndex = currentPage * pageElementCount;
            }
            else
            {
                currentPage = 0;
                pageBeginIndex = 0;
                pagesCount = 1;
            }

            // Calculate sub-rects
            listRect.Set(globalRect.x, globalRect.y, globalRect.width, globalRect.height);

            // Header
            if (enableHeader)
            {
                headerRect.Set(globalRect.x, globalRect.y, globalRect.width, HEADER_HEIGHT);
                headerContentRect.Set(
                    headerRect.x + 6,
                    headerRect.y + 2,
                    headerRect.width - 12,
                    headerRect.height - 4
                );
                listRect.yMin += HEADER_HEIGHT;
            }

            if (enableSearch)
            {
                float searchY = enableHeader ? headerRect.yMax : globalRect.y;
                searchRect.Set(globalRect.x, searchY, globalRect.width, SEARCH_HEIGHT);

                searchFieldRect.Set(
                    searchRect.x + 6,
                    searchRect.y + 2,
                    searchRect.width - 12, // Reserve space for clear button
                    searchRect.height - 4
                );

                searchClearButtonRect.Set(
                    searchFieldRect.xMax - 18, // ← Nằm trong search field
                    searchFieldRect.y - 1,
                    20,
                    searchFieldRect.height - 2
                );

                listRect.yMin += SEARCH_HEIGHT;
            }

            // Footer
            float borderWidth = globalBackgroundStyle.backgroundConfig.GetLayerByType(LayerType.Border)?.borderWidth.w ?? 0;
            
            footerButtonsRect.Set(
                globalRect.x,
                globalRect.yMax - FOOTER_HEIGHT + borderWidth - 1,
                globalRect.width,
                FOOTER_HEIGHT
            );

            globalRect.yMax -= FOOTER_HEIGHT - borderWidth;
            listRect.yMax -= FOOTER_HEIGHT;

            // Pagination
            if (enablePagination)
            {
                footerPaginationRect.Set(
                    globalRect.x,
                    footerButtonsRect.y - PAGINATION_HEIGHT,
                    globalRect.width,
                    PAGINATION_HEIGHT
                );
                listRect.yMax -= PAGINATION_HEIGHT;

                // Pagination content
                paginationContentRect.Set(
                    footerPaginationRect.x + 6,
                    footerPaginationRect.y + 2,
                    footerPaginationRect.width - 12,
                    footerPaginationRect.height - 4
                );

                // Pagination buttons
                float buttonWidth = 25f;
                float buttonHeight = 16f;
                float buttonY = paginationContentRect.y + (paginationContentRect.height - buttonHeight) / 2;

                firstPageButtonRect.Set(
                    paginationContentRect.xMin,
                    buttonY,
                    buttonWidth,
                    buttonHeight
                );

                previousPageButtonRect.Set(
                    firstPageButtonRect.xMax,
                    buttonY,
                    buttonWidth,
                    buttonHeight
                );

                nextPageButtonRect.Set(
                    paginationContentRect.xMax - (2 * buttonWidth),
                    buttonY,
                    buttonWidth,
                    buttonHeight
                );

                lastPageButtonRect.Set(
                    paginationContentRect.xMax - buttonWidth,
                    buttonY,
                    buttonWidth,
                    buttonHeight
                );

                paginationLabelRect.Set(
                    previousPageButtonRect.xMax,
                    buttonY,
                    nextPageButtonRect.xMin - previousPageButtonRect.xMax,
                    buttonHeight
                );
            }

            // List content
            listContentRect.Set(
                listRect.x + 6,
                listRect.y + 2,
                listRect.width - 12,
                listRect.height - 4
            );

            // Element header template
            elementHeaderRect.Set(
                listContentRect.x,
                listContentRect.y,
                listContentRect.width,
                collapsedElementHeight
            );

            // Drag handle
            draggingHandleRect.Set(
                elementHeaderRect.x + 5,
                elementHeaderRect.yMax - 6 - 6,
                DRAG_HANDLE_WIDTH,
                6
            );

            // Label
            labelRect.Set(
                elementHeaderRect.x + DRAG_HANDLE_ALLOCATED_SPACE,
                elementHeaderRect.y,
                elementHeaderRect.width - DRAG_HANDLE_ALLOCATED_SPACE,
                elementHeaderRect.height
            );

            // Remove button
            if (enableElementRemoveButton)
            {
                removeButtonRect.Set(
                    elementHeaderRect.xMax - REMOVE_BUTTON_WIDTH,
                    elementHeaderRect.y,
                    REMOVE_BUTTON_WIDTH,
                    collapsedElementHeight
                );
                labelRect.xMax -= REMOVE_BUTTON_ALLOCATED_SPACE;
            }

            // Calculate filled elements rect
            int displayElementCount = enablePagination
                ? Mathf.Min(pageElementCount, ArraySize() - pageBeginIndex)
                : ArraySize();

            filledElementsRect.Set(
                listContentRect.x,
                listContentRect.y,
                listContentRect.width,
                displayElementCount * collapsedElementHeight
            );
        }

        private void DrawHeader()
        {
            LayerDrawingSystem.DrawLayers(headerRect, customListStyle.header.backgroundConfig);

            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.alignment = TextAnchor.MiddleLeft;

            if (currentCustomStyle != null && currentCustomStyle.customListStyle.header != null)
            {
                headerStyle.normal.textColor = currentCustomStyle.customListStyle.header.textColor;
            }

            EditorGUI.LabelField(headerContentRect, GetHeaderLabel(), headerStyle);

            GUIStyle sizeStyle = new GUIStyle(GUI.skin.label);
            sizeStyle.alignment = TextAnchor.MiddleRight;

            if (currentCustomStyle != null && currentCustomStyle.customListStyle.header != null)
            {
                sizeStyle.normal.textColor = currentCustomStyle.customListStyle.header.textColor;
            }

            EditorGUI.LabelField(headerContentRect, $"Size:  {ArraySize()}", sizeStyle);
        }

        private void DrawList()
        {
            LayerDrawingSystem.DrawLayers(listRect, customListStyle.list.backgroundConfig);

            if (isSearchActive && filteredIndices.Count == 0)
            {
                elementRect.Set(
                    listContentRect.x,
                    listContentRect.y,
                    listContentRect.width,
                    collapsedElementHeight
                );
                GUI.Label(elementRect, noResultsMessage);
                return;
            }

            if (ArraySize() == 0)
            {
                elementRect.Set(
                    listContentRect.x,
                    listContentRect.y,
                    listContentRect.width,
                    collapsedElementHeight
                );
                GUI.Label(elementRect, emptyListMessage);
                return;
            }

            float currentY = listContentRect.y;

            List<int> indicesToDraw = isSearchActive ? filteredIndices : null;
            int displayCount = isSearchActive ? filteredIndices.Count : ArraySize();

            int endIndex = enablePagination
                ? Mathf.Min(pageBeginIndex + pageElementCount, displayCount)
                : displayCount;

            if (dragging)
            {
                for (int i = pageBeginIndex; i < endIndex; i++)
                {
                    int actualIndex = isSearchActive ? filteredIndices[i] : i;
                    if (actualIndex == startDragIndex)
                    {
                        if (currentDragIndex == startDragIndex)
                        {
                            currentY += collapsedElementHeight;
                        }

                        continue;
                    }

                    if (actualIndex == currentDragIndex && currentDragIndex != startDragIndex)
                    {
                        currentY += collapsedElementHeight;
                    }

                    isSelected = false;
                    elementRect.Set(
                        listContentRect.x,
                        currentY,
                        listContentRect.width,
                        collapsedElementHeight
                    );
                    DrawElement(elementRect, isSelected, actualIndex);
                    currentY += collapsedElementHeight;
                }

                isSelected = true;
                elementRect.Set(
                    listContentRect.x,
                    draggedElementY,
                    listContentRect.width,
                    draggedElementHeight
                );
                DrawElement(elementRect, isSelected, startDragIndex);
            }
            else
            {
                for (int i = pageBeginIndex; i < endIndex; i++)
                {
                    int actualIndex = isSearchActive ? filteredIndices[i] : i;
                    isSelected = (actualIndex == selectedIndex);

                    elementRect.Set(
                        listContentRect.x,
                        currentY,
                        listContentRect.width,
                        collapsedElementHeight
                    );

                    DrawElement(elementRect, isSelected, actualIndex);

                    currentY += collapsedElementHeight;
                }
            }
        }

        private void DrawPagination()
        {
            LayerDrawingSystem.DrawLayers(footerPaginationRect, customListStyle.pagination.backgroundConfig);

            GUIStyle buttonStyle = new GUIStyle("RL FooterButton");
            buttonStyle.contentOffset = new Vector2(0, -1);
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;

            // First page button
            using (new EditorGUI.DisabledScope(currentPage <= 0))
            {
                if (GUI.Button(firstPageButtonRect, "<<", buttonStyle))
                {
                    currentPage = 0;
                    selectedIndex = -1;
                }
            }

            // Previous page button
            using (new EditorGUI.DisabledScope(currentPage == 0))
            {
                if (GUI.Button(previousPageButtonRect, "<", buttonStyle))
                {
                    currentPage--;
                    selectedIndex = -1;
                }
            }

            string paginationText;
            if (isSearchActive)
            {
                paginationText = $"{currentPage + 1} / {pagesCount} ({filteredIndices.Count} results)";
            }
            else
            {
                paginationText = $"{currentPage + 1} / {pagesCount}";
            }

            // Page label
            GUI.Label(paginationLabelRect, paginationText, labelStyle);

            // Next page button
            using (new EditorGUI.DisabledScope(currentPage == pagesCount - 1))
            {
                if (GUI.Button(nextPageButtonRect, ">", buttonStyle))
                {
                    currentPage++;
                    selectedIndex = -1;
                }
            }

            // Last page button
            using (new EditorGUI.DisabledScope(currentPage >= pagesCount - 1))
            {
                if (GUI.Button(lastPageButtonRect, ">>", buttonStyle))
                {
                    currentPage = pagesCount - 1;
                    selectedIndex = -1;
                }
            }
        }

        private void DrawElement(Rect rect, bool isSelected, int index)
        {
            SerializedProperty currentElementProperty = GetElement(index);

            Rect elementBgRect = new Rect(rect.x, rect.y, rect.width, collapsedElementHeight);

            if (isSelected)
                LayerDrawingSystem.DrawLayers(elementBgRect, customListStyle.element.selectedBackgroundConfig);
            else
                LayerDrawingSystem.DrawLayers(elementBgRect, customListStyle.element.unselectedBackgroundConfig);

            if (currentEvent.type == EventType.Repaint)
            {
                Rect dragRect = GetDragHandleRect(rect);
                GUIStyle dragStyle = new GUIStyle("RL DragHandle");
                dragStyle.Draw(dragRect, false, false, false, false);
            }

            Rect currentLabelRect = new Rect(
                rect.x + DRAG_HANDLE_ALLOCATED_SPACE,
                rect.y,
                rect.width - DRAG_HANDLE_ALLOCATED_SPACE,
                collapsedElementHeight
            );

            if (enableElementRemoveButton)
            {
                currentLabelRect.xMax -= REMOVE_BUTTON_ALLOCATED_SPACE;
            }

            string label = GetElementLabel(currentElementProperty, index);
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            if (currentCustomStyle != null && currentCustomStyle.customListStyle.element != null)
            {
                labelStyle.normal.textColor = currentCustomStyle.customListStyle.element.textColor;
            }

            GUI.Label(currentLabelRect, label);

            if (!dragging && currentEvent.type == EventType.MouseDown &&
                rect.Contains(currentEvent.mousePosition) &&
                currentEvent.button == 1)
            {
                if (selectedIndex != index)
                {
                    OnSelectionChanged(index);
                }

                currentEvent.Use();
                RequestRepaint();
                return;
            }

            if (!dragging && currentEvent.type == EventType.ContextClick &&
                rect.Contains(currentEvent.mousePosition))
            {
                if (displayContextMenuCallback != null)
                {
                    displayContextMenuCallback.Invoke(index);
                }
                else
                {
                    ShowDefaultContextMenu(index);
                }

                currentEvent.Use();
                return;
            }

            Rect headerButtonRect = new Rect(
                currentLabelRect.x,
                currentLabelRect.y,
                currentLabelRect.width,
                currentLabelRect.height
            );

            if (!dragging && currentEvent.type == EventType.MouseUp &&
                headerButtonRect.Contains(currentEvent.mousePosition) &&
                currentEvent.button == 0)
            {
                double currentTime = EditorApplication.timeSinceStartup;
                bool isDoubleClick = false;

                if (lastClickedIndex == index && (currentTime - lastClickTime) < DOUBLE_CLICK_TIME)
                {
                    isDoubleClick = true;

                    lastClickTime = 0;
                    lastClickedIndex = -1;
                }
                else
                {
                    lastClickTime = currentTime;
                    lastClickedIndex = index;
                }

                if (isDoubleClick)
                {
                    if (elementDoubleClickedCallback != null)
                    {
                        elementDoubleClickedCallback.Invoke(index);
                        currentEvent.Use();
                        return;
                    }
                }
                else
                {
                    OnSelectionChanged(index);
                }

                currentEvent.Use();
            }

            if (!dragging && currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                Rect dragHandleRect = GetDragHandleRect(rect);

                dragHandleRect.yMin -= 3;
                dragHandleRect.yMax += 3;

                if (dragHandleRect.Contains(currentEvent.mousePosition))
                {
                    lastMouseDownPosition = currentEvent.mousePosition;
                    lastMouseDownIndex = index;
                    lastMouseDownHeight = rect.height;
                }
            }
        }

        private void DrawFooterButtons()
        {
            float rightEdge = footerButtonsRect.xMax - 10;
            float leftEdge = rightEdge - 4 - 4;
            leftEdge -= 25;

            if (enableFooterAddButton && enableFooterRemoveButton)
                leftEdge -= 25;

            float borderBackground = globalBackgroundStyle.backgroundConfig.GetLayerByType(LayerType.Border)?.borderWidth.z ?? 0;

            buttonsRect.Set(
                leftEdge,
                footerButtonsRect.y - borderBackground,
                rightEdge - leftEdge,
                FOOTER_HEIGHT
            );

            LayerDrawingSystem.DrawLayers(buttonsRect, customListStyle.footerButtons.backgroundConfig);

            float footerButtonHeight = 16f;
            float footerButtonY = buttonsRect.y + (buttonsRect.height - footerButtonHeight) / 2;

            footerButtonRect.Set(
                leftEdge + 4,
                footerButtonY,
                25,
                16
            );

            GUIStyle buttonStyle = new GUIStyle("RL FooterButton");

            if (enableFooterAddButton)
            {
                GUIContent addIcon = addElementWithDropdownCallback != null
                    ? EditorGUIUtility.TrIconContent("Toolbar Plus More")
                    : EditorGUIUtility.TrIconContent("Toolbar Plus");

                if (GUI.Button(footerButtonRect, addIcon, buttonStyle))
                {
                    if (addElementWithDropdownCallback != null)
                    {
                        addElementWithDropdownCallback.Invoke(footerButtonRect);
                    }
                    else
                    {
                        AddElement();
                    }
                }

                footerButtonRect.x += 25;
            }

            if (enableFooterRemoveButton)
            {
                using (new EditorGUI.DisabledScope(selectedIndex < 0 || selectedIndex >= ArraySize()))
                {
                    if (GUI.Button(footerButtonRect, EditorGUIUtility.TrIconContent("Toolbar Minus"), buttonStyle))
                    {
                        RemoveElement();
                    }
                }
            }
        }

        #endregion

        #region Search

        private void DrawSearch()
        {
            LayerDrawingSystem.DrawLayers(searchRect, customListStyle.header.backgroundConfig);

            Rect adjustedSearchFieldRect = searchFieldRect;
            if (!string.IsNullOrEmpty(searchQuery))
            {
                adjustedSearchFieldRect.width -= 20;
            }

            EditorGUI.BeginChangeCheck();

            GUI.SetNextControlName("SearchField");
            string newSearch =
                EditorGUI.TextField(adjustedSearchFieldRect, searchQuery, UnityEditor.EditorStyles.toolbarSearchField);

            if (EditorGUI.EndChangeCheck())
            {
                searchQuery = newSearch;
                UpdateSearchFilter();
                currentPage = 0;
                RequestRepaint();
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                GUIStyle clearButtonStyle = new GUIStyle(GUI.skin.label);
                clearButtonStyle.alignment = TextAnchor.MiddleCenter;
                clearButtonStyle.fontSize = 14;
                clearButtonStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                clearButtonStyle.hover.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                clearButtonStyle.active.textColor = Color.white;

                if (GUI.Button(searchClearButtonRect, "×", clearButtonStyle))
                {
                    searchQuery = "";
                    UpdateSearchFilter();
                    currentPage = 0;
                    GUI.FocusControl(null);
                    RequestRepaint();
                }
            }
        }

        private void UpdateSearchFilter()
        {
            filteredIndices.Clear();

            if (string.IsNullOrEmpty(searchQuery))
            {
                isSearchActive = false;
                return;
            }

            isSearchActive = true;

            for (int i = 0; i < ArraySize(); i++)
            {
                if (MatchesSearchQuery(i))
                {
                    filteredIndices.Add(i);
                }
            }
        }

        private bool MatchesSearchQuery(int index)
        {
            if (string.IsNullOrEmpty(searchQuery))
                return true;

            if (searchFilterCallback != null)
            {
                SerializedProperty prop = GetElement(index);
                return searchFilterCallback(prop, index, searchQuery);
            }

            SerializedProperty element = GetElement(index);
            string label = GetElementLabel(element, index);

            return label.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion

        #region Context Menu

        private void ShowDefaultContextMenu(int index)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Duplicate"), false, () => { DuplicateElement(index); });

            menu.AddSeparator("");

            if (index > 0)
            {
                menu.AddItem(new GUIContent("Move to Top"), false, () =>
                {
                    string elementName = GetElementLabel(GetElement(index), index);
                    UndoCallback($"Move {elementName} to Top");

                    MoveElement(index, 0);
                    OnSelectionChanged(0);
                    RequestRepaint();
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Move to Top"));
            }

            if (index < ArraySize() - 1)
            {
                menu.AddItem(new GUIContent("Move to Bottom"), false, () =>
                {
                    string elementName = GetElementLabel(GetElement(index), index);
                    UndoCallback($"Move {elementName} to Bottom");

                    MoveElement(index, ArraySize() - 1);
                    OnSelectionChanged(ArraySize() - 1);
                    RequestRepaint();
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Move to Bottom"));
            }

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Delete"), false, () =>
            {
                selectedIndex = index;
                RemoveElement();
                RequestRepaint();
            });

            menu.ShowAsContext();
        }

        private void DuplicateElement(int index)
        {
            string elementName = "Element";
            if (!usingListInterface)
            {
                SerializedProperty prop = GetElement(index);
                elementName = GetElementLabel(prop, index);
            }

            UndoCallback($"Duplicate {elementName}");

            if (usingListInterface)
            {
                var item = elements[index];
                elements.Insert(index + 1, item);
            }
            else if (usingPropertyList)
            {
                // Cannot duplicate SerializedProperty directly
                Debug.LogWarning("Duplicate not supported for List<SerializedProperty>");
                return;
            }
            else
            {
                // Insert duplicate using Unity's serialization
                elementsProperty.InsertArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
            }

            listChangedCallback?.Invoke();
            OnSelectionChanged(index + 1);
            RequestRepaint();
        }

        #endregion

        #region Drag & Drop

        private void HandleDraggingDetection()
        {
            if (!dragging)
            {
                // Detect drag start - CHỈ trong vùng list
                if (currentEvent.type == EventType.MouseDrag &&
                    listContentRect.Contains(currentEvent.mousePosition) &&
                    lastMouseDownIndex >= 0 &&
                    (currentEvent.delta.magnitude < 5f) &&
                    ((lastMouseDownPosition - currentEvent.mousePosition).magnitude <= 1f + Mathf.Epsilon))
                {
                    DraggingStarted();
                }
            }
            else
            {
                // Handle drag
                if (currentEvent.type == EventType.MouseDrag)
                {
                    UpdateDrag();
                }
                else if (currentEvent.type == EventType.MouseUp)
                {
                    DraggingFinished();
                }
            }
        }

        private void DraggingStarted()
        {
            dragging = true;
            startDragIndex = lastMouseDownIndex;
            currentDragIndex = startDragIndex;
            draggedElementHeight = lastMouseDownHeight;

            // Tính offset từ vị trí click đến đầu element
            float elementY = listContentRect.y + (startDragIndex - pageBeginIndex) * collapsedElementHeight;
            dragOffset = lastMouseDownPosition.y - elementY;

            draggedElementY = Mathf.Clamp(
                currentEvent.mousePosition.y - dragOffset,
                filledElementsRect.yMin,
                filledElementsRect.yMax - draggedElementHeight
            );

            Debug.Log($"Drag Started:  startIndex={startDragIndex}, currentIndex={currentDragIndex}");

            currentEvent.Use();
        }

        private void UpdateDrag()
        {
            // Update vị trí Y của element đang kéo
            draggedElementY = Mathf.Clamp(
                currentEvent.mousePosition.y - dragOffset,
                filledElementsRect.yMin,
                filledElementsRect.yMax - draggedElementHeight
            );

            // Tính CENTER của element đang drag
            float draggedElementTopRelative = draggedElementY - listContentRect.y;
            float draggedElementCenter = draggedElementTopRelative + (draggedElementHeight * 0.5f);

            // Tính index mà center đang nằm trong đó
            int relativeIndex = Mathf.RoundToInt(draggedElementCenter / collapsedElementHeight);
            currentDragIndex = pageBeginIndex + relativeIndex;

            int endIndex = enablePagination
                ? Mathf.Min(pageBeginIndex + pageElementCount, ArraySize())
                : ArraySize();

            if (enablePagination)
            {
                int minIndex = Mathf.Max(0, pageBeginIndex);
                int maxIndex = Mathf.Min(ArraySize(), endIndex);

                currentDragIndex = Mathf.Clamp(currentDragIndex, minIndex, maxIndex);
            }
            else
            {
                currentDragIndex = Mathf.Clamp(currentDragIndex, 0, ArraySize());
            }

            if (currentEvent.type == EventType.MouseDrag)
            {
                GUI.changed = true;
            }

            currentEvent.Use();
        }

        private void DraggingFinished()
        {
            Debug.Log($"Drag Finished:   from {startDragIndex} to {currentDragIndex}");

            dragging = false;

            if (startDragIndex < 0 || startDragIndex >= ArraySize())
            {
                Debug.LogError($"Invalid startDragIndex: {startDragIndex}, arraySize={ArraySize()}");
                currentEvent.Use();
                return;
            }

            // FIXED: Clamp currentDragIndex về range hợp lệ
            // Nếu có pagination, clamp về cuối page hiện tại
            int targetIndex;
            if (enablePagination)
            {
                int endIndex = Mathf.Min(pageBeginIndex + pageElementCount, ArraySize());
                // Clamp về [pageBeginIndex, endIndex - 1]
                targetIndex = Mathf.Clamp(currentDragIndex, pageBeginIndex, endIndex - 1);
            }
            else
            {
                // Không có pagination:  clamp về toàn bộ array
                targetIndex = Mathf.Clamp(currentDragIndex, 0, ArraySize() - 1);
            }

            Debug.Log($"TargetIndex after clamp: {targetIndex}");

            // Move element nếu có thay đổi
            if (startDragIndex != targetIndex)
            {
                MoveElement(startDragIndex, targetIndex);
                OnSelectionChanged(targetIndex);

                listReorderedCallback?.Invoke();
                listReorderedCallbackWithDetails?.Invoke(startDragIndex, targetIndex);
            }

            currentEvent.Use();
        }

        #endregion

        #region Keyboard Navigation

        private void HandleKeyboardNavigation()
        {
            if (currentEvent.keyCode == KeyCode.None)
                return;

            bool handled = false;

            switch (currentEvent.keyCode)
            {
                case KeyCode.UpArrow:
                    // Move selection up
                    if (selectedIndex > 0)
                    {
                        OnSelectionChanged(selectedIndex - 1);

                        // Auto switch page if needed
                        if (enablePagination && selectedIndex < pageBeginIndex)
                        {
                            currentPage--;
                        }

                        handled = true;
                    }

                    break;

                case KeyCode.DownArrow:
                    // Move selection down
                    if (selectedIndex < ArraySize() - 1)
                    {
                        OnSelectionChanged(selectedIndex + 1);

                        // Auto switch page if needed
                        if (enablePagination && selectedIndex >= pageBeginIndex + pageElementCount)
                        {
                            currentPage++;
                        }

                        handled = true;
                    }

                    break;

                case KeyCode.LeftArrow:
                    // Previous page
                    if (enablePagination && currentPage > 0)
                    {
                        currentPage--;
                        selectedIndex = -1;
                        handled = true;
                    }

                    break;

                case KeyCode.RightArrow:
                    // Next page
                    if (enablePagination && currentPage < pagesCount - 1)
                    {
                        currentPage++;
                        selectedIndex = -1;
                        handled = true;
                    }

                    break;

                case KeyCode.Home:
                    // Select first element
                    if (ArraySize() > 0)
                    {
                        OnSelectionChanged(0);
                        if (enablePagination)
                        {
                            currentPage = 0;
                        }

                        handled = true;
                    }

                    break;

                case KeyCode.End:
                    // Select last element
                    if (ArraySize() > 0)
                    {
                        OnSelectionChanged(ArraySize() - 1);
                        if (enablePagination)
                        {
                            currentPage = pagesCount - 1;
                        }

                        handled = true;
                    }

                    break;

                case KeyCode.Delete:
                case KeyCode.Backspace:
                    // Delete selected element
                    if (selectedIndex >= 0 && selectedIndex < ArraySize())
                    {
                        RemoveElement();
                        handled = true;
                    }

                    break;

                case KeyCode.PageUp:
                    // First page
                    if (enablePagination && currentPage > 0)
                    {
                        currentPage = 0;
                        selectedIndex = -1;
                        handled = true;
                    }

                    break;

                case KeyCode.PageDown:
                    // Last page
                    if (enablePagination && currentPage < pagesCount - 1)
                    {
                        currentPage = pagesCount - 1;
                        selectedIndex = -1;
                        handled = true;
                    }

                    break;
            }

            if (handled)
            {
                currentEvent.Use();
                RequestRepaint();
            }
        }

        #endregion

        #region Scroll Wheel Navigation

        private void HandleScrollWheel()
        {
            if (!enablePagination)
                return;

            if (!listRect.Contains(currentEvent.mousePosition))
                return;

            if (currentEvent.type != EventType.ScrollWheel)
                return;

            float scrollDelta = currentEvent.delta.y;
            bool handled = false;

            if (scrollDelta > 0)
            {
                if (currentPage < pagesCount - 1)
                {
                    currentPage++;
                    handled = true;
                }
            }
            else if (scrollDelta < 0)
            {
                if (currentPage > 0)
                {
                    currentPage--;
                    handled = true;
                }
            }

            if (handled)
            {
                selectedIndex = -1;
                currentEvent.Use();
                RequestRepaint();
            }
        }

        #endregion

        #region Undo System

        private void UndoCallback(string undoMessage)
        {
            if (listUndoCallback != null)
            {
                listUndoCallback.Invoke(undoMessage);
            }
        }

        #endregion

        #region Helper Methods

        private string GetHeaderLabel()
        {
            return getHeaderLabelCallback?.Invoke() ?? "List";
        }

        private string GetElementLabel(SerializedProperty elementProperty, int index)
        {
            if (usingListInterface)
            {
                return getLabelCallback?.Invoke(null, index) ?? $"Element {index}";
            }

            if (useLabelProperty)
            {
                SerializedProperty labelProp = elementProperty.FindPropertyRelative(labelPropertyName);
                return labelProp != null ? labelProp.stringValue : $"Element {index}";
            }
            else
            {
                return getLabelCallback?.Invoke(elementProperty, index) ?? $"Element {index}";
            }
        }

        private void OnSelectionChanged(int index)
        {
            selectedIndex = index;
            selectionChangedCallback?.Invoke();
        }

        private void AddElement()
        {
            UndoCallback("Add Element");
            addElementCallback?.Invoke();
            listChangedCallback?.Invoke();
        }

        private void RemoveElement()
        {
            string elementName = "Element";
            if (selectedIndex >= 0 && selectedIndex < ArraySize())
            {
                if (!usingListInterface)
                {
                    SerializedProperty prop = GetElement(selectedIndex);
                    elementName = GetElementLabel(prop, selectedIndex);
                }
            }

            UndoCallback($"Remove {elementName}");
            removeElementCallback?.Invoke();
            listChangedCallback?.Invoke();
        }

        // Helper method để tính toán vị trí drag handle
        private Rect GetDragHandleRect(Rect elementRect)
        {
            return new Rect(
                elementRect.x + 5,
                elementRect.yMax - 6 - 6,
                DRAG_HANDLE_WIDTH,
                6
            );
        }

        #endregion
    }
}