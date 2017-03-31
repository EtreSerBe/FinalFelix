using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void NuevoJuego()
	{
		Application.LoadLevel ("NuevoJuego");
	}

	public void Opciones()
	{
		Application.LoadLevel ("Opciones");
	}

	public void MenuPrincipal()
	{
		Application.LoadLevel ("Menu");
	}

	public void Salir()
	{
		Application.Quit ();
	}
}
