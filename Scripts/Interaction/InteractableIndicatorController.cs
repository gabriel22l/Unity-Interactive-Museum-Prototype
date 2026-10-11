using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InteractableIndicatorController : MonoBehaviour
{
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Sprite indicatorSprite;
    [SerializeField] private Sprite currentInteractableSprite;
    private Queue<GameObject> indicators = new Queue<GameObject>();
    private Dictionary<IInteractable, GameObject> interactableIndicatorDictionary = new Dictionary<IInteractable, GameObject>();
    private readonly int maxIndicators = 3;
    private Camera cam;
    private Vector3 targetOffset = new Vector3(0, 1, 0);
    
    public int IndicatorCount => maxIndicators;
    
    private IInteractable lastCurrentInteractable;
    
    //fade in animation variables
    private float startAlpha = 0f;
    private float targetAlpha = 1f;
    private float animationDuration = 0.33f;

    private void Awake()
    {
        cam = Camera.main;
        for (int i = 0; i < maxIndicators; i++)
        {
            GameObject indicator = Instantiate(indicatorPrefab, this.transform);
            indicators.Enqueue(indicator);
            indicator.SetActive(false);
        }
    }
    public void UpdateIndicators(List<IInteractable> interactableInRangeList, IInteractable currentInteractable)
    {
        RemoveOutOfRangeIndicators(interactableInRangeList);
        AddIndicators(interactableInRangeList);
        SetCurrentInteractableIcon(currentInteractable);
    }
    private void RemoveOutOfRangeIndicators(List<IInteractable> interactableInRangeList)
    {
        //Remove IInteractables out of range
        List<IInteractable> toRemove = new List<IInteractable>();
        foreach (KeyValuePair<IInteractable, GameObject> kvp in interactableIndicatorDictionary)
        {
            if (!interactableInRangeList.Contains(kvp.Key))
            {
                toRemove.Add(kvp.Key);
                kvp.Value.SetActive(false);
                indicators.Enqueue(kvp.Value);
            }
        }
        foreach (IInteractable interactable in toRemove)
            interactableIndicatorDictionary.Remove(interactable);
    }
    private void AddIndicators(List<IInteractable> interactableInRangeList)
    {
        //Add indicators for each interactable in range
        foreach (IInteractable interactable in interactableInRangeList)
        {
            if (indicators.Count <= 0)
                break;
            if (interactableIndicatorDictionary.ContainsKey(interactable))
                continue;
            
            GameObject indicator = indicators.Dequeue();
            indicator.SetActive(true);
            if(indicator.TryGetComponent(out Image img))
                StartCoroutine(FadeIn(img));
            interactableIndicatorDictionary.Add(interactable, indicator);
        }
    }
    private void SetCurrentInteractableIcon(IInteractable currentInteractable)
    {
        foreach (KeyValuePair<IInteractable, GameObject> kvp in interactableIndicatorDictionary)
        {
            if (!kvp.Value.TryGetComponent(out Image img)) 
                continue;
            if (kvp.Key == currentInteractable)
            {
                img.sprite = currentInteractableSprite;
                if (currentInteractable != lastCurrentInteractable)
                {
                    StartCoroutine(FadeIn(img));
                }
            }
            else
                img.sprite = indicatorSprite;
        }
        lastCurrentInteractable = currentInteractable;
    }
    private void LateUpdate()
    {
        if (cam == null)
            return;
        foreach (KeyValuePair<IInteractable, GameObject> kvp in interactableIndicatorDictionary)
        {
            if (kvp.Key is MonoBehaviour mb)
                kvp.Value.transform.position = mb.transform.position + targetOffset;
            kvp.Value.transform.forward = cam.transform.forward;
        }
    }
    private IEnumerator FadeIn(Image img)
    {
        img.color = new Color(1,1,1, startAlpha);
        Color c = img.color;
        float timer = 0;
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, timer / animationDuration);
            c.a = a;
            img.color = c;
            yield return null;
        }
        c.a = targetAlpha;
        img.color = c;
    }
}
