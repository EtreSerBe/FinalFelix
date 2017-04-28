using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWASDController : MonoBehaviour {

    public bool m_bDebug = true;
	

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        
        if (! (Application.isEditor) )
        {
            Debug.LogWarning("You are debugging with WASD outside the editor, that was for testing purposes only. Please deactivate.");
            return;
        }
        if (m_bDebug == false)
        {
            return; // better to deactivate the script or remove it.
        }

        //Else, we do whatever we want.
        if (Input.GetKey(KeyCode.W))
        {
            EventManager.TriggerEvent(eGyroEvents.UP_DOWN);
        }
        if (Input.GetKey(KeyCode.A))
        {
            EventManager.TriggerEvent(eGyroEvents.DOWN_UP);
        }
        if (Input.GetKey(KeyCode.S))
        {
            EventManager.TriggerEvent(eGyroEvents.LEFT);
        }
        if (Input.GetKey(KeyCode.W))
        {
            EventManager.TriggerEvent(eGyroEvents.RIGHT);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            EventManager.TriggerEvent(eGyroEvents.LEFT_RIGHT);
        }
        if (Input.GetKey(KeyCode.E))
        {
            EventManager.TriggerEvent(eGyroEvents.RIGHT_LEFT);
        }



    }
}
