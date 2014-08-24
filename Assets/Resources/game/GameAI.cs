using UnityEngine;
using System.Threading;

public class GameAI : Object {

	private LDGGame game;

	static public float difficulty = 0.75f;

	private bool continueRunningThread = true;

	private Thread processingThread1;

	public void PerformAIOnThread() {

		System.Random random = new System.Random ();

		Thread.Sleep (4000);

		while(continueRunningThread){

			// AI TASKS:
			// 1) Determine what kind of ship we need (quick to build, or do we have luxury of time?
			// 2) Nab equipment from the center and add it to the build queue
			// 3) When you have enough equipment, press the non-existant "build" button




			// 2) Nab equipment from the center and add it to the build queue
			int idealNumberOfEquipments = 4;





			int numberOfEquipmentNeeded = idealNumberOfEquipments - game.redPlanet ().Equipments.Count;

			if(numberOfEquipmentNeeded > 0 && game.Equipments.Count > 0){

				PlanetUnityGameObject.ScheduleTask (new Task (() => {

					int n = game.Equipments.Count;
					LDGEquipment bestEquipment = null;

					foreach (LDGEquipment e in game.Equipments) {
						if (bestEquipment == null || e.aiValue () > bestEquipment.aiValue ()) {
							bestEquipment = e;
						}
					}

					//if (bestEquipment != null) {
					//	Debug.Log ("AI chose: " + bestEquipment.name );
					//}

					if (bestEquipment != null && bestEquipment.beingDragged == false) {
						game.AddEquipmentToPlanetBuildQueue (bestEquipment, game.redPlanet ());
					}
				}));

				// need to pause to give the user a chance, you know?
				int waitTime = (int)(game.averageUserDecisionTime * difficulty);

				if (waitTime > 2000) {
					waitTime = 2000;
				}
				if (waitTime < 100) {
					waitTime = 100;
				}
				waitTime += random.Next () % 200;

				Thread.Sleep (waitTime);

				continue;
			}

			// 3) When you have enough equipment, press the non-existant "build" button
			if (numberOfEquipmentNeeded <= 0) {
				PlanetUnityGameObject.ScheduleTask (new Task (() => {
					if(game.redPlanet ().Equipments.Count >= idealNumberOfEquipments) {
						game.BuildCurrentShipForPlanet (game.redPlanet ());
					}
				}));
				Thread.Sleep (500);
				continue;
			}


			Thread.Sleep (100);
		}


	}

	public void BeginAIForGame(LDGGame _game) {

		game = _game;

		processingThread1 = new Thread (PerformAIOnThread);
		processingThread1.Start ();
	}

	public void Abort(){

		continueRunningThread = false;
		processingThread1.Join ();
	}
}

