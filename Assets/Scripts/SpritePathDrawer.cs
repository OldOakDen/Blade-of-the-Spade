using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpritePathDrawer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public DetectManager detectManager;
    //public Transform spriteTransform; // Transform vašeho spritu

    private List<Vector3> pathPositions = new List<Vector3>();
    private Vector3 lastMapCoord; //koodrinaty posledniho bodu v mape

    private void Start()
    {
        lastMapCoord = transform.position;
        pathPositions.Add(transform.position);
        //lineRenderer.SetPosition(0, transform.position);
    }

    private void Update()
    {
        //otetujeme, jestli je hrac vzdalen alespon metr (nebo urcita jina vzdalenost) od predchoziho bodu v mape
        if (Vector3.Distance(lastMapCoord, transform.position) > 8)
        {
            // Pøidáme aktuální pozici spritu do seznamu
            pathPositions.Add(transform.position);
            detectManager.sixthSenseMeter.AddSixthSense(); //pricti sesty smysl
            lastMapCoord = transform.position; //novy bod, od ktereho se bude merit dalsi vzdalenost pro dalsi zaznamenani bodu

            // Nastavíme poèet bodù pro Line Renderer
            lineRenderer.positionCount = pathPositions.Count;
            //VisualElement detectingUI = detectManager.sceneUI.GetComponent<UIDocument>().rootVisualElement;
            //detectingUI.Q<Label>("FindCounter").text = "Bodu LineRendereru: " + pathPositions.Count;
            // print ("Bodu LineRendereru: " + pathPositions.Count);

            // Nastavíme pozice bodù èáry
            for (int i = 0; i < pathPositions.Count; i++)
            {
                lineRenderer.SetPosition(i, pathPositions[i]);
            }
        }
    }
}
