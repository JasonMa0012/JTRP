using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_moveThis : MonoBehaviour
{
    private float startX;
    private float startY;
    private float startZ;
    private bool firstEnable = true;
	
	public float translationSpeedX=0f;
	public float translationSpeedY=1f;
	public float translationSpeedZ=0f;
	
	[SerializeField]
	public bool local=true;

    private void OnEnable()
    {
        if (firstEnable)
        {
            startX = transform.localPosition.x;
            startY = transform.localPosition.y;
            startZ = transform.localPosition.z;
            firstEnable = false;
        }
        else
            transform.localPosition = new Vector3(startX, startY, startZ);
    }

    // Update is called once per frame
    void Update()
    {
		if (local==true)
			transform.Translate(new Vector3(translationSpeedX,translationSpeedY,translationSpeedZ)*Time.deltaTime);
		else
			transform.Translate(new Vector3(translationSpeedX,translationSpeedY,translationSpeedZ)*Time.deltaTime, Space.World);
    }
}