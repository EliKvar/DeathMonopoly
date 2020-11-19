using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringBoard : MonoBehaviour
{
    private float timer = .5f;
    private bool timerBool = false;

    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            this.GetComponentInChildren<Renderer>().enabled = true;
            foreach(Transform child in other.gameObject.transform)
            {
                if(child.name == "redChecker (1)")
                {
                    Debug.Log("SPRING");

                    GameObject obj = child.gameObject;
                    obj.GetComponent<Renderer>().enabled = true;
                }
                foreach (Transform chil in child.gameObject.transform)
                {
                    if (chil.name == "redChecker (1)")
                    {
                        Debug.Log("SPRING");

                        GameObject obj = chil.gameObject;
                        obj.GetComponent<Renderer>().enabled = true;
                    }
                }
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
           
            

        }
    }


    // Update is called once per frame
    void Update()
    {
     
    }
}
