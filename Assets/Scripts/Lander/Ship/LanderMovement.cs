using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanderMovement : MonoBehaviour {

    public Rigidbody2D rigidBody;

    public Transform leftThruster;
    public Transform rightThruster;

    public float sideThrusterPower = 0.1f;
    public float middleThrustPower = 0.1f;

    public float maxGas = 200f;
    public float gas = 200f;

    void Start()
    {
        gas = maxGas;
    }

    public void refill()
    {
        this.gas = maxGas;
    }

    public void thrust(float thrustAmount)
    {
        float power = fuelLines(middleThrustPower, thrustAmount);
        power = power > 0 ? power : 0;
        rigidBody.AddForce(new Vector2(transform.up.x * power, transform.up.y * power), ForceMode2D.Impulse);
    }

    public void fireLeftThruster(float thrustAmount)
    {
        //rigidBody.AddForceAtPosition(new Vector2(0, fuelLines(sideThrusterPower, thrustAmount)), new Vector2(-10, 10));
        float power = fuelLines(sideThrusterPower, thrustAmount);
        power = power > 0 ? power : 0;
        rigidBody.AddForceAtPosition(new Vector2(leftThruster.up.x * power, leftThruster.up.y * power),leftThruster.position);
    }

    public void fireRightThruster(float thrustAmount)
    {
        //rigidBody.AddForceAtPosition(new Vector2(0, ), new Vector2(10, 10));
        float power = fuelLines(sideThrusterPower, thrustAmount);
        power = power > 0 ? power : 0;
        rigidBody.AddForceAtPosition(new Vector2(rightThruster.up.x * power, rightThruster.up.y * power),rightThruster.position);
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
