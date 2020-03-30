using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class AnimatorFrameSet : MonoBehaviour
{
#if UNITY_EDITOR

    public string stateName;
    public int frame;

    void Update()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            AnimatorStateInfo stateInfo;
            if (!string.IsNullOrEmpty(stateName))
            {
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.shortNameHash != Animator.StringToHash(stateName))
                {
                    animator.Play(Animator.StringToHash(stateName), 0, 0f);
                    animator.Update(0f);
                }
            }

            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.length == 0)
            {
                animator.Update(0f);
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }

            if (stateInfo.length > 0)
            {
                animator.Play(stateInfo.shortNameHash, 0, frame * Time.fixedDeltaTime / stateInfo.length);
                animator.Update(0f);
            }
        }
    }

#endif
}

