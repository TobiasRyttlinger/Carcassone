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
            GameObject objToSpawn = new GameObject("Center");
            t.PossibleMeepPos.Add(objToSpawn.transform);
            foreach (Transform g in t.GetComponentsInChildren<Transform>())
            {
                if (g.childCount > 0)
                {
                    continue;
                }
                t.PossibleMeepPos.Add(g);
                Debug.Log(g.name);

                g.gameObject.AddComponent<ParticleSystem>();
                ParticleSystem part = g.gameObject.GetComponent(typeof(ParticleSystem)) as ParticleSystem;
                g.gameObject.AddComponent<ParticleSystemRenderer>();
                var pRenderer = g.gameObject.GetComponent(typeof(ParticleSystemRenderer)) as ParticleSystemRenderer;
                pRenderer.material = PsMat;
                //initialise selectionRings;
                var main = part.main;
                main.startColor = Color.yellow;
                main.startSize = 0.02f;
                main.startLifetime = 1.15f;
                main.startSpeed = 0.0f;


                var em = part.emission;
                em.rateOverTime = 500f;

                var sh = part.shape;
                sh.shapeType = ParticleSystemShapeType.Circle;
                sh.radius = 0.12f;
                sh.arc = 360f;
                sh.radiusThickness = 0.0f;
                sh.arcMode = ParticleSystemShapeMultiModeValue.Loop;

                em.enabled = false;
                g.gameObject.SetActive(true);

                var tile = g.gameObject.GetComponentInParent(typeof(Tile)) as Tile;
                if (g.name == "north") //TOP
                {
                    g.transform.Translate(new Vector3(-0.3f, 0.1f, 0), Space.Self);
                    tile.TOP = g.gameObject;
                    tile.TopOrgPos = g.position;
                }
                if (g.name == "west") // LEFT
                {
                    g.transform.Translate(new Vector3(0, 0.1f, -0.3f), Space.Self);
                    tile.LEFT = g.gameObject;
                    tile.LeftOrgPos = g.position;
                }
                if (g.name == "east") //RIGHT
                {
                    g.transform.Translate(new Vector3(0, 0.1f, 0.3f), Space.Self);
                    tile.RIGHT = g.gameObject;
                    tile.RightOrgPos = g.position;

                }
                if (g.name == "south") //DOWN
                {
                    g.transform.Translate(new Vector3(0.3f, 0.1f, 0), Space.Self);
                    tile.DOWN = g.gameObject;
                    tile.DownOrgPos = g.position;
                }
                g.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            }

        }

        //initialise first tile
        if (placedTiles == 0)
        {
            playGrid.placeTile((int)playGrid.avaliableGrids[0].x, (int)playGrid.avaliableGrids[0].y, Tiles[0]);

            playGrid.generateAvaliableSpots(Tiles[0]);

            Tiles.Remove(Tiles[0]);
        }
        currentPhase = GamePhases.TilePhase;

    }

    // Update is called once per frame
    [System.Obsolete]
    void Update()
    {
        CurrPlayer = playerManager.getCurrentPlayer();
        //Update UI Text
        updateUITexts();

        //TilePhase
        if (currentPhase == GamePhases.TilePhase)
        {
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
                        {
                            p.PlaceButton.interactable = false;
                        }

                    }
                }

            }
        }

        //MeepPhase
        else if (currentPhase == GamePhases.MeepPhase)
        {
            // CurrPlayer.PlaceButton.enabled = false;
            if (!CurrPlayer.placedMeep && CurrPlayer.placedTile && CurrPlayer.meeples.Count > 0)
            {





                Debug.Log("Meepphase");
                foreach (Tile.directions side in CurrPlayer.lastPlacedTile.Sides)
                {
                    List<Grid.connectionsList> temp = playGrid.find_City(CurrPlayer.lastPlacedTile, side);

                    CurrPlayer.lastPlacedTile.TOP.gameObject.active = false;
                    CurrPlayer.lastPlacedTile.DOWN.gameObject.active = false;
                    CurrPlayer.lastPlacedTile.RIGHT.gameObject.active = false;
                    CurrPlayer.lastPlacedTile.LEFT.gameObject.active = false;



                    bool canPlaceMeepCity = true;
                    bool canPlaceMeepChapel = true;
                    bool canPlaceMeepRoad = true;
                    bool canPlaceMeepField = true;

                    //Loop through connections to check if any meeples exists in the chain
                    foreach (Grid.connectionsList conn in temp)
                    {
                        //TileChain contains meep already! Cant place a new one
                        if (conn.tile.placedMeepleCity)
                        {
                            canPlaceMeepCity = false;
                            break;
                        }

                        foreach (Transform g in CurrPlayer.lastPlacedTile.GetComponentsInChildren<Transform>())
                        {
                            if (g.childCount > 0)
                            {
                                continue;
                            }
                            //                            g.gameObject.active = false;
                        }


                        //Activate MeepPositions
                        activateMeeplePostions(conn.direction);
                    }

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

                    if (canPlaceMeepCity)
                    {
                        // placeMeep();
                        // CurrPlayer.virtualCamera.m_LookAt = CurrPlayer.lastPlacedTile.transform;

                        //CurrPlayer.placedMeep = true;


                    }

                    temp.Clear();
                }

                CurrPlayer.placedTile = false;

                //CurrPlayer.virtualCamera.m_LookAt = CurrPlayer.camerasys.transform;
            }
            currentPhase = GamePhases.ScorePhase;
        }

        else if (currentPhase == GamePhases.ScorePhase)
        {
            currentPhase = GamePhases.TilePhase;
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

    public void placeMeep()
    {
        if (CurrPlayer.meeples.Count <= 0)
        {
            return;
        }
        Meep placeableMeep = CurrPlayer.meeples[CurrPlayer.meeples.Count - 1];
        CurrPlayer.lastPlacedTile.placedMeepleCity = placeableMeep;


        placeableMeep.transform.Translate(CurrPlayer.lastPlacedTile.transform.position, Space.Self);

        CurrPlayer.meeples.RemoveAt(CurrPlayer.meeples.Count - 1);

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


}
