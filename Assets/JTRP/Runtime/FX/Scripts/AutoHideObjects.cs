using UnityEngine;
using System.Collections;

public class AutoHideObjects : MonoBehaviour
{

    public GameObject[] hideObjects;
    public UnityEngine.Component[] hideComponent;
    public float delayTime;

    bool hide = false;
    float showTime = 0f;

    void OnEnable()
    {
        SetHide(true);
        hide = false;
        showTime = 0f;
    }

    void Update()
    {
        if (!hide)
        {
            showTime += Time.deltaTime;
            if (showTime > delayTime)
            {
                PerformHide();
            }
        }
    }

    void PerformHide()
    {
        SetHide(false);
        hide = true;
    }

    void SetHide(bool isHide)
    {
        if (hideObjects != null)
            foreach (GameObject obj in hideObjects)
            {
                obj.SetActive(isHide);
            }

        if (hideComponent != null)
            foreach (var item in hideComponent)
            {
                if (item is TrailRenderer)
                {
                    (item as TrailRenderer).emitting = isHide;
                }
                else if (item is Renderer)
                {
                    (item as Renderer).enabled = isHide;
                }
                else
                {
                    Debug.LogWarning("暂不支持此类型componet");
                }
            }

    }
}
