using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Node {

    public float lastOutput;

    public float bias;

    public float dendriteCount
    {
        get
        {
            return allDendrites.Count;
        }
    }

    List<Dendrite> allDendrites = new List<Dendrite>();

    public Node(int dendrideCount)
    {
        for (int x = 0; x < dendrideCount; x++)
        {
            allDendrites.Add(new Dendrite());
        }

        bias = UnityEngine.Random.Range(-0.01f, 0.01f);
    }

    public Node(List<Dendrite> dendrites,float bias,float lastOutput)
    {
        this.allDendrites = dendrites;
        this.bias = bias;
        this.lastOutput = lastOutput;
    }


    public float calculateOutput(List<float> inputs)
    {
        if(inputs == null) { Debug.Log("Null inputs on node");return -1; }
        if (inputs.Count != allDendrites.Count) { Debug.Log("Mismatched input lengths on node");return -1; }

        float output = 0;

        for(int x =0;x< allDendrites.Count;x++)
        {
            output += allDendrites[x].calculateNewOutput(inputs[x]);
        }


        //output += bias;
        //output = sigmoid(output);
        output = Tanh(output);

        lastOutput = output;

        return lastOutput;
    }

    public void mutate(float mutationRate, float mutationAmount)
    {
        foreach(Dendrite d in allDendrites)
        {
            d.mutate(mutationRate, mutationAmount);
        }

        mutateBias(mutationRate, mutationAmount);
    }

    /// <summary>
    /// Mutates the bias on a mutationRate chance, mutationRate 0-1, mutationAmount is percent to increase or decrease
    /// </summary>
    /// <param name="mutationRate"></param>
    /// <param name="mutationAmount"></param>
    void mutateBias(float mutationRate, float mutationAmount)
    {
        float diceRoll = UnityEngine.Random.Range(0f, 1f);

        if (diceRoll >= mutationRate)
        {
            diceRoll = UnityEngine.Random.Range(0f, 1f);//roll another dice to decide if we are mutating up, or down, aka are we adding or subtracting to our bias
            if (diceRoll <= 0.5f)
            {
                bias += bias * mutationAmount;
            }
            else
            {
                bias -= bias * mutationAmount;
            }
        }

    }

    public float sigmoid(float value)
    {
        return 1.0f / (1.0f + (float)Mathf.Exp(-value));
    }

    public float Tanh(float x)
    {
        return 2f / (1f + Mathf.Exp(-(2f * x))) - 1f;
    }

    public void print()
    {
        for (int x = 0; x < allDendrites.Count; x++)
        {
            Debug.Log(x + ": " +  allDendrites[x].weight);
        }
    }

    public Node copy()
    {
        List<Dendrite> dendriteCopy = new List<Dendrite>();
        foreach(Dendrite d in allDendrites)
        {
            dendriteCopy.Add(d.copy());
        }
        return new Node(dendriteCopy, bias,lastOutput);
    }
}
