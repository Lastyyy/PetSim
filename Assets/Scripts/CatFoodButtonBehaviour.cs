using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CatFoodButtonBehaviour : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Image catFoodNoBg;
    [SerializeField] float RotationSpeed;
    [SerializeField] List<GameObject> foodPrefabs = new List<GameObject>();
    [SerializeField] AudioSource popAudio;

    public event Action OnSpawningFood;
    public event Action PackOfFoodInHand;
    public event Action PackOfFoodNotInHand;

    int num_of_food = 0;

    bool packAboveTheBowl = false;

    float timerForSpawningFood = 0.0f;
    [SerializeField] float interval;

    private SpawnableManager spawnableManager;

    InfoBox infoBox;

    private void Start()
    {
        spawnableManager = FindObjectOfType<SpawnableManager>();
        infoBox = FindObjectOfType<InfoBox>();
    }

    private void Update()
    {
        if (packAboveTheBowl)
        {
            Quaternion rotateTo = Quaternion.Euler(0, 0, 144f);

            catFoodNoBg.rectTransform.rotation = Quaternion.Slerp(
            catFoodNoBg.rectTransform.rotation, rotateTo, Time.deltaTime * RotationSpeed);

            timerForSpawningFood += Time.deltaTime;

            if (timerForSpawningFood >= interval)
            {
                timerForSpawningFood = 0.0f;
                GameObject meat = SpawnFood(spawnableManager.GetSpawnedBowl().transform.position + Vector3.up * 0.1f);
                FoodSpawned();
                
                Vector3 meatViewport = Camera.main.WorldToViewportPoint(meat.transform.position);

                Vector2 catFoodPoint = new Vector2(catFoodNoBg.transform.position.x, catFoodNoBg.transform.position.y);
                Vector2 catFoodPos = new Vector2((float)catFoodPoint.x / (float)Screen.width, (float)catFoodPoint.y / (float)Screen.height);
                
                // TODO 0.04 zamiast 0.02
                while(meatViewport.y < catFoodPos.y - 0.02)
                {
                    meat.transform.position = meat.transform.position + Vector3.up * 0.004f;
                    meatViewport = Camera.main.WorldToViewportPoint(meat.transform.position);
                }
            }
        }
        else
        {
            Quaternion rotateTo = Quaternion.Euler(0, 0, 0);

            catFoodNoBg.rectTransform.rotation = Quaternion.Slerp(
            catFoodNoBg.rectTransform.rotation, rotateTo, Time.deltaTime * RotationSpeed);
        }

    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (spawnableManager.GetSpawnedBowl())
        {
            infoBox.ShowTheInfo("Drag the pack of the cat's food above the Bowl to fill it with food!");
        }
        else
        {
            string text = "Click the Bowl button and then on the surface where you want to place it! ";
            text += "Drag the pack of the cat's food above the Bowl to fill it with food!";
            infoBox.ShowTheInfo(text, 11f);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!spawnableManager.GetSpawnedBowl())
        {
            string text = "Click the Bowl button and then on the surface where you want to place it! ";
            text += "Drag the pack of the cat's food above the Bowl to fill it with food!";
            infoBox.ShowTheInfo(text, 11f);
        }
        else
        {
            // Hide button
            transform.localScale = new Vector3(0, 0, 0);
            catFoodNoBg.enabled = true;
            catFoodNoBg.rectTransform.position = eventData.position;
            PackOfFoodInHand?.Invoke();
        }
    }

    public void OnDrag(PointerEventData data)
    {
        if (spawnableManager.GetSpawnedBowl())
        {
            catFoodNoBg.rectTransform.position = data.position;

            // check if bowl is visible
            Vector3 viewportPosition = Camera.main.WorldToViewportPoint(
                spawnableManager.GetBowlsRenderer().bounds.center);
            
            bool isBowlVisible = viewportPosition.x > 0 && viewportPosition.x < 1 &&
                viewportPosition.y > 0 && viewportPosition.y < 1 && viewportPosition.z > 0;

            if (isBowlVisible)
            {
                Vector2 catFoodPoint = new Vector2(data.position.x, data.position.y);
                Vector2 catFoodPos = new Vector2((float)catFoodPoint.x / (float)Screen.width, (float)catFoodPoint.y / (float)Screen.height);

                if (catFoodPos.y > viewportPosition.y + 0.03 && IsBetween(catFoodPos.x, viewportPosition.x - 0.1, viewportPosition.x + 0.12))
                {
                    packAboveTheBowl = true;
                }
                else
                {
                    packAboveTheBowl = false;
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        packAboveTheBowl = false;
        if (spawnableManager.GetSpawnedBowl())
        {
        catFoodNoBg.enabled = false;
        // Show button
        transform.localScale = new Vector3(1, 1, 1);
        }
        PackOfFoodNotInHand?.Invoke();
    }

    private GameObject SpawnFood(Vector3 spawnPosition)
    {
        popAudio.Play();
        int randomPrefabIndex = UnityEngine.Random.Range(0, foodPrefabs.Count);
        GameObject randomPrefab = foodPrefabs[randomPrefabIndex];
        GameObject obj;
        obj = Instantiate(randomPrefab, spawnPosition, Quaternion.identity);
        obj.transform.rotation = UnityEngine.Random.rotation;
    
        obj.name = ("Food" + num_of_food);
        num_of_food += 1;
        return obj;
    }

    public void FoodSpawned()
    {
        // Check if any subscribers are present before invoking the event
        OnSpawningFood?.Invoke();
    }

    public bool IsBetween(double testValue, double bound1, double bound2)
    {
        return (testValue >= Math.Min(bound1,bound2) && testValue <= Math.Max(bound1,bound2));
    }
}
