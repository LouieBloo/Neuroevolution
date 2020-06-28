using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour {

    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    public void setSize(float size,float maxSize)
    {
        Vector2 currentScale = this.transform.localScale;
        this.transform.localScale = new Vector2(currentScale.x, size);

        spriteRenderer.color = Color.Lerp(Color.red, Color.green, size / maxSize);
    }
}
