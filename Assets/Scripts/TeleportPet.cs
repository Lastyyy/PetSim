using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TeleportPet : MonoBehaviour, IPointerClickHandler
{
    public event Action OnTeleportingPet;

    InfoBox infoBox;
    bool hasAlreadyTeleported = false;

    void Start()
    {
        infoBox = FindObjectOfType<InfoBox>();
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        TeleportPetToTheUser();
        if (!hasAlreadyTeleported) infoBox.ShowTheInfo("Teleporting button, use if your cat magically disappeared!");
        hasAlreadyTeleported = true;
    }

    public void TeleportPetToTheUser()
    {
        // Check if any subscribers are present before invoking the event
        OnTeleportingPet?.Invoke();
    }
}
