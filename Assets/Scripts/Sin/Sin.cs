using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sin : NetworkManager
{
    public Text goalNumber;
    public Text guessNumber;

    bool force = false;
    bool fast = false;
    bool goNextFrame = false;

    void Start()
    {
        Application.runInBackground = true;
        startPressed();
    }

    
    protected override void testNetworksParallel(List<NeuralNetwork> networks)
    {
        StartCoroutine(frame(networks));
    }

    public void goNext()
    {
        goNextFrame = true;
    }

    public void forceee()
    {
        force = !force;
    }

    public void fastee()
    {
        fast = !fast;
    }

    IEnumerator frame(List<NeuralNetwork> networks)
    {
        yield return null;

        int x = 0;
        while(x < 2)
        {
            float testNumber = Random.Range(-1f, 1f);
            goalNumber.text = Mathf.Sin(testNumber) + "";
            updateTexts();
            foreach (NeuralNetwork n in networks)
            {
                while (!fast && !force && !goNextFrame)
                {
                    yield return null;
                }
                List<float> output = n.feedInputs(new List<float>() { testNumber });
                guessNumber.text = output[0] + "";
                n.setFitness(fitnessFunc(testNumber, output[0]));

                updateTexts();
                currentChromosome++;

                
                goNextFrame = false;
            }

            force = false;
            x++;
            yield return null;
        }



        testedNetworks.AddRange(networks);
        untestedNetworks.Clear();
        parseTestingResults();
    }
    

    float fitnessFunc(float testNumber,float actualNumber)
    {
        float f =-1f * (Mathf.Abs(actualNumber - Mathf.Sin(testNumber)));

        return f;
    }
}
