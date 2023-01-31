using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meep : MonoBehaviour
{
    public Player owner;

    public Tile.directions placedPositionOnTile;

    public MeshRenderer myRenderer;
    // Start is called before the first frame update
    void Start()
    {

    }


    public void setMaterial(Texture2D inTex){
        myRenderer.material.color = inTex.GetPixel(1,1);
    }
}
