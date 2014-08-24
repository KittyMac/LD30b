
using UnityEngine;
using System.IO;

public partial class LDGShip : LDGShipBase {

	public PUSprite sprite;

	public float buildTime(){
		float bt = 0;
		foreach (LDGEquipment e in Equipments) {
			bt += 2;
		}
		return bt;
	}

	public void InitCombatValues() {

		structure = 0;
		armor = 0;
		shields = 0;

		foreach (LDGEquipment e in Equipments) {
			structure += e.structure;
			armor += e.armor;
			shields += e.shields;
		}
	}

	public int ShipSize()
	{
		if (Equipments.Count <= 4) {
			return 0;
		} else if (Equipments.Count <= 7) {
			return 1;
		} else {
			return 2;
		}
		return 0;
	}

	public string ShipSizeAsString()
	{
		switch (ShipSize ()) {
		case 0:
			return "Scout";
		case 1:
			return "Frigate";
		case 2:
			return "Capital";
		}
		return "Scout";
	}

	public string PlayerAsString()
	{
		if (player == 0) {
			return "blue";
		}
		return "red";
	}

	public string TexturePath()
	{
		return string.Format ("{0}{1}", PlayerAsString (), ShipSizeAsString ());
	}

}
