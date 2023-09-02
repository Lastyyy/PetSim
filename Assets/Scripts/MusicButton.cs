using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MusicButton : MonoBehaviour, IPointerClickHandler
{
    Image mutingMusicImage;
    AudioSource backgroundMusic;

    [SerializeField] Sprite unmutedSprite;
    [SerializeField] Sprite mutedSprite;

    bool muted = false;
    float defaultVolume;

    // Start is called before the first frame update
    void Start()
    {
        mutingMusicImage = GetComponent<Image>();
        backgroundMusic = GetComponentInParent<AudioSource>();
        defaultVolume = backgroundMusic.volume;
    }

    // Update is called once per frame
    void Update()
    {
        if (muted)
        {
            mutingMusicImage.sprite = mutedSprite;
            backgroundMusic.volume = 0f;
        }
        else
        {
            mutingMusicImage.sprite = unmutedSprite;
            backgroundMusic.volume = defaultVolume;
        }
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (muted)
        {
            muted = false;
        }
        else
        {
            muted = true;
        }
    }
}
