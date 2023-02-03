using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Grid
{

    public class connectionsList
    {
        public connectionsList(Tile inTile, Tile.directions indir)
        {
            this.tile = inTile;
            this.direction = indir;
        }
        public Tile tile;
        public Tile.directions direction;
        public bool finished = false;
        public Meep containsMeep = null;

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;
            connectionsList c = obj as connectionsList;
            if ((System.Object)c == null)
                return false;
            return (tile == c.tile && direction == c.direction);
        }
        public bool Equals(connectionsList c)
        {
            if ((object)c == null)
                return false;
            return (tile == c.tile && direction == c.direction);
        }


    }

    private int maxX = 100;
    private int maxY = 100;
    public Tile[,] gridArr;
    public List<Vector2> avaliableGrids;
    public List<Vector2> placedGrids;

    public Tile.directions[,] lastTurn;

    public Grid(int maxX, int maxY)
    {


        this.maxX = maxX;
        this.maxY = maxY;
        gridArr = new Tile[maxX, maxY];

        avaliableGrids = new List<Vector2>();
        placedGrids = new List<Vector2>();
        avaliableGrids.Add(new Vector2(50 / 2, 50 / 2));



    }
    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, 0, y);
    }

    public void placeTile(int x, int y, Tile inTile)
    {
        gridArr[x, y] = inTile;
        // Debug.Log("PlaceTile:" + x + ", " + y);
        //update tiles position and place it in the correct cell.
        inTile.SetCoordinates(x, y);
        placedGrids.Add(new Vector2(x, y));
        avaliableGrids.Remove(new Vector2(x, y));
    }
    public void generateAvaliableSpots(Tile inTile)
    {

        //River
        //Loop through all placed tiles!
        foreach (Vector2 placedCoords in placedGrids)
        {

            List<Vector2> neighbours = gridArr[(int)placedCoords.x, (int)placedCoords.y].getNeighbours((int)placedCoords.x, (int)placedCoords.y);
            foreach (Vector2 coords in neighbours)
            {
                if (tileIsOccupied((int)coords.x, (int)coords.y))
                {
                    continue;
                }

                Tile top = gridArr[(int)coords.x, (int)coords.y + 1];
                Tile down = gridArr[(int)coords.x, (int)coords.y - 1];
                Tile left = gridArr[(int)coords.x - 1, (int)coords.y];
                Tile right = gridArr[(int)coords.x + 1, (int)coords.y];

                bool isFine = true;

                if (top == null && down == null && left == null && right == null || inTile.Sides.Count == 0) continue;
                //Check each side of the tile in hand.
                foreach (var side in inTile.Sides)
                {
                    Tile.terrainType tp = inTile.getTerrainType(side);


                    if (side == Tile.directions.TOP)
                    {
                        if (isFine && (top == null || tp == top.getTerrainType(Tile.directions.DOWN)))
                        {
                            if (top != null)
                            {
                                if (inTile.river && top.getTerrainType(Tile.directions.DOWN) != Tile.terrainType.RIVER)
                                {
                                    isFine = false;
                                    break;
                                }
                            }
                            isFine = true;
                        }
                        else
                        {
                            isFine = false;
                        }
                    }

                    if (side == Tile.directions.DOWN)
                    {
                        if (isFine && (down == null || tp == down.getTerrainType(Tile.directions.TOP)))
                        {
                            if (down != null)
                            {
                                if (inTile.river && down.getTerrainType(Tile.directions.TOP) != Tile.terrainType.RIVER)
                                {
                                    isFine = false;
                                    break;
                                }
                            }
                            isFine = true;
                        }
                        else
                        {
                            isFine = false;
                        }
                    }
                    if (side == Tile.directions.RIGHT)
                    {
                        if (isFine && (right == null || tp == right.getTerrainType(Tile.directions.LEFT)))
                        {
                            if (right != null)
                            {
                                if (inTile.river && right.getTerrainType(Tile.directions.LEFT) != Tile.terrainType.RIVER)
                                {
                                    isFine = false;
                                    break;
                                }
                            }
                            isFine = true;
                        }
                        else
                        {
                            isFine = false;
                        }
                    }
                    if (side == Tile.directions.LEFT)
                    {
                        if (isFine && (left == null || tp == left.getTerrainType(Tile.directions.RIGHT)))
                        {
                            if (left != null)
                            {
                                if (inTile.river && left.getTerrainType(Tile.directions.RIGHT) != Tile.terrainType.RIVER)
                                {
                                    isFine = false;
                                    break;
                                }
                            }
                            isFine = true;
                        }
                        else
                        {
                            isFine = false;
                        }
                    }
                }

                //if all is fine then add the coords to avaliablePositions
                if (isFine)
                {
                    avaliableGrids.Add(new Vector2(coords.x, coords.y));
                }

            }

        }

    }
    public bool tileIsOccupied(int x, int y)
    {
        if (x < 0 || y < 0) return true;
        if (gridArr[x, y])
        {
            return true;
        }
        return false;
    }



    public List<connectionsList> findConnections(Tile inTile, Tile.directions side, string Type, GameManager.GamePhases inPhase)
    {
        List<connectionsList> connected = find_connected_sides(inTile, side, Type);
        Stack<connectionsList> open_Edges = new Stack<connectionsList>(find_adjacent_sides(connected));
        List<connectionsList> explored = union(connected, open_Edges);

        while (open_Edges.Count > 0)
        {
            connectionsList tileToExplore = open_Edges.Pop();

            List<connectionsList> new_connected = find_connected_sides(tileToExplore.tile, tileToExplore.direction, Type);

            //Union
            connected = union(connected, new_connected);

            Stack<connectionsList> new_open_Edges = new Stack<connectionsList>(find_adjacent_sides(new_connected));

            explored = union(explored, new_connected);
            foreach (connectionsList new_To_Explore in new_open_Edges)
            {
                if (!(explored.Contains(new_To_Explore)))
                {
                    open_Edges.Push(new_To_Explore);
                    explored.Add(new_To_Explore);
                }
            }
        }

        //  Debug.Log("For " + Type + " " + side + ", " + connected.Count + " connected sides");
        //  Debug.Log("For " + Type + " " + side + ", " + explored.Count + " explored sides");

        bool connectionsAreConnected = true;
        if (inPhase == GameManager.GamePhases.ScorePhase)
        {
            foreach (Grid.connectionsList g in connected)
            {
                if (g.finished == false)
                {
                    connectionsAreConnected = false;
                    break;
                }
            }
            if (connectionsAreConnected)
            {
                switch (inTile.getTerrainType(side).ToString())
                {
                    case "CITY":
                        inTile.finishedCity = true;
                        break;
                    case "ROAD":
                        inTile.finishedRoad = true;
                        break;
                    case "GRASS":
                        inTile.finishedRoad = true;
                        break;
                    default:
                        // code block
                        break;
                }
                Debug.Log("intileFinished something");
                inTile.finishedDirection.Add(side);
            }

        }

        return connected;
    }


    public List<connectionsList> find_connected_sides(Tile intile, Tile.directions side, string Type)
    {
        List<connectionsList> newConnections = new List<connectionsList>();
        List<Tile.TileConnections> typeOfConnection =
            Type == "CITY" ? intile.cityConnections :
            Type == "ROAD" ? intile.roadConnections : intile.grassConnections;

        foreach (Tile.TileConnections connectedtile in typeOfConnection)
        {
            //Failsafe in case side does not match type.
            if (connectedtile.list.Contains(side))
            {
                foreach (Tile.directions connectedside in connectedtile.list)
                {
                    newConnections.Add(new connectionsList(intile, connectedside));
                }
            }
        }

        return newConnections;
    }

    public List<connectionsList> find_adjacent_sides(List<connectionsList> connections)
    {
        List<connectionsList> adjacentTiles = new List<connectionsList>();
        bool finished = true;

        foreach (connectionsList con in connections)
        {
            if (con.direction == Tile.directions.CENTER)
            {
                con.finished = true;
                continue;
            }
            Tile.directions dir = con.direction;
            Tile inTile = con.tile;
            // Debug.Log("Finding adjacent sides for " + inTile.name + " in direction " + dir);
            Tile.directions adjacent_direction = adjacent(dir);
            int x = inTile.x, y = inTile.y;
            if (x == 0 && y == 0)
            {
                x = y = 25;

            }
            if (adjacent_direction == Tile.directions.DOWN)
            {
                if (!tileIsOccupied(x, y + 1))
                {
                    finished = false;
                    continue;
                }
                adjacentTiles.Add(new connectionsList(gridArr[x, y + 1], Tile.directions.DOWN));
                con.finished = true;
            }
            if (adjacent_direction == Tile.directions.TOP)
            {
                if (!tileIsOccupied(x, y - 1))
                {
                    finished = false;
                    continue;
                }
                adjacentTiles.Add(new connectionsList(gridArr[x, y - 1], Tile.directions.TOP));
                con.finished = true;
            }
            if (adjacent_direction == Tile.directions.LEFT)
            {
                if (!tileIsOccupied(x + 1, y))
                {
                    finished = false;
                    continue;
                }
                adjacentTiles.Add(new connectionsList(gridArr[x + 1, y], Tile.directions.LEFT));
                con.finished = true;
            }
            if (adjacent_direction == Tile.directions.RIGHT)
            {
                if (!tileIsOccupied(x - 1, y))
                {
                    finished = false;
                    continue;
                }
                adjacentTiles.Add(new connectionsList(gridArr[x - 1, y], Tile.directions.RIGHT));
                con.finished = true;
            }

        }
        foreach (connectionsList connection in adjacentTiles)
        {
            connection.finished = finished;

        }
        return adjacentTiles;
    }

    public Tile.directions adjacent(Tile.directions side)
    {
        switch (side)
        {
            case Tile.directions.TOP:
                return Tile.directions.DOWN;
            case Tile.directions.DOWN:
                return Tile.directions.TOP;
            case Tile.directions.LEFT:
                return Tile.directions.RIGHT;
            default:
                return Tile.directions.LEFT;
        }
    }


    public List<connectionsList> union(List<connectionsList> a, List<connectionsList> b)
    {
        foreach (connectionsList conn in b)
        {
            connectionsList newTile = conn;

            if (!a.Contains(newTile))
            {
                a.Add(newTile);

            }

        }
        return a;
    }

    public List<connectionsList> union(List<connectionsList> a, Stack<connectionsList> b)
    {
        foreach (connectionsList conn in b)
        {
            connectionsList newTile = conn;

            if (!a.Contains(newTile))
            {
                a.Add(newTile);

            }

        }
        return a;
    }



    public List<Tile> checkForChapelCompletion(Tile placedTile)
    {
        List<Tile> completedChapels = new List<Tile>();
        List<Vector2> allNeighbours = placedTile.getNeighboursWithDiagonals((int)placedTile.x, (int)placedTile.y);
        allNeighbours.Add(new Vector2(placedTile.x,placedTile.y));
        foreach (Vector2 cord in allNeighbours)
        {
            if (!tileIsOccupied((int)cord.x, (int)cord.y))
            {
                continue;
            }
            Tile testTile = gridArr[(int)cord.x, (int)cord.y];
            if (testTile.Chapel && testTile.placedMeepleChapel)
            {
                List<Vector2> chapelNeighbours = testTile.getNeighboursWithDiagonals(testTile.x,testTile.y);
                if(chapelNeighbours.Count == 8){
                    completedChapels.Add(testTile);
                }
            }
        }
        return completedChapels;
    }
}
