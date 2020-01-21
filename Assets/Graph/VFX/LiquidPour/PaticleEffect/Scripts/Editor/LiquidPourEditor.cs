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
            var rot = lp.transform.rotation * Quaternion.Euler(90, 0, 0);
            var pos = lp.transform.position;
            var rad = lp.bottleNeckDiameter * 0.005f;

            Color oldHandleColor = Handles.color;
            Handles.color = Color.red;
            Handles.CircleHandleCap(0, pos, rot, rad, EventType.Repaint);
            Handles.ArrowHandleCap(0, pos, rot * Quaternion.Euler(180, 0, 0), rad * 2, EventType.Repaint);
            Handles.color = oldHandleColor;
        }
        
    }
}