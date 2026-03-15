using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomLeafs : MonoBehaviour
{
    public GameObject ground;
    public Vector3 sizeG;
    public Vector3 centerG;
    public GameObject leaf;
    public int nrOfLeafs = 100;

    // Start is called before the first frame update
    void Start()
    {
        centerG = ground.transform.position;
        //sizeG = ground.transform.localScale;

        //print("X:"+sizeG.x + " Y:"+sizeG.y + " Z:" + sizeG.z);

        while (nrOfLeafs > 0)
        {
            SpawnLeafs();
            nrOfLeafs--;
        }

    }

    public void SpawnLeafs()
    {
        Vector3 pos = centerG + new Vector3(Random.Range(-sizeG.x / 2, sizeG.x / 2), 0.5f, Random.Range(-sizeG.z / 2, sizeG.z / 2));
        GameObject newLeaf = Instantiate(leaf, pos, Quaternion.Euler(new Vector3(0, Random.Range(0,360), 0)),transform);
        newLeaf.transform.localScale = newLeaf.transform.localScale*Random.Range(0.75f, 1.25f);
    }
}
