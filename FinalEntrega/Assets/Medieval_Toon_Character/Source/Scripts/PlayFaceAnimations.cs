
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Eyes_Expressions { Happy = 0, Mad = 1, Sad = 2, Tired = 3, Closed = 4, Closed_happy = 5, Closed_smile = 6, Closed_mad = 7 };
public enum Mouth_Expressions { Happy_open = 0, Terrified_open = 1, Surprised_open = 2, Surprised2_open = 3, Unconcerned_closed = 4, Sad_closed = 5, Happy_closed = 6, Cute = 7 };

[ExecuteInEditMode]
public class PlayFaceAnimations : MonoBehaviour
{
	public Eyes_Expressions m_eEye;
	public Mouth_Expressions m_eMouth;
	public FaceAnimations m_FaceAnims;

	void Start( )
	{
		m_FaceAnims = gameObject.GetComponent<FaceAnimations>();
	}

	// Update is called once per frame
	void Update( )
	{
		if ( m_FaceAnims.m_FaceExpressions.Length > 0 )
		{
			if ( m_eEye == Eyes_Expressions.Happy )
			{
				m_FaceAnims.m_FaceExpressions[0].m_RightEye.SetActive( true );
				m_FaceAnims.m_FaceExpressions[0].m_LeftEye.SetActive( true );
			}
			else
			{
				m_FaceAnims.m_FaceExpressions[0].m_RightEye.SetActive( false );
				m_FaceAnims.m_FaceExpressions[0].m_LeftEye.SetActive( false );
			}
			if ( m_eEye == Eyes_Expressions.Mad )
			{
				m_FaceAnims.m_FaceExpressions[1].m_RightEye.SetActive( true );
				m_FaceAnims.m_FaceExpressions[1].m_LeftEye.SetActive( true );
			}
			else
			{
				m_FaceAnims.m_FaceExpressions[1].m_RightEye.SetActive( false );
				m_FaceAnims.m_FaceExpressions[1].m_LeftEye.SetActive( false );
			}
			if ( m_eEye == Eyes_Expressions.Sad )
			{
				m_FaceAnims.m_FaceExpressions[2].m_RightEye.SetActive( true );
				m_FaceAnims.m_FaceExpressions[2].m_LeftEye.SetActive( true );
			}
			else
			{
				m_FaceAnims.m_FaceExpressions[2].m_RightEye.SetActive( false );
				m_FaceAnims.m_FaceExpressions[2].m_LeftEye.SetActive( false );
			}
			if ( m_eEye == Eyes_Expressions.Tired )
			{
				m_FaceAnims.m_FaceExpressions[3].m_RightEye.SetActive( true );
				m_FaceAnims.m_FaceExpressions[3].m_LeftEye.SetActive( true );
			}
			else
			{
				m_FaceAnims.m_FaceExpressions[3].m_RightEye.SetActive( false );
				m_FaceAnims.m_FaceExpressions[3].m_LeftEye.SetActive( false );
			}
			if ( m_eEye == Eyes_Expressions.Closed )
			{
				m_FaceAnims.m_FaceExpressions[4].m_RightEye.SetActive( true );
				m_FaceAnims.m_FaceExpressions[4].m_LeftEye.SetActive( true );
			}
			else
			{
				m_FaceAnims.m_FaceExpressions[4].m_RightEye.SetActive( false );
				m_FaceAnims.m_FaceExpressions[4].m_LeftEye.SetActive( false );
			}
			if ( m_eEye == Eyes_Expressions.Closed_happy )
			{
				m_FaceAnims.m_FaceExpressions[5].m_RightEye.SetActive( true );
				m_FaceAnims.m_FaceExpressions[5].m_LeftEye.SetActive( true );
			}
			else
			{
				m_FaceAnims.m_FaceExpressions[5].m_RightEye.SetActive( false );
				m_FaceAnims.m_FaceExpressions[5].m_LeftEye.SetActive( false );
			}
			if ( m_eEye == Eyes_Expressions.Closed_smile )
			{
				m_FaceAnims.m_FaceExpressions[6].m_RightEye.SetActive( true );
				m_FaceAnims.m_FaceExpressions[6].m_LeftEye.SetActive( true );
			}
			else
			{
				m_FaceAnims.m_FaceExpressions[6].m_RightEye.SetActive( false );
				m_FaceAnims.m_FaceExpressions[6].m_LeftEye.SetActive( false );
			}
			if ( m_eEye == Eyes_Expressions.Closed_mad )
			{
				m_FaceAnims.m_FaceExpressions[7].m_RightEye.SetActive( true );
				m_FaceAnims.m_FaceExpressions[7].m_LeftEye.SetActive( true );
			}
			else
			{
				m_FaceAnims.m_FaceExpressions[7].m_RightEye.SetActive( false );
				m_FaceAnims.m_FaceExpressions[7].m_LeftEye.SetActive( false );
			}
		}
		if ( m_FaceAnims.m_Mouths.Length > 0 )
		{
			if ( m_eMouth == Mouth_Expressions.Happy_open ) m_FaceAnims.m_Mouths[0].SetActive( true );
			else m_FaceAnims.m_Mouths[0].SetActive( false );
			if ( m_eMouth == Mouth_Expressions.Terrified_open ) m_FaceAnims.m_Mouths[1].SetActive( true );
			else m_FaceAnims.m_Mouths[1].SetActive( false );
			if ( m_eMouth == Mouth_Expressions.Surprised_open ) m_FaceAnims.m_Mouths[2].SetActive( true );
			else m_FaceAnims.m_Mouths[2].SetActive( false );
			if ( m_eMouth == Mouth_Expressions.Surprised2_open ) m_FaceAnims.m_Mouths[3].SetActive( true );
			else m_FaceAnims.m_Mouths[3].SetActive( false );
			if ( m_eMouth == Mouth_Expressions.Unconcerned_closed ) m_FaceAnims.m_Mouths[4].SetActive( true );
			else m_FaceAnims.m_Mouths[4].SetActive( false );
			if ( m_eMouth == Mouth_Expressions.Sad_closed ) m_FaceAnims.m_Mouths[5].SetActive( true );
			else m_FaceAnims.m_Mouths[5].SetActive( false );
			if ( m_eMouth == Mouth_Expressions.Happy_closed ) m_FaceAnims.m_Mouths[6].SetActive( true );
			else m_FaceAnims.m_Mouths[6].SetActive( false );
			if ( m_eMouth == Mouth_Expressions.Cute ) m_FaceAnims.m_Mouths[7].SetActive( true );
			else m_FaceAnims.m_Mouths[7].SetActive( false );

		}
	}
}
