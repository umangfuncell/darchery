using DG.Tweening;
using TMPro;
using UnityEngine;

public class WindSystem : MonoBehaviour
{
    public static WindSystem instance;

    [SerializeField] Vector2 windDirection;
    [SerializeField] float _windForce;

    public TextMeshProUGUI windForceTxt;
    public RectTransform windDirectionArrow;
    public GameObject windUI;
    public float angle;

    private void Awake()
    {
        instance = this;
    }

    public float WindForce 
    { 
        get => _windForce;
        set 
        { 
            _windForce = value;
            windForceTxt.text=_windForce.ToString("0.0");
        }
    }

    public Vector2 WindDirection 
    { 
        get => windDirection;
        set 
        { 
            windDirection = value;
            RotateToWindDirection(windDirection);
        }
    }

    void RotateToWindDirection(Vector2 dir)
    {
        Vector3 direction = dir - new Vector2(0, 0);
        angle=Vector3.SignedAngle(new Vector2(1,0),dir,Vector3.forward);
        windDirectionArrow.DOLocalRotate(new Vector3(0, 0, angle), 0.5f);
    }

    public void GenerateWind()
    {
        windUI.SetActive(true);
        GenerateWindDirection();
        GenerateWindForce();

        if (_windForce <= 0 || windDirection.Equals(Vector2.zero))
            windUI.SetActive(false);
    }

    public void GenerateWindForce()
    {
        WindForce = Random.Range(0, 5f);
    }

    public void GenerateWindDirection()
    {
        float x = Random.Range(-1, 2);
        float y = Random.Range(-1, 2);

        WindDirection = new Vector2(x, y);
    }

}
