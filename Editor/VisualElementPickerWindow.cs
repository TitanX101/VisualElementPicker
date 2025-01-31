using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Empress.UI {

    public class VisualElementPickerWindow : EditorWindow {

        // =====================================================================================================================

        #region Enums & Structs
        enum SearchFilter { NONE, BY_TYPE, BY_TYPE_NAME, BY_STRING }

        struct Filter {
            public SearchFilter searchFilter;
            public Type targetType;
            public string targetStr;
            public bool skipEmpty;
        }

        struct EntryData {
            public string name;
            public string type;
        }
        #endregion

        #region Classes
        sealed class EntryItem : VisualElement {
            public string Text {
                get => m_TextLabel.text;
                set => m_TextLabel.text = value;
            }

            public string Type {
                get => m_TypeLabel.text;
                set => m_TypeLabel.text = value;
            }

            public Action update;
            public Action onClick;

            private readonly Label m_TextLabel;
            private readonly Label m_TypeLabel;

            public EntryItem() : base() {
                style.flexDirection = FlexDirection.Row;
                style.flexGrow = 1;
                RegisterCallback<ClickEvent>(evt => onClick?.Invoke());

                // Primary label
                Add(m_TextLabel = new Label());
                m_TextLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                m_TextLabel.RegisterCallback<GeometryChangedEvent>(evt => update?.Invoke());

                // Secundary label
                Add(m_TypeLabel = new Label("Type"));
                m_TypeLabel.style.unityTextAlign = TextAnchor.MiddleRight;
                m_TypeLabel.style.flexGrow = 1;
                m_TypeLabel.style.color = Color.gray;
                m_TypeLabel.style.marginRight = 10;
            }

            public float GetTextSize() {
                return m_TextLabel.resolvedStyle.width + GetMeasureSize(m_TextLabel) + GetMeasureSize(m_TypeLabel);
            }

            float GetMeasureSize(Label label) {
                const MeasureMode MEASURE_MODE = MeasureMode.Undefined;
                var result = label.MeasureTextSize(label.text, label.resolvedStyle.width, MEASURE_MODE, label.resolvedStyle.height, MEASURE_MODE);
                return result.x;
            }
        }
        #endregion

        // =====================================================================================================================

        const float WINDOW_HEIGHT = 400;
        TreeView m_TreeView;
        Filter m_DefaultFilter;
        VisualElement m_RootElement;
        Action<string> m_OnPick;
        
        // =====================================================================================================================

        public static void Show(Rect buttonRect, VisualElementPickerAttribute att, VisualElement root, Action<string> onPick) {
            var window = CreateInstance<VisualElementPickerWindow>();
            window.m_DefaultFilter = new Filter { 
                searchFilter = att.TargetType != null ? SearchFilter.BY_TYPE : SearchFilter.NONE,
                targetType = att.TargetType,
                skipEmpty = att.SkipEmpty
            };
            window.m_RootElement = root;
            window.m_OnPick = onPick;

            window.ShowAsDropDown(
                new Rect(GUIUtility.GUIToScreenPoint(buttonRect.position), Vector2.zero),
                new Vector2(buttonRect.width, WINDOW_HEIGHT)
            );
            window.InitTreeView();
        }

        void InitTreeView() {
            m_TreeView = new TreeView {
                showBorder = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                makeItem = MakeItem,
                bindItem = BindItem
            };

            BuildTreeView(m_DefaultFilter);

            // Search field
            var searchField = new ToolbarSearchField();
            searchField.style.marginRight = 10;

            // Update search
            searchField.RegisterValueChangedCallback(evt => {
                // Filter the TreeView based on the search value
                var searchValue = evt.newValue.ToLower();

                if (string.IsNullOrEmpty(searchValue)) {
                    // If the search value is empty, go to default list
                    BuildTreeView(m_DefaultFilter);
                }
                else if (searchValue.StartsWith("t:")) {
                    // Search by type
                    var searchType = searchValue[2..].Trim();
                    BuildTreeView(new Filter { 
                        searchFilter = SearchFilter.BY_TYPE_NAME, 
                        targetStr = searchType,
                        skipEmpty = m_DefaultFilter.skipEmpty
                    });
                }
                else {
                    // Search by name
                    BuildTreeView(new Filter { 
                        searchFilter = SearchFilter.BY_STRING, 
                        targetStr = searchValue,
                        skipEmpty = m_DefaultFilter.skipEmpty
                    });
                }
            });

            // Add the SearchField and TreeView to the root element
            rootVisualElement.Add(searchField);
            rootVisualElement.Add(m_TreeView);
        }

        void BuildTreeView(Filter filter) {
            var data = new List<TreeViewItemData<EntryData>>();
            var index = 0;

            PopulateHierarchy(m_RootElement, data, ref index, filter);

            m_TreeView.Clear();
            m_TreeView.SetRootItems(data);
            m_TreeView.Rebuild();
            m_TreeView.ExpandAll();
        }

        void PopulateHierarchy(VisualElement root, List<TreeViewItemData<EntryData>> data, ref int index, Filter filter) {
            foreach (var child in root.Children()) {
                var isEmpty = string.IsNullOrEmpty(child.name);
                var elementType = child.GetType();

                var entry = new EntryData {
                    name = isEmpty ? "VisualElement" : ("#" + child.name),
                    type = elementType.Name
                };

                if (filter.searchFilter != SearchFilter.NONE || filter.skipEmpty) {
                    var match = false;

                    switch (filter.searchFilter) {
                        case SearchFilter.NONE:
                            match = true; // Just skip empty entries
                            break;
                        case SearchFilter.BY_TYPE:
                            match = elementType == filter.targetType;
                            break;
                        case SearchFilter.BY_TYPE_NAME:
                            match = elementType.Name.ToLower() == filter.targetStr;
                            break;
                        case SearchFilter.BY_STRING:
                            match = child.name.ToLower().Contains(filter.targetStr);
                            break;
                    }

                    if (match && (!filter.skipEmpty || (filter.skipEmpty && !isEmpty))) {
                        var id = index++;
                        data.Add(new TreeViewItemData<EntryData>(id, entry, null));
                    }

                    PopulateHierarchy(child, data, ref index, filter);
                }
                else {
                    var id = index++;
                    var subItems = new List<TreeViewItemData<EntryData>>();
                    PopulateHierarchy(child, subItems, ref index, filter);
                    data.Add(new TreeViewItemData<EntryData>(id, entry, subItems));
                }
            }
        }

        VisualElement MakeItem() {
            var item = new EntryItem();

            // Update width
            item.update = () => {
                var textSize = item.GetTextSize();

                if (textSize <= minSize.x)
                    return;

                var newSize = minSize;
                newSize.x = textSize;
                minSize = newSize;
            };

            // Pick the item
            item.onClick = () => {
                var result = item.Text.Equals("VisualElement") ? "" : item.Text;
                result = result.StartsWith("#") ? result[1..] : result;
                m_OnPick?.Invoke(result);
                Close();
            };

            return item;
        }

        void BindItem(VisualElement element, int index) {
            var item = (EntryItem)element;
            var id = m_TreeView.GetIdForIndex(index);
            var data = m_TreeView.GetItemDataForId<EntryData>(id);

            item.Text = data.name;
            item.Type = data.type;

            if (item.Text.Equals("VisualElement"))
                item.style.color = StyleKeyword.Initial;
            else
                item.style.color = new StyleColor(new Color32(50, 70, 180, 255));
        }
    }
}
