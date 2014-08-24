
using UnityEngine;

public class NavigationController : MonoBehaviour, IPUCode {


	public void GoToGame(){
		Application.LoadLevel("game");
	}

}
