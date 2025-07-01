using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableEnable : MonoBehaviour
{
    public GameObject object1;
    public GameObject object2;
    public GameObject object3;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            object1.SetActive(true);
            object2.SetActive(false);
            object3.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            object1.SetActive(false);
            object2.SetActive(true);
            object3.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            object1.SetActive(false);
            object2.SetActive(false);
            object3.SetActive(true);
        }
    }
}
