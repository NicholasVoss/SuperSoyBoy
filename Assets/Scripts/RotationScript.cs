using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationScript : MonoBehaviour
{
    public float rotationsPerMinute = 640f;
	
	// Update is called once per frame
	void Update ()
    {
        transform.Rotate(0, 0, rotationsPerMinute * Time.deltaTime, Space.Self);
    }
}
