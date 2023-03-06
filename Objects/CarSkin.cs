using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ObjectInstances;

public class CarSkin : Skin
{
    [SerializeField] private Renderer colorRend;

    [SerializeField] private GameObject tyreFab;

    [SerializeField] private float wheelsDepth;
    [SerializeField] private Transform tyreParentL;
    [SerializeField] private Transform tyreParentR;

    public int wheels { set { tyres = value; } }
    public int tires { set { tyres = value; } }
    public int tyres
    {
        set
        {
            // destroy any tyres
            Transform[] parents = new Transform[] { tyreParentR, tyreParentL };
            for (int i = 0; i < 2; i++)
                for (int j = parents[i].childCount - 1; j >= 0; j--)
                    Destroy(parents[i].GetChild(j).gameObject);


            int totalTyres = value;
            // create new tyres
            int[] tyreCounts = new int[] { ((totalTyres / 2) + totalTyres % 2), (totalTyres / 2) };
            for (int i = 0; i < 2; i++)
            {
                if (tyreCounts[i] <= 0) return;

                Instantiate(tyreFab, parents[i]);

                if (tyreCounts[i] > 1)
                {
                    float step = wheelsDepth / (tyreCounts[i] - 1);
                    for (int j = 1; j < tyreCounts[i]; j++)
                    {
                        Transform t = Instantiate(tyreFab, parents[i]).transform;
                        t.localPosition = new Vector3(step * j, 0f, 0f);
                    }
                }
            }
        }
    }

    public override Color _colour { set { } }

    public override string colour
    {
        set
        {
            // set the colour of the car
            colorRend.material.color = getColour(value);
        }
    }
}
