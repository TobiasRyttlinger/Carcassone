using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    public Meep placedMeepleCity;
    public Meep placedMeepleRoad;
    public Meep placedMeepleChapel;
    public Meep placedMeepleGrass;

    public List<Transform> PossibleMeepPos;
    public GameObject TOP;

    public GameObject RIGHT;
    public GameObject DOWN;
    public GameObject LEFT;

    public Vector3 TopOrgPos;
    public Vector3 RightOrgPos;
    public Vector3 LeftOrgPos;
    public Vector3 DownOrgPos;
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
        CENTER
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

        //Add sides
        Sides.Add(directions.TOP);
        Sides.Add(directions.DOWN);
        Sides.Add(directions.LEFT);
        Sides.Add(directions.RIGHT);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetCoordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
        Debug.Log(x + ", " + y);
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



}
