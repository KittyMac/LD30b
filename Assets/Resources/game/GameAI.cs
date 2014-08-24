using UnityEngine;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;
using System.Threading;

public class GameAI : Object {

	private LDGGame game;


	public void PerformAIOnThread() {

		System.Random random = new System.Random ();

		Thread.Sleep (4000);

		while(true){

			// AI TASKS:
			// 1) Determine what kind of ship we need (quick to build, or do we have luxury of time?
			// 2) Nab equipment from the center and add it to the build queue
			// 3) When you have enough equipment, press the non-existant "build" button




			// 2) Nab equipment from the center and add it to the build queue
			int idealNumberOfEquipments = 4;
			int numberOfEquipmentNeeded = idealNumberOfEquipments - game.redPlanet ().Equipments.Count;

			if(numberOfEquipmentNeeded > 0 && game.Equipments.Count > 0){

				// TODO: Pick beter things than random equipment
				int n = game.Equipments.Count;
				int j = random.Next () % n;
	
				LDGEquipment e = game.Equipments [j] as LDGEquipment;

				PlanetUnityGameObject.ScheduleTask (new Task (() => {
					game.AddEquipmentToPlanetBuildQueue(e, game.redPlanet());
				}));

				// need to pause to give the user a chance, you know?
				Thread.Sleep (2000);
				continue;
			}

			// 3) When you have enough equipment, press the non-existant "build" button
			if (numberOfEquipmentNeeded <= 0) {
				PlanetUnityGameObject.ScheduleTask (new Task (() => {
					game.BuildCurrentShipForPlanet (game.redPlanet ());
				}));
				Thread.Sleep (1000);
				continue;
			}
		}


	}

	public void BeginAIForGame(LDGGame _game) {

		game = _game;

		Thread processingThread = new Thread (PerformAIOnThread);
		processingThread.Start ();

	}
}

