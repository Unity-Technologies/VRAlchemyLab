#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.XR;

namespace UnityEngine.XR.Interaction.Toolkit
{
    // Multi-column TreeView that shows Input Devices
    class XRInputDevicesTreeView : TreeView
    {
        public static XRInputDevicesTreeView Create(ref TreeViewState treeState, ref MultiColumnHeaderState headerState)
        {
            if (treeState == null)
                treeState = new TreeViewState();

            var newHeaderState = CreateHeaderState();
            if (headerState != null)
                MultiColumnHeaderState.OverwriteSerializedFields(headerState, newHeaderState);
            headerState = newHeaderState;

            var header = new MultiColumnHeader(headerState);
            return new XRInputDevicesTreeView(treeState, header);
        }

        const float kRowHeight = 20f;

        class Item : TreeViewItem
        {
            public string deviceRole;
            public string featureType;
            public string featureValue;
        }

        enum ColumnId
        {
            Name,
            Role,
            Type,
            Value,

            COUNT
        }

        static MultiColumnHeaderState CreateHeaderState()
        {
            var columns = new MultiColumnHeaderState.Column[(int)ColumnId.COUNT];

            columns[(int)ColumnId.Name] =
                new MultiColumnHeaderState.Column
                {
                    width = 240,
                    minWidth = 60,
                    headerContent = new GUIContent("Name")
                };
            columns[(int)ColumnId.Role] =
                new MultiColumnHeaderState.Column
                {
                    width = 200,
                    minWidth = 60,
                    headerContent = new GUIContent("Role")
                };
            columns[(int)ColumnId.Type] =
                new MultiColumnHeaderState.Column { width = 200, headerContent = new GUIContent("Type") };
            columns[(int)ColumnId.Value] =
                new MultiColumnHeaderState.Column { width = 200, headerContent = new GUIContent("Value") };

            return new MultiColumnHeaderState(columns);
        }

        XRInputDevicesTreeView(TreeViewState state, MultiColumnHeader header)
            : base(state, header)
        {
            showBorder = false;
            rowHeight = kRowHeight;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var rootItem = BuildInputDevicesTree();

            // Wrap root control in invisible item required by TreeView.
            return new Item
            {
                displayName = "Input Devices",
                id = 0,
                children = new List<TreeViewItem> { rootItem },
                depth = -1
            };
        }

        string GetFeatureValue(InputDevice device, InputFeatureUsage featureUsage)
        {
            switch (featureUsage.type.ToString())
            {
                case "System.Boolean":
                    bool boolValue;
                    if (device.TryGetFeatureValue(featureUsage.As<bool>(), out boolValue))
                        return boolValue.ToString();
                    break;
                case "System.UInt32":
                    uint uintValue;
                    if (device.TryGetFeatureValue(featureUsage.As<uint>(), out uintValue))
                        return uintValue.ToString();
                    break;
                case "System.Single":
                    float floatValue;
                    if (device.TryGetFeatureValue(featureUsage.As<float>(), out floatValue))
                        return floatValue.ToString();
                    break;
                case "UnityEngine.Vector2":
                    Vector2 Vector2Value;
                    if (device.TryGetFeatureValue(featureUsage.As<Vector2>(), out Vector2Value))
                        return Vector2Value.ToString();
                    break;
                case "UnityEngine.Vector3":
                    Vector3 Vector3Value;
                    if (device.TryGetFeatureValue(featureUsage.As<Vector3>(), out Vector3Value))
                        return Vector3Value.ToString();
                    break;
                case "UnityEngine.Quaternion":
                    Quaternion QuaternionValue;
                    if (device.TryGetFeatureValue(featureUsage.As<Quaternion>(), out QuaternionValue))
                        return QuaternionValue.ToString();
                    break;
                case "UnityEngine.XR.Hand":
                    Hand HandValue;
                    if (device.TryGetFeatureValue(featureUsage.As<Hand>(), out HandValue))
                        return HandValue.ToString();
                    break;
                case "UnityEngine.XR.Bone":
                    Bone BoneValue;
                    if (device.TryGetFeatureValue(featureUsage.As<Bone>(), out BoneValue))
                    {
                        Vector3 bonePosition;
                        Quaternion boneRotation;
                        if (BoneValue.TryGetPosition(out bonePosition) && BoneValue.TryGetRotation(out boneRotation))
                            return string.Format("{0}, {1}", bonePosition.ToString(), boneRotation.ToString());
                    }
                    break;
                case "UnityEngine.XR.Eyes":
                    Eyes EyesValue;
                    if (device.TryGetFeatureValue(featureUsage.As<Eyes>(), out EyesValue))
                    {
                        Vector3 fixation, left, right;
                        float leftOpen, rightOpen;
                        if (EyesValue.TryGetFixationPoint(out fixation) &&
                            EyesValue.TryGetLeftEyePosition(out left) &&
                            EyesValue.TryGetRightEyePosition(out right) && 
                            EyesValue.TryGetLeftEyeOpenAmount(out leftOpen) &&
                            EyesValue.TryGetRightEyeOpenAmount(out rightOpen))
                            return string.Format("{0}, {1}, {2}, {3}, {4}", fixation.ToString(), left.ToString(), right.ToString(), leftOpen, rightOpen);
                    }
                    break;
            }

            return "";
        }

        TreeViewItem BuildInputDevicesTree()
        {
            int id = 0;
            var rootItem = new Item
            {
                id = id++,
                displayName = "Devices",
                depth = 0
            };

            // Build children.
            var inputDevices = new List<InputDevice>();
            InputDevices.GetDevices(inputDevices);

            var deviceChildren = new List<TreeViewItem>();

            // Add device children
            foreach (var device in inputDevices)
            {
                var deviceItem = new Item
                {
                    id = device.GetHashCode(),
                    displayName = device.name,
                    // TODO: need to display new characteristics API here.
#pragma warning disable 612, 618                    
                    deviceRole = device.role.ToString(),
#pragma warning restore 612, 618
                    depth = 1
                };
                deviceItem.parent = rootItem;
                
                List<InputFeatureUsage> featureUsages = new List<InputFeatureUsage>();
                device.TryGetFeatureUsages(featureUsages);
                
                var featureChildren = new List<TreeViewItem>();
                foreach (var featureUsage in featureUsages)
                {
                    Type featureType = featureUsage.type;
                    var featureItem = new Item
                    {
                        id = device.GetHashCode() ^ (featureUsage.GetHashCode() >> 1),
                        displayName = featureUsage.name,
                        featureType = featureType.ToString(),
                        featureValue = GetFeatureValue(device, featureUsage),
                        depth = 2
                    };
                    featureItem.parent = deviceItem;
                    featureChildren.Add(featureItem);
                }

                deviceItem.children = featureChildren;
                deviceChildren.Add(deviceItem);
            }

            // Sort deviceChildren by name.
            deviceChildren.Sort((a, b) => string.Compare(a.displayName, b.displayName));
            rootItem.children = deviceChildren;

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

            switch (column)
            {
                case (int)ColumnId.Role:
                    GUI.Label(cellRect, item.deviceRole);
                    break;
                case (int)ColumnId.Type:
                    if (item.depth == 2)
                        GUI.Label(cellRect, item.featureType);
                    break;
                case (int)ColumnId.Value:
                    if (item.depth == 2)
                        GUI.Label(cellRect, item.featureValue);
                    break;
            }
        }
    }
}
#endif // UNITY_EDITOR