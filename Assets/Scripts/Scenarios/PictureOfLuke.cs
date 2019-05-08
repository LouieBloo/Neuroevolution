using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PictureOfLuke : NetworkManager
{
    public TextAsset imageTA;

    public SpriteRenderer madeSprite;
    public Texture2D texture;

    void Start()
    {
        //Texture2D tex = new Texture2D(4, 4);
        //tex.LoadImage(imageTA.bytes);
        //madeSprite.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));


        Vector2 newDimentions = getResizeDimentions(texture);

        texture.Resize((int)newDimentions.x, (int)newDimentions.y);
        texture.Apply();

        madeSprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
    }

    Vector2 getResizeDimentions(Texture2D inputTex)
    {
        float aspectRatio = (float)inputTex.width / (float)inputTex.height;
        Vector2 returnDimentions = new Vector2();

        Debug.Log("AR: " + aspectRatio);

        if(aspectRatio > 1f)//width bigger than height
        {
            returnDimentions.y = 150;
            returnDimentions.x = 150 * aspectRatio;
        }
        else
        {
            returnDimentions.x = 150;
            returnDimentions.y = 150 * aspectRatio;
        }
        Debug.Log(returnDimentions);
        return returnDimentions;
    }

    protected override void testNetwork(NeuralNetwork networkToTest)
    {
    }

    void networkFinishedTesting(NeuralNetwork networkTested, int foodCount, float timeSurvived)
    {
        //Debug.Log("Dead: " + foodCount);
        //networkTested.fitness = fitnessFunc(foodCount, timeSurvived);

        untestedNetworks.Remove(networkTested);
        testedNetworks.Add(networkTested);


        if (untestedNetworks.Count > 0)
        {
            currentChromosome++;
            updateTexts();
            testNetwork(untestedNetworks[0]);
        }
        else
        {
            parseTestingResults();
        }
    }

    float fitnessFunc(int foodEaten, float timeSurvived)
    {
        //return Mathf.Exp(foodEaten) + (timeSurvived / 10f);
        //return timeSurvived;
        return foodEaten;
    }
}
