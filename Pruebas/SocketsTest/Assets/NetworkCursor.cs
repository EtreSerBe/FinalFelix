using UnityEngine;
using System.Collections;

public class NetworkCursor : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        while (true)
        {
            MessageData msg = Server.PopMessage();
            if (msg == null)
                break;

            transform.position = new Vector3(msg.mousex, msg.mousey, transform.position.z);
        }

	}
}
