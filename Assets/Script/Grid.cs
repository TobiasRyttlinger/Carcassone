using System.Collections;
using System.Collections.Generic;
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



    public List<connectionsList> find_City(Tile inTile, Tile.directions side)
    {

        List<connectionsList> connected = find_connected_sides(inTile, side);
        List<connectionsList> unexplored = find_adjacent_sides(connected);
        List<connectionsList> ignored = new List<connectionsList>();

        while (unexplored.Count != 0)
        {
            Debug.Log(unexplored.Count);
            connectionsList tileToExplore = unexplored[unexplored.Count - 1];
            unexplored.RemoveAt(unexplored.Count - 1);

            Tile new_tile = gridArr[tileToExplore.tile.x, tileToExplore.tile.y];

            if (new_tile == null)
            {
                ignored.Add(tileToExplore);
                continue;
            }


            List<connectionsList> new_connected = find_connected_sides(tileToExplore.tile, tileToExplore.direction);

            foreach (connectionsList conn in new_connected)
            {
                connectionsList newTile = conn;
                if (!connected.Contains(newTile))
                {
                    connected.Add(newTile);
                }
            }
            List<connectionsList> new_unexpored = find_adjacent_sides(new_connected);
            foreach (connectionsList new_To_Explore in new_unexpored)
            {
                if (!connected.Contains(new_To_Explore))
                {

                    unexplored.Add(new_To_Explore);
                }

            }

        }



        return connected;


    }


    public List<connectionsList> find_connected_sides(Tile intile, Tile.directions side)
    {
        List<connectionsList> newConnections = new List<connectionsList>();

        foreach (Tile.TileConnections connectedtile in intile.cityConnections)
        {
            foreach (Tile.directions connectedside in connectedtile.list)
            {

                newConnections.Add(new connectionsList(intile, connectedside));

            }

        }

        return newConnections;
    }

    public List<connectionsList> find_adjacent_sides(List<connectionsList> connections)
    {
        List<connectionsList> adjacentTiles = new List<connectionsList>();
        foreach (connectionsList con in connections)
        {
            Tile.directions dir = con.direction;
            Tile inTile = con.tile;
            Tile.directions adjacent_direction = adjacent(dir);
            if (adjacent_direction == Tile.directions.DOWN)
            {
                if (!tileIsOccupied(inTile.x, inTile.y + 1)) continue;
                adjacentTiles.Add(new connectionsList(gridArr[inTile.x, inTile.y + 1], Tile.directions.DOWN));
            }
            if (adjacent_direction == Tile.directions.TOP)
            {
                if (!tileIsOccupied(inTile.x, inTile.y - 1)) continue;
                adjacentTiles.Add(new connectionsList(gridArr[inTile.x, inTile.y - 1], Tile.directions.TOP));
            }
            if (adjacent_direction == Tile.directions.LEFT)
            {
                if (!tileIsOccupied(inTile.x + 1, inTile.y)) continue;
                adjacentTiles.Add(new connectionsList(gridArr[inTile.x + 1, inTile.y], Tile.directions.LEFT));
            }
            if (adjacent_direction == Tile.directions.RIGHT)
            {
                if (!tileIsOccupied(inTile.x - 1, inTile.y)) continue;
                adjacentTiles.Add(new connectionsList(gridArr[inTile.x - 1, inTile.y], Tile.directions.RIGHT));
            }

        }

        return adjacentTiles;
    }

    public Tile.directions adjacent(Tile.directions side)
    {
        if (side == Tile.directions.TOP)
        {
            return Tile.directions.DOWN;
        }
        else if (side == Tile.directions.DOWN)
        {
            return Tile.directions.TOP;
        }
        else if (side == Tile.directions.LEFT)
        {
            return Tile.directions.RIGHT;
        }
        else
        {
            return Tile.directions.LEFT;
        }


    }


}
