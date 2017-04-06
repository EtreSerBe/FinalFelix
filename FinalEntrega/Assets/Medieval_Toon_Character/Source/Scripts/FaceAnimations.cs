
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class eyeExpressions
{
	public GameObject m_RightEye;
	public GameObject m_LeftEye;
}

public class FaceAnimations : MonoBehaviour
{
	private int m_CurrentEye;
	private int m_OldEye;
	private int m_CurrentMouth;
	private int m_OldMouth;

	public eyeExpressions[] m_FaceExpressions;
	public GameObject[] m_Mouths;

	void Start ()
	{
		m_CurrentEye = 0;
		m_OldEye = 0;
		m_CurrentMouth = 0;
		m_OldMouth = 0;
		if ( m_FaceExpressions.Length > 0 )
		{
			m_FaceExpressions[0].m_RightEye.SetActive( true );
			m_FaceExpressions[0].m_LeftEye.SetActive( true );
		}
		if ( m_Mouths.Length > 0 )
		{
			m_Mouths[0].SetActive( true );
		}
	}
}
