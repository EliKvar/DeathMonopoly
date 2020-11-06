using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Springboard : MonoBehaviour
{
    //Veloctiy to spring player
    public float sbVelocityUp = 5f;
    //Velocity to retract spring
    public float sbVelocityDown = 1.25f;
    //Upper bound for springboard to retract to
    public Vector3 sbUBound = new Vector3(0, 0.5f, 0);
    public Vector3 sbLBound = new Vector3(0, 0, 0);
    
    public void OnTriggerEnter(Collider other)
    {
        StartCoroutine(SpringUp());
    }

    private IEnumerator SpringUp()
    {
        yield return new WaitForSeconds(2.5f);
        Debug.Log("SPRANG!");
        do
        {
            this.gameObject.GetComponent<Rigidbody>().AddForce(transform.up * (sbVelocityUp * Time.deltaTime));
            this.gameObject.transform.Translate(Vector3.up * (sbVelocityUp * Time.deltaTime), Space.Self);
        } while (this.gameObject.transform.position.y <= sbUBound.y);

    }

    public void OnTriggerExit(Collider other)
    {
        StartCoroutine(SpringDownAndReset());
    }

    private IEnumerator SpringDownAndReset()
    {
        yield return new WaitForSeconds(3.5f);
        Debug.Log("Resetting springboard");
        do
        {
            this.gameObject.transform.Translate(Vector3.down * (sbVelocityUp * Time.deltaTime), Space.Self);
        } while (this.gameObject.transform.position.y >= sbLBound.y);
    }
}
