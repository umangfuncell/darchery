using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BoardModify : MonoBehaviour
{
    public static BoardModify Instance;

    public string levelName;

    [Serializable]
    public class SegmentData
    {
        [Range(0, 360)]
        public int angle = 10;
        public int score = 1;

        [ReadOnly]
        public float startAngle, endAngle;

        [HideInInspector] public List<Segment> segemnts = new();
        public SegmentType segmentType;

        public Color color1 = Color.cyan;

        [ShowIf("@this.segmentType == SegmentType.TwoColor || this.segmentType == SegmentType.ThreeColor")]
        public Color color2 = Color.yellow;

        [ShowIf("@this.segmentType == SegmentType.TwoColor || this.segmentType == SegmentType.ThreeColor")]
        [PropertyRange(0, 1)]
        public float distributionAtRadius = 0.5f;

        [ShowIf("@this.segmentType == SegmentType.ThreeColor")]
        public Color color3 = Color.green;

        [ShowIf("@this.segmentType == SegmentType.ThreeColor")]
        [PropertyRange(0, 1)]
        public float distributionAtRadius2 = 0.5f;

        public Transform lineSperate;
    }

    public List<Segment> allSegments;
    public List<SegmentData> segmentDatas = new();
    public SkinnedMeshRenderer bullsEye;
    public GameObject LineSeperator;
    public Transform lineParent;
    public List<GameObject> lines = new();
    public int counter = 0;
    public float boardDisatance = 0;
    [SerializeField] Transform board;

    private void Awake()
    {
        Instance = this;
    }

    private void OnValidate()
    {
        // Delete lineSeperator
        if (!Application.isPlaying)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (i >= segmentDatas.Count)
                {
                    DestroyImmediate(lines[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (i >= segmentDatas.Count)
                {
                    Destroy(lines[i]);
                }
            }
        }

        lines.RemoveAll(x => x == null);

        for (int i = 0; i < segmentDatas.Count; i++)
        {
            segmentDatas[i].angle = (int)Mathf.Round(segmentDatas[i].angle / 10f) * 10;
        }

        counter = 0;

        //Create Segment according to data
        for (int i = 0; i < segmentDatas.Count; i++)
        {
            var segment = segmentDatas[i];
            int x = segment.angle / 10;
            segment.segemnts.Clear();
            segment.startAngle = counter * 10;

            GameObject line;
            if (segment.lineSperate == null)
            {
                line = Instantiate(LineSeperator, lineParent);
                line.transform.localPosition = Vector3.zero;
                lines.Add(line);
                segment.lineSperate = line.transform;
            }
            else
            {
                line = segment.lineSperate.gameObject;
            }

            line.transform.localEulerAngles = new Vector3(0, 0, -segment.startAngle);

            for (int j = counter; j < counter + x && j < allSegments.Count; j++)
            {
                segment.segemnts.Add(allSegments[j]);
            }
            counter = counter + x;
            segment.endAngle = counter * 10;

            for (int j = 0; j < segment.segemnts.Count; j++)
            {
                var seg = segment.segemnts[j];
                seg.SegmentType = segment.segmentType;
                seg.currentColorShift.SetColor(segment.color1, segment.color2, segment.color3);
                seg.RadiusRing2X = segment.distributionAtRadius;
                seg.RadiusRing3X = segment.distributionAtRadius2;
            }
        }

        //Set Board Distance
        board.position = new Vector3(0, 0, boardDisatance);

    }

    public void GetScore(Vector2 dir)
    {
        int score = 0;
        float angle = GetDirectionAngle(dir);

        for (int i = 0; i < segmentDatas.Count; i++)
        {
            var segment = segmentDatas[i];

            if (angle > segment.startAngle && angle < segment.endAngle)
            {
                score = segment.score;
                break;
            }
        }

        Debug.Log(score);
    }

    public void SaveData()
    {

    }

    public static float GetDirectionAngle(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return (angle < 0) ? angle + 360f : angle;
    }
}
