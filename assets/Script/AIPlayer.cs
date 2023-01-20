using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    public bool MyTurn = false;
    public bool HasTile = false;
    public Tile tileInHand;
    private int score;
    public Tile currTileVis;
    public int meeplesCount;
    public Camera myCamera;
    public Camera UICamera;
    public RenderTexture renderTexture;
    private Vector2 selectedPos;
    public Grid gridManager;
    private bool three_D;
    Plane plane = new Plane(Vector3.up, 0);

    private bool tileInstantiated = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (MyTurn)
        {
            if (HasTile)
            {
                //Update render texture!
                if (!tileInstantiated)
                {
                    currTileVis = Instantiate(tileInHand, new Vector3(75, 75, 0), Quaternion.Euler(new Vector3(0, 90, 0)));
                    tileInstantiated = true;

                }

                gridManager.generateAvaliableSpots(tileInHand);
                if (gridManager.avaliableGrids.Count == 0)
                {

                    while (gridManager.avaliableGrids.Count == 0)
                    {
                        rotate();
                        gridManager.generateAvaliableSpots(tileInHand);
                    }
                }

                List<Vector2> avaliablePlacements = gridManager.avaliableGrids;
                selectedPos = gridManager.avaliableGrids[Random.Range(0,avaliablePlacements.Count)];
            }
        }
    }



    public void rotate()
    {
        if (!tileInHand) return;
        currTileVis.transform.Rotate(new Vector3(0, 90, 0), Space.World);
        tileInHand.transform.Rotate(new Vector3(0, 90, 0), Space.World);
        tileInHand.TileIsRotated = true;
        tileInHand.numOfRotations++;

        tileInHand.swapSides(tileInHand.riverConnections);
        tileInHand.swapSides(tileInHand.cityConnections);
        tileInHand.swapSides(tileInHand.grassConnections);
        tileInHand.swapSides(tileInHand.roadConnections);


        gridManager.avaliableGrids.Clear();
        gridManager.generateAvaliableSpots(tileInHand);
    }

    public void place()
    {
        MyTurn = false;
        HasTile = false;
        Debug.Log("placed tile:" + tileInHand.name + " at:" + this.selectedPos.x + ", " + this.selectedPos.y);

        //Place tile in the correct cell
        gridManager.placeTile((int)this.selectedPos.x, (int)this.selectedPos.y, tileInHand);
        Destroy(currTileVis.gameObject);
        tileInstantiated = false;

    }

}
