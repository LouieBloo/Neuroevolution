using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class ExtensionMethods
{

    // Deep clone
    public static T DeepClone<T>(this T a)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, a);
            stream.Position = 0;
            return (T)formatter.Deserialize(stream);
        }
    }

    //5 is really close to us, -5 is max distance away
    public static float normalizeDistanceFromRaycast(RaycastHit2D hit)
    {
        if (!hit)
        {
            return -5f;
        }

        float distance = hit.distance > 2f ? 2f : hit.distance;
        return 5f - (distance * 5f);
    }

    //public static float normalizeNumber(float input,float minAllowed, float maxAllowed,float min, float max)
    //{
    //    return ((maxAllowed - minAllowed) * (input - min)) / (max - min) + minAllowed;
    //}

    public static int highestIndexInList(List<float> inputs)
    {
        float highest = inputs[0];
        int index = 0;

        for (int x = 0; x < inputs.Count; x++)
        {
            if (inputs[x] > highest)
            {
                highest = inputs[x];
                index = x;
            }
        }

        return index;
    }


    
}


