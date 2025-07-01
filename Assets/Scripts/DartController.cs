using UnityEngine;
using System;
using System.Collections;

public class DartController : MonoBehaviour
{
    [SerializeField] Transform _dart;
    [SerializeField] RectTransform _crosshair;
    [SerializeField] float _dartSpeed;
    [SerializeField] float _dartForce;
    [SerializeField] Vector2 _windDirection;
    [SerializeField] float _windSpeed;
    [SerializeField] Vector3 movement;
    [SerializeField] float offsetMultiplier;

    [SerializeField] GameObject canvas;
    public GameObject defaultCam, holdCam;
    [SerializeField] Vector3 holdPos;
    [SerializeField] Vector3 offsetPos;
    [SerializeField] Vector3 lastPos;
    [SerializeField] bool isHolding = false;
    public bool ApplyWind;

    [SerializeField] Dart dartPrefab;
    [SerializeField] Transform dartSpawnPos;
    [SerializeField] Dart currentDart;
    [SerializeField] Vector3 pointInSpace;

    bool isHitting = false;
    Ray ray;
    Camera mainCam;

    Vector3 delayedTargetPosition;
    [SerializeField] float movementDelay = 0.5f;

    [SerializeField] TrajectoryRenderer trajectoryRenderer;
[SerializeField] float initialForce = 15f;

    void Start()
    {
        mainCam = Camera.main;
        delayedTargetPosition = _dart.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.GetRemainingMoves() <= 0)
            return;
        if (Input.GetMouseButtonDown(0) && !isHolding)
        {
            _dart.transform.position = new Vector3(0.340000004f, -0.300000012f, -8.88100052f);
            if(ApplyWind == true)
            {
            WindSystem.instance.GenerateWind();
            }
            currentDart = Instantiate(dartPrefab, dartSpawnPos);
            currentDart.transform.localPosition = Vector3.zero;
            currentDart.transform.localEulerAngles = Vector3.zero;
            defaultCam.SetActive(false);
            holdCam.SetActive(true);
            lastPos = holdPos = Input.mousePosition;
            canvas.SetActive(true);
            isHolding = true;
            //Debug.LogError("Arrow Spawned");
        }

        Vector3 velocity = Vector3.zero;

        if (isHolding)
        {
//        trajectoryRenderer.ShowTrajectory(dartSpawnPos.position, velocity);
            if ((lastPos - Input.mousePosition).magnitude > 0.05f)
                offsetPos = Input.mousePosition - lastPos;

            var pos = _dart.transform.position;

            delayedTargetPosition = Vector3.Lerp(delayedTargetPosition, pos + offsetPos, Time.deltaTime / movementDelay);

            _dart.transform.position = Vector3.SmoothDamp(pos, delayedTargetPosition, ref velocity, _dartSpeed);

            //_dart.transform.position =Vector3.SmoothDamp(pos,pos+ offsetPos,ref velocity, _dartSpeed);

            //Adding Wind Direction with Force
            var rayPostion = _crosshair.position;
            if(ApplyWind == true)
            {
            rayPostion += ((Vector3)WindSystem.instance.WindDirection.normalized * WindSystem.instance.WindForce * offsetMultiplier);
            }
            ray = mainCam.ScreenPointToRay(rayPostion);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 150))
            {
                Debug.DrawRay(ray.origin, ray.direction * 150f, Color.green);
                pointInSpace = hitInfo.point;
                isHitting = true;
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * 150f, Color.red);
                isHitting = false;
                trajectoryRenderer.HideTrajectory();
            }
        }

        if (isHolding && Input.GetMouseButtonUp(0))
        {
            if (isHitting)
            {
                currentDart.isHit = true;
                currentDart.transform.LookAt(pointInSpace);
             //   currentDart.DoMoveToTarget(pointInSpace);


            }
            else
            {
                currentDart.isHit = false;
            }

            Vector2 direction = (Vector2)pointInSpace - Vector2.zero;
            BoardModify.Instance.GetScore(direction);
            currentDart.transform.SetParent(null);
            currentDart.transform.localScale = Vector3.one * 0.32f;
            isHolding = false;
            defaultCam.SetActive(true);
            holdCam.SetActive(false);
            canvas.SetActive(false);
            currentDart.force = _dartForce;
            currentDart.Release = true;
            StartCoroutine(DeductMoveWhenHit()); // Deduct Moves

        }

        lastPos = Input.mousePosition;
    }
    IEnumerator DeductMoveWhenHit()
    {
        yield return new WaitForSeconds(2.5f); //Wait for dart to reach target before deducting move
        GameManager.instance.DeductMove();
    }
}

