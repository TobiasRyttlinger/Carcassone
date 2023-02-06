using System.Collections;
using System.Collections.Generic;
using TMPro;
using Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
public class Player : MonoBehaviour
{

    public GameManager GM;

    //Camera
    #region 
    public Camera myCamera;
    public Camera UICamera;
    public GameObject camerasys;
    public GameObject selectedMeeplePos;

    public CinemachineVirtualCamera virtualCamera;
    #endregion

    //Variables
    #region 
    public List<Meep> meeples;
    public bool MyTurn = false;
    public bool HasTile = false;
    public Tile tileInHand;
    private int score;
    public Tile currTileVis;
    public bool autoPlace = false;
    public Button PlaceButton;
    public Button RotateButton;
    public RenderTexture renderTexture;
    private Vector2 selectedPos;
    public Grid gridManager;
    private bool three_D;
    Plane plane = new Plane(Vector3.up, 0);
    public Tile lastPlacedTile;
    private bool tileInstantiated = false;
    public bool placedTile = false;
    public bool placedMeep = false;

    public Texture2D color;
    #endregion

    //UI
    #region 
    public Image currentTileTex;
    public TextMeshProUGUI tilesLeft;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI MeeplesLeft;
    public TextMeshProUGUI buttonText;

    #endregion


    void Start()
    {
        score = 0;
        selectedPos.x = -1;
        selectedPos.y = -1;


        foreach (Meep m in meeples)
        {
            m.setMaterial(color);
        }
        three_D = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (MyTurn)
        {


            if (GM.currentPhase == GameManager.GamePhases.TilePhase)
            {
                buttonText.text = "Place";
            }
            else if (GM.currentPhase == GameManager.GamePhases.MeepPhase)
            {
                buttonText.text = "Skip";
                PlaceButton.interactable = true;
            }
            else
            {
                PlaceButton.interactable = false;
                buttonText.text = "Place";
            }


            //Make Mouse pick cell!
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 pos = myCamera.ScreenToWorldPoint(Input.mousePosition);
                var ray = myCamera.ScreenPointToRay(Input.mousePosition);

                float distance;
                if (plane.Raycast(ray, out distance))
                {
                    pos = ray.GetPoint(distance);
                }
                if (GM.currentPhase == GameManager.GamePhases.TilePhase)
                {
                    if (pos.x > 0 && pos.z > 0 && gridManager.avaliableGrids.Contains(new Vector2(Mathf.Floor(pos.x), Mathf.Floor(pos.z))))
                    {
                        //Dont record clicks over ui elements.
                        if (IsPointerOverUIObject())
                        {
                            return;
                        }
                        //Store selection
                        selectedPos.x = Mathf.Floor(pos.x);
                        selectedPos.y = Mathf.Floor(pos.z);
                    }

                    if (gridManager.tileIsOccupied((int)selectedPos.x, (int)selectedPos.y))
                    {
                        PlaceButton.interactable = false;
                    }
                    else
                    {

                        PlaceButton.interactable = true;
                    }
                }

                else if (GM.currentPhase == GameManager.GamePhases.MeepPhase)
                {
                    if (!gridManager.avaliableGrids.Contains(selectedPos))
                    {
                        PlaceButton.interactable = false;
                    }

                    if (meeples.Count < 1)
                    {
                        GM.currentPhase = GameManager.GamePhases.ScorePhase;
                        GM.DeactivateMeeplePos(lastPlacedTile, Tile.directions.ALL);

                    }
                    else
                    {
                        int LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");

                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit))
                        {

                            if (hit.transform.gameObject.layer != LayerIgnoreRaycast)
                            {
                                Debug.Log(hit.transform.name + " is clicked by mouse");
                                selectedMeeplePos = hit.transform.gameObject;
                                GM.placeMeep();

                                GM.DeactivateMeeplePos(lastPlacedTile, Tile.directions.ALL);
                            }

                        }
                    }

                }

            }


            if (HasTile)
            {
                //Update render texture!
                if (!tileInstantiated)
                {
                    currTileVis = Instantiate(tileInHand, new Vector3(75, 75, 0), Quaternion.Euler(new Vector3(0, 90, 0)));
                    tileInstantiated = true;

                }

                gridManager.avaliableGrids.Clear();
                gridManager.generateAvaliableSpots(tileInHand);
                GM.checkRiverTurn(tileInHand, lastPlacedTile);

                List<Vector2> avaliablePlacements = gridManager.avaliableGrids;

                foreach (Vector2 v in avaliablePlacements)
                {
                    Debug.DrawLine(new Vector3(v.x, 0, v.y), new Vector3(v.x + 1, 0, v.y), Color.yellow, 0.1f);
                    Debug.DrawLine(new Vector3(v.x, 0, v.y), new Vector3(v.x, 0, v.y + 1), Color.yellow, 0.1f);

                    Debug.DrawLine(new Vector3(v.x + 1, 0, v.y), new Vector3(v.x + 1, 0, v.y + 1), Color.white, 0.1f);
                    Debug.DrawLine(new Vector3(v.x, 0, v.y + 1), new Vector3(v.x + 1, 0, v.y + 1), Color.white, 0.1f);
                }
                int failsafe = 0;

                //Autoplace tiles
                if (autoPlace && tileInHand.river)
                {
                    while (avaliablePlacements.Count < 1)
                    {
                        rotate();
                        if (failsafe > 4)
                        {
                            //Discard!
                            tileInHand = GM.Tiles[GM.Tiles.Count - 1];

                            break;
                        }
                        failsafe++;
                    }
                    int rand = Random.Range(0, avaliablePlacements.Count - 1);
                    if (avaliablePlacements.Count > 0)
                    {
                        if (!gridManager.tileIsOccupied((int)avaliablePlacements[rand].x, (int)avaliablePlacements[rand].y))
                        {
                            selectedPos.x = Mathf.Floor(avaliablePlacements[rand].x);
                            selectedPos.y = Mathf.Floor(avaliablePlacements[rand].y);
                            place();
                            avaliablePlacements.Clear();
                        }
                    }

                }

            }
            myCamera.enabled = true;
            PlaceButton.gameObject.SetActive(true);

        }
        else
        {
            myCamera.enabled = false;
            PlaceButton.gameObject.SetActive(false);
        }

    }

    public void rotate()
    {
        if (!tileInHand || !currTileVis) return;
        selectedPos.x = -1;
        selectedPos.y = -1;
        PlaceButton.interactable = false;
        currTileVis.transform.Rotate(new Vector3(0, 90, 0), Space.World);
        tileInHand.transform.Rotate(new Vector3(0, 90, 0), Space.World);
        tileInHand.TileIsRotated = true;
        tileInHand.numOfRotations++;

        tileInHand.swapSides(tileInHand.riverConnections);
        tileInHand.swapSides(tileInHand.cityConnections);
        tileInHand.swapSides(tileInHand.grassConnections);
        tileInHand.swapSides(tileInHand.roadConnections);
        tileInHand.swapChildSides(tileInHand);

        gridManager.avaliableGrids.Clear();
        gridManager.generateAvaliableSpots(tileInHand);
        GM.checkRiverTurn(tileInHand, lastPlacedTile);
    }

    public void place()
    {

        //Skip is pressed
        if (GM.currentPhase == GameManager.GamePhases.MeepPhase)
        {

            GM.DeactivateMeeplePos(lastPlacedTile, Tile.directions.ALL);


            GM.currentPhase = GameManager.GamePhases.ScorePhase;
            return;
        }
        //Skip is pressed
        if (GM.currentPhase == GameManager.GamePhases.ScorePhase)
        {

            PlaceButton.interactable = false;

            return;

        }

        if ((selectedPos.x < 0 || selectedPos.y < 0) && !gridManager.avaliableGrids.Contains(selectedPos))
        {
            PlaceButton.interactable = false;
            return;

        }

        HasTile = false;
        // Debug.Log("placed tile:" + tileInHand.name + " at:" + this.selectedPos.x + ", " + this.selectedPos.y);

        //Place tile in the correct cell
        gridManager.placeTile((int)this.selectedPos.x, (int)this.selectedPos.y, tileInHand);
        Destroy(currTileVis.gameObject);
        tileInstantiated = false;
        placedTile = true;

        GM.DeactivateMeeplePos(lastPlacedTile, Tile.directions.ALL);
        lastPlacedTile = tileInHand;
        selectedPos.x = -1;
        selectedPos.y = -1;
        PlaceButton.interactable = false;
        GM.currentPhase = GameManager.GamePhases.MeepPhase;
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public void addScore(int inScore)
    {
        this.score += inScore;
        scoreText.text = "Score: " + this.score.ToString();
    }


}
