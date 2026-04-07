using System.Collections.Generic;
using System.Linq;
using GameplayIngredients;
using GameplayIngredients.Actions;
using GameplayIngredients.Events;
using GameplayIngredients.Logic;
using GameplayIngredients.StateMachines;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;

// Aliases typés
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<int>;
using TreeView = UnityEditor.IMGUI.Controls.TreeView<int>;
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;

namespace GameplayIngredients.Editor
{
    public class CallTreeWindow : EditorWindow
    {
        [SerializeField] TreeViewState m_TreeViewState;
        CallTreeTreeView m_TreeView;

        string m_Search = "";
        GameObject m_ObjectFilter;

        [MenuItem("Window/Gameplay Ingredients/Callable Tree Explorer", priority = MenuItems.kWindowMenuPriority)]
        static void Open()
        {
            var w = GetWindow<CallTreeWindow>();
            w.titleContent = new GUIContent("Callable Tree Explorer");
            w.Show();
        }

        void OnEnable()
        {
            if (m_TreeViewState.Equals(default(TreeViewState)))
                m_TreeViewState = new TreeViewState();

            m_TreeView = new CallTreeTreeView(m_TreeViewState);
            ReloadTree();
        }

        void OnHierarchyChange()
        {
            ReloadTree();
        }

        void ReloadTree()
        {
            m_TreeView.Build(m_ObjectFilter, m_Search);
            Repaint();
        }

        void OnGUI()
        {
            DrawToolbar();

            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            m_TreeView.OnGUI(rect);
        }

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    ReloadTree();

                GUILayout.Space(8);

                // Search
                string newSearch = GUILayout.TextField(m_Search, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.Width(200));
                if (newSearch != m_Search)
                {
                    m_Search = newSearch;
                    ReloadTree();
                }

                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    m_Search = "";
                    ReloadTree();
                }

                GUILayout.FlexibleSpace();

                // Object filter
                var newObj = (GameObject)EditorGUILayout.ObjectField(m_ObjectFilter, typeof(GameObject), true, GUILayout.Width(200));
                if (newObj != m_ObjectFilter)
                {
                    m_ObjectFilter = newObj;
                    ReloadTree();
                }
            }
        }
    }

    class CallTreeTreeView : TreeView
    {
        string m_Search = "";
        GameObject m_ObjectFilter;

        public CallTreeTreeView(TreeViewState state) : base(state)
        {
            showBorder = true;
            Reload();
        }

        public void Build(GameObject objectFilter, string search)
        {
            m_ObjectFilter = objectFilter;
            m_Search = search?.ToLower() ?? "";
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            // Root (id = 0, depth = -1) obligatoire
            var root = new TreeViewItem(0, -1, "Root");

            var categories = new List<TreeViewItem>();

            AddCategory<EventBase>("Events", categories);
            AddCategory<LogicBase>("Logic", categories);
            AddCategory<ActionBase>("Actions", categories);
            AddCategory<StateMachine>("State Machines", categories);
            AddCategory<Factory>("Factories", categories);

            if (categories.Count == 0)
            {
                root.AddChild(new TreeViewItem(1, 0, "No callables found"));
            }
            else
            {
                foreach (var cat in categories)
                    root.AddChild(cat);
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        void AddCategory<T>(string categoryName, List<TreeViewItem> categories) where T : MonoBehaviour
        {
            var list = Resources.FindObjectsOfTypeAll<T>()
                .Where(o => o.gameObject.scene.IsValid() && o.gameObject.scene.isLoaded)
                .ToList();

            if (list.Count == 0)
                return;

            var cat = new TreeViewItem(categoryName.GetHashCode(), 0, categoryName)
            {
                icon = EditorGUIUtility.FindTexture("Folder Icon")
            };
            categories.Add(cat);

            foreach (var mb in list)
            {
                if (m_ObjectFilter != null && mb.gameObject != m_ObjectFilter)
                    continue;

                string name = mb.name;
                if (!string.IsNullOrEmpty(m_Search) && !name.ToLower().Contains(m_Search))
                    continue;

                // Unity 6 : utiliser GetEntityId() au lieu de GetInstanceID()
                int id = mb.GetEntityId().GetHashCode();

                var item = new TreeViewItem(id, 1, name)
                {
                    icon = GetIconFor(mb)
                };
                cat.AddChild(item);
            }

            if (cat.children == null || cat.children.Count == 0)
            {
                categories.Remove(cat);
            }
        }

        Texture2D GetIconFor(Object obj)
        {
            if (obj is EventBase) return EditorGUIUtility.FindTexture("d_UnityEvent Icon");
            if (obj is LogicBase) return EditorGUIUtility.FindTexture("d_FilterByLabel");
            if (obj is ActionBase) return EditorGUIUtility.FindTexture("d_PlayButton On");
            if (obj is StateMachine) return EditorGUIUtility.FindTexture("d_AnimatorController Icon");
            if (obj is Factory) return EditorGUIUtility.FindTexture("Prefab Icon");
            return EditorGUIUtility.FindTexture("d_DefaultAsset Icon");
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args); // look standard Unity
        }
    }
}
