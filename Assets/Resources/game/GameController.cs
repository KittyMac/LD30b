using UnityEngine;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;

public class GameController : MonoBehaviour, IPUCode {

	public PUGameObject EquipmentContainer;
	public PUGameObject ShipsContainer;
	public LDGGame game = null;

	public PULabel EquipmentLabel;
	public PUColor EquipmentLabelColor;
	public PUGameObject EquipmentLabelGO;

	public PULabel redPlanetHealth;
	public PULabel bluePlanetHealth;

	public PULabel redPlanetBuildTime;
	public PULabel bluePlanetBuildTime;




	protected LDGEquipment mousedEquipment = null;

	public void Start() {

		if (game == null) {
			game = LDGGame.CreateGame ();
		}

		// load the game!
		// 1) Load all space equipment and put it in the scene...
		foreach (LDGEquipment equipment in game.Equipments) {
			equipment.GetSprite (EquipmentContainer);
		}
	}

	public void BuildShip() {
		game.BuildCurrentShipForPlanet (game.bluePlanet());
	}

	public void FixedUpdate() {

		game.AdvanceGame (ShipsContainer);

		// 0) Find the closest piece of equipment to the mouse position
		// 1) if its under a certain delta scale up the equipment so its easier to see, and show the label for
		//    what it is.
		if (Input.GetMouseButton (0) == false) {

			if (mousedEquipment != null) {

				Vector3 pos = mousedEquipment.sprite.gameObject.transform.localPosition;

				if(pos.y > 180 && pos.x > 888){
					// We dropped an equipment; is it in our build queue list?
					game.AddEquipmentToPlanetBuildQueue (mousedEquipment, game.bluePlanet ());
				}
			}


			Vector3 mouse = Input.mousePosition;
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

	}

	public void UpdateSpaceEquipment(LDGEquipment e)
	{
		float currentScale = e.sprite.gameObject.transform.localScale.x;
		float targetScale = e.baseScale;
		Vector3 mousePos = Input.mousePosition;

		mousePos.x = Mathf.Clamp (mousePos.x, 0, 960);
		mousePos.y = Mathf.Clamp (mousePos.y, 0, 960);

		if (e == mousedEquipment) {
			// 1) if its under a certain delta scale up the equipment so its easier to see, and show the label
			targetScale = e.baseScale * 3.0f;

			// User is dragging an equipment pod
			if (Input.GetMouseButton (0)) {
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
			queuePosition = new Vector3(33,407-n*yDelta,0);
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

