using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using GameplayIngredients;
using NaughtyAttributes;

[RequireComponent(typeof(VisualEffect))]
public class GrimoireControl : MonoBehaviour
{
    [Tooltip("Current (Left) Page")]
    public int CurrentPage = 0;

    [Min(2)]
    public int PageCount = 8;

    public float AnimDuration = 1.0f;

    // VFX property names
    [Header("Properties")]
    public string AnimationDurationProperty = "Animation Duration";
    public string BasePageLeftProperty = "Base Page (Left)";
    public string BasePageRightProperty = "Base Page (Right)";
    public string TurnPageLeftProperty = "Turn Page (Left Side)";
    public string TurnPageRightProperty = "Turn Page (Right Side)";

    [Header("Events")]
    public string TurnLeftEvent = "TurnLeft";
    [ReorderableList] public Callable[] OnTurnLeft;

    public string TurnRightEvent = "TurnRight";
    [ReorderableList] public Callable[] OnTurnRight;

    private VisualEffect m_VFX;
    private bool isTurning = false;

    private void OnValidate()
    {
        m_VFX = GetComponent<VisualEffect>();

        CurrentPage = Mathf.Clamp(CurrentPage, 0, PageCount - 2);
        CurrentPage -= CurrentPage % 2;

        if (m_VFX == null)
            return;

        // Use safe setters to avoid missing property errors
        SafeSetInt(BasePageLeftProperty, CurrentPage);
        SafeSetInt(BasePageRightProperty, CurrentPage + 1);
    }

    private void OnEnable()
    {
        m_VFX = GetComponent<VisualEffect>();
        isTurning = false;
    }

    // Turn forward (left)
    public void TurnLeft()
    {
        if (isTurning || CurrentPage >= PageCount - 2)
            return;

        CurrentPage += 2;

        int newIndexLeft = CurrentPage;
        int newIndexRight = CurrentPage + 1;

        isTurning = true;

        SafeSetFloat(AnimationDurationProperty, AnimDuration);
        SafeSendEvent(TurnLeftEvent);

        Callable.Call(OnTurnLeft, gameObject);

        SafeSetInt(TurnPageRightProperty, newIndexLeft);
        SafeSetInt(TurnPageLeftProperty, m_VFX.GetInt(BasePageRightProperty));
        SafeSetInt(BasePageRightProperty, newIndexRight);

        StartCoroutine(SetLeftCoroutine(AnimDuration, newIndexLeft));
    }

    // Turn backward (right)
    public void TurnRight()
    {
        if (isTurning || CurrentPage <= 0)
            return;

        CurrentPage -= 2;

        int newIndexLeft = CurrentPage;
        int newIndexRight = CurrentPage + 1;

        isTurning = true;

        SafeSetFloat(AnimationDurationProperty, AnimDuration);
        SafeSendEvent(TurnRightEvent);

        Callable.Call(OnTurnRight, gameObject);

        SafeSetInt(TurnPageLeftProperty, newIndexRight);
        SafeSetInt(TurnPageRightProperty, m_VFX.GetInt(BasePageLeftProperty));
        SafeSetInt(BasePageLeftProperty, newIndexLeft);

        StartCoroutine(SetRightCoroutine(AnimDuration, newIndexRight));
    }

    private IEnumerator SetLeftCoroutine(float duration, int index)
    {
        yield return new WaitForSeconds(duration);
        SafeSetInt(BasePageLeftProperty, index);
        isTurning = false;
    }

    private IEnumerator SetRightCoroutine(float duration, int index)
    {
        yield return new WaitForSeconds(duration);
        SafeSetInt(BasePageRightProperty, index);
        isTurning = false;
    }

    // -----------------------------
    // Safe VFX API wrappers
    // -----------------------------

    private void SafeSetInt(string property, int value)
    {
        if (string.IsNullOrEmpty(property))
            return;

        if (m_VFX.HasInt(property))
            m_VFX.SetInt(property, value);
    }

    private void SafeSetFloat(string property, float value)
    {
        if (string.IsNullOrEmpty(property))
            return;

        if (m_VFX.HasFloat(property))
            m_VFX.SetFloat(property, value);
    }

    private void SafeSendEvent(string evt)
    {
        if (string.IsNullOrEmpty(evt))
            return;

        m_VFX.SendEvent(evt);
    }
}
