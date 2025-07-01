using UnityEngine;

public enum SegmentType
{
    OneColor, TwoColor, ThreeColor
}
public class Segment : MonoBehaviour
{

    [SerializeField] SegmentType segmentType;
    public SegmentType SegmentType
    {
        get => segmentType;
        set
        {
            segmentType = value;
            SetCurrentRing();
        }
    }

    public float RadiusRing2X { get => radiusRing2X; set { radiusRing2X = value; SetRadius(); } }
    public float RadiusRing3X { get => radiusRing3X; set { radiusRing3X = value; SetRadius(); } }

    public GameObject ringA, ringB, ringC;

    public SkinnedMeshRenderer currentRing;
    public ColorShift currentColorShift;
    private float radiusRing2X = 0.5f;
    private float radiusRing3X = 0.5f;

    void SetCurrentRing()
    {
        switch (segmentType)
        {
            case SegmentType.OneColor:
                ringA.SetActive(true);
                ringB.SetActive(false);
                ringC.SetActive(false);
                currentColorShift = ringA.GetComponent<ColorShift>();
                currentRing = ringA.GetComponent<SkinnedMeshRenderer>();
                break;

            case SegmentType.TwoColor:
                ringB.SetActive(true);
                ringA.SetActive(false);
                ringC.SetActive(false);
                currentColorShift = ringB.GetComponent<ColorShift>();
                currentRing = ringB.GetComponent<SkinnedMeshRenderer>();
                break;
            case SegmentType.ThreeColor:
                ringC.SetActive(true);
                ringA.SetActive(false);
                ringB.SetActive(false);
                currentColorShift = ringC.GetComponent<ColorShift>();
                currentRing = ringC.GetComponent<SkinnedMeshRenderer>();
                break;
        }
    }

    void SetRadius()
    {
        switch (segmentType)
        {
            case SegmentType.TwoColor:
                currentRing.SetBlendShapeWeight(0, RadiusRing2X * 100);
                currentRing.SetBlendShapeWeight(1, (RadiusRing2X * 100) + 11.01f);
                break;
            case SegmentType.ThreeColor:
                currentRing.SetBlendShapeWeight(0, (RadiusRing2X * 100) - 18);
                currentRing.SetBlendShapeWeight(1, (RadiusRing2X * 100) + 1);
                currentRing.SetBlendShapeWeight(2, (1 - RadiusRing3X) * 100);
                currentRing.SetBlendShapeWeight(3, ((1 - RadiusRing3X) * 100) + 30);
                break;

        }
    }

    //private void OnValidate()
    //{
    //    if(SegmentType!=segmentType)
    //        SegmentType = segmentType;
    //}
}
