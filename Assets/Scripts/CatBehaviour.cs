using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CatBehaviour : MonoBehaviour
{
    float walkingInterval = 10f;
    float walkingTimer = 0f;
    float hungerCheckInterval = 6f;
    float hungerCheckTimer = 0f;
    float meowingCooldown = 15f;
    float meowingTimer = 0f;

    float hungerInfoCooldown = 80f;
    float hungerInfoTimer = 50f;
    float happinessInfoCooldown = 80f;
    float happinessInfoTimer = 30f;

    Vector3 newCatDestination = new Vector3(-999, 999, -999);
    Vector3 currentVelocity;
    int walkingCooldown = 0;

    Animator catAnimator;
    CatState currentState = CatState.Idle;
    string currentClipName;
    CapsuleCollider capsuleCollider;

    ARPlane currentPlane;
    Ray ray;

    ARRaycastManager raycastManager;
    SpawnableManager spawnableManager;
    
    [SerializeField] GameObject spawnablePrefab;
    [SerializeField] GameObject spawnablePrefab1;

    [SerializeField] float walkingSpeed;
    [SerializeField] float rotationSpeed;
    [SerializeField] AudioSource hungryCatAudio;
    [SerializeField] AudioSource happyCatAudio;
    [SerializeField] AudioSource purrCatAudio;
    [SerializeField] AudioSource eatenFoodAudio;
    [SerializeField] AudioSource levelUpAudio;

    CallPet summonPet;
    TeleportPet teleportPet;
    CatFoodButtonBehaviour catFoodButtonBehaviour;

    Camera mainCamera;

    Vector3 curnewPositionxP = new Vector3(0,0,0);
    Vector3 curnewPositionxM = new Vector3(0,0,0);
    Vector3 curnewPositionzP = new Vector3(0,0,0);
    Vector3 curnewPositionzM = new Vector3(0,0,0);

    // GameObject posxPobj = null;
    // GameObject posxMobj = null;
    // GameObject poszPobj = null;
    // GameObject poszMobj = null;

    Vector3 collisionVector;

    List<Vector3Triplet> triangles = new List<Vector3Triplet>();
    List<Vector3> points = new List<Vector3>();

    float hunger;
    [SerializeField] float hungerIncrease;
    float distanceToFood = 0.22f;
    float foodValue = 6.0f;
    float timeFromLastEaten = 0f;
    bool goingToFood = false;
    bool goingToBowl = false;
    bool wasCalled = false;

    float happiness;
    [SerializeField] float happinessDecrease;
    float happinessFromPettingCooldown = 0.1f;
    float happinessFromPettingTimer = 0f;
    float timeFromLastHappinessIncrease = 0f;

    bool firstTimePlaying;
    DateTime lastPlayingTime;

    float loveToNextLevel = 0f;
    int loveLevel = 1;
    float loveIncrease = 1f;

    [SerializeField] bool resetLevel = false;

    GameObject currentFood = null;
    
    int num_of_cylinder = 0;

    InfoBox infoBox;
    bool hasCalledPet = false;
    bool fingerDown = false;
    Vector2 currentFingerPosition;
    bool purringAudio = false;

    bool emptyHand = true;
    PlaceBowl placeBowl;

    // Start is called before the first frame update
    void Start()
    {
        newCatDestination = transform.position;
        catAnimator = GetComponent<Animator>();
        ChangeCatState(CatState.Idle);

        raycastManager = FindObjectOfType<ARRaycastManager>();
        spawnableManager = FindObjectOfType<SpawnableManager>();

        summonPet = FindObjectOfType<CallPet>();
        summonPet.OnCallingPet += CallPet;

        teleportPet = FindObjectOfType<TeleportPet>();
        teleportPet.OnTeleportingPet += TeleportPet;

        catFoodButtonBehaviour = FindObjectOfType<CatFoodButtonBehaviour>();
        catFoodButtonBehaviour.OnSpawningFood += CheckIfHungryWhenFoodSpawned;
        catFoodButtonBehaviour.PackOfFoodInHand += HandNotEmpty;
        catFoodButtonBehaviour.PackOfFoodNotInHand += HandEmpty;

        placeBowl = FindObjectOfType<PlaceBowl>();
        placeBowl.OnPlacingBowl += HandNotEmpty;

        spawnableManager.BowlHasBeenPlaced += HandEmpty;
        
        mainCamera = FindObjectOfType<Camera>();

        InvokeRepeating("GetHungry", 0, 0.5f);
        InvokeRepeating("DecreaseHappiness", 0, 0.5f);
        InvokeRepeating("IncreaseLoveToNextLevel", 0, 0.5f);

        hunger = PlayerPrefs.GetFloat("hunger", 100.0f);
        happiness = PlayerPrefs.GetFloat("happiness", 100.0f);
        
        if (PlayerPrefs.GetString("firstTimePlaying", "True") == "True")
        {
            PlayerPrefs.SetString("firstTimePlaying", "False");
            PlayerPrefs.Save();
        }
        else
        {
            lastPlayingTime = DateTime.FromBinary(Convert.ToInt64(PlayerPrefs.GetString("lastPlayingTime")));
            float hoursFromLastPlaying = (float)System.DateTime.Now.Subtract(lastPlayingTime).TotalHours;

            DecreaseHappiness(hoursFromLastPlaying * 2.8f);
            GetHungry(hoursFromLastPlaying * 2.4f);
        }

        if (resetLevel)
        {
            loveIncrease = 1f;
            loveLevel = 1;
            loveToNextLevel = 0f;
            resetLevel = false;
        }
        else
        {
            loveIncrease = PlayerPrefs.GetFloat("loveIncrease", 1f);
            loveLevel = PlayerPrefs.GetInt("loveLevel", 1);
            loveToNextLevel = PlayerPrefs.GetFloat("loveToNextLevel", 0f);
        }

        infoBox = FindObjectOfType<InfoBox>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        infoBox.ShowTheInfo("Take care of the cat to increase the \nAttachment Level between both of you!");
    }

    // Update is called once per frame 
    void Update()
    {
        currentClipName = catAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        Vector3 eulerRotation = transform.rotation.eulerAngles;
        eulerRotation.x = eulerRotation.z = 0;
        transform.rotation = Quaternion.Euler(eulerRotation);

        walkingTimer += Time.deltaTime;
        hungerCheckTimer += Time.deltaTime;
        meowingTimer += Time.deltaTime;
        hungerInfoTimer += Time.deltaTime;
        happinessInfoTimer += Time.deltaTime;
        timeFromLastEaten += Time.deltaTime;
        timeFromLastHappinessIncrease += Time.deltaTime;

        if (walkingTimer >= walkingInterval)
        {
            if(walkingCooldown == 1 && currentState == CatState.Idle)
            {
                AssignNewPosition();
                walkingCooldown = 0;
            }
            else if(walkingCooldown > 1)
            {
                walkingCooldown = 0;
            }
            else
            {
                walkingCooldown += 1;
            }
        }

        // Checking if the cat is petted by user during Walking or being Idle, but not Meowing or Eating
        if (fingerDown && CheckIfFingerOnPet() && emptyHand == true &&
        (currentClipName == "Idle" || currentClipName == "Take 001" || currentClipName == "Walk") &&
        (currentState == CatState.Idle || currentState == CatState.Walking || currentState == CatState.Petted) &&
        currentState != CatState.Meowing && currentClipName != "sound")
        {
            ChangeCatState(CatState.Petted);
            happinessFromPettingTimer += Time.deltaTime;
            if (happinessFromPettingTimer > happinessFromPettingCooldown)
            {
                happinessFromPettingTimer = 0f;
                IncreaseHappiness(0.33f);
            }
            if (!purringAudio) 
            {
                purrCatAudio.Play();
                purringAudio = true;
            }
        }
        else if(currentState == CatState.Petted && (!fingerDown || !CheckIfFingerOnPet()))
        {
            ChangeCatState(CatState.Idle);
            purrCatAudio.Stop();
            purringAudio = false;
        }
        else if (currentState == CatState.Walking)
        {
            WalkToNewDestination();
        }
        else if(currentState == CatState.Idle && currentClipName == "Idle" && hungerCheckTimer >= hungerCheckInterval)
        {
            // Random Go Eat
            if (Math.Max(UnityEngine.Random.Range(0f, hunger*hunger), 1f) < 888f && hunger <= 95f)
            {
                GoEat();
            }
        }
        else if (currentState == CatState.Eating)
        {
            Eat();

            // Adjusting the collider to fit the head going down while eating
            string clipName = catAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            float normalizedTime = catAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;

            if (normalizedTime > 0.25f && normalizedTime < 0.88f && clipName == "Eat")
            {
                float downY = 0.345f;

                Vector3 newCenter = capsuleCollider.center;
                newCenter.y = downY;
                newCenter.z = 0.225f;
                capsuleCollider.center = newCenter;
                float downRadius = 0.13f;
                capsuleCollider.radius = downRadius;
            }
            else
            {
                float upY = 0.57f;
                Vector3 newCenter = capsuleCollider.center;
                newCenter.y = upY;
                newCenter.z = 0.2f;
                capsuleCollider.center = newCenter;
                float upRadius = 0.23f;
                capsuleCollider.radius = upRadius;
            }
        }
        else if (currentState == CatState.Meowing)
        {
            float animationLength = catAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            string clipName = catAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

            if (animationLength > 0.7f && clipName == "sound")
            {
                ChangeCatState(CatState.Idle);
                meowingTimer = 0f;
            }
        }

        walkingTimer %= walkingInterval;
        hungerCheckTimer %= hungerCheckInterval;

        if (hungerInfoTimer >= hungerInfoCooldown && timeFromLastEaten >= 30f)
        {
            if (hunger <= 50.0f) infoBox.ShowTheInfo("Remember to feed your pet!\nPlace the bowl and fill it up with delicious food!", 8f, false);
            hungerInfoTimer = 0f;
        }
        else if (happinessInfoTimer >= happinessInfoCooldown && timeFromLastHappinessIncrease >= 30f)
        {
            if(happiness <= 50.0f) infoBox.ShowTheInfo("Remember to keep pet happy!\nFeed it well and don't hesitate to pet it!", 8f, false);
            happinessInfoTimer = 0f;
        }
        
    }

    public void HandEmpty()
    {
        emptyHand = true;
    }

    public void HandNotEmpty()
    {
        emptyHand = false;
    }

    public void IncreaseLoveToNextLevel()
    {
        loveToNextLevel += loveIncrease * ((hunger + happiness) / 300);
        if (loveToNextLevel >= 100f)
        {
            levelUpAudio.Play();
            loveLevel += 1;

            loveIncrease = 1f;
            for (int i = 1; i < loveLevel; i++)
            {
                loveIncrease -= loveIncrease * 0.05f;
            }

            loveToNextLevel %= 100f;
            PlayerPrefs.SetInt("loveLevel", loveLevel);
            PlayerPrefs.SetFloat("loveIncrease", loveIncrease);
        }
        PlayerPrefs.SetFloat("loveToNextLevel", loveToNextLevel);
        PlayerPrefs.Save();
    }

    public float GetLoveToNextLevel()
    {
        return loveToNextLevel;
    }

    public int GetLoveLevel()
    {
        return loveLevel;
    }

    private void IncreaseHappiness(float happinessIncreare)
    {
        timeFromLastHappinessIncrease = 0f;
        happiness = Math.Min(100, happiness + happinessIncreare);
    }

    private void DecreaseHappiness()
    {   
        happiness = Math.Max(0, (happiness - happinessDecrease));
        happiness = Math.Max(0, (happiness - (100.0f - hunger) * 0.0008f));
        PlayerPrefs.SetFloat("happiness", happiness);
        PlayerPrefs.Save();
    }

    private void DecreaseHappiness(float decreaseValue)
    {
        happiness = Math.Max(0, (happiness - decreaseValue));
        PlayerPrefs.SetFloat("happiness", happiness);
        PlayerPrefs.Save();
    }

    public float GetHappiness()
    {
        return happiness;
    }

    private bool CheckIfFingerOnPet()
    {
        Vector3 catViewport = Camera.main.WorldToViewportPoint(transform.position);
        // Check if finger is on cat = if petting the pet
        return (IsBetween(currentFingerPosition.x, catViewport.x - 0.1, catViewport.x + 0.1) &&
                IsBetween(currentFingerPosition.y, catViewport.y - 0.03, catViewport.y + 0.2));
    }

    private void FingerDown(UnityEngine.InputSystem.EnhancedTouch.Finger finger)
    {
        if (finger.index != 0) return;
        fingerDown = true;
        currentFingerPosition = new Vector2(
            (float)finger.screenPosition.x / (float)Screen.width, 
            (float)finger.screenPosition.y / (float)Screen.height);
    }

    private void FingerMove(UnityEngine.InputSystem.EnhancedTouch.Finger finger)
    {
        if (finger.index != 0) return; 
        currentFingerPosition = new Vector2(
            (float)finger.screenPosition.x / (float)Screen.width, 
            (float)finger.screenPosition.y / (float)Screen.height);
    }

    private void FingerUp(UnityEngine.InputSystem.EnhancedTouch.Finger finger)
    {
        if (finger.index != 0) return;
        fingerDown = false;
    }
 
    private void CheckIfHungryWhenFoodSpawned()
    {
        if (hunger < 66f && (currentState == CatState.Idle || currentState == CatState.Walking))
        {
            GoEat();
        }
    }

    private void Eat()
    {
        float normalizedTime = catAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
        string clipName = catAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        
        if (currentFood != null && normalizedTime > 0.5f && normalizedTime < 0.97f && clipName == "Eat")
        {
            Destroy(currentFood);
            eatenFoodAudio.Play();
            currentFood = null;
            EatFood(foodValue);
            IncreaseHappiness(2.22f);
            PlayerPrefs.SetFloat("hunger", hunger);
            PlayerPrefs.Save();
        }
        else if(normalizedTime > 0.97f && clipName == "Eat")
        {
            // Random Go Eat Next Food
            if (UnityEngine.Random.Range(0f, hunger*hunger) < 6666f && hunger < 98f &&
            GameObject.FindGameObjectsWithTag("Food").Length > 0)
            {
                GoEat();
            }
            else 
            {
                ChangeCatState(CatState.Idle);
                goingToFood = goingToBowl = wasCalled = false;
            }
        }
        else if(normalizedTime < 0.5f && clipName == "Eat")
        {
            Vector3 pointInFrontOfCat = transform.TransformPoint(Vector3.forward * 1.0f);
            Vector3 directionToCurrentFood = currentFood.transform.position - transform.position;
            Vector3 directionForward = pointInFrontOfCat - transform.position;
        
            if (Vector3.Angle(directionForward, directionToCurrentFood) > 8f)
            {
                RotateCat(currentFood.transform.position);
            }
        }
    }

    private void GoEat()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        float closestFoodDistance = float.MaxValue;
        GameObject closestFood = null;
        foreach (GameObject food in foods)
        {
            if (Vector3.Distance(transform.position, food.transform.position) < closestFoodDistance &&
            food.transform.position.y <= transform.position.y + 0.18f && food.transform.position.y >= transform.position.y - 0.02f)
            {
                closestFood = food;
                closestFoodDistance = Vector3.Distance(transform.position, food.transform.position);
            }
        }
        
        if (closestFood != null)
        {
            newCatDestination = closestFood.transform.position;
            goingToFood = true;
            currentFood = closestFood;
            if (closestFoodDistance > 1.44f *distanceToFood) ChangeCatState(CatState.Walking);
        }
        else if (meowingTimer >= meowingCooldown)
        {
            GameObject bowl = spawnableManager.GetSpawnedBowl();
            if (bowl != null && bowl.transform.position.y >= transform.position.y - 0.01f && 
            bowl.transform.position.y <= transform.position.y + 0.18f)
            {
                float distanceToBowl = Vector3.Distance(transform.position, bowl.transform.position);
                if (distanceToBowl > 1.22f * distanceToFood) 
                {
                    ChangeCatState(CatState.Walking);
                    newCatDestination = bowl.transform.position;
                    goingToBowl = true;
                }
            }
            else if(currentState == CatState.Idle && currentClipName == "Idle")
            {
                ChangeCatState(CatState.Meowing);
                hungryCatAudio.PlayDelayed(0.15f);
            }
        }
        else
        {
            ChangeCatState(CatState.Idle);
            goingToFood = goingToBowl = wasCalled = false;
        }
        
    }

    private void GetHungry()
    {   
        hunger = Math.Max(0, (hunger - hungerIncrease));
        PlayerPrefs.SetFloat("hunger", hunger);
        PlayerPrefs.SetString("lastPlayingTime", System.DateTime.Now.ToBinary().ToString());
        PlayerPrefs.Save();
    }
    
    private void GetHungry(float valueOfGettingHungry)
    {
        hunger = Math.Max(0, (hunger - valueOfGettingHungry));
        PlayerPrefs.SetFloat("hunger", hunger);
        PlayerPrefs.Save();
    }

    public float GetHunger()
    {
        return hunger;
    }

    public void EatFood(float foodValue)
    {
        timeFromLastEaten = 0f;
        hunger = Math.Min(100.0f, (hunger + foodValue));
    }

    private void CallPet()
    {
        Vector3 cameraWithoutY = mainCamera.transform.position;
        cameraWithoutY.y = transform.position.y;
        Vector3 direction = cameraWithoutY - transform.position;
        newCatDestination = cameraWithoutY - (direction.normalized * 0.25f);
        ChangeCatState(CatState.Walking);
        wasCalled = true;
        goingToFood = goingToBowl = false;
        if (!hasCalledPet) infoBox.ShowTheInfo("Call the cat to you, so it gets to know your room better!");
        hasCalledPet = true;
    }

    private void TeleportPet()
    {
        transform.position = mainCamera.transform.position + mainCamera.transform.rotation * Vector3.forward * 0.9f;
        transform.LookAt(mainCamera.transform.position);
        Vector3 eulerRotation = transform.rotation.eulerAngles;
        eulerRotation.x = eulerRotation.z = 0;
        transform.rotation = Quaternion.Euler(eulerRotation);
        ChangeCatState(CatState.Idle);
    }

    private void WalkToNewDestination()
    {
        float distanceToTarget;
        if (goingToFood || goingToBowl) distanceToTarget = distanceToFood;
        else distanceToTarget = 0.1f;

        if(Vector3.Distance(transform.position, newCatDestination) > distanceToTarget)
        {
            transform.position = Vector3.SmoothDamp(transform.position, newCatDestination, ref currentVelocity, 0.21f, walkingSpeed);
            RotateCat(newCatDestination);
        }
        else
        {
            walkingCooldown = 0;

            if (goingToFood)
            {
                ChangeCatState(CatState.Eating);
                goingToFood = false;
            }
            else if(goingToBowl)
            {
                ChangeCatState(CatState.Meowing);
                hungryCatAudio.PlayDelayed(0.15f);
                goingToBowl = false;
            }
            else if(wasCalled)
            {
                ChangeCatState(CatState.Meowing);
                happyCatAudio.PlayDelayed(0.15f);
                wasCalled = false;
            }
            else 
            {
                ChangeCatState(CatState.Idle);
            }

            // Checking if the point is within 'safe' zone
            // If not - looking for 2 closest points and creating the triangle with them as the newly added safe zone
            if (!CheckPointInQuadrilateralAndTriangles(transform.position))
            {
                float firstClosest = float.MaxValue;
                Vector3 firstClosestPoint = new Vector3(0,0,0);
                float secondClosest = float.MaxValue;
                Vector3 secondClosestPoint = new Vector3(0,0,0);

                UpdateBiggestPlane();

                float indentX = Math.Max(currentPlane.extents.x/10, Math.Min(0.35f, currentPlane.extents.x/2));
                float indentZ = Math.Max(currentPlane.extents.y/10, Math.Min(0.35f, currentPlane.extents.y/2));

                curnewPositionxP = currentPlane.center + new Vector3(0, 0, currentPlane.extents.x - indentX);
                curnewPositionxM = currentPlane.center - new Vector3(0, 0, currentPlane.extents.x - indentX);
                curnewPositionzP = currentPlane.center + new Vector3(currentPlane.extents.y - indentZ, 0, 0);
                curnewPositionzM = currentPlane.center - new Vector3(currentPlane.extents.y - indentZ, 0, 0);

                foreach (Vector3 pointChecked in points)
                {
                    float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(pointChecked.x, pointChecked.z));
                    if (dist < firstClosest)
                    {
                        secondClosest = firstClosest;
                        secondClosestPoint = firstClosestPoint;
                        firstClosest = dist;
                        firstClosestPoint = new Vector2(pointChecked.x, pointChecked.z);
                    }
                    else if (dist < secondClosest)
                    {
                        secondClosest = dist;
                        secondClosestPoint = new Vector2(pointChecked.x, pointChecked.z);
                    }
                }
                foreach (Vector3 pointChecked in new List<Vector3>{curnewPositionxM, curnewPositionxP, curnewPositionzM, curnewPositionzP})
                {
                    float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(pointChecked.x, pointChecked.z));
                    if (dist < firstClosest)
                    {
                        secondClosest = firstClosest;
                        secondClosestPoint = firstClosestPoint;
                        firstClosest = dist;
                        firstClosestPoint = new Vector2(pointChecked.x, pointChecked.z);
                    }
                    else if (dist < secondClosest)
                    {
                        secondClosest = dist;
                        secondClosestPoint = new Vector2(pointChecked.x, pointChecked.z);
                    }
                }
                
                triangles.Add(new Vector3Triplet(secondClosestPoint, firstClosestPoint, transform.position));
                
                // SpawnSthAtNewPosition(new Vector3(secondClosestPoint.x, 0, secondClosestPoint.y));
                // SpawnSthAtNewPosition(new Vector3(firstClosestPoint.x, 0, firstClosestPoint.y));
                // SpawnSthAtNewPosition(transform.position, true);
                
                points.Add(transform.position);
            }
        }
    }

    private bool CheckPointInQuadrilateralAndTriangles(Vector3 point)
    {
        if (IsPointInQuadrilateral(
            new Vector2(curnewPositionxP.x, curnewPositionxP.z),
            new Vector2(curnewPositionzP.x, curnewPositionzP.z),
            new Vector2(curnewPositionxM.x, curnewPositionxM.z),
            new Vector2(curnewPositionzM.x, curnewPositionzM.z),
            new Vector2(point.x, point.z)
        ))
        {
            return true;
        }
        else
        {
            if (IsPointInTriangles(new Vector2(point.x, point.z)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private void RotateCat(Vector3 pointTowards)
    {
        Vector3 direction = pointTowards  - transform.position;
        Quaternion rotationTowards = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, rotationTowards, Time.deltaTime * rotationSpeed);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.gameObject.CompareTag("Food"))
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.point.y > transform.position.y + 0.02f || collision.collider.gameObject.CompareTag("Spawnable"))
                {
                    ChangeCatState(CatState.Idle);
                    collisionVector = new Vector3(contact.point.x, transform.position.y, contact.point.z) - transform.position;
                    newCatDestination = transform.position - 0.49f * collisionVector.normalized;
                    ChangeCatState(CatState.Walking);
                    goingToFood = goingToBowl = wasCalled = false;
                }
            }
        }
    }

    private void AssignNewPosition()
    {
        UpdateBiggestPlane();
        if (currentState == CatState.Idle)
        {
            newCatDestination = GetRandomPosition();

            if (newCatDestination != new Vector3(-999, 999, -999))
            {
                ChangeCatState(CatState.Walking);
                goingToFood = goingToBowl = wasCalled = false;
            }
        }
    }

    private Vector3 GetRandomPosition()
    {
        Boolean pointInQuadrilateralAndTriangles = false;
        
        Vector3 newPosition = new Vector3(0, 0, 0);
        Vector3 planexP = new Vector3(0, 0, 0);
        Vector3 planexM = new Vector3(0, 0, 0);
        Vector3 planezP = new Vector3(0, 0, 0);
        Vector3 planezM = new Vector3(0, 0, 0);

        int max_tries = 88;
        int num_of_tries = 0;

        while (!pointInQuadrilateralAndTriangles && num_of_tries < max_tries && currentPlane != null)
        {
            num_of_tries += 1;

            float rangeFromCat = UnityEngine.Random.Range(0.69f, 2.8f);
            float randomAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            
            float xOffset = Mathf.Cos(randomAngle) * rangeFromCat;
            float zOffset = Mathf.Sin(randomAngle) * rangeFromCat;

            newPosition = new Vector3(
                transform.position.x + xOffset, 
                transform.position.y, 
                transform.position.z + zOffset);
            
            float indentX = Math.Max(currentPlane.extents.x/10, Math.Min(0.35f, currentPlane.extents.x/2));
            float indentZ = Math.Max(currentPlane.extents.y/10, Math.Min(0.35f, currentPlane.extents.y/2));

            planexP = currentPlane.center + new Vector3(0, 0, currentPlane.extents.x - indentX);
            planexM = currentPlane.center - new Vector3(0, 0, currentPlane.extents.x - indentX);
            planezP = currentPlane.center + new Vector3(currentPlane.extents.y - indentZ, 0, 0);
            planezM = currentPlane.center - new Vector3(currentPlane.extents.y - indentZ, 0, 0);

            curnewPositionxP = planexP;
            curnewPositionxM = planexM;
            curnewPositionzP = planezP;
            curnewPositionzM = planezM;
            
            
            // if(posxPobj) Destroy(posxPobj);
            // if(posxMobj) Destroy(posxMobj);
            // if(poszPobj) Destroy(poszPobj);
            // if(poszMobj) Destroy(poszMobj);

            // posxPobj = SpawnSthAtNewPosition(curnewPositionxP);
            // posxMobj = SpawnSthAtNewPosition(curnewPositionxM);
            // poszPobj = SpawnSthAtNewPosition(curnewPositionzP);
            // poszMobj = SpawnSthAtNewPosition(curnewPositionzM);
            

            // xP -> zP -> xM -> zM            
            if (CheckPointInQuadrilateralAndTriangles(new Vector3(newPosition.x, newPosition.y, newPosition.z)))
            {
                pointInQuadrilateralAndTriangles = true;
            }
        }

        if (pointInQuadrilateralAndTriangles && num_of_tries <= max_tries)
        {
            return newPosition;
        }
        else
        {
            return new Vector3(-999, 999, -999);
        }
    }

    private void UpdateBiggestPlane()
    {
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.02f;
        ray.origin = raycastOrigin;
        ray.direction = Vector3.down;

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if(raycastManager.Raycast(ray, hits))
        {
            float biggestExtent = 0;

            foreach (var hit in hits)
            { 
                // Check if the hit corresponds to a horizontal plane
                if (hit.trackable is ARPlane plane && 
                    (plane.alignment == PlaneAlignment.HorizontalUp))
                {
                    // Calculate the distance from the hit point to the cat's position
                    float planeMaxExtent = Math.Max(plane.extents.x, plane.extents.y);

                    // Update the closest hit if the current hit is closer
                    if (biggestExtent < planeMaxExtent)
                    {
                        biggestExtent = planeMaxExtent;
                        currentPlane = plane;
                    }
                }
            }
            //Debug.Log("Current plane:" + currentPlane);
        }
        else
        {
            Debug.Log("No current plane found!");
        }
    }

    private GameObject SpawnSthAtNewPosition(Vector3 spawnPosition, bool one = false)
    {
        GameObject pref;
        if (!one)
        {
            pref = Instantiate(spawnablePrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            pref = Instantiate(spawnablePrefab1, spawnPosition, Quaternion.identity);
        }
        pref.name = ("Cy" + num_of_cylinder);
        num_of_cylinder += 1;
        return pref;
    }

    bool IsPointInTriangles(Vector2 P)
    {
        foreach (Vector3Triplet triangle in triangles)
        {
            Vector2 A = triangle.First;
            Vector2 B = triangle.Second;
            Vector2 C = triangle.Third;

            float denominator = (B.y - C.y) * (A.x - C.x) + (C.x - B.x) * (A.y - C.y);

            float alpha = ((B.y - C.y) * (P.x - C.x) + (C.x - B.x) * (P.y - C.y)) / denominator;
            float beta = ((C.y - A.y) * (P.x - C.x) + (A.x - C.x) * (P.y - C.y)) / denominator;
            float gamma = 1 - alpha - beta;

            if (alpha >= 0 && beta >= 0 && gamma >= 0)
            {
                return true;
            }
        }
        return false;
    }

    bool IsPointInQuadrilateral(Vector2 A, Vector2 B, Vector2 C, Vector2 D, Vector2 P)
    {
        // Calculate barycentric coordinates for triangle ABC
        float alphaABC = ((B.y - C.y) * (P.x - C.x) + (C.x - B.x) * (P.y - C.y)) /
                        ((B.y - C.y) * (A.x - C.x) + (C.x - B.x) * (A.y - C.y));

        float betaABC = ((C.y - A.y) * (P.x - C.x) + (A.x - C.x) * (P.y - C.y)) /
                        ((B.y - C.y) * (A.x - C.x) + (C.x - B.x) * (A.y - C.y));

        float gammaABC = 1 - alphaABC - betaABC;

        // Calculate barycentric coordinates for triangle ACD
        float alphaACD = ((C.y - D.y) * (P.x - D.x) + (D.x - C.x) * (P.y - D.y)) /
                        ((C.y - D.y) * (A.x - D.x) + (D.x - C.x) * (A.y - D.y));

        float betaACD = ((D.y - A.y) * (P.x - D.x) + (A.x - D.x) * (P.y - D.y)) /
                        ((C.y - D.y) * (A.x - D.x) + (D.x - C.x) * (A.y - D.y));

        float gammaACD = 1 - alphaACD - betaACD;

        // Check if the point is within the quadrilateral's boundaries
        return (alphaABC >= 0 && alphaABC <= 1 && betaABC >= 0 && betaABC <= 1 && gammaABC >= 0 && gammaABC <= 1) ||
            (alphaACD >= 0 && alphaACD <= 1 && betaACD >= 0 && betaACD <= 1 && gammaACD >= 0 && gammaACD <= 1);
    }

    private void ChangeCatState(CatState newState)
    {
        catAnimator.SetBool("IsIdle", false);
        catAnimator.SetBool("IsWalking", false);
        catAnimator.SetBool("IsEating", false);
        catAnimator.SetBool("IsMeowing", false);
        catAnimator.SetBool("IsPetted", false);
        currentState = newState;

        switch (currentState)
        {
            case CatState.Idle:
                catAnimator.SetBool("IsIdle", true);
                break;
            case CatState.Walking:
                catAnimator.SetBool("IsWalking", true);
                break;
            case CatState.Eating:
                catAnimator.SetBool("IsEating", true);
                break;
            case CatState.Meowing:
                catAnimator.SetBool("IsMeowing", true);
                break;
            case CatState.Petted:
                catAnimator.SetBool("IsPetted", true);
                break;
        }
    }    

    public enum CatState
    {
        Idle,
        Walking,
        Eating,
        Meowing,
        Petted
    }

    public class Vector3Triplet
    {
        public Vector3 First { get; set; }
        public Vector3 Second { get; set; }
        public Vector3 Third { get; set; }

        public Vector3Triplet(Vector3 first, Vector3 second, Vector3 third)
        {
            First = first;
            Second = second;
            Third = third;
        }
    }

    public bool IsBetween(double testValue, double bound1, double bound2)
    {
        return (testValue >= Math.Min(bound1,bound2) && testValue <= Math.Max(bound1,bound2));
    }

    private void OnEnable() {
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += FingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += FingerMove;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += FingerUp;
    }

    private void OnDisable() {
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= FingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= FingerMove;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp -= FingerUp;
    }

}
