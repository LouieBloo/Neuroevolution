using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelerTurret : MonoBehaviour {

    
    public Transform turret;

    public GameObject bulletPrefab;

    public float fireThreshold = 0.0f;
    public float attackSpeed = 1f;
    float attackTimer = 0;
	
	// Update is called once per frame
	void Update () {

        //Vector2 offset = getOffset();
        //float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        //turret.rotation = Quaternion.Euler(0, 0, angle + 270f);

        attackTimer += Time.deltaTime;
        
    }

    public void rotate(float input)
    {
        turret.rotation = Quaternion.Euler(0, 0, (1f + input)*(360f / 2f));
    }

    public void fire(int teamNumber,float amount,Dueler owner)
    {
        if(amount >= fireThreshold && attackTimer >= attackSpeed)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<DuelBullet>().setup(turret.up, teamNumber, owner);

            attackTimer = 0;
        }
    }

    public void reset()
    {
        this.attackTimer = 999;
    }

    Vector2 getOffset()
    {
        Vector3 mouse = Input.mousePosition;
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.localPosition);
        Vector2 offset = new Vector2(mouse.x - screenPoint.x, mouse.y - screenPoint.y);
        return offset;
    }
}
