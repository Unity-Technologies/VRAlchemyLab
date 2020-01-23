using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GrimoireTurnPage : MonoBehaviour
{
    public enum TurnType
    {
        TurnLeft,
        TurnRight
    }

    public TurnType Turn = TurnType.TurnLeft;
    public GrimoireControl GrimoireControl;

    public void OnMouseDown()
    {
        if (GrimoireControl == null)
            return;

        switch (Turn)
        {
            default:
            case TurnType.TurnLeft:
                GrimoireControl.TurnLeft();
                break;
            case TurnType.TurnRight:
                GrimoireControl.TurnRight();
                break;
        }
    }
}
