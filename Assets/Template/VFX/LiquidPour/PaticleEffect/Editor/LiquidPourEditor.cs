using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LiquidPour))]
public class LiquidPourEditor : Editor
{
    void OnSceneGUI()
    {
        if (Event.current.type == EventType.Repaint)
        {
            var lp = target as LiquidPour;
            if (lp == null)
                return;
            var q = Quaternion.Euler(90, 0, 0);
            Color oldHandleColor = Handles.color;
            Handles.color = Color.red;
            //Handles.CircleCap(0, lp.transform.position, q, 0.1f);
            Handles.CircleHandleCap(
                0,
                lp.transform.position,
                q,
                lp.bottleNeckDiameter * 0.005f, EventType.Repaint
                );
            Handles.color = oldHandleColor;

            Debug.Log(lp.transform.position);
        }
        
    }
}