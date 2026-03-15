using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SixthSenseMeter : MonoBehaviour
{
    public int sixthSenseProgress = 0; // progres sesteho smyslu, ktery bude pravdepodobne v PlayerPrefs a bude se pouzivat naskrz hrou jako moznost napovedy
    public float sixthSenseMaximum = 100; //maximum bodu, ktere je potreba pro 100% nabiti sesteho smyslu

    public Image sixthSenseBar;

    private void Start()
    {
        sixthSenseBar.fillAmount = sixthSenseProgress / sixthSenseMaximum;
    }
    public void AddSixthSense() //pricte a zobrazi sesty smysl na meraku
    {
        sixthSenseProgress++;
        //dodelat zobrazeni
        sixthSenseBar.fillAmount = sixthSenseProgress / sixthSenseMaximum;
    }
}
