using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * SLEEP: When the viewport is near the start.
 * MOVING: When the player is moving.
 * TIME_OUT: When the player didn't finish the move to walk.
 * TIME_COUNT: When the time starts to detect if the player wants to move.
 */
public enum eGesturePhase { SLEEP, TIME_OUT, TIME_COUNT }

public class CMovement : MonoBehaviour
{
	/*< Variable to enable or disable debug.log messaged. */
	public bool m_bDebug = false;

	[Range(0,0.1f)]
	public float m_fAphaFB = 0.02f;
	[Range(0,0.1f)]
	public float m_fAphaRL = 0.01f;
	public float m_fDetectMoveOffsetY = 40f;
	public float m_fOffsetCenter      = 10f;
	public float m_fMaxDetectTimeMove = 2f;

	float m_fTime;
	eGesturePhase m_GPState = eGesturePhase.SLEEP;
	bool m_bUpFirst     = false;
	bool m_bMoving      = false;

	Transform m_CachedTransform;
	Vector3 m_vStartLooking;

	void Start( )
	{
		m_CachedTransform	= transform.FindChild( "Main Camera" );
		m_vStartLooking		= m_CachedTransform.eulerAngles;
	}

	void Update( )
	{
		// If the player is out of the offset the timer start to count
		if ( m_GPState == eGesturePhase.TIME_COUNT )
			m_fTime += Time.deltaTime;

		/* ======================== Begin Movement ======================== */

		// Detect up/down to start timer to start movement
		if ( (m_GPState == eGesturePhase.SLEEP || m_bMoving ) && Mathf.Abs(m_vStartLooking.x - m_CachedTransform.eulerAngles.x) > m_fDetectMoveOffsetY )
		{
			if ( m_bDebug )
				Debug.Log("TIME_COUNT started");
			m_fTime = 0;
			m_GPState = eGesturePhase.TIME_COUNT;
			m_bUpFirst = ( m_vStartLooking.x - m_CachedTransform.eulerAngles.x > 0 ) ? true : false;
		}

		// Detect if it's near the start looking
		if( m_GPState == eGesturePhase.TIME_OUT && Mathf.Abs( m_vStartLooking.x - m_CachedTransform.eulerAngles.x ) < m_fOffsetCenter )
		{
			if ( m_bDebug )
				Debug.Log( "Current state: SLEEP" );
			m_GPState = eGesturePhase.SLEEP;
		}

		// If the player was not trying to move/stop.
		if ( m_GPState == eGesturePhase.TIME_COUNT && m_fTime > m_fMaxDetectTimeMove )
			m_GPState = eGesturePhase.TIME_OUT;

		// The player wants to walk/stop.
		if ( m_GPState == eGesturePhase.TIME_COUNT && m_vStartLooking.x - m_CachedTransform.eulerAngles.x > ( m_bUpFirst ? m_fDetectMoveOffsetY : -m_fDetectMoveOffsetY ) )
		{
			m_bMoving = !m_bMoving;
			m_GPState = eGesturePhase.SLEEP;
		}

		/* ======================== Movement ======================== */
		if ( Input.GetKey( KeyCode.W ) || m_bMoving )
		{
			Vector3 tmpVecFB = m_fAphaFB * new Vector3( m_CachedTransform.forward.x, 0, m_CachedTransform.forward.z );
			transform.Translate( tmpVecFB );
		}
		else if ( Input.GetKey( KeyCode.S ) )
		{
			Vector3 tmpVecFB = m_fAphaFB * new Vector3( m_CachedTransform.forward.x, 0, m_CachedTransform.forward.z );
			transform.Translate( tmpVecFB * -1 );
		}

		if ( Input.GetKey( KeyCode.A ) )
		{
			Vector3 tmpVecRL = m_fAphaRL * new Vector3( m_CachedTransform.right.x, 0, m_CachedTransform.right.z );
			transform.Translate( tmpVecRL * -1 );
		}
		else if ( Input.GetKey( KeyCode.D ) )
		{
			Vector3 tmpVecRL = m_fAphaRL * new Vector3( m_CachedTransform.right.x, 0, m_CachedTransform.right.z );
			transform.Translate( tmpVecRL );
		}
	}

}
