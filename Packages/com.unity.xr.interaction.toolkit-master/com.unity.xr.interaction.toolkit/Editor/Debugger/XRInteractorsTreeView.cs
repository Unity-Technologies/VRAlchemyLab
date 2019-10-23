#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    // Multi-column TreeView that shows Interactors
    class XRInteractorsTreeView : TreeView
    {
        public static XRInteractorsTreeView Create(XRInteractionManager interactionManager, ref TreeViewState treeState, ref MultiColumnHeaderState headerState)
        {
            if (treeState == null)
                treeState = new TreeViewState();

            var newHeaderState = CreateHeaderState();
            if (headerState != null)
                MultiColumnHeaderState.OverwriteSerializedFields(headerState, newHeaderState);
            headerState = newHeaderState;

            var header = new MultiColumnHeader(headerState);
            return new XRInteractorsTreeView(interactionManager, treeState, header);
        }

        const float kRowHeight = 20f;

        class Item : TreeViewItem
        {
            public XRBaseInteractor interactor;
        }

        enum ColumnId
        {
            Name,
            Type,
            HoverActive,
            SelectActive,
            HoverInteractable,
            SelectInteractable,

            COUNT
        }

        XRInteractionManager m_InteractionManager;

        static MultiColumnHeaderState CreateHeaderState()
        {
            var columns = new MultiColumnHeaderState.Column[(int)ColumnId.COUNT];

            columns[(int)ColumnId.Name] =
                new MultiColumnHeaderState.Column
            {
                width = 180,
                minWidth = 60,
                headerContent = new GUIContent("Name")
            };
            columns[(int)ColumnId.Type] =
                new MultiColumnHeaderState.Column
            {
                width = 120,
                minWidth = 60,
                headerContent = new GUIContent("Type")
            };
            columns[(int)ColumnId.HoverActive] =
                new MultiColumnHeaderState.Column { width = 120, headerContent = new GUIContent("Hover Active") };
            columns[(int)ColumnId.SelectActive] =
                new MultiColumnHeaderState.Column { width = 120, headerContent = new GUIContent("Select Active") };
            columns[(int)ColumnId.HoverInteractable] =
                new MultiColumnHeaderState.Column {width = 140, headerContent = new GUIContent("Hover Interactable")};
            columns[(int)ColumnId.SelectInteractable] =
                new MultiColumnHeaderState.Column {width = 140, headerContent = new GUIContent("Select Interactable")};

            return new MultiColumnHeaderState(columns);
        }

        XRInteractorsTreeView(XRInteractionManager manager, TreeViewState state, MultiColumnHeader header)
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
            if (m_InteractionManager != null && m_InteractionManager.interactors.Count > 0)
            {
                var children = new List<TreeViewItem>();
                foreach (var interactor in m_InteractionManager.interactors)
                {
                    var childItem = new Item
                    {
                        id = id,
                        displayName = interactor.name,
                        interactor = interactor,
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

        List<XRBaseInteractable> m_HoverTargetList = new List<XRBaseInteractable>();
        void ColumnGUI(Rect cellRect, Item item, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            if (column == (int)ColumnId.Name)
            {
                args.rowRect = cellRect;
                base.RowGUI(args);
            }

            if (item.interactor != null)
            {
                switch (column)
                {
                    case (int)ColumnId.Type:
                        GUI.Label(cellRect, item.interactor.GetType().Name);
                        break;
                    case (int)ColumnId.HoverActive:
                        GUI.Label(cellRect, item.interactor.isHoverActive.ToString());
                        break;
                    case (int)ColumnId.SelectActive:
                        GUI.Label(cellRect, item.interactor.isSelectActive.ToString());
                        break;
                    case (int)ColumnId.HoverInteractable:
                        item.interactor.GetHoverTargets(m_HoverTargetList);
                        if (m_HoverTargetList.Count > 0)
                        {
                            foreach (var target in m_HoverTargetList)
                                GUI.Label(cellRect, target.name);
                        }
                        break;
                    case (int)ColumnId.SelectInteractable:
                        if (item.interactor.selectTarget != null)
                            GUI.Label(cellRect, item.interactor.selectTarget.name);
                        break;
                }
            }
        }
    }
}
#endif // UNITY_EDITOR
