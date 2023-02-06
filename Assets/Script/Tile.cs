using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    public Meep placedMeepsCity;
    public Meep placedMeepsRoad;
    public Meep placedMeepleChapel;
    public Meep placedMeepsGrass;

    public List<Transform> PossibleMeepPos;
    public GameObject TOP;

    public GameObject RIGHT;
    public GameObject DOWN;
    public GameObject LEFT;
    public GameObject CENTER;
    public bool finishedCity = false;
    public bool finishedRoad = false;
    public List<directions> finishedDirection;
    public Vector3 TopOrgPos;
    public Vector3 RightOrgPos;
    public Vector3 LeftOrgPos;
    public Vector3 DownOrgPos;
    public Vector3 CenterOrgPos;
    public int x;
    public int y;
    public bool placeable;
    public bool Shield = false;
    public bool Chapel = false;
    public bool Flowers = false;

    //only used with rivertiles
    public bool Turn = false;

    public bool TileIsRotated = false;
    public int numOfRotations = 0;
    public bool Inn = false;

    public bool river = false;

    public bool Cathedral = false;
    public List<Tile> neighbours;
    // Start is called before the first frame update
    public enum terrainType
    {
        GRASS,
        CITY,
        ROAD,
        CHAPEL,
        RIVER,
        FLOWERS,
        UNPLAYABLE

    }

    public enum directions
    {
        TOP,
        LEFT,
        RIGHT,
        DOWN,
        TOP_LEFT,
        TOP_RIGHT,
        DOWN_LEFT,
        DOWN_RIGHT,
        TOP_LEFT_LEFT,
        TOP_LEFT_TOP,
        TOP_RIGHT_TOP,
        TOP_RIGHT_RIGHT,
        DOWN_LEFT_LEFT,
        DOWN_LEFT_DOWN,
        DOWN_RIGHT_RIGHT,
        DOWN_RIGHT_DOWN,
        CENTER,

        ALL
    }

    [System.Serializable]
    public class TileConnections
    {
        public List<directions> list;
    }
    public List<directions> Sides;
    public List<TileConnections> cityConnections;
    public List<TileConnections> riverConnections;
    public List<TileConnections> roadConnections;

    public List<TileConnections> grassConnections;

    void Start()
    {
        x = 0;
        y = 0;
        placeable = false;
        neighbours = new List<Tile>();
        finishedDirection = new List<directions>();
        //Add sides
        Sides.Add(directions.TOP);
        Sides.Add(directions.DOWN);
        Sides.Add(directions.LEFT);
        Sides.Add(directions.RIGHT);
        Sides.Add(directions.CENTER);
        //Grass connections
        Sides.Add(directions.TOP_LEFT);
        Sides.Add(directions.TOP_RIGHT);
        Sides.Add(directions.DOWN_LEFT);
        Sides.Add(directions.DOWN_RIGHT);
        

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetCoordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
        // Debug.Log(x + ", " + y);
        transform.position = new Vector3(x + 0.5f, 0, y + 0.5f);

    }

    public void swapSides(List<TileConnections> inList)
    {
        foreach (TileConnections Tlist in inList)
        {
            for (int i = 0; i < Tlist.list.Count; i++)
            {
                if (Tlist.list[i] == directions.TOP)
                {
                    Tlist.list[i] = directions.RIGHT;
                    continue;
                }
                if (Tlist.list[i] == directions.RIGHT)
                {
                    Tlist.list[i] = directions.DOWN;
                    continue;
                }
                if (Tlist.list[i] == directions.LEFT)
                {
                    Tlist.list[i] = directions.TOP;
                    continue;
                }
                if (Tlist.list[i] == directions.DOWN)
                {
                    Tlist.list[i] = directions.LEFT;
                    continue;
                }
                if (Tlist.list[i] == directions.TOP_LEFT_TOP)
                {
                    Tlist.list[i] = directions.TOP_RIGHT_RIGHT;
                    continue;
                }
                if (Tlist.list[i] == directions.TOP_RIGHT_TOP)
                {
                    Tlist.list[i] = directions.DOWN_RIGHT_RIGHT;
                    continue;
                }
                if (Tlist.list[i] == directions.TOP_RIGHT_RIGHT)
                {
                    Tlist.list[i] = directions.DOWN_RIGHT_DOWN;
                    continue;
                }
                if (Tlist.list[i] == directions.DOWN_RIGHT_RIGHT)
                {
                    Tlist.list[i] = directions.DOWN_LEFT_DOWN;
                    continue;
                }

                if (Tlist.list[i] == directions.DOWN_RIGHT_DOWN)
                {
                    Tlist.list[i] = directions.DOWN_LEFT_LEFT;
                    continue;
                }
                if (Tlist.list[i] == directions.DOWN_LEFT_DOWN)
                {
                    Tlist.list[i] = directions.TOP_LEFT_LEFT;
                    continue;
                }
                if (Tlist.list[i] == directions.DOWN_LEFT_LEFT)
                {
                    Tlist.list[i] = directions.TOP_LEFT_TOP;
                    continue;
                }
                if (Tlist.list[i] == directions.TOP_LEFT_LEFT)
                {
                    Tlist.list[i] = directions.TOP_RIGHT_TOP;
                    continue;
                }
            }
        }
    }

    public void swapChildSides(Tile tileInHand)
    {

        foreach (Transform g in tileInHand.GetComponentsInChildren<Transform>())
        {

            if (g.childCount > 0)
            {
                continue;
            }

            if (g.name == "north") //is now on the right!
            {
                g.position = tileInHand.TopOrgPos;
            }
            if (g.name == "west") // //is now on the top!
            {
                g.position = tileInHand.LeftOrgPos;
            }
            if (g.name == "east") //is now down!
            {
                g.position = tileInHand.RightOrgPos;
            }
            if (g.name == "south") //is now left
            {
                g.position = tileInHand.DownOrgPos;
            }
            if (g.name == "center") //is now center
            {
                g.position = tileInHand.CenterOrgPos;
            }



        }

    }



    public terrainType getTerrainType(directions indir)
    {
        //River
        foreach (TileConnections connections in riverConnections)
        {
            if (connections.list.Contains(indir))
            {
                return terrainType.RIVER;
            }
        }

        if (indir == directions.CENTER && Chapel)
        {
            return terrainType.CHAPEL;
        }
        if (indir == directions.CENTER && Flowers)
        {
            return terrainType.FLOWERS;
        }
        //ROAD
        foreach (TileConnections connections in roadConnections)
        {
            if (connections.list.Contains(indir))
            {
                return terrainType.ROAD;
            }
        }
        //City
        foreach (TileConnections connections in cityConnections)
        {
            if (connections.list.Contains(indir))
            {
                return terrainType.CITY;
            }
        }
        //GRASS
        foreach (TileConnections connections in grassConnections)
        {
            if (connections.list.Contains(indir))
            {
                return terrainType.GRASS;
            }
        }


        return terrainType.UNPLAYABLE;
    }


    public List<Vector2> getNeighbours(int x, int y)
    {
        List<Vector2> neighbourIndices = new List<Vector2>();

        //Left
        neighbourIndices.Add(new Vector2(x - 1, y));
        //Right
        neighbourIndices.Add(new Vector2(x + 1, y));
        //Down
        neighbourIndices.Add(new Vector2(x, y - 1));
        //Top
        neighbourIndices.Add(new Vector2(x, y + 1));

        return neighbourIndices;

    }


    public List<Vector2> getNeighboursWithDiagonals(int x, int y)
    {
        List<Vector2> neighbourIndices = new List<Vector2>();

        //Left
        neighbourIndices.Add(new Vector2(x - 1, y));
        //Right
        neighbourIndices.Add(new Vector2(x + 1, y));
        //Down
        neighbourIndices.Add(new Vector2(x, y - 1));
        //Top
        neighbourIndices.Add(new Vector2(x, y + 1));

        //Top Left
        neighbourIndices.Add(new Vector2(x - 1, y + 1));

        //Top right
        neighbourIndices.Add(new Vector2(x + 1, y + 1));

        //Down Left
        neighbourIndices.Add(new Vector2(x - 1, y - 1));

        //Down right
        neighbourIndices.Add(new Vector2(x + 1, y - 1));


        return neighbourIndices;

    }

    public void returnMeep(Player owningPlayer, Tile.terrainType tType)
    {
        if (placedMeepsCity && tType == Tile.terrainType.CITY)
        {
            if (placedMeepsCity.owner == owningPlayer)
            {
                placedMeepsCity.transform.position = new Vector3(1000, 0, 0);
                owningPlayer.meeples.Add(placedMeepsCity);
                placedMeepsCity = null;
            }
        }
        if (placedMeepsRoad && tType == Tile.terrainType.ROAD)
        {
            if (placedMeepsRoad.owner == owningPlayer)
            {
                placedMeepsRoad.transform.position = new Vector3(1000, 0, 0);
                owningPlayer.meeples.Add(placedMeepsRoad);
                placedMeepsRoad = null;
            }
        }
        if (placedMeepleChapel && tType == Tile.terrainType.CHAPEL)
        {
            if (placedMeepleChapel.owner = owningPlayer)
            {
                placedMeepleChapel.transform.position = new Vector3(1000, 0, 0);
                owningPlayer.meeples.Add(placedMeepleChapel);
                placedMeepleChapel = null;
            }
        }



    }


    public void initialise(Tile t, Material PsMat)
    {
        GameObject objToSpawn = new GameObject("center");

        objToSpawn.transform.parent = t.transform;
        objToSpawn.transform.localScale = new Vector3(1, 1, 1);

        foreach (Transform g in t.GetComponentsInChildren<Transform>())
        {
            if (g.childCount > 0)
            {
                int LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
                g.gameObject.layer = LayerIgnoreRaycast;
                Destroy(g.gameObject.GetComponent<BoxCollider>());
                continue;
            }
            t.PossibleMeepPos.Add(g);
            //Debug.Log(g.name);
            if (!g.gameObject.GetComponent(typeof(ParticleSystem)))
            {
                g.gameObject.AddComponent<ParticleSystem>();
            }

            ParticleSystem part = g.gameObject.GetComponent(typeof(ParticleSystem)) as ParticleSystem;

            if (!g.gameObject.GetComponent(typeof(ParticleSystemRenderer)))
            {
                g.gameObject.AddComponent<ParticleSystemRenderer>();
            }
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

            g.gameObject.AddComponent<BoxCollider>();
            var BC = g.gameObject.GetComponent(typeof(BoxCollider)) as BoxCollider;


            BC.size = new Vector3(0.15f, 0.15f, 3.0f);
            BC.center = Vector3.zero;


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
            if (g.name == "center")
            {
                g.transform.position = (tile.transform.position + new Vector3(0, 0.1f, 0));
                tile.CENTER = g.gameObject;
                tile.CenterOrgPos = g.position;
            }
            g.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));


        }
    }


}
