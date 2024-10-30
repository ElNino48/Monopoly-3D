using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AnimatorScript : MonoBehaviour
{
    public GameObject objectToAnimate;
    public Animator animator;

    public void ActivatePulsating(bool active)
    {
        objectToAnimate.SetActive(active);
    }
    public void OnMouseEnter()
    {
        animator.SetTrigger("Hover"); // Запускает анимацию увеличения
    }

    public void OnMouseExit()
    {
        animator.SetTrigger("Exit"); // Запускает анимацию возврата
    }
}
