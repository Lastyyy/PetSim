using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARSubsystems;
using System;

public class SpawnableManager : MonoBehaviour
{
    private ARPlaneManager planeManager;

    [SerializeField]
    ARRaycastManager m_RaycastManager;
    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    [SerializeField]
    GameObject catPrefab;
    [SerializeField]
    GameObject bowlPrefab;

    Camera arCam;
    GameObject spawnedCat = null;
    GameObject spawnedBowl = null;

    bool placingBowl = false;

    PlaceBowl placeBowl;

    InfoBox infoBox;
    CatBehaviour cat;

    public event Action BowlHasBeenPlaced;

    // Start is called before the first frame update
    void Start()
    {
        EnhancedTouchSupport.Enable();
        arCam = GameObject.Find("Main Camera").GetComponent<Camera>();

        planeManager = FindObjectOfType<ARPlaneManager>();

        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
        else
        {
            Debug.LogError("ARPlaneManager script not found in the scene.");
        }

        placeBowl = FindObjectOfType<PlaceBowl>();
        placeBowl.OnPlacingBowl += WantToPlaceBowl;

        infoBox = FindObjectOfType<InfoBox>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cat == null)
        {
            cat = FindObjectOfType<CatBehaviour>();
        }
    }

    private void FingerDown(UnityEngine.InputSystem.EnhancedTouch.Finger finger)
    {
        if (finger.index != 0 || !placingBowl) return;

        if (m_RaycastManager.Raycast(finger.currentTouch.screenPosition, m_Hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (ARRaycastHit hit in m_Hits)
            {
                if (planeManager.GetPlane(hit.trackableId).alignment == PlaneAlignment.HorizontalUp)
                {
                    if (spawnedBowl != null)
                    {
                        Destroy(spawnedBowl);
                        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Food"))
                        {
                            Destroy(obj);
                        }
                    }
                    
                    SpawnBowl(hit.pose.position);
                    placingBowl = false;

                    if (spawnedBowl.transform.position.y < cat.transform.position.y - 0.01f || 
                    spawnedBowl.transform.position.y > cat.transform.position.y + 0.18f)
                    {
                        infoBox.ShowTheInfo("The bowl could be inaccessible for the cat.\n If so, try placing it elsewhere");
                    }
                }
            }
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (spawnedCat == null)
        {
            foreach (ARPlane plane in args.added)
            {
                if (plane.alignment == PlaneAlignment.HorizontalUp)
                {
                    SpawnCat(plane.center + Vector3.up * 0.4f);
                    return;
                }
            }
        }
    }

    public void WantToPlaceBowl()
    {
        if (GetSpawnedBowl() == null) infoBox.ShowTheInfo("Click on the surface where you would like to put the bowl!");
        else infoBox.ShowTheInfo("Click on the surface where you'd like to relocate the bowl!");
        placingBowl = true;
    }

    public GameObject GetSpawnedBowl()
    {
        return spawnedBowl;
    }

    public MeshRenderer GetBowlsRenderer()
    {
        return spawnedBowl.GetComponentInChildren<MeshRenderer>();
    }

    private void SpawnCat(Vector3 spawnPosition)
    {
        spawnedCat = Instantiate(catPrefab, spawnPosition, Quaternion.identity);
    }

    private void SpawnBowl(Vector3 spawnPosition)
    {
        spawnedBowl = Instantiate(bowlPrefab, spawnPosition, Quaternion.identity);
        BowlHasBeenPlaced?.Invoke();
    }

    private void OnEnable() {
        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
        UnityEngine.InputSystem.EnhancedTouch.TouchSimulation.Enable();
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable() {
        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Disable();
        UnityEngine.InputSystem.EnhancedTouch.TouchSimulation.Disable();
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= FingerDown;
    }

}
