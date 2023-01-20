using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Tile> Tiles;
    public List<Tile> RiverTiles;
    public List<Tile> BasicTiles;
    public PlayerManager playerManager;
    public Player CurrPlayer;
    public int placedTiles = 0;

    public bool river = true;
    public Grid playGrid;
    // Start is called before the first frame update
    void Start()
    {
        playGrid = new Grid(100, 100);
        if (playerManager == null)
        {
            playerManager = GameObject.FindObjectOfType<PlayerManager>();
        }
        foreach (Player p in playerManager.GetPlayers())
        {
            p.gridManager = playGrid;
        }
 

        scramblePile();


        //Combine expansion decks to a tiledeck!
        if (river)
        {

            Tiles.AddRange(RiverTiles);
        }

        //Initialise more expansions

        //Add basic tiles to the final deck
        Tiles.AddRange(BasicTiles);


        //initialise first tile
        if (placedTiles == 0)
        {

            playGrid.placeTile((int)playGrid.avaliableGrids[0].x, (int)playGrid.avaliableGrids[0].y, Tiles[0]);
            playGrid.generateAvaliableSpots(Tiles[0]);
            Tiles.Remove(Tiles[0]);
        }

    }

    // Update is called once per frame
    void Update()
    {

        CurrPlayer = playerManager.getCurrentPlayer();
        foreach (Player p in playerManager.GetPlayers())
        {
            p.tilesLeft.text = Tiles.Count.ToString();
        }

        if (Tiles.Count > 0)
        {
            drawTile();
        }
        else
        {
            foreach (Player p in playerManager.GetPlayers())
            {
                if (!p.HasTile)
                {
                    p.PlaceButton.interactable = false;
                }

            }
        }

    }


    public void drawTile()
    {
        if (Tiles.Count >= 1 && !CurrPlayer.HasTile)
        {

            Tile currentTile = Tiles[0];

            currentTile.gameObject.SetActive(true);

            CurrPlayer.HasTile = true;
            CurrPlayer.tileInHand = currentTile;
            Tiles.Remove(currentTile);
            return;

        }
      
    }

    public void scramblePile()
    {
        if (RiverTiles.Count > 0)
        {
            for (int i = 1; i < RiverTiles.Count - 1; i++)
            {
                Tile temp = RiverTiles[i];
                int randomIndex = Random.Range(i, RiverTiles.Count - 1);
                RiverTiles[i] = RiverTiles[randomIndex];
                RiverTiles[randomIndex] = temp;
            }
        }
        if (BasicTiles.Count > 0)
        {
            for (int i = 0; i < BasicTiles.Count; i++)
            {
                Tile temp = BasicTiles[i];
                int randomIndex = Random.Range(i, BasicTiles.Count);
                BasicTiles[i] = BasicTiles[randomIndex];
                BasicTiles[randomIndex] = temp;
            }
        }

    }

}
