using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Dendrite {


    public float weight;
    public float lastOutput = 0;

    public Dendrite()
    {
        weight = UnityEngine.Random.Range(-0.5f, 0.5f);
    }

    public Dendrite(float weight,float lastOutput)
    {
        this.weight = weight;
        this.lastOutput = lastOutput;
    }

    public float calculateNewOutput(float input)
    {
        lastOutput = weight * input;
        return lastOutput;
    }

    /// <summary>
    /// Mutates the dendrites weight on a mutationRate chance, mutationRate 0-1, mutationAmount is percent to increase or decrease
    /// </summary>
    /// <param name="mutationRate"></param>
    /// <param name="mutationAmount"></param>
    public void mutate(float mutationRate,float mutationAmount)
    {
        float diceRoll = UnityEngine.Random.Range(0f, 1f);

        if(diceRoll >= mutationRate)
        {
            diceRoll = UnityEngine.Random.Range(0f, 1f);//roll another dice to decide if we are mutating up, or down, aka are we adding or subtracting to our weight
            if(diceRoll <= 0.5f)
            {
                weight += UnityEngine.Random.Range(0,mutationAmount);
            }
            else
            {
                weight -= UnityEngine.Random.Range(0, mutationAmount);
            }
        }

    }

    public Dendrite copy()
    {
        return new Dendrite(weight, lastOutput);
    }
}
