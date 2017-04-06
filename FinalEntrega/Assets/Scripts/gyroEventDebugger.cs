using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class gyroEventDebugger : MonoBehaviour
{
	public bool m_bDebug;

	void OnEnable( )
	{
		if ( m_bDebug )
		{
			EventManager.StartListening( eGyroEvents.UP, gyroUP );
			EventManager.StartListening( eGyroEvents.DOWN, gyroDown );
			EventManager.StartListening( eGyroEvents.RIGHT, gyroRight );
			EventManager.StartListening( eGyroEvents.LEFT, gyroLeft );

			EventManager.StartListening( eGyroEvents.UP_DOWN, gyroUPDown );
			EventManager.StartListening( eGyroEvents.DOWN_UP, gyroDownUP );
			EventManager.StartListening( eGyroEvents.RIGHT_LEFT, gyroRightLeft );
			EventManager.StartListening( eGyroEvents.LEFT_RIGHT, gyroLeftRight );

			EventManager.StartListening( eGyroEvents.THRESHOLD, gyroInThreshold );
		}
	}

	void OnDisable( )
	{
		if ( m_bDebug )
		{
			EventManager.StopListening( eGyroEvents.UP, gyroUP );
			EventManager.StopListening( eGyroEvents.DOWN, gyroDown );
			EventManager.StopListening( eGyroEvents.RIGHT, gyroRight );
			EventManager.StopListening( eGyroEvents.LEFT, gyroLeft );

			EventManager.StopListening( eGyroEvents.UP_DOWN, gyroUPDown );
			EventManager.StopListening( eGyroEvents.DOWN_UP, gyroDownUP );
			EventManager.StopListening( eGyroEvents.RIGHT_LEFT, gyroRightLeft );
			EventManager.StopListening( eGyroEvents.LEFT_RIGHT, gyroLeftRight );

			EventManager.StopListening( eGyroEvents.THRESHOLD, gyroInThreshold );
		}
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

	void gyroInThreshold()
	{
		Debug.Log( "THRESHOLD" );
	}
}