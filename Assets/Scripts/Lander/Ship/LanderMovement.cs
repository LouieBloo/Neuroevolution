using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanderMovement : MonoBehaviour {

    public Rigidbody2D rigidBody;

    public float sideThrusterPower = 0.1f;
    public float middleThrustPower = 0.1f;
    public float gas = 100f;

    public void thrust(float thrustAmount)
    {
        rigidBody.AddForce(new Vector2(0, fuelLines(middleThrustPower,thrustAmount)), ForceMode2D.Impulse);
    }

    public void fireLeftThruster(float thrustAmount)
    {
        rigidBody.AddForceAtPosition(new Vector2(0, -fuelLines(sideThrusterPower, thrustAmount)), new Vector2(-1, 1));
    }

    public void fireRightThruster(float thrustAmount)
    {
        rigidBody.AddForceAtPosition(new Vector2(0, fuelLines(sideThrusterPower, thrustAmount)), new Vector2(1, 1));
    }

    float fuelLines(float power,float amount)
    {
        if(gas > 0)
        {
            float total = power * amount * Time.deltaTime;
            gas -= total > 0f ? total : 0f;
            return total;
        }
        return 0;
    }
}
