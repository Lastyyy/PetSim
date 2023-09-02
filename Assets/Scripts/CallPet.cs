using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CallPet : MonoBehaviour, IPointerClickHandler
{
    public event Action OnCallingPet;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        CallPetToTheUser();
    }

    public void CallPetToTheUser()
    {
        // Check if any subscribers are present before invoking the event
        OnCallingPet?.Invoke();
    }
}
