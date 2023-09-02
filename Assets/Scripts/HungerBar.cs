using UnityEngine;
using UnityEngine.UI;

public class HungerBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;

    CatBehaviour pet = null;
    float hunger = 100f;
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
            hunger = pet.GetHunger();
        }
        
        slider.value = hunger;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

}
