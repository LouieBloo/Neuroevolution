using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Balancerod : MonoBehaviour {

    Action callback;

	public void setup(Action callback)
    {
        this.callback = callback;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if(coll.transform.CompareTag("Floor"))
        {
            callback();
        }
    }
}
