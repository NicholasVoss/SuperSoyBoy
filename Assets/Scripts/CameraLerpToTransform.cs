using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLerpToTransform : MonoBehaviour {

	//vars for moving camera
	public Transform camTarget;
	public float trackingSpeed;
	public float minX;
	public float minY;
	public float maxX;
	public float maxY;
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		//check if camera is targeting a valid object
		if(camTarget != null)
        {
			//smoothly move camera
			var newPos = Vector2.Lerp(transform.position,
									camTarget.position,
									Time.deltaTime * trackingSpeed);
			var camPosition = new Vector3(newPos.x, newPos.y, -10f);
			var v3 = camPosition;
			var clampX = Mathf.Clamp(v3.x, minX, maxX);
			var clampY = Mathf.Clamp(v3.y, minY, maxY);
			transform.position = new Vector3(clampX, clampY, -10f);
        }
	}
}
