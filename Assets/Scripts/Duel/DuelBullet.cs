using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelBullet : MonoBehaviour {

    public float moveSpeed;
    public int teamNumber;

    Vector3 direction;

    Dueler ownerDueler;
	
    public void setup(Vector2 direction,int teamNumber,Dueler owner,Quaternion rotation)
    {
        this.direction = direction;
        this.teamNumber = teamNumber;
        this.ownerDueler = owner;
        this.transform.rotation = rotation;

        this.gameObject.layer =  LayerMask.NameToLayer("Team" + teamNumber);
    }
	
	// Update is called once per frame
	void Update () {
        transform.Translate(direction * Time.deltaTime * moveSpeed , Space.World);
        if(transform.position.x >= DuelGame.mapSize || transform.position.x <= -DuelGame.mapSize || transform.position.y >= DuelGame.mapSize || transform.position.y <= -DuelGame.mapSize)
        {
            Destroy(this.gameObject);
        }
    }

    public void alertOwnerOfHit()
    {
        ownerDueler.killedSomeone();
    }
}
