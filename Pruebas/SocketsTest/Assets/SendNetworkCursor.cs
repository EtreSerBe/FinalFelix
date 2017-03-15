using UnityEngine;
using System.Collections;

public class SendNetworkCursor : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
        // Create the message it and send it off!
        MessageData msgData = new MessageData();
        msgData.mousex = Input.mousePosition.x / Screen.width;
        msgData.mousey = Input.mousePosition.y / Screen.height;
        msgData.stringData = "Hello World";

        Client.Send(msgData);

	}
}
