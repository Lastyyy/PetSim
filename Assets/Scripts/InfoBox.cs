using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class InfoBox : MonoBehaviour
{
    Animator infoBoxAnimator;
    
    float timeOfShowingInfo;
    float timeOfAlreadyOnScreen = 0f;
    bool isInfoOnScreen = false;

    TextMeshProUGUI infoText;
    BoxState lastState = BoxState.Disappearing;

    bool hasBeenInterrupted = false;
    string interruptingInfoText;
    float interruptingInfoTime;
    bool interruptingInfoCanInterrupt;
    string currentInfoText;

    // Start is called before the first frame update
    void Start()
    {
        infoBoxAnimator = GetComponent<Animator>();
        infoText = GetComponentInChildren<TextMeshProUGUI>();
        ShowTheInfo("Move your camera to start scanning your room!", 9f);
    }

    // Update is called once per frame
    void Update()
    {
        if (isInfoOnScreen)
        {
            timeOfAlreadyOnScreen += Time.deltaTime;
            if (timeOfAlreadyOnScreen - (65.0 / 60.0) > timeOfShowingInfo)
            {
                ChangeBoxState(BoxState.Disappearing);
                timeOfAlreadyOnScreen = 0f;
            }
        }

        string currentAnimation = infoBoxAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        if (currentAnimation == "StandInvisible" && lastState == BoxState.Disappearing)
        {
            isInfoOnScreen = false;
            if (hasBeenInterrupted)
            {
                ShowTheInfo(interruptingInfoText, interruptingInfoTime, interruptingInfoCanInterrupt);
                hasBeenInterrupted = false;
            }
        }
    }

    public void ShowTheInfo(string text, float time = 7f, bool canInterrupt = true)
    {
        if (isInfoOnScreen && canInterrupt && currentInfoText != text)
        {
            ChangeBoxState(BoxState.Disappearing);
            timeOfAlreadyOnScreen = 0f;
            hasBeenInterrupted = true;

            interruptingInfoText = text;
            interruptingInfoTime = time;
            interruptingInfoCanInterrupt = canInterrupt;
        }
        else
        {
            timeOfShowingInfo = time;
            infoText.text = text;
            currentInfoText = text;
            ChangeBoxState(BoxState.Appearing);
        }
    }

    private void ChangeBoxState(BoxState newState)
    {
        infoBoxAnimator.SetBool("IsAppearing", false);
        infoBoxAnimator.SetBool("IsDisappearing", false);

        if (newState == BoxState.Appearing)
        {
            isInfoOnScreen = true;
        }

        lastState = newState;

        switch (newState)
        {
            case BoxState.Appearing:
                infoBoxAnimator.SetBool("IsAppearing", true);
                break;
            case BoxState.Disappearing:
                infoBoxAnimator.SetBool("IsDisappearing", true);
                break;
        }

    }

    public enum BoxState
    {
        Appearing,
        Disappearing,
    }

}
