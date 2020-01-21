using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[RequireComponent(typeof(VisualEffect))]
public class GrimoireControl : MonoBehaviour
{
    [Tooltip("Current (Left) Page")]
    public int CurrentPage = 0;
    [Min(2)]
    public int PageCount = 8;
    public float AnimDuration = 1.0f;

    [Header("Properties")]
    public ExposedProperty AnimationDurationProperty = "Animation Duration";
    public ExposedProperty BasePageLeftProperty = "Base Page (Left)";
    public ExposedProperty BasePageRightProperty = "Base Page (Right)";
    public ExposedProperty TurnPageLeftProperty = "Turn Page (Left Side)";
    public ExposedProperty TurnPageRightProperty = "Turn Page (Right Side)";

    [Header("Events")]
    public ExposedProperty TurnLeftEvent = "TurnLeft";
    public ExposedProperty TurnRightEvent = "TurnRight";

    VisualEffect m_VFX;
    bool isTurning = false;

    private void OnValidate()
    {
        m_VFX = GetComponent<VisualEffect>();
        CurrentPage = Mathf.Clamp(CurrentPage, 0, PageCount - 2);
        CurrentPage -= CurrentPage % 2;
        m_VFX.SetInt(BasePageLeftProperty, CurrentPage);
        m_VFX.SetInt(BasePageRightProperty, CurrentPage+1);
    }

    private void OnEnable()
    {
        m_VFX = GetComponent<VisualEffect>();
        isTurning = false;
    }

    // Advance One Page
    public void TurnLeft()
    {
        if (isTurning || CurrentPage == PageCount - 2)
            return;

        CurrentPage += 2;
        int newIndexRight = CurrentPage+1;
        int newIndexLeft = CurrentPage;

        isTurning = true;
        m_VFX.SetFloat(AnimationDurationProperty, AnimDuration - Time.deltaTime);
        m_VFX.SendEvent(TurnLeftEvent);


        m_VFX.SetInt(TurnPageRightProperty, newIndexLeft);
        m_VFX.SetInt(TurnPageLeftProperty, m_VFX.GetInt(BasePageRightProperty));
        m_VFX.SetInt(BasePageRightProperty, newIndexRight);
        StartCoroutine(SetLeftCoroutine(AnimDuration, newIndexLeft));
    }

    // Go back one page
    public void TurnRight()
    {
        if (isTurning || CurrentPage == 0)
            return;

        CurrentPage -= 2;
        int newIndexRight = CurrentPage + 1;
        int newIndexLeft = CurrentPage;

        isTurning = true;
        m_VFX.SetFloat(AnimationDurationProperty, AnimDuration - Time.deltaTime);
        m_VFX.SendEvent(TurnRightEvent);

        m_VFX.SetInt(TurnPageLeftProperty, newIndexRight);
        m_VFX.SetInt(TurnPageRightProperty, m_VFX.GetInt(BasePageLeftProperty));
        m_VFX.SetInt(BasePageLeftProperty, newIndexLeft);
        StartCoroutine(SetRightCoroutine(AnimDuration, newIndexRight));
    }

    public IEnumerator SetLeftCoroutine(float duration, int index)
    {
        yield return new WaitForSeconds(duration - (Time.deltaTime * 3));
        m_VFX.SetInt(BasePageLeftProperty, index);
        isTurning = false;
    }

    public IEnumerator SetRightCoroutine(float duration, int index)
    {
        yield return new WaitForSeconds(duration - (Time.deltaTime * 3));
        m_VFX.SetInt(BasePageRightProperty, index);
        isTurning = false;
    }

}
