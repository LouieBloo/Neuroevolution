using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Layer {

    
    public List<float> lastOutput;

    List<Node> allNodes = new List<Node>();

    public Layer(int nodeCount, int dendriteCount)
    {
        for (int x = 0; x < nodeCount; x++)
        {
            allNodes.Add(new Node(dendriteCount));
        }
    }

    public Layer(List<Node> nodes,List<float> lastOutput)
    {
        this.allNodes = nodes;
        this.lastOutput = lastOutput;
    }

    public List<float> calculateOutput(List<float> input)
    {
        lastOutput = new List<float>();
        foreach(Node n in allNodes)
        {
            lastOutput.Add(n.calculateOutput(input));
        }

        return lastOutput;
    }

    public void printOutput()
    {
        for (int x = 0; x < lastOutput.Count; x++)
        {
            Debug.Log("Node " + x + ": " + lastOutput[x]);
        }
    }

    public void printLayer()
    {
        for(int x = 0; x < allNodes.Count;x ++)
        {
            Debug.Log("Node " + x + ": ");
            allNodes[x].print();
        }
    }

    public float nodeCount
    {
        get
        {
            return allNodes.Count;
        }
    }

    public float dendriteCount
    {
        get
        {
            return allNodes.Count > 0 ? allNodes[0].dendriteCount : 0;
        }
    }

    public List<Node> getNodes
    {
        get
        {
            return allNodes;
        }
    }

    public void mutate(float mutationRate,float mutationAmount)
    {
        foreach(Node n in allNodes)
        {
            n.mutate(mutationRate, mutationAmount);
        }
    }

    public Layer copy()
    {
        List<Node> nodeCopy = new List<Node>();
        List<float> lastOutputCopy = new List<float>();

        foreach(Node n in allNodes)
        {
            nodeCopy.Add(n.copy());
        }

        foreach(float f in lastOutput)
        {
            lastOutputCopy.Add(f);
        }

        return new Layer(nodeCopy, lastOutputCopy);
    }
}
