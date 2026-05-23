using UnityEngine;
using UnityEngine.UI;
using System;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject[] tabs;
    [SerializeField] private GameObject[] pages;

    private int currentIndex = -1;
    public event Action OnTabChange;
    private void OnEnable()
    {
        SetActiveTab(0);
    }
    public void SetActiveTab(int index)
    {
        if (index == currentIndex) return;
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].GetComponent<Image>().color = Color.darkGray; //grey out inactive tabs
            pages[i].SetActive(false);
        }
        pages[index].SetActive(true);
        tabs[index].GetComponent<Image>().color = Color.white;
        currentIndex = index;
        OnTabChange?.Invoke();
    }
}
