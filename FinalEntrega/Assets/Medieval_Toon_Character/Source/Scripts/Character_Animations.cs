using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Animations : MonoBehaviour
{
	enum eLookingAt { LEFT, RIGHT, NONE }
	internal Animator m_Animator;
	public float m_fVertical;
	public float m_fHorizontal;
	public float m_fRun;

	bool m_bRunning = false;
	eLookingAt m_eLookingAt = eLookingAt.NONE;
	void Start( )
	{
		m_Animator = GetComponent<Animator>();
		EventManager.StartListening( eGyroEvents.UP_DOWN, Run );
		EventManager.StartListening( eGyroEvents.DOWN_UP, Run );

		EventManager.StartListening( eGyroEvents.LEFT_RIGHT, LookLeft );
		EventManager.StartListening( eGyroEvents.RIGHT_LEFT, LookRight );
	}
	
	void LookLeft()
	{
		m_fHorizontal = ( m_eLookingAt == eLookingAt.NONE || m_eLookingAt == eLookingAt.RIGHT ) ? -1f : 0;
		m_eLookingAt = ( m_eLookingAt == eLookingAt.NONE || m_eLookingAt == eLookingAt.RIGHT ) ? eLookingAt.LEFT : eLookingAt.NONE;
	}
	void LookRight()
	{
		m_fHorizontal = ( m_eLookingAt == eLookingAt.NONE || m_eLookingAt == eLookingAt.LEFT ) ? 1f : 0;
		m_eLookingAt = ( m_eLookingAt == eLookingAt.NONE || m_eLookingAt == eLookingAt.LEFT ) ? eLookingAt.RIGHT : eLookingAt.NONE;
	}

	void Run()
	{
		m_bRunning = !m_bRunning;
		if ( m_bRunning )
			m_fVertical = 1f;
		else
			m_fVertical = 0;
	}


	/*
	void Update( )
	{
		m_fHorizontal = Input.GetAxis( "Horizontal" );
		if ( m_Animator.GetFloat( "Run" ) == 0.2 )
		{
			if ( Input.GetKeyDown( "space" ) )
			{
				m_Animator.SetBool( "Jump", true );
			}
		}
		Sprinting();
	}
	*/

	void FixedUpdate( )
	{
		m_Animator.SetFloat( "Walk", m_fVertical );
		//m_Animator.SetFloat( "Run", m_fRun );
		m_Animator.SetFloat( "Turn", m_fHorizontal );
	}

	void Sprinting( )
	{
		if ( Input.GetKey( KeyCode.LeftShift ) )
		{
			m_fRun = 0.2f;
		}
		else
		{
			m_fRun = 0.0f;
		}
	}
}
