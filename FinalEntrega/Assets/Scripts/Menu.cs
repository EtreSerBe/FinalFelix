using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{

	Color m_selectedColor = new Color( 29/255f, 249/255f, 67/255f, 234/255f );
	public GameObject[] m_Buttons;
	int m_iActualButton = 0;


	void Start( )
	{
		m_Buttons[m_iActualButton].GetComponent<Image>().color = m_selectedColor;
		EventManager.StartListening( eGyroEvents.UP, gyroUP );
		EventManager.StartListening( eGyroEvents.UP_DOWN, gyroUP );

		EventManager.StartListening( eGyroEvents.DOWN, gyroDown );
		EventManager.StartListening( eGyroEvents.DOWN_UP, gyroDown );

		EventManager.StartListening( eGyroEvents.RIGHT_LEFT, select );
		EventManager.StartListening( eGyroEvents.LEFT_RIGHT, select );
	}

	void OnDisable( )
	{
		EventManager.StopListening( eGyroEvents.UP, gyroUP );
		EventManager.StopListening( eGyroEvents.UP_DOWN, gyroUP );

		EventManager.StopListening( eGyroEvents.DOWN, gyroDown );
		EventManager.StopListening( eGyroEvents.DOWN_UP, gyroDown );

		EventManager.StopListening( eGyroEvents.RIGHT_LEFT, select );
		EventManager.StopListening( eGyroEvents.LEFT_RIGHT, select );
	}

	void gyroUP( )
	{
		m_Buttons[m_iActualButton].GetComponent<Image>().color = Color.white;
		m_iActualButton = m_iActualButton - 1 >= 0 ? m_iActualButton - 1 : m_Buttons.Length - 1;
		m_Buttons[m_iActualButton].GetComponent<Image>().color = m_selectedColor;
	}

	void gyroDown( )
	{
		m_Buttons[m_iActualButton].GetComponent<Image>().color = Color.white;
		m_iActualButton = ( m_iActualButton + 1 ) % m_Buttons.Length;
		m_Buttons[m_iActualButton].GetComponent<Image>().color = m_selectedColor;
	}

	void select( )
	{
		if ( m_Buttons[m_iActualButton].tag == "New Game" )
		{
			Application.LoadLevel( "New Game" );
		}
		else if ( m_Buttons[m_iActualButton].tag == "Options" )
		{
			Application.LoadLevel( "Options" );
		}
		else if ( m_Buttons[m_iActualButton].tag == "Exit" )
		{
			Application.Quit();
		}
	}

	/**********************************************************
	 *						Temporal						  *
	 *********************************************************/
	public void NuevoJuego( )
	{
		Application.LoadLevel( "NuevoJuego" );
	}

	public void Opciones( )
	{
		Application.LoadLevel( "Opciones" );
	}

	public void MenuPrincipal( )
	{
		Application.LoadLevel( "Menu" );
	}

	public void Salir( )
	{
		Application.Quit();
	}
}
