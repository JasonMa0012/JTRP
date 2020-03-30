using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class DelayPlay : MonoBehaviour
{
    [Header("帧(30/s)")]
    public float delayTime;
    List<Transform> hideChilds = new List<Transform>();
    private Coroutine curCoroutine;
    void OnEnable()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            hideChilds.Add(child);
        }

        if (curCoroutine != null)
        {
            this.StopCoroutine(curCoroutine);
        }
        
        curCoroutine = StartCoroutine(DelayPlayCoroutine(delayTime / 30));
    }

    IEnumerator DelayPlayCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        foreach (Transform child in hideChilds)
        {
            child.gameObject.SetActive(true);
        }
    }
}
