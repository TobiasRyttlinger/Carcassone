using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MeeplesUI : MonoBehaviour
{
    // Start is called before the first frame update

    public int MeeplesCount;


    public Image[] meepImage;
    public Sprite meepSprite;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < meepImage.Length; i++)
        {
            if (i < MeeplesCount)
            {
                meepImage[i].enabled = true;
            }
            else
            {
                meepImage[i].enabled = false;
            }


        }
    }
}
