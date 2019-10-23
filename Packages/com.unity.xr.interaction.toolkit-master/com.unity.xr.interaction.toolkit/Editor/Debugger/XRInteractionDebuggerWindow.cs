#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace UnityEngine.XR.Interaction.Toolkit
{
    class XRInteractionDebuggerWindow : EditorWindow
    {
        static XRInteractionDebuggerWindow s_Instance;
        [SerializeField] Vector2 m_ScrollPosition;
        [SerializeField] bool m_ShowInputDevices;
        [SerializeField] bool m_ShowInteractors;
        [SerializeField] bool m_ShowInteractables;

        [SerializeField] Vector2 m_InputDevicesTreeScrollPosition;
        [NonSerialized] XRInputDevicesTreeView m_InputDevicesTree;
        [SerializeField] TreeViewState m_InputDevicesTreeState;
        [SerializeField] MultiColumnHeaderState m_InputDevicesTreeHeaderState;

        [SerializeField] Vector2 m_InteractablesTreeScrollPosition;
        [NonSerialized] XRInteractablesTreeView m_InteractablesTree;
        [SerializeField] TreeViewState m_InteractablesTreeState;
        [SerializeField] MultiColumnHeaderState m_InteractablesTreeHeaderState;

        [SerializeField] Vector2 m_InteractorsTreeScrollPosition;
        [NonSerialized] XRInteractorsTreeView m_InteractorsTree;
        [SerializeField] TreeViewState m_InteractorsTreeState;
        [SerializeField] MultiColumnHeaderState m_InteractorsTreeHeaderState;

        [SerializeField] XRInteractionManager m_InteractionManager { set; get; }

        [MenuItem("Window/Analysis/XR Interaction Debugger", false, 2100)]

        public static void Init()
        {
            if (s_Instance == null)
            {
                s_Instance = GetWindow<XRInteractionDebuggerWindow>();
                s_Instance.Show();
                s_Instance.titleContent = new GUIContent("XR Interaction Debugger");
            }
            else
            {
                s_Instance.Show();
                s_Instance.Focus();
            }
        }

        void SetupInputDevicesTree()
        {
            if (m_InputDevicesTreeState == null)
                m_InputDevicesTreeState = new TreeViewState();
            m_InputDevicesTree = XRInputDevicesTreeView.Create(ref m_InputDevicesTreeState, ref m_InputDevicesTreeHeaderState);
            m_InputDevicesTree.ExpandAll();
        }

        void SetupInteractorsTree()
        {
            m_InteractionManager = UnityEngine.Object.FindObjectOfType<XRInteractionManager>();
            if (m_InteractionManager)
            {
                m_InteractorsTree = XRInteractorsTreeView.Create(m_InteractionManager, ref m_InteractorsTreeState, ref m_InteractorsTreeHeaderState);
                m_InteractorsTree.ExpandAll();
            }
        }

        void SetupInteractablesTree()
        {
            m_InteractionManager = UnityEngine.Object.FindObjectOfType<XRInteractionManager>();
            if (m_InteractionManager)
            {
                m_InteractablesTree = XRInteractablesTreeView.Create(m_InteractionManager, ref m_InteractablesTreeState, ref m_InteractablesTreeHeaderState);
                m_InteractablesTree.ExpandAll();
            }
        }

        public void OnInspectorUpdate()
        {
            // TODO: Only do this when devices or interaction manager updates
            SetupInputDevicesTree();
            SetupInteractorsTree();
            SetupInteractablesTree();

            if (m_InputDevicesTree != null)
            {
                m_InputDevicesTree.Reload();
                m_InputDevicesTree.Repaint();
            }
            if (m_InteractablesTree != null)
                m_InteractablesTree.Repaint();
            Repaint();
        }
        
        public void OnGUI()
        {
            DrawToolbarGUI();
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            if (m_ShowInputDevices && m_InputDevicesTree != null)
                DrawInputDevicesGUI();
            if (m_ShowInteractors && m_InteractorsTree != null)
                DrawInteractorsGUI();
            if (m_ShowInteractables && m_InteractablesTree != null)
                DrawInteractablesGUI();
                
            EditorGUILayout.EndScrollView();
        }

        void DrawInputDevicesGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Devices", GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            m_InputDevicesTreeScrollPosition = EditorGUILayout.BeginScrollView(m_InputDevicesTreeScrollPosition);
            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            m_InputDevicesTree.OnGUI(rect);
            EditorGUILayout.EndScrollView();
        }

        void DrawInteractorsGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Interactors", GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            ////REVIEW: I'm not sure tree view needs a scroll view or whether it does that automatically
            m_InteractorsTreeScrollPosition = EditorGUILayout.BeginScrollView(m_InteractorsTreeScrollPosition);
            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            m_InteractorsTree.OnGUI(rect);
            EditorGUILayout.EndScrollView();
        }

        void DrawInteractablesGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Interactables", GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            ////REVIEW: I'm not sure tree view needs a scroll view or whether it does that automatically
            m_InteractablesTreeScrollPosition = EditorGUILayout.BeginScrollView(m_InteractablesTreeScrollPosition);
            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            m_InteractablesTree.OnGUI(rect);
            EditorGUILayout.EndScrollView();
        }

        void DrawToolbarGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            m_ShowInputDevices 
                = GUILayout.Toggle(m_ShowInputDevices, Contents.showInputDevices, EditorStyles.toolbarButton);
            m_ShowInteractables 
                = GUILayout.Toggle(m_ShowInteractables, Contents.showInteractablesContent, EditorStyles.toolbarButton);
            m_ShowInteractors 
                = GUILayout.Toggle(m_ShowInteractors, Contents.showInteractorsContent, EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        static class Styles
        {
        }

        static class Contents
        {
            public static GUIContent noneContent = new GUIContent("None");
            public static GUIContent showInputDevices = new GUIContent("Input Devices");
            public static GUIContent showInteractablesContent = new GUIContent("Interactables");
            public static GUIContent showInteractorsContent = new GUIContent("Interactors");
        }
    }
}

#endif // UNITY_EDITOR
