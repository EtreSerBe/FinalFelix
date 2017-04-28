using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

	// Use this for initialization
	AudioSource m_cachedAudio;
	void Start( )
	{
		
		foreach ( string device in Microphone.devices )
		{
			Debug.Log( "Name: " + device );
			m_cachedAudio = GetComponent<AudioSource>();
			m_cachedAudio.clip = Microphone.Start( device, true, 10, 10000 );
			m_cachedAudio.loop = true;
			m_cachedAudio.Play();
		}
	}

	// Update is called once per frame
	void Update( )
	{
		/*
		if( Microphone.IsRecording( m_cachedAudio.name ) )
		{
			Debug.Log( m_cachedAudio.timeSamples );
		}
		*/
	}
}
