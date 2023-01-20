using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{


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
        avaliableGrids.Add(new Vector2(30 / 2, 30 / 2));



    }
    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, 0, y);
    }

    public void placeTile(int x, int y, Tile inTile)
    {
        gridArr[x, y] = inTile;
        Debug.Log("PlaceTile:" + x + ", " + y);
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

            List<Vector2> neighbours = gridArr[(int)placedCoords.x, (int)placedCoords.y].getNeighbours((int)placedCoords.x,(int)placedCoords.y);
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
        if(x < 0 || y < 0) return true;
        if (gridArr[x, y])
        {
            return true;
        }
        return false;
    }
}
