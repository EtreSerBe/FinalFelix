using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class EventTest : MonoBehaviour
{
	void OnEnable( )
	{
		EventManager.StartListening( eGyroEvents.UP, gyroUP );
		EventManager.StartListening( eGyroEvents.DOWN, gyroDown );
		EventManager.StartListening( eGyroEvents.RIGHT, gyroRight );
		EventManager.StartListening( eGyroEvents.LEFT, gyroLeft );

		EventManager.StartListening( eGyroEvents.UP_DOWN, gyroUPDown );
		EventManager.StartListening( eGyroEvents.DOWN_UP, gyroDownUP );
		EventManager.StartListening( eGyroEvents.RIGHT_LEFT, gyroRightLeft );
		EventManager.StartListening( eGyroEvents.LEFT_RIGHT, gyroLeftRight );
	}

	void OnDisable( )
	{
		//EventManager.StopListening( "test", someListener );
		//EventManager.StopListening( "Spawn", SomeOtherFunction );
		//EventManager.StopListening( "Destroy", SomeThirdFunction );
	}

	void gyroUP( )
	{
		Debug.Log( "UP" );
	}

	void gyroDown( )
	{
		Debug.Log( "DOWN" );
	}

	void gyroRight( )
	{
		Debug.Log( "RIGHT" );
	}

	void gyroLeft( )
	{
		Debug.Log( "LEFT" );
	}

	void gyroUPDown( )
	{
		Debug.Log( "UP_DOWN" );
	}

	void gyroDownUP( )
	{
		Debug.Log( "DOWN_UP" );
	}

	void gyroRightLeft( )
	{
		Debug.Log( "RIGHT_LEFT" );
	}

	void gyroLeftRight( )
	{
		Debug.Log( "LEFT_RIGHT" );
	}
}