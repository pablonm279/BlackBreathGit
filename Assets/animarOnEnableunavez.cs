using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animarOnEnableunavez : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string stateName; // optional: set a specific state to play on enable

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (animator == null)
            return;

        // Reset animator and restart the desired state from the beginning each time the object enables
        animator.Rebind();
        animator.Update(0f);

        if (!string.IsNullOrEmpty(stateName))
        {
            animator.Play(stateName, 0, 0f);
        }
        else
        {
            var state = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(state.fullPathHash, 0, 0f);
        }
    }
}
