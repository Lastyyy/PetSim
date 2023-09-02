using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoveLevelBar : MonoBehaviour
{
    public Slider slider;

    CatBehaviour pet = null;
    float loveToNextLevel = 100f;
    public Image fill;
    GameObject loveLevelGameObject;
    TextMeshProUGUI loveLevel;

    // Start is called before the first frame update
    void Start()
    {
        slider.maxValue = 100f;
        loveLevelGameObject = GameObject.FindGameObjectWithTag("LoveLevel");
        loveLevel = loveLevelGameObject.GetComponent<TextMeshProUGUI>();
        loveLevel.transform.position = transform.position;
        loveLevel.text = "LVL";
    }

    // Update is called once per frame
    void Update()
    {
        loveLevel.transform.position = transform.position;
        if (pet == null)
        {
            pet = FindObjectOfType<CatBehaviour>();
        }
        else
        {
            loveToNextLevel = pet.GetLoveToNextLevel();
            loveLevel.text = pet.GetLoveLevel().ToString();
        }
        slider.value = loveToNextLevel;
    }

}
