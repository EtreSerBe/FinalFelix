using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeFaces : MonoBehaviour
{
	PlayFaceAnimations m_FaceAnimations;
	int m_iTotalEyeExpressions=7;
	int m_iTotalMouthExpressions=7;

	// Use this for initialization
	void Start( )
	{
		m_FaceAnimations = gameObject.GetComponent<PlayFaceAnimations>();
	}

	void OnGUI( )
	{
		if ( GUI.Button( new Rect( 200, 65, 80, 30 ), "Eyes" ) )
		{
			if ( (int)m_FaceAnimations.m_eEye < m_iTotalEyeExpressions ) m_FaceAnimations.m_eEye += 1;
			else m_FaceAnimations.m_eEye = 0;
		}
		if ( GUI.Button( new Rect( 200, 100, 80, 30 ), "Mouth" ) )
		{
			if ( (int)m_FaceAnimations.m_eMouth < m_iTotalMouthExpressions ) m_FaceAnimations.m_eMouth += 1;
			else m_FaceAnimations.m_eMouth = 0;
		}
	}
}
