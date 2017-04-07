using UnityEngine;
using System.Collections;


public class GyroEventTrigger : MonoBehaviour
{
	/**
	* SLEEP: When the viewport is near the start.
	* MOVING: When the player is moving.
	* TIME_OUT: When the player didn't finish the move to walk.
	* TIME_COUNT: When the time starts to detect if the player wants to move.
	*/
	private enum eGesturePhase { SLEEP, TIME_OUT, TIME_COUNT };
	/*< Variable of the cached gyroscope. */
	private Gyroscope       m_CachedGyroscope;
	/*< States to administrate gyroscope inputs. */
	private eGesturePhase   m_GPState = eGesturePhase.SLEEP;
	/*< Started looking at position. */
	private Vector3         m_vStartLooking;
	/*< Vector to know in which position was the trying to move. */
	private Vector2         m_vMovedPos;
	/*< Timer to know if trying to do a combination. */
	private float           m_fTimeCount;

	/*< Limits of the axis. */
	[Range(0,1)]
	public float    m_fThresholds = 0.2f;
	/*< Max time that player has to do a combination input. */
	[Range(0,5)]
	public float    m_fMaxDetectTimeForReset = 1f;
	public float    m_fMaxInactiveTime = 1f;

	float m_fLastStateUpdate = 0;
	void Start( )
	{
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		m_CachedGyroscope = Input.gyro;
		m_CachedGyroscope.enabled = true;
		m_vStartLooking = GyroToUnityVec();
		m_fTimeCount = 0;
	}

	void Update( )
	{
		if( m_fLastStateUpdate > m_fMaxInactiveTime )
		{
			Debug.Log("Changed reference point.");
			m_vStartLooking = GyroToUnityVec();
			m_fLastStateUpdate = 0;
		}

		if ( m_GPState == eGesturePhase.SLEEP || m_GPState == eGesturePhase.TIME_OUT )
			m_fLastStateUpdate += Time.deltaTime;
		else
			m_fLastStateUpdate = 0;

		//If is outside of the threshold.
		if ( m_GPState == eGesturePhase.TIME_COUNT )
			m_fTimeCount += Time.deltaTime;

		Vector3 m_vActualGyroPos = GyroToUnityVec( );
		if ( m_GPState == eGesturePhase.SLEEP )
		{
			if ( !IsGyroInsideThreshold( true, false ) )
			{
				m_fTimeCount = 0;
				m_GPState = eGesturePhase.TIME_COUNT;
				m_vMovedPos.x = ( m_vStartLooking.x - m_vActualGyroPos.x > m_fThresholds ) ? 1 : -1;
			}
			if ( !IsGyroInsideThreshold( false, true ) )
			{
				m_fTimeCount = 0;
				m_GPState = eGesturePhase.TIME_COUNT;
				m_vMovedPos.y = ( m_vStartLooking.y - m_vActualGyroPos.y > m_fThresholds ) ? 1 : -1;
			}
		}
		// Detect if it's near the start looking
		if ( m_GPState == eGesturePhase.TIME_OUT && IsGyroInsideThreshold( true, true ) )
		{
			m_GPState = eGesturePhase.SLEEP;
			EventManager.TriggerEvent( eGyroEvents.THRESHOLD );
		}

		// If the player was not trying to move/stop.
		if ( m_GPState == eGesturePhase.TIME_COUNT )
		{
			if ( m_fTimeCount > m_fMaxDetectTimeForReset )
			{
				m_GPState = eGesturePhase.TIME_OUT;

				if ( m_vMovedPos.x != 0 )
					EventManager.TriggerEvent( m_vMovedPos.x > 0 ? eGyroEvents.LEFT : eGyroEvents.RIGHT );
				if ( m_vMovedPos.y != 0 )
					EventManager.TriggerEvent( m_vMovedPos.y > 0 ? eGyroEvents.UP : eGyroEvents.DOWN );
				m_vMovedPos.x = 0;
				m_vMovedPos.y = 0;
			}
			else if( IsGyroInsideThreshold( true, true ) )
			{
				if ( m_vMovedPos.x != 0 )
					EventManager.TriggerEvent( m_vMovedPos.x > 0 ? eGyroEvents.LEFT_RIGHT : eGyroEvents.RIGHT_LEFT );
				if ( m_vMovedPos.y != 0 )
					EventManager.TriggerEvent( m_vMovedPos.y > 0 ? eGyroEvents.UP_DOWN : eGyroEvents.DOWN_UP );
				m_vMovedPos.x = 0;
				m_vMovedPos.y = 0;
				m_GPState = eGesturePhase.SLEEP;
			}
		}
	}

	/*
	 * @Param in_x flag to know if is inside the threshold in axis X.
	 * @Param in_y flag to know if is inside the threshold in axis Y.
	 * @Return true if was inside the specified axis, otherwise false.
	 */
	bool IsGyroInsideThreshold( bool in_x, bool in_y )
	{
		Vector3 m_vActualGyroPos = GyroToUnityVec( );

		if ( in_x && in_y )
			return Mathf.Abs( m_vStartLooking.x - m_vActualGyroPos.x ) < m_fThresholds && Mathf.Abs( m_vStartLooking.y - m_vActualGyroPos.y ) < m_fThresholds;
		else if ( in_x )
			return Mathf.Abs( m_vStartLooking.x - m_vActualGyroPos.x ) < m_fThresholds;
		else if ( in_y )
			return Mathf.Abs( m_vStartLooking.y - m_vActualGyroPos.y ) < m_fThresholds;

		return false;
	}

	/* 
	 * @Return Vector3 with the change of hand form gyroscope to the hand that uses unity.
	 */
	Vector3 GyroToUnityVec( )
	{
		//Vector3 q = m_CachedGyroscope.userAcceleration;
		Quaternion q = m_CachedGyroscope.attitude;
		return new Vector3( q.y, q.x, -q.z );//This depends on the orientation. (landscape)
	}

	/* 
	 * @Return Quaternion with the change of hand form gyroscope to the hand that uses unity.
	 */
	Quaternion GyroToUnity( )
	{
		Quaternion q = m_CachedGyroscope.attitude;
		return new Quaternion( q.y, q.x, -q.z, -q.w );//This depends on the orientation. (landscape)
	}
	
}