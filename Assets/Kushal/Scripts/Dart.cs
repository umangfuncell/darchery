using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class Dart : MonoBehaviour
{
    private bool release = false;
    public float force = 10f;
    public float timeToReturn = 1f;
    public bool isHit = false;
    public bool hasScored = false;
public bool scoredOnBoard = false;


    [SerializeField] GameObject followCam,trail;
    [SerializeField] CinemachineImpulseSource impulseSource;

    public bool Release { get => release; set { release = value; trail.SetActive(release); followCam.SetActive(release); } }
// public ScoreArea scoreArea;
  



    void Update()
    {
       // if (!Release)
       //     return;
        
      //  if(!isHit)
      //      transform.position += transform.forward * force * 500f * Time.deltaTime;
        
    //    timeToReturn -= Time.deltaTime;
     //   if(timeToReturn <= 0)
       //     Destroy(gameObject);
            

    }


    
    
}
