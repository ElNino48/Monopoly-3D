using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHover : MonoBehaviour
{
    Vector3 originalLocalPosition;
    Vector3 targetLocalPosition;
    float duration = 0.2f;
    private Coroutine hoverCoroutine;
    private void Start()
    {
        originalLocalPosition = transform.localPosition;
        targetLocalPosition = new Vector3(originalLocalPosition.x, originalLocalPosition.y + 0.5f, originalLocalPosition.z);
    }
    public void OnMouseEnter()
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
        }
        hoverCoroutine = StartCoroutine(HoverEffect(targetLocalPosition));
    }
    public void OnMouseExit()
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
        }
        hoverCoroutine = StartCoroutine(HoverEffect(originalLocalPosition));
    }
    private IEnumerator HoverEffect(Vector3 targetLocalPosition)
    {
        Vector3 initialPosition = transform.localPosition;
        float elapsedTime = 0f;
        while (elapsedTime< duration)
        {
            transform.localPosition = Vector3.Lerp(initialPosition, targetLocalPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

}
