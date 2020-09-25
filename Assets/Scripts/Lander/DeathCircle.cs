using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCircle : MonoBehaviour {

    public float growthRate;

    IEnumerator growRoutine;

    public bool running = false;
	
    public void startGrowing(Vector2 startPosition)
    {
        this.gameObject.SetActive(true);
        this.transform.position = startPosition;
        this.running = true;
        this.transform.localScale = new Vector3(1,1, 1);

        growRoutine = grow();
        StartCoroutine(growRoutine);
    }

    public void stopGrowing()
    {
        this.running = false;
        
        if(growRoutine != null)
        {
            StopCoroutine(growRoutine);
            growRoutine = null;
        }

        this.gameObject.SetActive(false);
    }

    IEnumerator grow()
    {
        float size = 1f;
        while (true)
        {
            this.transform.localScale = new Vector3(size, size, 1);
            yield return null;
            size += growthRate * Time.deltaTime;
        }
    }
}
