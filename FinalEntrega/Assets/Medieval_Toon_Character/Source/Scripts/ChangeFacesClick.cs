using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeFacesClick : MonoBehaviour
{
	PlayFaceAnimations m_FaceAnimations;
	int m_iTotalEyeExpressions=7;
	int m_iTotalMouthExpressions=7;

	void Start ()
	{
		m_FaceAnimations = gameObject.GetComponent<PlayFaceAnimations>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( Input.GetMouseButtonDown( 0 ) )
		{
			if ( (int)m_FaceAnimations.m_eEye < m_iTotalEyeExpressions ) m_FaceAnimations.m_eEye += 1;
			else m_FaceAnimations.m_eEye = 0;
		}
		if ( Input.GetMouseButtonDown( 1 ) )
		{
			if ( (int)m_FaceAnimations.m_eMouth < m_iTotalMouthExpressions ) m_FaceAnimations.m_eMouth += 1;
			else m_FaceAnimations.m_eMouth = 0;
		}
	}
}
