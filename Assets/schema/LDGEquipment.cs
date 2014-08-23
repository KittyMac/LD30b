using UnityEngine;

public partial class LDGEquipment : LDGEquipmentBase {

	public PUSprite sprite = null;
	public float baseScale = 32;

	public LDGEquipment Clone() {
		return (LDGEquipment)this.MemberwiseClone();
	}

	public PUSprite GetSprite(PUGameObject parent){
		if (sprite == null) {
			sprite = new PUSprite ("game/equipment" + icon, position, baseScale, new cRect(0,0,0,0));
			sprite.loadIntoPUGameObject (parent);
			sprite.gameObject.transform.localPosition = new Vector3 (position.x, position.y, position.z);

			sprite.gameCollider2D = (CircleCollider2D) sprite.gameObject.AddComponent(typeof(CircleCollider2D));
			CircleCollider2D circleCollider = sprite.gameCollider2D as CircleCollider2D;
			circleCollider.radius = 0.2f;

			sprite.rigidBody2D = (Rigidbody2D)sprite.gameObject.AddComponent (typeof(Rigidbody2D));
			sprite.rigidBody2D.gravityScale = 0.0f;
		}
		return sprite;
	}

}
