#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    // Multi-column TreeView that shows Interactables
    class XRInteractablesTreeView : TreeView
    {
        public static XRInteractablesTreeView Create(XRInteractionManager interactionManager, ref TreeViewState treeState, ref MultiColumnHeaderState headerState)
        {
            if (treeState == null)
                treeState = new TreeViewState();

            var newHeaderState = CreateHeaderState();
            if (headerState != null)
                MultiColumnHeaderState.OverwriteSerializedFields(headerState, newHeaderState);
            headerState = newHeaderState;

            var header = new MultiColumnHeader(headerState);
            return new XRInteractablesTreeView(interactionManager, treeState, header);
        }

        const float kRowHeight = 20f;

        class Item : TreeViewItem
        {
            public XRBaseInteractable interactable;
        }

        enum ColumnId
        {
            Name,
            Type,
            LayerMask,
            Colliders,
            Hover,
            Select,

            COUNT
        }

        XRInteractionManager m_InteractionManager;

        static MultiColumnHeaderState CreateHeaderState()
        {
            var columns = new MultiColumnHeaderState.Column[(int)ColumnId.COUNT];

            columns[(int)ColumnId.Name]         = new MultiColumnHeaderState.Column { width = 180, minWidth = 80, headerContent = new GUIContent("Name") };
            columns[(int)ColumnId.Type]         = new MultiColumnHeaderState.Column {width = 120, minWidth = 80, headerContent = new GUIContent("Type") };
            columns[(int)ColumnId.LayerMask]    = new MultiColumnHeaderState.Column { width = 120, minWidth = 80, headerContent = new GUIContent("Layer Mask") };
            columns[(int)ColumnId.Colliders]    = new MultiColumnHeaderState.Column { width = 120, minWidth = 80, headerContent = new GUIContent("Colliders") };
            columns[(int)ColumnId.Hover]        = new MultiColumnHeaderState.Column {width = 80, minWidth = 80, headerContent = new GUIContent("Hover") };
            columns[(int)ColumnId.Select]       = new MultiColumnHeaderState.Column {width = 80, minWidth = 80, headerContent = new GUIContent("Select") };

            return new MultiColumnHeaderState(columns);
        }

        XRInteractablesTreeView(XRInteractionManager manager, TreeViewState state, MultiColumnHeader header)
            : base(state, header)
        {
            m_InteractionManager = manager;
            showBorder = false;
            rowHeight = kRowHeight;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var rootItem = BuildInteractableTree();

            // Wrap root control in invisible item required by TreeView.
            return new Item
            {
                displayName = "Interaction Manager",
                id = 0,
                children = new List<TreeViewItem> {rootItem},
                depth = -1
            };
        }

        TreeViewItem BuildInteractableTree()
        {
            int id = 0;
            var rootItem = new Item
            {
                id = id++,
                displayName = m_InteractionManager == null ? "-" : m_InteractionManager.name,
                depth = 0
            };

            // Build children.
            if (m_InteractionManager != null && m_InteractionManager.interactables.Count > 0)
            {
                var children = new List<TreeViewItem>();
                foreach (var interactable in m_InteractionManager.interactables)
                {
                    var childItem = new Item
                    {
                        id = id,
                        displayName = interactable.name,
                        interactable = interactable,
                        depth = 1
                    };
                    childItem.parent = rootItem;
                    children.Add(childItem);
                }

                // Sort children by name.
                children.Sort((a, b) => string.Compare(a.displayName, b.displayName));
                rootItem.children = children;
            }

            return rootItem;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (Item)args.item;

            var columnCount = args.GetNumVisibleColumns();
            for (var i = 0; i < columnCount; ++i)
            {
                ColumnGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
            }
        }

        void ColumnGUI(Rect cellRect, Item item, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            if (column == (int)ColumnId.Name)
            {
                args.rowRect = cellRect;
                base.RowGUI(args);
            }

            if (item.interactable != null)
            {
                switch (column)
                {
                    case (int)ColumnId.Type:
                        GUI.Label(cellRect, item.interactable.GetType().Name);
                        break;
                    case (int)ColumnId.LayerMask:
                        GUI.Label(cellRect, item.interactable.interactionLayerMask.value.ToString());
                        break;
                    case (int)ColumnId.Colliders:
                        var colliderNames = item.interactable.colliders.Select(x => x.gameObject.name).ToList<string>();
                        GUI.Label(cellRect, string.Join(",", colliderNames.ToArray()));
                        break;
                    case (int)ColumnId.Hover:
                        GUI.Label(cellRect, item.interactable.isHovered ? "True" : "False");
                        break;
                    case (int)ColumnId.Select:
                        GUI.Label(cellRect, item.interactable.isSelected ? "True" : "False");
                        break;
                }
            }
        }
    }
}
#endif // UNITY_EDITOR
