/* This wizard will replace a selection with an object or prefab.
 * Scene objects will be cloned (destroying their prefab links).
 * Original coding by 'yesfish', nabbed from Unity Forums
 * 'keep parent' added by Dave A (also removed 'rotation' option, using localRotation
 * Updated with new APIs (prefabutility and undo system)
 */
using UnityEngine;
using UnityEditor;
using System.Collections;

public class ReplaceSelected : ScriptableWizard
{
    static GameObject replacement = null;
    static bool keep = false;

    public GameObject ReplacementObject = null;
    public bool KeepOriginals = false;

    [MenuItem("Tools/Replace Selection... _%#R")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard(
            "Replace Selection", typeof(ReplaceSelected), "Replace");
    }

    public ReplaceSelected()
    {
        ReplacementObject = replacement;
        KeepOriginals = keep;
    }

    void OnWizardUpdate()
    {
        replacement = ReplacementObject;
        keep = KeepOriginals;
    }

    void OnWizardCreate()
    {
        if (replacement == null)
            return;

        //Undo.RegisterSceneUndo("Replace Selection");
        //Undo.RegisterCreatedObjectUndo(global, "Undo Replacement");

        Transform[] transforms = Selection.GetTransforms(
            SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable);

        foreach (Transform t in transforms)
        {
            GameObject g;
            PrefabType pref = PrefabUtility.GetPrefabType(replacement);

            if (pref == PrefabType.Prefab || pref == PrefabType.ModelPrefab)
            {
                g = (GameObject)PrefabUtility.InstantiatePrefab(replacement);
            }
            else
            {
                g = (GameObject)Editor.Instantiate(replacement);
            }

            Transform gTransform = g.transform;
            gTransform.parent = t.parent;
            g.name = replacement.name;
            gTransform.localPosition = t.localPosition;
            gTransform.localScale = t.localScale;
            gTransform.localRotation = t.localRotation;

            Undo.RegisterCreatedObjectUndo(g, "Undo Replacement");
        }

        if (!keep)
        {
            foreach (GameObject g in Selection.gameObjects)
            {
                Undo.DestroyObjectImmediate(g);
            }
        }
    }
}