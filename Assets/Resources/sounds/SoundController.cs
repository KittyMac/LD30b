
using UnityEngine;

public class SoundController : MonoBehaviour, IPUCode {

	public AudioSource musicPlayer;

	public AudioClip backgroundMusic;
	
	public AudioClip addedEquipment;
	public AudioClip bombHittingPlanet;
	public AudioClip buildButton;
	public AudioClip laserFire;
	public AudioClip weaponsFire;

	void Start () {

		backgroundMusic = (AudioClip)Resources.Load ("sounds/music");

		addedEquipment = (AudioClip)Resources.Load ("sounds/addedEquipment");
		bombHittingPlanet = (AudioClip)Resources.Load ("sounds/bombHittingPlanet");
		buildButton = (AudioClip)Resources.Load ("sounds/buildButton");
		laserFire = (AudioClip)Resources.Load ("sounds/laserFire");
		weaponsFire = (AudioClip)Resources.Load ("sounds/weaponsFire");

		musicPlayer = (AudioSource)gameObject.AddComponent ("AudioSource");
		musicPlayer.Stop ();
		musicPlayer.clip = backgroundMusic;
		musicPlayer.loop = true;
		musicPlayer.Play ();

		musicPlayer.volume = 0.5f;
	}

	public void Play(AudioClip clip){
		musicPlayer.PlayOneShot(clip, 0.25f);
	}

}
