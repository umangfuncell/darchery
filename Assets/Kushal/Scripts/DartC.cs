using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;  // Import Cinemachine
using UnityEngine.UI;
using TMPro;

public class DartC : MonoBehaviour
{
    public bool isGameActive = true; // Default: true

    [Header("Dart Settings")]
    public GameObject dartPrefab;    
    public Transform spawnPoint; 
    public Transform spawnPointmain; 

    public Transform dartboard;     
    public GameObject spherePrefab;  
    
    public int trajectoryPoints = 5000; // Number of spheres
    public float shootForce = 5f;    

    public float timeStep = 0.0005f; // Smaller step = more points = closer spacing
    public float dragSensitivity = 0.05f;  
     public float curveMultiplier = 0.3f; //  Curve Adjustment


    public GameObject currentDart;
    private bool isHolding = false;
    private List<GameObject> trajectorySpheres = new List<GameObject>();
    private Vector3 aimDirection;
     private Coroutine activeCoroutine;
     private bool canDragToAim = false;
     private Quaternion targetDartRotation = Quaternion.identity;
private Vector3 accumulatedOffset = Vector3.zero;
public Vector2 xClamp = new Vector2(-0.5f, 0.5f);
public Vector2 yClamp = new Vector2(-0.5f, 0.5f);
public Vector2 zClamp = new Vector2(0f, 0f); // Z isn't used but added for flexibility

private float followSmoothTime = 0.1f;
private Vector3 followVelocity = Vector3.zero;
private bool wasDragging = false;

public float dragCooldown = 5f; // Delay before FollowCamera activates
public float dragCooldownTimer = 0f;

private Vector3 dragStartOffset; // Stores the offset when drag begins
private Vector2 dragStartMouse;  // Stores mouse position when drag begins

     [Header("Camera Settings")]
    public CinemachineVirtualCamera defaultCam;
    private float fovHold = 40f;
    public float fovNormal = 30f;
    public float fovLerpSpeed = 3f;
        public float fovNEWLerpSpeed = 1f;

    public Vector3 cameraHoldPosition; 
    public Vector3 cameraReleasePosition; 
    private Coroutine fovCoroutine;

    [Header("Trajectory Line")]
    public LineRenderer trajectoryLine; //  Assign via Inspector
    private Coroutine lineFadeRoutine;


    [Header("Crossbow Transform Settings")]
    public Transform crossbow;  // Assign the crossbow transform in the inspector
    public Vector3 holdPosition;  // Position for holding
    public Vector3 releasePosition; // Position for release
    public float transitionSpeed = 5f; // Speed of lerping
    public Animator crossbowanim; //Animator of crossbow

    private Coroutine movementCoroutine;

    private bool isDragging = false;  // Track dragging state
private Vector2 lastMousePosition;
public float dragThreshold = 0.2f; // Minimum movement to detect drag
private Coroutine trajectoryCoroutine;

public static DartC instance;
private Coroutine followRoutine;

private bool isPointerDown = false;
private float holdStartTime = 0f;
private float holdThreshold = 0.2f; // Time (in seconds) required to count as hold

private bool isReadyForNextDart = true;

private bool isPreparingHold = false; // in progress, but not ready to aim
private Coroutine holdCoroutine;
 [Header("UI Fading")]
    public Image movesImage;
    public TMP_Text movesText;

    private Color movesImageOrigColor;
    private Color movesTextOrigColor;

private Transform scoreDummyTarget;

public float fovZoomed = 20f; // Ultra zoomed-in FOV when dart is flying
private bool hasPlayedStretch = false;

public bool shouldEnableCameraEffect = true;


public Cinemachine.CinemachineVirtualCamera holdCam; // Assign in Inspector
public Transform dartFollowOffset; // Optional: set as a child of dart for offset

public GameObject crossIconPrefab;
public GameObject activeCrossIcon;


private Quaternion dragStartRotation;

private Quaternion holdRotation;
public float arcVerticalOffset = -0.5f; // Tweak this negative value to move arc lower (e.g. -0.2f, -0.5f, etc.)
public bool useParabolaVisual = true; // Toggle: true = custom arc, false = physics sim
public float arcPeak = -0.08f; // Adjust: lower for flatter arc, 0 = straight, negative = dips
private bool sniperMode = true; // Set to true for straight line sniper mode

private Coroutine sniperTimerRoutine;
private float sniperTimerDuration = 10f; // can adjust in inspector

private Image sniperTimerImage; // reference to UI fill image
private bool isSniperTimerRunning = false;


void Awake()
{
    instance = this;
}

IEnumerator ManageTrajectory()
{
    while (isHolding && canDragToAim)
    {
        UpdateTrajectory();
        yield return null; // Wait for the next frame
    }
}



 void Update()
{
       if (!isGameActive)
        return;

    if (!isReadyForNextDart)
        return;

    if (Input.GetMouseButtonDown(0))
{
 //  Time.timeScale = 1f;
    isPointerDown = true;
    holdStartTime = Time.time;
    lastMousePosition = Input.mousePosition;


     dragStartOffset = accumulatedOffset;
    dragStartMouse = Input.mousePosition;
}
else if (Input.GetMouseButton(0) && isPointerDown)
{
  //  SetMovesUIAlpha(0f); // Fade out moves UI
    float heldTime = Time.time - holdStartTime;

    // --- STRETCH SOUND TRIGGER ---
    if (!hasPlayedStretch && heldTime >= holdThreshold)
    {
     //   GameObject.Find("AudioManager")?.GetComponent<AudioManager>()?.PlayStretch();
        hasPlayedStretch = true;
    }
    // --- END STRETCH SOUND TRIGGER ---

    if (!isHolding && !isPreparingHold && heldTime >= holdThreshold)
    {
        Debug.Log("Triggering StartHolding from Update...");
        isPreparingHold = true; // Block re-entrance immediately!
        StartHolding(); // Only after hold threshold
  //      Time.timeScale = 1f;
    }

    if (isHolding && canDragToAim)
    {
        float mouseDelta = Vector2.Distance(Input.mousePosition, lastMousePosition);
        isDragging = mouseDelta > dragThreshold;

        if (isDragging)
        {
            dragCooldownTimer = 0f;
            DragToAim();
            wasDragging = true;
     //       Time.timeScale = 1f;
        }
    }
}


   else if (Input.GetMouseButtonUp(0))
{
     
    if (isHolding)
    {
        ReleaseDart(); // Only if fully holding
        Time.timeScale = 1f; /////1.5
    }
   else if (isPreparingHold)
{
    // User released early: cancel animation, go back to release pos
    if (holdCoroutine != null)
    {
        StopCoroutine(holdCoroutine);
        holdCoroutine = null;
    }
    StartCoroutine(SmoothCameraToReleasePosition()); // optional smooth-back anim
    isPreparingHold = false;
    isHolding = false;
    isPointerDown = false;  // Move this here if needed
}

    else
    {
        CancelHold();
   //     Time.timeScale = 1f;
    }
    isDragging = false;
    wasDragging = false;
    isPointerDown = false;
}


    //  Handle smooth camera follow only when not dragging
    if (isHolding && !isDragging)
    {
        if (wasDragging)
        {
            dragCooldownTimer = dragCooldown; // Start timer once dragging ends
            wasDragging = false;
        }

        if (dragCooldownTimer > 0f)
        {
            dragCooldownTimer -= Time.deltaTime;
        }
        else
        {
            FollowCrossbowWithCamera(); // Only follow when cooldown ends
        }
    }
}

void LateUpdate()
{
       if (!isGameActive)
        return;
    if (currentDart != null && currentDart.transform.parent == crossbow) // only scale when dart is NOT yet parented to board
    {
        float fov = defaultCam.m_Lens.FieldOfView;

        // Adjust these based on your visual need
        float minFOV = 25f;
        float maxFOV = 50f;

        float minScale = 0.6f;
        float maxScale = 0.9f;

        float t = Mathf.InverseLerp(minFOV, maxFOV, fov);
        float targetScale = Mathf.Lerp(minScale, maxScale, t);

        currentDart.transform.localScale = Vector3.one * targetScale;
    }
}





   void MoveCrossbow(Vector3 targetPosition)
{
    crossbow.localPosition = targetPosition; //  Instantly move to position (No coroutine needed)
}


void Start()
{
     // Auto-assign LineRenderer by name if not set manually
    if (trajectoryLine == null)
    {
        GameObject obj = GameObject.Find("TrajectoryLine");
        if (obj != null)
        {
            trajectoryLine = obj.GetComponent<LineRenderer>();
        }
        else
        {
            Debug.LogWarning("TrajectoryLine object not found in scene.");
        }
    }
     StartCoroutine(ReadyNewDart());
if (movesImage == null)
            movesImage = GameObject.Find("MovesImage")?.GetComponent<Image>();
        if (movesText == null)
            movesText = GameObject.Find("MovesGlobal")?.GetComponent<TMP_Text>();

        if (movesImage != null)
            movesImageOrigColor = movesImage.color;
        if (movesText != null)
            movesTextOrigColor = movesText.color;

 GameObject targetObj = GameObject.Find("ScoreDummyPosition");
    if (targetObj != null)
        scoreDummyTarget = targetObj.transform;
    else
        Debug.LogWarning("ScoreDummyPosition not found in scene!");
if (holdCam == null)
    {

        GameObject obj = GameObject.Find("HoldCam");
        
        if (obj != null)
            holdCam = obj.GetComponent<CinemachineVirtualCamera>();
        else
            Debug.LogWarning("HoldCam not found in scene!");
    }
}


void StartHolding()
{
    Debug.Log("StartHolding called! Is pointer down? " + isPointerDown + " | isHolding: " + isHolding + " | isPreparingHold: " + isPreparingHold);
    if (holdCoroutine != null)
        StopCoroutine(holdCoroutine);
        holdRotation = crossbow.localRotation;
         accumulatedOffset = Vector3.zero; // <-- ADD THIS LINE
    isPreparingHold = true;
    holdCoroutine = StartCoroutine(HandleHoldSetup());


}




IEnumerator HandleHoldSetup()
{
    float duration = 0.35f;
    float elapsed = 0f;

    float startFOV = defaultCam.m_Lens.FieldOfView;
    Vector3 startCrossbowPos = crossbow.localPosition;
    Vector3 startCameraPos = defaultCam.transform.position;
    Quaternion startRot = crossbow.localRotation;
   Quaternion targetRot = Quaternion.Euler(startRot.eulerAngles.x, 0f, startRot.eulerAngles.z); //-20 at first

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        defaultCam.m_Lens.FieldOfView = Mathf.Lerp(startFOV, fovHold, t);
        crossbow.localPosition = Vector3.Lerp(startCrossbowPos, holdPosition, t);
        defaultCam.transform.position = Vector3.Lerp(startCameraPos, cameraHoldPosition, t); // Camera Movement
        crossbow.localRotation = Quaternion.Lerp(startRot, targetRot, t);

        yield return null;
    }

    defaultCam.m_Lens.FieldOfView = fovHold;
    crossbow.localPosition = holdPosition;
   // defaultCam.transform.position = cameraHoldPosition;

    Debug.Log("HoldSetup finished. isPointerDown: " + isPointerDown);

    if (isPointerDown)
    {
      //   dragStartMouse = Input.mousePosition;
    dragStartRotation = crossbow.localRotation;
        Debug.Log("Enabling drag/aim mode!");
        canDragToAim = true;
         lastMousePosition = Input.mousePosition;  // <<<< Crucial: Set on enter aim
  //  dragStartMouse = Input.mousePosition;     // <<<< For safety
        isHolding = true;
        isPreparingHold = false;
        aimDirection = (dartboard.position - spawnPoint.position).normalized;

        ClearTrajectorySpheres();
        trajectoryLine.enabled = true;
        if (trajectoryCoroutine != null) StopCoroutine(trajectoryCoroutine);
        trajectoryCoroutine = StartCoroutine(ManageTrajectory());



    }
    else
    {
        Debug.Log("Player released too early, NOT enabling drag/aim.");
        isHolding = false;
        isPreparingHold = false;
        canDragToAim = false;
    }
}

 void FollowCrossbowWithCamera()
{
    float followSpeed = 1f;
    float movementMultiplier = 8f; // Adjust to move more left/right/up/down

    // Calculate crossbow offset from hold position with adjustable multiplier
    Vector3 crossbowOffset = (crossbow.localPosition - holdPosition) * movementMultiplier;
    Vector3 targetCameraPos = cameraHoldPosition + crossbowOffset;

    // Optional Wind Effect (Slowed down with lower frequency)
    float windEffectStrength = 0.2f;
    float windEffectFrequencyX = GameManager.instance != null ? GameManager.instance.windEffectFrequencyX : 0.5f; // Default fallback
    float windEffectFrequencyY = 0.7f; // Slower vertical movement

    Vector3 windEffect = new Vector3(
        Mathf.Sin(Time.time * windEffectFrequencyX) * windEffectStrength,
        Mathf.Sin(Time.time * windEffectFrequencyY) * windEffectStrength,
        0);

    // Combine with camera movement
   /// targetCameraPos += windEffect;

    // Smooth follow using Lerp
    defaultCam.transform.position = Vector3.Lerp(defaultCam.transform.position, targetCameraPos, Time.deltaTime * followSpeed);
}


//Reset after throwing
public IEnumerator ResetAfterThrow()
    {
        Time.timeScale = 1f;
if (holdCam != null)
        holdCam.gameObject.SetActive(false); // Disable hold cam
        isReadyForNextDart = true;
        StartCoroutine(ReadyNewDartPostRelease());

        float duration = 0.8f;
        float elapsed = 0f;

        float startFOV = defaultCam.m_Lens.FieldOfView;
        Vector3 startCrossbowPos = crossbow.localPosition;
        Vector3 startCameraPos = defaultCam.transform.position;
Quaternion startRot = crossbow.localRotation;
Quaternion targetRot = Quaternion.Euler(0f, startRot.eulerAngles.y, startRot.eulerAngles.z);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            defaultCam.m_Lens.FieldOfView = Mathf.Lerp(startFOV, fovNormal, t);
            crossbow.localPosition = Vector3.Lerp(startCrossbowPos, releasePosition, t);
            defaultCam.transform.position = Vector3.Lerp(startCameraPos, cameraReleasePosition, t); //  Camera Reset
crossbow.localRotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }

        defaultCam.m_Lens.FieldOfView = fovNormal;
        crossbow.localPosition = releasePosition;
        defaultCam.transform.position = cameraReleasePosition;
crossbow.localEulerAngles = Vector3.zero;


    //    SetMovesUIAlpha(1f); // Fade in moves UI
        


    }

  /* void DragToAim() //NO CAM MOVEMENT
{
    if (currentDart == null) return;

    Vector2 currentMousePosition = Input.mousePosition;
    Vector2 delta = currentMousePosition - lastMousePosition;
    lastMousePosition = currentMousePosition;

    float dragSpeed = 1.5f;
    Vector3 inputDelta = new Vector3(delta.x / Screen.width, delta.y / Screen.height, 0f) * dragSpeed;

    // Accumulate drag as offset
    accumulatedOffset += inputDelta;

    // Clamp accumulated offset to avoid excessive rotation
    accumulatedOffset.x = Mathf.Clamp(accumulatedOffset.x, xClamp.x, xClamp.y);
    accumulatedOffset.y = Mathf.Clamp(accumulatedOffset.y, yClamp.x, yClamp.y);

    // Convert accumulated offset to rotation angles (tweak sensitivity as needed)
    float rotationY = accumulatedOffset.x * 90f;  // X offset → yaw (left/right)
    float rotationX = -accumulatedOffset.y * 60f; // Y offset → pitch (up/down)

    // Apply rotation relative to holdRotation (e.g. Quaternion.Euler for local space)
    Quaternion dragRot = Quaternion.Euler(rotationX, rotationY, 0);
    crossbow.localRotation = holdRotation * dragRot;

    // Update aim direction
    aimDirection = crossbow.forward.normalized;

    UpdateTrajectory();
}
*/




 void DragToAim()
{
    if (currentDart == null) return;

    Vector2 currentMousePosition = Input.mousePosition;
    Vector2 delta = currentMousePosition - lastMousePosition;
    lastMousePosition = currentMousePosition;

    float dragSpeed = 1.5f;
    Vector3 inputDelta = new Vector3(delta.x / Screen.width, delta.y / Screen.height, 0f) * dragSpeed;

    // Accumulate offset
    accumulatedOffset += inputDelta;

    // Clamp offset
    accumulatedOffset.x = Mathf.Clamp(accumulatedOffset.x, xClamp.x, xClamp.y);
    accumulatedOffset.y = Mathf.Clamp(accumulatedOffset.y, yClamp.x, yClamp.y);

    // 🔁 Apply same WORLD offset to both camera and crossbow from their base positions
    Vector3 targetCrossbowPos = holdPosition + accumulatedOffset;
    Vector3 targetCameraPos = cameraHoldPosition + accumulatedOffset;

   // crossbow.localPosition = Vector3.Lerp(crossbow.localPosition, targetCrossbowPos, Time.deltaTime * 12f);
    defaultCam.transform.localPosition = cameraHoldPosition + accumulatedOffset;

    UpdateTrajectory();
} 

IEnumerator SniperTimerRoutine()
{
    float timer = sniperTimerDuration;
    if (sniperTimerImage != null)
        sniperTimerImage.fillAmount = 1f;

    while (timer > 0f && isHolding && sniperMode)
    {
        timer -= Time.deltaTime;
        if (sniperTimerImage != null)
            sniperTimerImage.fillAmount = Mathf.Clamp01(timer / sniperTimerDuration);

        yield return null;
    }

    // Only auto-shoot if still holding and timer expired (not interrupted by shot)
    if (timer <= 0f && isHolding && sniperMode)
    {

        Time.timeScale = 1f; ////1.5f
isPointerDown = false;
isDragging = false;
wasDragging = false;

        ReleaseDart();
    }
    isSniperTimerRunning = false;
}

//public float arcPeak = -0.5f; // Negative = flatten/lower arc, 0 = normal, positive = higher


void UpdateTrajectory()
{
    if (trajectoryLine != null)
        trajectoryLine.enabled = true;

    List<Vector3> linePoints = new List<Vector3>();

  
    
if (sniperMode)
{
    // Use the same arc and wind logic as non-sniper mode:
    Vector3 initialVelocity = (aimDirection * shootForce) + (Vector3.up * (shootForce * 0.3f));
    Vector3 position = spawnPoint.position;
    Vector3 velocity = initialVelocity;

    float stepMultiplier = 0.5f;
    List<Vector3> physicsPositions = new List<Vector3>();
    physicsPositions.Add(position);

    for (int i = 1; i < trajectoryPoints; i++)
    {
       if (position.z >= -0.35f)
            break;

        // --- Add wind ---
        float windSpeed = 2f;
        float windIntensity = 0.015f;
        float windOffset = Mathf.Sin(Time.time * windSpeed) * windIntensity;
        velocity.x += windOffset;

        // --- Physics step ---
        velocity += Physics.gravity * (timeStep * stepMultiplier);
        position += velocity * (timeStep * stepMultiplier);

        physicsPositions.Add(position);
    }

    // --- Arc curve adjustment ---
    int N = physicsPositions.Count;
    for (int i = 0; i < N; i++)
    {
        Vector3 p = physicsPositions[i];
        float t = (N == 1) ? 0 : i / (float)(N - 1);
        float curve = 4 * t * (1 - t);

        p.y += arcPeak * curve; // Use same arcPeak variable as before
        linePoints.Add(p);

        // Spheres logic (same as before)
        GameObject sphere;
        if (trajectorySpheres.Count <= i)
        {
            sphere = Instantiate(spherePrefab, p, Quaternion.identity);
            trajectorySpheres.Add(sphere);
        }
        else
        {
            sphere = trajectorySpheres[i];
            sphere.transform.position = p;
        }
        sphere.SetActive(true);
    }
}


    
    else
    {

        
        // --- ORIGINAL ARC/WIND/PHYSICS LOGIC ---
        Vector3 initialVelocity = (aimDirection * shootForce) + (Vector3.up * (shootForce * 0.3f));
        Vector3 position = spawnPoint.position;
        Vector3 velocity = initialVelocity;

        float stepMultiplier = 0.5f;
        List<Vector3> physicsPositions = new List<Vector3>();
        physicsPositions.Add(position);
        for (int i = 1; i < trajectoryPoints; i++)
        {
            if (position.z >= -0.4f)
                break;

            // Add wind
            float windSpeed = 3f;
            float windIntensity = 0.02f;
            float windOffset = Mathf.Sin(Time.time * windSpeed) * windIntensity;
            velocity.x += windOffset;

            // Physics step
            velocity += Physics.gravity * (timeStep * stepMultiplier);
            position += velocity * (timeStep * stepMultiplier);

            physicsPositions.Add(position);
        }

        // --- Parabola Y adjustment, arcPeak ---
        int N = physicsPositions.Count;
        for (int i = 0; i < N; i++)
        {
            Vector3 p = physicsPositions[i];

            float t = (N == 1) ? 0 : i / (float)(N - 1);
            float curve = 4 * t * (1 - t);

            p.y += arcPeak * curve;
            linePoints.Add(p);

            // Spheres
            GameObject sphere;
            if (trajectorySpheres.Count <= i)
            {
                sphere = Instantiate(spherePrefab, p, Quaternion.identity);
                trajectorySpheres.Add(sphere);
            }
            else
            {
                sphere = trajectorySpheres[i];
                sphere.transform.position = p;
            }
            sphere.SetActive(true);
        }
    }

    // --- Cross icon at end ---
   // --- Cross icon at end ---
if (linePoints.Count > 0)
{
    if (activeCrossIcon == null)
        activeCrossIcon = Instantiate(crossIconPrefab);

    Vector3 crossPos = linePoints[linePoints.Count - 1];
    crossPos.z = -0.5f;
    activeCrossIcon.transform.position = crossPos;
    activeCrossIcon.SetActive(true);

    // --- SNIPER MODE TIMER ---
    if (sniperMode)
    {
        if (!isSniperTimerRunning && !GameManager.instance.isTutorial)
        {
            // Find fill image (child: Canvas/Image)
           if (sniperTimerImage == null)
{
    Transform canvas = activeCrossIcon.transform.Find("Canvas");
    if (canvas)
    {
        sniperTimerImage = canvas.GetComponentInChildren<Image>(true); // true = include inactive
    }
}

            // Start the timer
            if (sniperTimerRoutine != null)
                StopCoroutine(sniperTimerRoutine);
            sniperTimerRoutine = StartCoroutine(SniperTimerRoutine());
            isSniperTimerRunning = true;
        }
    }
    else
    {
        // Not sniper, cancel any timer/fill
        if (sniperTimerRoutine != null)
            StopCoroutine(sniperTimerRoutine);
        if (sniperTimerImage != null)
            sniperTimerImage.fillAmount = 0f;
        isSniperTimerRunning = false;
    }
}
else if (activeCrossIcon != null)
{
    activeCrossIcon.SetActive(false);
    // Not sniper, cancel any timer/fill
    if (sniperTimerRoutine != null)
        StopCoroutine(sniperTimerRoutine);
    if (sniperTimerImage != null)
        sniperTimerImage.fillAmount = 0f;
    isSniperTimerRunning = false;
}
  // --- LineRenderer ---
    trajectoryLine.positionCount = linePoints.Count;
    trajectoryLine.SetPositions(linePoints.ToArray());
    if (sniperMode)
    {
        // Fully transparent
        Color transparent = new Color(1, 1, 1, 0.02f); // tweak alpha as needed
        trajectoryLine.startColor = transparent;
        trajectoryLine.endColor = transparent;
    }
    else
    {
        SetLineRendererAlphaGradient();  // restores normal gradient
    }

    

    // --- Disable unused spheres ---
    for (int i = linePoints.Count; i < trajectorySpheres.Count; i++)
    {
        if (trajectorySpheres[i] != null)
            trajectorySpheres[i].SetActive(false);
    }
}


void SetLineRendererAlphaGradient()
{
    if (trajectoryLine == null) return;

   Gradient gradient = new Gradient();

// Color stays white (or any color you like)
GradientColorKey[] colorKeys = new GradientColorKey[2];
colorKeys[0] = new GradientColorKey(Color.white, 0f);
colorKeys[1] = new GradientColorKey(Color.white, 1f);

// Alpha fades from 1 -> 0 over time
GradientAlphaKey[] alphaKeys = new GradientAlphaKey[6];
// alphaKeys[0] = new GradientAlphaKey(1.0f, 0f);      // 0% - Fully visible
// alphaKeys[1] = new GradientAlphaKey(1.0f, 0.05f);   // 5% - Still fully visible
// alphaKeys[2] = new GradientAlphaKey(0.3f, 0.06f);    // 6% - Fading begins
// alphaKeys[3] = new GradientAlphaKey(0.0f, 0.2f);    // 20% - Almost invisible
// alphaKeys[4] = new GradientAlphaKey(0.0f, 1f);      // 100% - Fully transparent

alphaKeys[0] = new GradientAlphaKey(1.0f, 0f);     // 0% - Fully visible
alphaKeys[1] = new GradientAlphaKey(1.0f, 0.1f);   // 10% - Fully visible
alphaKeys[2] = new GradientAlphaKey(0.6f, 0.4f);   // 40% - 60% visible
alphaKeys[3] = new GradientAlphaKey(0.4f, 0.6f);   // 60% - 40% visible
alphaKeys[4] = new GradientAlphaKey(0.2f, 1.0f);   // 80% - 20% visible
//alphaKeys[5] = new GradientAlphaKey(0.0f, 1.0f);   // 100% - Fully transparent



gradient.SetKeys(colorKeys, alphaKeys);
trajectoryLine.colorGradient = gradient;
}

    

    void SetObjectTransparency(GameObject obj, float alpha)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            Color color = mat.color;
            color.a = Mathf.Clamp01(alpha);  
            mat.color = color;
        }
    }

 void ReleaseDart()
{
    
    if (currentDart == null) return;
isReadyForNextDart = false;

isPointerDown = false;
isDragging = false;
wasDragging = false;
canDragToAim = false;
isHolding = false;


// --- Cancel sniper timer on manual shot ---
if (sniperTimerRoutine != null)
{
    StopCoroutine(sniperTimerRoutine);
    sniperTimerRoutine = null;
    isSniperTimerRunning = false;
}
if (sniperTimerImage != null)
    sniperTimerImage.fillAmount = 0f;
        DartC.instance.activeCrossIcon.SetActive(false);


    // Unparent the dart while keeping its world position and rotation
    currentDart.transform.SetParent(null);
   
// Ensure dart matches crossbow's rotation
//currentDart.transform.rotation = crossbow.transform.rotation;

//New Idea
//currentDart.transform.SetParent(null, true);
// Only adjust slightly to match spawn point visually if needed
//currentDart.transform.position = Vector3.Lerp(currentDart.transform.position, spawnPoint.position, 0.05f);
 if (fovCoroutine != null) StopCoroutine(fovCoroutine);
    fovCoroutine = StartCoroutine(LerpMoreFOV(fovZoomed)); // Zoom in when dart is released

//currentDart.transform.position = spawnPoint.position; // Ensure correct start position
//currentDart.transform.rotation = crossbow.transform.rotation; // Apply accurate rotation
crossbowanim.SetBool("Start", false);
    crossbowanim.SetBool("Release", true);
GameObject.Find("AudioManager")?.GetComponent<AudioManager>()?.PlayRelease();

// Check ahead of dart where it's going
//GameObject predictedHit;
//bool shouldEnableCameraEffect = PredictTargetRequiresCamera(out predictedHit);

if (shouldEnableCameraEffect)
{
     trajectoryLine.enabled = false;
    // Enable camera-following trail, etc.
   // currentDart.transform.GetChild(0).gameObject.SetActive(true);
   // currentDart.transform.GetChild(1).gameObject.SetActive(true);

    if (holdCam != null)
    {
        
     CinemachineTransposer transposer = holdCam.GetCinemachineComponent<CinemachineTransposer>();
if (transposer != null)
    transposer.m_FollowOffset = new Vector3(0, 0, -1); // Example offset: tweak as needed

       holdCam.gameObject.SetActive(true);
        holdCam.Follow = currentDart.transform; // or dartFollowOffset if you want custom offset
        holdCam.LookAt = currentDart.transform;
    }
}
else
{
         trajectoryLine.enabled = false;

  currentDart.transform.GetChild(0).gameObject.SetActive(false);
    currentDart.transform.GetChild(1).gameObject.SetActive(false);

}
               

       



   

    // Ensure Rigidbody is not Kinematic
  Rigidbody rb = currentDart.GetComponent<Rigidbody>();
rb.isKinematic = false;
rb.useGravity = true;
rb.velocity = Vector3.zero;
rb.angularVelocity = Vector3.zero;


    // Apply force based on the crossbow's actual rotation
    Vector3 correctedDirection = crossbow.transform.forward.normalized;
float arcFactor = 5f; // controls the height of the arc (stable)
float speedFactor = 100f; // controls how fast the dart flies (increase this only)

Vector3 arc = Vector3.up * arcFactor;
Vector3 direction = crossbow.forward.normalized;

Vector3 initialVelocity = (direction + arc).normalized * shootForce * speedFactor;



initialVelocity *= 2f; //  2x speed without changing the arc shape (dart speed)

float dartSpeedMultiplier = 10f; // Try 1.5f, 2f, 3f, etc.
rb.AddForce(initialVelocity * dartSpeedMultiplier, ForceMode.VelocityChange);
//rb.AddForce(Vector3.forward * 200f, ForceMode.VelocityChange);

//Time.timeScale = 1f; //2.5


    // Start trajectory following
followRoutine = StartCoroutine(FollowTrajectory());
    isHolding = false;
    canDragToAim = false;

    if (fovCoroutine != null) StopCoroutine(fovCoroutine);
///    StartCoroutine(ResetAfterThrow());
}


  IEnumerator FollowTrajectory()
{
    Rigidbody rb = currentDart.GetComponent<Rigidbody>();
    rb.isKinematic = false;

    for (int i = 0; i < trajectorySpheres.Count - 1; i++)
    {
        // Safety checks!
        if (currentDart == null) yield break;
        if (currentDart.transform == null) yield break;

        GameObject startSphere = trajectorySpheres[i];
        GameObject endSphere = trajectorySpheres[i + 1];

        Vector3 startPos = startSphere.transform.position;
        Vector3 endPos = endSphere.transform.position;

        float segmentDistance = Vector3.Distance(startPos, endPos);
        float segmentSpeed = shootForce * 0.5f; // tweak multiplier for realism
        float segmentTime = segmentDistance / segmentSpeed;

        float elapsed = 0f;

        while (elapsed < segmentTime)
        {
            if (currentDart == null) yield break;    // <- fix: dart destroyed, exit
            if (currentDart.transform == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / segmentTime);

            // Position
            currentDart.transform.position = Vector3.Lerp(startPos, endPos, t);

            // Rotation
            Vector3 direction = endPos - currentDart.transform.position;
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                currentDart.transform.rotation = Quaternion.Slerp(currentDart.transform.rotation, targetRot, Time.deltaTime * shootForce);
            }

            yield return null;
        }
    }

    // Always check again before using
    if (currentDart != null)
    {
        currentDart = null;
    }
    ClearTrajectorySpheres();
    if (trajectoryCoroutine != null)
    {
        StopCoroutine(trajectoryCoroutine);
        trajectoryCoroutine = null;
    }

    trajectoryLine.enabled = false;
}


private List<Vector3> debugPredictionPoints = new List<Vector3>();
private GameObject predictedHitTarget = null;


public void StopDartFollowing()
{
    if (followRoutine != null)
    {
        StopCoroutine(followRoutine);
        followRoutine = null;
    }
}

private void OnDrawGizmos()
{
    if (debugPredictionPoints == null || debugPredictionPoints.Count < 2)
        return;

    Gizmos.color = Color.cyan;

    for (int i = 0; i < debugPredictionPoints.Count - 1; i++)
    {
        Gizmos.DrawLine(debugPredictionPoints[i], debugPredictionPoints[i + 1]);
        Gizmos.DrawSphere(debugPredictionPoints[i], 0.01f);
    }

    if (predictedHitTarget != null)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(predictedHitTarget.transform.position, 0.3f);
    }
}



 void ClearTrajectorySpheres()
{
    foreach (var sphere in trajectorySpheres)
    {
        if (sphere != null)
            Destroy(sphere);
    }
    trajectorySpheres.Clear();

    //  Clear & disable LineRenderer
    if (trajectoryLine != null)
    {
        trajectoryLine.positionCount = 0;
        trajectoryLine.enabled = false; //  Important: turn off rendering
    }
}



IEnumerator FadeLineRenderer(float duration = 1f)
{
    if (trajectoryLine == null) yield break;

    Gradient gradient = trajectoryLine.colorGradient;
    GradientAlphaKey[] originalAlphas = gradient.alphaKeys;

    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        float newAlpha = Mathf.Lerp(1f, 0f, t); // Alpha goes from 1 to 0

        GradientAlphaKey[] fadedAlphas = new GradientAlphaKey[originalAlphas.Length];
        for (int i = 0; i < originalAlphas.Length; i++)
        {
            fadedAlphas[i] = new GradientAlphaKey(newAlpha, originalAlphas[i].time);
        }

        Gradient newGradient = new Gradient();
        newGradient.SetKeys(gradient.colorKeys, fadedAlphas);
        trajectoryLine.colorGradient = newGradient;

        yield return null;
    }

    trajectoryLine.enabled = false; // Hide after fade
}

     IEnumerator LerpFOV(float targetFOV)
    {
        float startFOV = defaultCam.m_Lens.FieldOfView;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fovLerpSpeed;
            defaultCam.m_Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsedTime);
            yield return null;
        }

        defaultCam.m_Lens.FieldOfView = targetFOV; // Ensure exact value
    }

    public float fovLerpDuration = 5f; // Make this public to tweak in Inspector

IEnumerator LerpMoreFOV(float targetFOV)
{
    float startFOV = defaultCam.m_Lens.FieldOfView;
    float elapsed = 0f;

    while (elapsed < fovLerpDuration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / fovLerpDuration);
        defaultCam.m_Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
        yield return null;
    }
    defaultCam.m_Lens.FieldOfView = targetFOV;
}

void CancelHold()
{
    // Reset anything visually started
    if (currentDart != null)
    {
     //   Destroy(currentDart);
      //  currentDart = null;
    }

    if (trajectoryCoroutine != null)
    {
        StopCoroutine(trajectoryCoroutine);
        trajectoryCoroutine = null;
    }

    ClearTrajectorySpheres();

    // Optional: reset FOV and camera instantly
    defaultCam.m_Lens.FieldOfView = fovNormal;
    defaultCam.transform.position = cameraReleasePosition;
    crossbow.localPosition = releasePosition;
   // crossbowanim.SetBool("Start", false);
}
IEnumerator SmoothCameraToReleasePosition()
{
    float duration = 0.3f;
    float elapsed = 0f;

    Vector3 startCrossbowPos = crossbow.localPosition;
    Vector3 startCameraPos = defaultCam.transform.position;
    float startFOV = defaultCam.m_Lens.FieldOfView;
    Quaternion startCrossbowRot = crossbow.localRotation;

    Vector3 endCrossbowPos = releasePosition;
    Vector3 endCameraPos = cameraReleasePosition;
    float endFOV = fovNormal;
    Quaternion endCrossbowRot = Quaternion.Euler(0f, startCrossbowRot.eulerAngles.y, startCrossbowRot.eulerAngles.z);

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        crossbow.localPosition = Vector3.Lerp(startCrossbowPos, endCrossbowPos, t);
        crossbow.localRotation = Quaternion.Lerp(startCrossbowRot, endCrossbowRot, t);

        defaultCam.transform.position = Vector3.Lerp(startCameraPos, endCameraPos, t);
        defaultCam.m_Lens.FieldOfView = Mathf.Lerp(startFOV, endFOV, t);

        yield return null;
    }

    // Ensure exact final values at end
    crossbow.localPosition = endCrossbowPos;
    crossbow.localRotation = endCrossbowRot;
    defaultCam.transform.position = endCameraPos;
    defaultCam.m_Lens.FieldOfView = endFOV;
}



public IEnumerator ReadyNewDart()
{
    Time.timeScale = 1f;
    isReadyForNextDart = false;

    // Play crossbow ready animation
    PlayCrossbowStartAnimation();

    // Wait 0.5 seconds
    yield return new WaitForSeconds(1.5f); //1.6


    // Spawn dart
    SpawnNewDart();

    // Dart is now ready (but NOT yet aiming/trajectory)
    isReadyForNextDart = true;
}
public IEnumerator ReadyNewDartPostRelease()
{
    Time.timeScale = 1f;
    isReadyForNextDart = false;

    // Play crossbow ready animation
    PlayCrossbowStartAnimation();

    // Wait 0.5 seconds
    yield return new WaitForSeconds(1.7f);


    // Spawn dart
    SpawnNewDart();

    // Dart is now ready (but NOT yet aiming/trajectory)
    isReadyForNextDart = true;
}

// Handles just the crossbow animation for starting/ready
public void PlayCrossbowStartAnimation()
{
    Time.timeScale = 1f;
    if (crossbowanim != null)
    {
        crossbowanim.SetBool("Release", false);
        crossbowanim.SetBool("Start", true);
    }
}

// Handles instantiating and setting up the dart
public void SpawnNewDart()
{
    Time.timeScale = 1f;
    if (currentDart != null)
    {
        Destroy(currentDart); // Safety: remove any leftover dart
    //    StopAllCoroutines(); // or keep reference and stop only the right coroutine!

    }
    currentDart = Instantiate(dartPrefab, spawnPointmain.position, Quaternion.identity);
    currentDart.transform.SetParent(crossbow, true);
    currentDart.transform.localScale = new Vector3(1f, 1f, 1f);
    currentDart.GetComponent<Rigidbody>().isKinematic = true;
    currentDart.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
}

  public void SetMovesUIAlpha(float alpha)
    {
        if (movesImage != null)
        {
            Color c = movesImage.color;
            c.a = alpha;
            movesImage.color = c;
        }
        if (movesText != null)
        {
            Color c = movesText.color;
            c.a = alpha;
            movesText.color = c;
        }
    }

}

