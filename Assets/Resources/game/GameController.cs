using UnityEngine;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;

public class GameController : MonoBehaviour, IPUCode {

	private static GameController singleton;

	public GameObject WeaponFlashParticles;

	protected PUImageButton endgameScreen;

	public PUGameObject EquipmentContainer;
	public PUGameObject ShipsContainer;
	public LDGGame game = null;

	public PULabel EquipmentLabel;
	public PUColor EquipmentLabelColor;
	public PUGameObject EquipmentLabelGO;

	public PUSprite redPlanet;
	public PUSprite bluePlanet;

	public PULabel redPlanetHealth;
	public PULabel bluePlanetHealth;

	public PULabel redPlanetBuildTime;
	public PULabel bluePlanetBuildTime;

	public PULabel redPlanetBuildQueue;
	public PULabel bluePlanetBuildQueue;

	public PULabel bluePlanetBuildQueueEstimatedTime;

	protected GameAI gameAI;

	protected LDGEquipment mousedEquipment = null;



	public static void PerformWeaponEffect(LDGEquipment e, LDGShip fromShip, LDGShip toShip){

		if (e.name.Contains ("Laser")) {
			SoundController controller = PUCode.GetSingletonByName ("SoundController") as SoundController;
			controller.Play (controller.laserFire);
		}

		ParticleSystem particleSystem = singleton.WeaponFlashParticles.GetComponent<ParticleSystem> ();
		particleSystem.transform.localPosition = toShip.sprite.gameObject.transform.localPosition;
		particleSystem.Emit (1);
	}

	public static void PerformWeaponEffect(LDGEquipment e, LDGShip fromShip, LDGPlanet toPlanet){
	
		SoundController controller = PUCode.GetSingletonByName ("SoundController") as SoundController;
		controller.Play (controller.bombHittingPlanet);

		ParticleSystem particleSystem = singleton.WeaponFlashParticles.GetComponent<ParticleSystem> ();
		particleSystem.transform.localPosition = toPlanet.sprite.gameObject.transform.localPosition;
		particleSystem.Emit (1);
	}


	public void Start() {

		singleton = this;

		WeaponFlashParticles.gameObject.layer = PlanetUnityOverride.puCameraLayer;

		if (game == null) {
			game = LDGGame.CreateGame ();
		}

		// load the game!
		// 1) Load all space equipment and put it in the scene...
		foreach (LDGEquipment equipment in game.Equipments) {
			equipment.GetSprite (EquipmentContainer);
		}

		game.bluePlanet ().sprite = bluePlanet;
		game.redPlanet ().sprite = redPlanet;


		// Boot up the AI
		gameAI = new GameAI ();
		gameAI.BeginAIForGame (game);
	}

	public void ReloadGame(){
		gameAI.Abort ();
		Application.LoadLevel ("game");
	}

	public void BuildShip() {
		if (game.BuildCurrentShipForPlanet (game.bluePlanet ())) {
			SoundController controller = PUCode.GetSingletonByName ("SoundController") as SoundController;
			controller.Play (controller.buildButton);
		}
	}


	public void AddEquipment(LDGEquipment e)
	{
		if (game.AddEquipmentToPlanetBuildQueue (e, game.bluePlanet ())) {
			SoundController controller = PUCode.GetSingletonByName ("SoundController") as SoundController;
			controller.Play (controller.addedEquipment);

			// Add a left hanging label to the equipment sprite
			PULabel equipmentLabel = new PULabel ("PlanetUnity/Label", null, 9, PlanetUnity.LabelAlignment.right, new cColor (172.0f/255.0f,172.0f/255.0f,172.0f/255.0f,1.0f), 
				e.name, null, null, new cRect (0, 0, 0, 0));
			equipmentLabel.renderQueueOffset = 1000;
			equipmentLabel.fontExists = false;
			equipmentLabel.shadowColorExists = false;
			equipmentLabel.shadowOffsetExists = false;
			equipmentLabel.loadIntoGameObject (e.sprite.gameObject);

			equipmentLabel.gameObject.transform.localPosition = new Vector3 (-0.3f, 0.45f, 0.0f);
			equipmentLabel.gameObject.transform.localScale = new Vector3 (1.0f / 32.0f, 1.0f / 32.0f, 1.0f);
		}
	}

	public void FixedUpdate() {

		game.AdvanceGame (ShipsContainer, EquipmentContainer);

		// 0) Find the closest piece of equipment to the mouse position
		// 1) if its under a certain delta scale up the equipment so its easier to see, and show the label for
		//    what it is.
		if (Input.GetMouseButton (0) == false) {
		
			if (mousedEquipment != null) {
				Vector3 pos = mousedEquipment.sprite.gameObject.transform.localPosition;

				if(pos.y > 180 && pos.x > 888){
					// We dropped an equipment; is it in our build queue list?
					AddEquipment (mousedEquipment);
				}
				else if(Input.GetMouseButtonUp(0) == true){
					AddEquipment (mousedEquipment);
				}

				mousedEquipment.beingDragged = false;
			}


			Vector3 mouse = PlanetUnityGameObject.MousePosition();
			float distance = 9999999.0f;
			LDGEquipment closestEquipment = null;

			// 0) Find the closest piece of equipment to the mouse position
			foreach (LDGEquipment equipment in game.Equipments) {
				float d = Vector3.Distance (mouse, equipment.sprite.gameObject.transform.localPosition);
				if (d < distance || closestEquipment == null) {
					distance = d;
					closestEquipment = equipment;
				}
			}

			mousedEquipment = null;
			if (distance <= 30) {
				mousedEquipment = closestEquipment;
			}
		}

		// Update all equipments
		foreach (LDGEquipment equipment in game.Equipments) {
			UpdateSpaceEquipment (equipment);
		}
		int n = 0;
		foreach (LDGEquipment equipment in game.redPlanet().Equipments) {
			UpdateQueueEquipment (equipment, game.redPlanet(), n);
			n++;
		}
		n = 0;
		foreach (LDGEquipment equipment in game.bluePlanet().Equipments) {
			UpdateQueueEquipment (equipment, game.bluePlanet(), n);
			n++;
		}

		// Handle doing stuff when equipment is moused over
		if (mousedEquipment == null && EquipmentLabelGO.gameObject.activeSelf == true) {
			EquipmentLabelGO.gameObject.SetActive (false);
		} else if(mousedEquipment != null) {
			EquipmentLabelGO.gameObject.SetActive (true);
			EquipmentLabel.LoadTextString (mousedEquipment.name);

			EquipmentLabelColor.bounds.w = EquipmentLabel.textWidth+20;
			EquipmentLabelColor.bounds.x = EquipmentLabelGO.bounds.w / 2 - EquipmentLabelColor.bounds.w / 2;
			EquipmentLabelColor.UpdateGeometry ();

			Vector3 pos = mousedEquipment.sprite.gameObject.transform.localPosition;
			pos.y += 18.0f;
			pos.x -= EquipmentLabelGO.bounds.w / 2;
			EquipmentLabelGO.gameObject.transform.localPosition = pos;

			pos = mousedEquipment.sprite.gameObject.transform.localPosition;
			if (pos.y > 180 && pos.x > 888) {
				mousedEquipment.sprite.spriteRenderer.color = new Color (0.5f, 0.5f, 1.0f, 1.0f);
			} else {
				mousedEquipment.sprite.spriteRenderer.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
			}
		}

		// Update the planet health displays
		redPlanetHealth.LoadTextString (""+game.redPlanet().health);
		bluePlanetHealth.LoadTextString (""+game.bluePlanet().health);

		// update buildtime displays
		redPlanetBuildTime.LoadTextString (game.redPlanet().buildTimeAsString());
		bluePlanetBuildTime.LoadTextString (game.bluePlanet().buildTimeAsString());

		// update buildqueue display
		redPlanetBuildQueue.LoadTextString (game.redPlanet().buildQueueAsString());
		bluePlanetBuildQueue.LoadTextString (game.bluePlanet().buildQueueAsString());

		if (bluePlanetBuildQueueEstimatedTime.LoadTextString (game.bluePlanet ().estimatedBuildTimeAsString ())) {
			bluePlanetBuildQueueEstimatedTime.gameObject.transform.localScale = new Vector3 (2, 2, 1);
			LeanTween.scale (bluePlanetBuildQueueEstimatedTime.gameObject, new Vector3 (1.0f, 1.0f, 1.0f), 0.5f);
		}

		if (endgameScreen == null) {
			if (game.bluePlanet ().health <= 0 || game.redPlanet ().health <= 0) {
				string imagePath = "game/victory";
				if (game.bluePlanet ().health <= 0) {
					imagePath = "game/defeat";
				}

				endgameScreen = new PUImageButton (imagePath, imagePath, new cColor (1, 1, 1, 1), 
					new cVector2 (960, 600), "ReloadGame", "Nothing", null, new cRect (0, 0, 960, 600));
				endgameScreen.loadIntoPUGameObject (ShipsContainer.scope () as PUGameObject);
			}
		}
	}

	public void UpdateSpaceEquipment(LDGEquipment e)
	{
		float currentScale = e.sprite.gameObject.transform.localScale.x;
		float targetScale = e.baseScale;
		Vector3 mousePos = PlanetUnityGameObject.MousePosition();

		mousePos.x = Mathf.Clamp (mousePos.x, 0, 960);
		mousePos.y = Mathf.Clamp (mousePos.y, 0, 600);

		if (e == mousedEquipment) {
			// 1) if its under a certain delta scale up the equipment so its easier to see, and show the label
			targetScale = e.baseScale * 3.0f;

			// User is dragging an equipment pod
			if (Input.GetMouseButton (0)) {
				mousedEquipment.beingDragged = true;
				e.sprite.gameObject.transform.localPosition = mousePos;
			}
		} else {
			Vector3 pos = e.sprite.gameObject.transform.localPosition;
			if (pos.y > 180 && pos.x > 888) {
				pos.x = 800;
				e.sprite.gameObject.transform.localPosition = pos;
			}
		}

		currentScale += (targetScale - currentScale) * 0.06125f;
		e.sprite.gameObject.transform.localScale = new Vector3 (currentScale, currentScale, currentScale);
	}

	public void UpdateQueueEquipment(LDGEquipment e, LDGPlanet p, int n)
	{
		float yDelta = 30;
		float currentScale = e.sprite.gameObject.transform.localScale.x;
		float targetScale = e.baseScale * 2.0f;

		currentScale += (targetScale - currentScale) * 0.06125f;
		e.sprite.gameObject.transform.localScale = new Vector3 (currentScale, currentScale, currentScale);

		Vector3 queuePosition = new Vector3(925,586-n*yDelta,0);
		if (p == game.redPlanet ()) {
			queuePosition = new Vector3(35,12+n*yDelta,0);
		}
		e.sprite.gameObject.transform.localPosition = MovePositionTowardsPosition (e.sprite.gameObject.transform.localPosition, queuePosition);
	}

	public Vector3 MovePositionTowardsPosition(Vector3 from, Vector3 to) {
		from.x += (to.x - from.x) * 0.06125f;
		from.y += (to.y - from.y) * 0.06125f;
		from.z += (to.z - from.z) * 0.06125f;
		return from;
	}
}

