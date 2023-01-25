using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePile : MonoBehaviour
{
    public List<Tile> Tiles;
    public PlayerManager playerManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }


public void drawTile(){
    if(Tiles.Count >= 1){
        Tile currentTile = Tiles[Random.Range(0,Tiles.Count)];
        
        currentTile.gameObject.SetActive(true);
        //Transform to correct pos!
        Player CurrPlayer = playerManager.getCurrentPlayer();
        CurrPlayer.HasTile = true;
        Tiles.Remove(currentTile);
        return;

    }
}


}
