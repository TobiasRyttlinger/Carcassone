using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
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

    public Material PsMat;


    public enum GamePhases
    {
        TilePhase,
        MeepPhase,
        ScorePhase

    }

    public GamePhases currentPhase;
    private int xSize = 100;
    private int ySize = 100;

    // Start is called before the first frame update
    void Start()
    {

        playGrid = new Grid(xSize, ySize);

        if (playerManager == null)
        {
            playerManager = GameObject.FindObjectOfType<PlayerManager>();
        }
        foreach (Player p in playerManager.GetPlayers())
        {
            p.gridManager = playGrid;
            p.GM = this;
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

        //Fix Meeple spawn positions on each tile
        foreach (Tile t in Tiles)
        {
            t.initialise(t, PsMat);

        }

        //initialise first tile
        if (placedTiles == 0)
        {
            playGrid.placeTile((int)playGrid.avaliableGrids[0].x, (int)playGrid.avaliableGrids[0].y, Tiles[0]);

            playGrid.generateAvaliableSpots(Tiles[0]);
            DeactivateMeeplePos(Tiles[0], Tile.directions.ALL);
            Tiles.Remove(Tiles[0]);
        }
        currentPhase = GamePhases.TilePhase;

    }

    // Update is called once per frame
    [System.Obsolete]
    void Update()
    {
        // Debug.Log(currentPhase);
        CurrPlayer = playerManager.getCurrentPlayer();
        //Update UI Text
        updateUITexts();

        //TilePhase
        if (currentPhase == GamePhases.TilePhase)
        {
            //  Debug.Log("TilePhase");
            CurrPlayer.PlaceButton.enabled = true;
            if (!CurrPlayer.placedTile)
            {
                if (Tiles.Count > 0)
                {
                    drawTile();
                    CurrPlayer.placedMeep = false;
                    playGrid.generateAvaliableSpots(CurrPlayer.tileInHand);
                    checkRiverTurn(CurrPlayer.tileInHand, CurrPlayer.lastPlacedTile);
                }
                else
                {
                    foreach (Player p in playerManager.GetPlayers())
                    {
                        if (!p.HasTile)
                            p.PlaceButton.interactable = false;
                    }
                }
            }
        }

        //MeepPhase
        else if (currentPhase == GamePhases.MeepPhase)
        {

            // CurrPlayer.PlaceButton.enabled = false;
            if (!CurrPlayer.placedMeep && CurrPlayer.placedTile)
            {
                int countr = 0;
                // Debug.Log("Meepphase");
                foreach (Tile.directions side in CurrPlayer.lastPlacedTile.Sides)
                {
                    bool disabledRoad = false;
                    bool disabledCity = false;
                    bool disabledGrass = false;
                    // Debug.Log("Checking " + side + " of placed tile.");
                    List<Grid.connectionsList> foundCityConnections = new List<Grid.connectionsList>();
                    List<Grid.connectionsList> foundRoadConnections = new List<Grid.connectionsList>();
                    List<Grid.connectionsList> foundGrassConnections = new List<Grid.connectionsList>();
                    if (CurrPlayer.lastPlacedTile.getTerrainType(side) == Tile.terrainType.CITY)
                    {
                        foundCityConnections = playGrid.findConnections(CurrPlayer.lastPlacedTile, side, "CITY", currentPhase);
                    }
                    if (CurrPlayer.lastPlacedTile.getTerrainType(side) == Tile.terrainType.ROAD)
                    {
                        foundRoadConnections = playGrid.findConnections(CurrPlayer.lastPlacedTile, side, "ROAD", currentPhase);
                    }
                    if (CurrPlayer.lastPlacedTile.getTerrainType(side) == Tile.terrainType.GRASS)
                    {
                        foundGrassConnections = playGrid.findGrassConnections(CurrPlayer.lastPlacedTile, side, "GRASS", currentPhase);
                    }

                    //General Meep positions
                    if (side == Tile.directions.TOP) CurrPlayer.lastPlacedTile.TOP.gameObject.active = false;
                    if (side == Tile.directions.DOWN) CurrPlayer.lastPlacedTile.DOWN.gameObject.active = false;
                    if (side == Tile.directions.RIGHT) CurrPlayer.lastPlacedTile.RIGHT.gameObject.active = false;
                    if (side == Tile.directions.LEFT) CurrPlayer.lastPlacedTile.LEFT.gameObject.active = false;

                    //Specific for farmer placement
                    if (side == Tile.directions.CENTER) CurrPlayer.lastPlacedTile.TL.gameObject.active = false;
                    if (side == Tile.directions.TOP_LEFT_LEFT || side == Tile.directions.TOP_LEFT_TOP) CurrPlayer.lastPlacedTile.CENTER.gameObject.active = false;
                    if (side == Tile.directions.DOWN_LEFT_LEFT || side == Tile.directions.DOWN_LEFT_DOWN) CurrPlayer.lastPlacedTile.DL.gameObject.active = false;
                    if (side == Tile.directions.DOWN_RIGHT_DOWN || side == Tile.directions.DOWN_RIGHT_RIGHT) CurrPlayer.lastPlacedTile.DR.gameObject.active = false;
                    if (side == Tile.directions.TOP_RIGHT_RIGHT || side == Tile.directions.TOP_RIGHT_TOP) CurrPlayer.lastPlacedTile.TR.gameObject.active = false;
                    //Loop through connections to check if any meeples exists in the chain
                    foreach (Grid.connectionsList conn in foundCityConnections)
                    {
                        //foreach (Meep meep in conn.tile.placedMeepsCities)
                        if (conn.tile.placedMeepsCity)
                        {
                            // DeactivateMeeplePos(CurrPlayer.lastPlacedTile, Tile.directions.ALL);
                            if (side == Tile.directions.TOP) CurrPlayer.lastPlacedTile.TOP.gameObject.active = false;
                            if (side == Tile.directions.DOWN) CurrPlayer.lastPlacedTile.DOWN.gameObject.active = false;
                            if (side == Tile.directions.RIGHT) CurrPlayer.lastPlacedTile.RIGHT.gameObject.active = false;
                            if (side == Tile.directions.LEFT) CurrPlayer.lastPlacedTile.LEFT.gameObject.active = false;
                            disabledCity = true;

                        }
                        foreach (Tile.TileConnections tc in CurrPlayer.tileInHand.cityConnections)
                        {
                            if (tc.list.Contains(conn.direction) && !disabledCity)
                            {
                                //Activate MeepPositions
                                activateMeeplePostions(side);
                                countr++;
                            }
                        }

                    }

                    foreach (Grid.connectionsList conn in foundRoadConnections)
                    {

                        //TileChain contains meep already! Cant place a new one
                        //foreach (Meep meep in conn.tile.placedMeepsRoads)
                        if (conn.tile.placedMeepsRoad)
                        {
                            //   Debug.Log("Found Meep in chain");
                            //  if (meep.placedPositionOnTile == conn.direction)
                            // {
                            // Debug.Log(side + " Chain contains road Meep! In " + conn.direction);
                            //  DeactivateMeeplePos(CurrPlayer.lastPlacedTile, Tile.directions.ALL);

                            if (side == Tile.directions.TOP) CurrPlayer.lastPlacedTile.TOP.gameObject.active = false;
                            if (side == Tile.directions.DOWN) CurrPlayer.lastPlacedTile.DOWN.gameObject.active = false;
                            if (side == Tile.directions.RIGHT) CurrPlayer.lastPlacedTile.RIGHT.gameObject.active = false;
                            if (side == Tile.directions.LEFT) CurrPlayer.lastPlacedTile.LEFT.gameObject.active = false;
                            disabledRoad = true;
                            //  break;
                            // }
                        }

                        foreach (Tile.TileConnections tc in CurrPlayer.lastPlacedTile.roadConnections)
                        {
                            if (tc.list.Contains(conn.direction) && !disabledRoad)
                            {
                                //Activate MeepPositions
                                activateMeeplePostions(side);
                                countr++;
                            }
                        }

                    }
                    //Grass 
                    foreach (Grid.connectionsList conn in foundGrassConnections)
                    {
                        //foreach (Meep meep in conn.tile.placedMeepsCities)
                        if (conn.tile.placedMeepsGrass)
                        {
                            bool falseMeep = true;
                            if (conn.tile.placedMeepsGrass.placedPositionOnTile == Tile.directions.DOWN_RIGHT)
                            {
                                if (foundGrassConnections.Contains(new Grid.connectionsList(conn.tile, Tile.directions.DOWN_RIGHT_RIGHT)) || foundGrassConnections.Contains(new Grid.connectionsList(conn.tile, Tile.directions.DOWN_RIGHT_DOWN)))
                                {
                                    falseMeep = false;
                                }
                            }
                            if (conn.tile.placedMeepsGrass.placedPositionOnTile == Tile.directions.TOP_RIGHT)
                            {
                                if (foundGrassConnections.Contains(new Grid.connectionsList(conn.tile, Tile.directions.TOP_RIGHT_RIGHT)) || foundGrassConnections.Contains(new Grid.connectionsList(conn.tile, Tile.directions.TOP_RIGHT_TOP)))
                                {
                                    falseMeep = false;
                                }
                            }

                            if (conn.tile.placedMeepsGrass.placedPositionOnTile == Tile.directions.TOP_LEFT)
                            {
                                if (foundGrassConnections.Contains(new Grid.connectionsList(conn.tile, Tile.directions.TOP_LEFT_LEFT)) || foundGrassConnections.Contains(new Grid.connectionsList(conn.tile, Tile.directions.TOP_LEFT_TOP)))
                                {
                                    falseMeep = false;
                                }
                            }

                            if (conn.tile.placedMeepsGrass.placedPositionOnTile == Tile.directions.DOWN_LEFT)
                            {
                                if (foundGrassConnections.Contains(new Grid.connectionsList(conn.tile, Tile.directions.DOWN_LEFT_DOWN)) || foundGrassConnections.Contains(new Grid.connectionsList(conn.tile, Tile.directions.DOWN_LEFT_LEFT)))
                                {
                                    falseMeep = false;
                                }
                            }


                            if (!falseMeep)
                            {
                                Debug.Log("Meep contained in conneciton");
                                // DeactivateMeeplePos(CurrPlayer.lastPlacedTile, Tile.directions.ALL);
                                if (side == Tile.directions.CENTER) CurrPlayer.lastPlacedTile.TL.gameObject.active = false;
                                if (side == Tile.directions.TOP) CurrPlayer.lastPlacedTile.TOP.gameObject.active = false;
                                if (side == Tile.directions.DOWN) CurrPlayer.lastPlacedTile.DOWN.gameObject.active = false;
                                if (side == Tile.directions.RIGHT) CurrPlayer.lastPlacedTile.RIGHT.gameObject.active = false;
                                if (side == Tile.directions.LEFT) CurrPlayer.lastPlacedTile.LEFT.gameObject.active = false;
                                if (side == Tile.directions.TOP_LEFT_LEFT || side == Tile.directions.TOP_LEFT_TOP) CurrPlayer.lastPlacedTile.TL.gameObject.active = false;
                                if (side == Tile.directions.DOWN_LEFT_LEFT || side == Tile.directions.DOWN_LEFT_DOWN) CurrPlayer.lastPlacedTile.DL.gameObject.active = false;
                                if (side == Tile.directions.DOWN_RIGHT_DOWN || side == Tile.directions.DOWN_RIGHT_RIGHT) CurrPlayer.lastPlacedTile.DR.gameObject.active = false;
                                if (side == Tile.directions.TOP_RIGHT_RIGHT || side == Tile.directions.TOP_RIGHT_TOP) CurrPlayer.lastPlacedTile.TR.gameObject.active = false;
                                disabledGrass = true;
                            }
                            else
                            {
                                Debug.Log("Meep is in another field");
                            }
                        }
                        foreach (Tile.TileConnections tc in CurrPlayer.tileInHand.grassConnections)
                        {
                            if (tc.list.Contains(conn.direction) && !disabledGrass)
                            {
                                //Activate MeepPositions
                                activateMeeplePostions(side);
                                countr++;
                            }
                        }

                    }
                    if (CurrPlayer.lastPlacedTile.Chapel)
                    {
                        countr++;
                        CurrPlayer.lastPlacedTile.CENTER.gameObject.active = true;
                    }

                    //Activate particle system
                    foreach (Transform g in CurrPlayer.lastPlacedTile.GetComponentsInChildren<Transform>())
                    {
                        if (g.childCount > 0)
                        {
                            continue;
                        }
                        if (!g.gameObject.active) continue;
                        ParticleSystem part = g.gameObject.GetComponent(typeof(ParticleSystem)) as ParticleSystem;
                        var em = part.emission;
                        em.enabled = true;
                    }





                }

                bool doneDeal = false;
                foreach (Transform g in CurrPlayer.lastPlacedTile.GetComponentsInChildren<Transform>())
                {
                    if (g.childCount > 0)
                    {
                        continue;
                    }
                    if (g.gameObject.active)
                    {
                        doneDeal = false;
                    }
                    else
                    {
                        countr = 0;
                        doneDeal = true;
                    }

                }

                if (countr == 0 || doneDeal)
                {
                    currentPhase = GamePhases.ScorePhase;

                }



                CurrPlayer.placedTile = false;

                CurrPlayer.virtualCamera.m_LookAt = CurrPlayer.camerasys.transform;
            }

        }

        else if (currentPhase == GamePhases.ScorePhase)
        {
            //Debug.Log("ScorePhase");
            List<Grid.connectionsList> foundCityConnections = new List<Grid.connectionsList>();
            List<Grid.connectionsList> foundRoadConnections = new List<Grid.connectionsList>();

            foreach (Tile.directions side in CurrPlayer.lastPlacedTile.Sides)
            {
                if (CurrPlayer.lastPlacedTile.getTerrainType(side) == Tile.terrainType.CITY)
                {
                    foundCityConnections = playGrid.findConnections(CurrPlayer.lastPlacedTile, side,
                    CurrPlayer.lastPlacedTile.getTerrainType(side).ToString(), currentPhase);
                }
                if (CurrPlayer.lastPlacedTile.getTerrainType(side) == Tile.terrainType.ROAD)
                {
                    foundRoadConnections = playGrid.findConnections(CurrPlayer.lastPlacedTile, side,
                    CurrPlayer.lastPlacedTile.getTerrainType(side).ToString(), currentPhase);
                }

            }
            #region //city
            if (CurrPlayer.lastPlacedTile.finishedCity)
            {
                Debug.Log("FinishedCity");
                int shieldCount = 0;
                List<Meep> foundMeeples = new List<Meep>();
                if (CurrPlayer.placedMeep)
                {
                    foundMeeples.Add(CurrPlayer.lastPlacedTile.placedMeepsCity);
                }
                foreach (Tile.directions side in CurrPlayer.lastPlacedTile.finishedDirection)
                {
                    List<Tile> uniqueTiles = foundCityConnections.GroupBy(z => z.tile).Select(z => z.Key).ToList();
                    foreach (Tile t in uniqueTiles)
                    {
                        Debug.Log(t.name);
                    }



                    //Loop through each tile.
                    foreach (Tile t in uniqueTiles)
                    {
                        if (t.placedMeepsCity)
                        {
                            foundMeeples.Add(t.placedMeepsCity);
                        }
                        if (t.Shield)
                        {
                            shieldCount++;
                        }
                    }
                    if (foundMeeples.Count == 0)
                    {
                        Debug.Log("Cant find meeples! Breaking!");
                        continue;
                    }
                    //Find out which player owns the city
                    Player OwningPlayer = getOwningPlayer(foundMeeples);


                    int cityScore = (uniqueTiles.Count * 2) + (shieldCount * 2);

                    OwningPlayer.addScore(cityScore);
                    Debug.Log(uniqueTiles.Count + " unique tiles make up this city, and it gives"
                     + OwningPlayer.name + " and it is worth: " + cityScore);
                    //return Meeps to players!
                    foreach (Grid.connectionsList currentConnection in foundCityConnections)
                    {
                        foreach (Player p in playerManager.GetPlayers())
                        {
                            currentConnection.tile.returnMeep(p, CurrPlayer.lastPlacedTile.getTerrainType(side));
                        }

                    }

                }
                foundMeeples.Clear();
            }
            #endregion

            #region  //road 
            if (CurrPlayer.lastPlacedTile.finishedRoad)
            {
                Debug.Log("FinishedRoad");

                List<Meep> foundMeeples = new List<Meep>();
                if (CurrPlayer.placedMeep)
                {
                    foundMeeples.Add(CurrPlayer.lastPlacedTile.placedMeepsRoad);
                }
                foreach (Tile.directions side in CurrPlayer.lastPlacedTile.finishedDirection)
                {
                    List<Tile> uniqueTiles = foundRoadConnections.GroupBy(z => z.tile).Select(z => z.Key).ToList();

                    foreach (Tile t in uniqueTiles)
                    {
                        if (t.placedMeepsRoad)
                        {
                            foundMeeples.Add(t.placedMeepsRoad);
                        }
                    }
                    if (foundMeeples.Count == 0)
                    {
                        // Debug.Log("Cant find meeples! Breaking!");
                        continue;
                    }
                    //Find out which player owns the Road
                    Player OwningPlayer = getOwningPlayer(foundMeeples);
                    int roadScore = uniqueTiles.Count;

                    OwningPlayer.addScore(roadScore);

                    //return Meeps to players!
                    foreach (Grid.connectionsList currentConnection in foundRoadConnections)
                    {
                        foreach (Player p in playerManager.GetPlayers())
                        {
                            currentConnection.tile.returnMeep(p, CurrPlayer.lastPlacedTile.getTerrainType(side));
                        }

                    }
                    foundMeeples.Clear();
                }

            }
            #endregion

            #region //Chapel
            List<Tile> connectedTiles = playGrid.checkForChapelCompletion(CurrPlayer.lastPlacedTile);
            foreach (Tile t in connectedTiles)
            {
                List<Vector2> neighbourCoords = t.getNeighboursWithDiagonals(t.x, t.y);
                bool completed = true;
                foreach (Vector2 vec in neighbourCoords)
                {
                    if (playGrid.tileIsOccupied((int)vec.x, (int)vec.y))
                    {
                        continue;
                    }
                    completed = false;
                }

                if (completed)
                {
                    t.placedMeepleChapel.owner.addScore(9);
                    t.returnMeep(t.placedMeepleChapel.owner, t.getTerrainType(Tile.directions.CENTER));
                }

            }
            #endregion


            CurrPlayer.MyTurn = false;
            currentPhase = GamePhases.TilePhase;
            CurrPlayer.selectedMeeplePos = null;
        }

    }

    public void activateMeeplePostions(Tile.directions inDir)
    {

        if (inDir == Tile.directions.TOP)
        {
            CurrPlayer.lastPlacedTile.TOP.SetActive(true);
        }
        if (inDir == Tile.directions.RIGHT)
        {
            CurrPlayer.lastPlacedTile.RIGHT.SetActive(true);
        }
        if (inDir == Tile.directions.DOWN)
        {
            CurrPlayer.lastPlacedTile.DOWN.SetActive(true);
        }
        if (inDir == Tile.directions.LEFT)
        {
            CurrPlayer.lastPlacedTile.LEFT.SetActive(true);
        }
        if (inDir == Tile.directions.TOP_RIGHT_TOP || inDir == Tile.directions.TOP_RIGHT_RIGHT)
        {
            CurrPlayer.lastPlacedTile.TR.SetActive(true);
        }
        if (inDir == Tile.directions.TOP_LEFT_LEFT || inDir == Tile.directions.TOP_LEFT_TOP)
        {
            CurrPlayer.lastPlacedTile.TL.SetActive(true);
        }
        if (inDir == Tile.directions.DOWN_LEFT_DOWN || inDir == Tile.directions.DOWN_LEFT_LEFT)
        {
            CurrPlayer.lastPlacedTile.DL.SetActive(true);
        }
        if (inDir == Tile.directions.DOWN_RIGHT_DOWN || inDir == Tile.directions.DOWN_RIGHT_RIGHT)
        {
            CurrPlayer.lastPlacedTile.DR.SetActive(true);
        }
        if (inDir == Tile.directions.CENTER)
        {
            CurrPlayer.lastPlacedTile.CENTER.SetActive(true);
        }
    }

    public void DeactivateMeeplePos(Tile inTile, Tile.directions inDir)
    {
        if (!inTile) return;

        if (inDir == Tile.directions.ALL)
        {
            inTile.TOP.SetActive(false);
            inTile.RIGHT.SetActive(false);
            inTile.LEFT.SetActive(false);
            inTile.DOWN.SetActive(false);
            inTile.CENTER.SetActive(false);
            inTile.TL.SetActive(false);
            inTile.TR.SetActive(false);
            inTile.DL.SetActive(false);
            inTile.DR.SetActive(false);

            return;
        }

        if (inDir == Tile.directions.TOP)
        {
            inTile.TOP.SetActive(false);
        }
        if (inDir == Tile.directions.RIGHT)
        {
            inTile.RIGHT.SetActive(false);
        }
        if (inDir == Tile.directions.DOWN)
        {
            inTile.DOWN.SetActive(false);
        }
        if (inDir == Tile.directions.LEFT)
        {
            inTile.LEFT.SetActive(false);
        }
        if (inDir == Tile.directions.CENTER)
        {
            inTile.CENTER.SetActive(false);
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


    public void updateUITexts()
    {
        foreach (Player p in playerManager.GetPlayers())
        {
            p.tilesLeft.text = Tiles.Count.ToString();
            p.MeeplesLeft.text = p.meeples.Count.ToString();
        }

    }

    public void scramblePile()
    {
        if (RiverTiles.Count > 0)
        {
            int lastIndex = RiverTiles.Count - 1;
            for (int i = 1; i < lastIndex; i++)
            {
                int randomIndex = Random.Range(i, lastIndex);
                (RiverTiles[i], RiverTiles[randomIndex]) = (RiverTiles[randomIndex], RiverTiles[i]);
            }
        }
        if (BasicTiles.Count > 0)
        {
            for (int i = 0; i < BasicTiles.Count; i++)
            {
                int randomIndex = Random.Range(i, BasicTiles.Count);
                (BasicTiles[i], BasicTiles[randomIndex]) = (BasicTiles[randomIndex], BasicTiles[i]);
            }
        }
    }

    public void placeMeep()
    {
        if (CurrPlayer.meeples.Count <= 0 || CurrPlayer.placedMeep)
        {
            return;
        }

        Meep placeableMeep = CurrPlayer.meeples[CurrPlayer.meeples.Count - 1];

        if (CurrPlayer.selectedMeeplePos)
        {
            string nameOfPos = CurrPlayer.selectedMeeplePos.name;
            Tile.directions dir = Tile.directions.CENTER;
            //Give directions
            #region
            if (nameOfPos == "north")
            {
                dir = Tile.directions.TOP;
            }
            if (nameOfPos == "east")
            {
                dir = Tile.directions.RIGHT;
            }
            if (nameOfPos == "west")
            {
                dir = Tile.directions.LEFT;
            }
            if (nameOfPos == "south")
            {
                dir = Tile.directions.DOWN;
            }
            if (nameOfPos == "center")
            {
                dir = Tile.directions.CENTER;
            }
            if (nameOfPos == "TL")
            {
                dir = Tile.directions.TOP_LEFT;
            }
            if (nameOfPos == "TR")
            {
                dir = Tile.directions.TOP_RIGHT;
            }
            if (nameOfPos == "DL")
            {
                dir = Tile.directions.DOWN_LEFT;
            }
            if (nameOfPos == "DR")
            {
                dir = Tile.directions.DOWN_RIGHT;
            }
            #endregion


            switch (CurrPlayer.lastPlacedTile.getTerrainType(dir))
            {
                case Tile.terrainType.CITY:
                    CurrPlayer.lastPlacedTile.placedMeepsCity = (placeableMeep);
                    break;
                case Tile.terrainType.ROAD:
                    CurrPlayer.lastPlacedTile.placedMeepsRoad = (placeableMeep);
                    break;
                case Tile.terrainType.CHAPEL:
                    CurrPlayer.lastPlacedTile.placedMeepleChapel = (placeableMeep);
                    break;
                default:
                    CurrPlayer.lastPlacedTile.placedMeepsGrass = (placeableMeep);
                    break;
            }



            placeableMeep.transform.position = CurrPlayer.selectedMeeplePos.transform.position;
            placeableMeep.transform.Translate(new Vector3(0, 0.077f, 0), Space.Self);
            placeableMeep.placedPositionOnTile = dir;


            CurrPlayer.meeples.RemoveAt(CurrPlayer.meeples.Count - 1);
            CurrPlayer.placedMeep = true;
            // CurrPlayer.tileInHand.placedMeepsCities.Add(placeableMeep);
            currentPhase = GamePhases.ScorePhase;

        }



    }


    public void checkRiverTurn(Tile inTile, Tile lastTile)
    {
        if (!lastTile) return;
        //check last places tile to all avaliable tiles
        if (inTile.river && inTile.Turn)
        {


            if (lastTile.Turn)
            {
                //For CurrentTile
                if (inTile.riverConnections[0].list.Contains(Tile.directions.DOWN) && inTile.riverConnections[0].list.Contains(Tile.directions.RIGHT) &&
                    lastTile.riverConnections[0].list.Contains(Tile.directions.LEFT) && lastTile.riverConnections[0].list.Contains(Tile.directions.DOWN))
                {
                    playGrid.avaliableGrids.Clear();
                }
                if (inTile.riverConnections[0].list.Contains(Tile.directions.DOWN) && inTile.riverConnections[0].list.Contains(Tile.directions.RIGHT) &&
                  lastTile.riverConnections[0].list.Contains(Tile.directions.TOP) && lastTile.riverConnections[0].list.Contains(Tile.directions.RIGHT))
                {
                    playGrid.avaliableGrids.Clear();
                }
                if (inTile.riverConnections[0].list.Contains(Tile.directions.LEFT) && inTile.riverConnections[0].list.Contains(Tile.directions.DOWN) &&
                 lastTile.riverConnections[0].list.Contains(Tile.directions.TOP) && lastTile.riverConnections[0].list.Contains(Tile.directions.LEFT))
                {
                    playGrid.avaliableGrids.Clear();
                }
                if (inTile.riverConnections[0].list.Contains(Tile.directions.TOP) && inTile.riverConnections[0].list.Contains(Tile.directions.RIGHT) &&
                 lastTile.riverConnections[0].list.Contains(Tile.directions.TOP) && lastTile.riverConnections[0].list.Contains(Tile.directions.LEFT))
                {
                    playGrid.avaliableGrids.Clear();
                }

                //For lastTile
                if (lastTile.riverConnections[0].list.Contains(Tile.directions.DOWN) && lastTile.riverConnections[0].list.Contains(Tile.directions.RIGHT) &&
              inTile.riverConnections[0].list.Contains(Tile.directions.LEFT) && inTile.riverConnections[0].list.Contains(Tile.directions.DOWN))
                {
                    playGrid.avaliableGrids.Clear();
                }
                if (lastTile.riverConnections[0].list.Contains(Tile.directions.DOWN) && lastTile.riverConnections[0].list.Contains(Tile.directions.RIGHT) &&
                  inTile.riverConnections[0].list.Contains(Tile.directions.TOP) && inTile.riverConnections[0].list.Contains(Tile.directions.RIGHT))
                {
                    playGrid.avaliableGrids.Clear();
                }
                if (lastTile.riverConnections[0].list.Contains(Tile.directions.LEFT) && lastTile.riverConnections[0].list.Contains(Tile.directions.DOWN) &&
                 inTile.riverConnections[0].list.Contains(Tile.directions.TOP) && inTile.riverConnections[0].list.Contains(Tile.directions.LEFT))
                {
                    playGrid.avaliableGrids.Clear();
                }
                if (lastTile.riverConnections[0].list.Contains(Tile.directions.TOP) && lastTile.riverConnections[0].list.Contains(Tile.directions.RIGHT) &&
                 inTile.riverConnections[0].list.Contains(Tile.directions.TOP) && inTile.riverConnections[0].list.Contains(Tile.directions.LEFT))
                {
                    playGrid.avaliableGrids.Clear();
                }
            }



        }

    }


    public Player getOwningPlayer(List<Meep> listOMeeps)
    {
        Player[] playersInGame = playerManager.GetPlayers();
        int[] playerMeepcCounts = new int[playersInGame.Length];

        if (playersInGame.Length == 1)
        {
            return playerManager.getCurrentPlayer();
        }

        int id = 0;
        foreach (Player p in playersInGame)
        {
            foreach (Meep m in listOMeeps)
            {
                if (m.owner == p)
                {
                    playerMeepcCounts[id]++;
                }
            }

            id++;
        }

        id = 0;

        int maxNumber = int.MinValue;
        int maxIndex = -1;

        for (int i = 0; i < playerMeepcCounts.Length; i++)
        {
            if (playerMeepcCounts[i] > maxNumber)
            {
                maxNumber = playerMeepcCounts[i];
                maxIndex = i;
            }
        }

        if (maxIndex < 0) return null;

        return playersInGame[maxIndex];


    }


}
