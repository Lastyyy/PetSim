using UnityEngine;
using UnityEngine.UI;

public class HappinessBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;

    CatBehaviour pet = null;
    float happiness = 100f;
    public Image fill;

    // Start is called before the first frame update
    void Start()
    {
        slider.maxValue = 100f;
    }

    // Update is called once per frame
    void Update()
    {
        if (pet == null)
        {
            pet = FindObjectOfType<CatBehaviour>();
        }
        else
        {
            happiness = pet.GetHappiness();
        }
        
        slider.value = happiness;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

}
