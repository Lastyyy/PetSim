using System;
using UnityEngine.EventSystems;
using UnityEngine;

public class PlaceBowl : MonoBehaviour, IPointerClickHandler
{
    public event Action OnPlacingBowl;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        WantToPutBowl();
    }

    public void WantToPutBowl()
    {
        // Check if any subscribers are present before invoking the event
        OnPlacingBowl?.Invoke();
    }
}
