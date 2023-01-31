using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private  Player[] Players;
    
    private  AIPlayer[] aiPlayers;
    private int playerIndex = 0;
    private int playerMax;
    
    // Start is called before the first frame update
    void Awake()
    {
        aiPlayers = GameObject.FindObjectsOfType(typeof(AIPlayer)) as AIPlayer[];
        Players = GameObject.FindObjectsOfType(typeof(Player)) as Player[];
        
        playerMax = Players.Length-1;
        Players[Random.Range(0,playerMax)].MyTurn = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!Players[playerIndex].MyTurn){
            swapPlayer();
           
        }
    }

    void swapPlayer(){
        playerIndex++;

        if(playerIndex > playerMax){
            playerIndex = 0;
        }
         Players[playerIndex].MyTurn = true;
    }

    public Player getCurrentPlayer(){
        return Players[playerIndex];
    }
    
    public Player[] GetPlayers(){
        return Players;
    }
       public AIPlayer[] GetAiPlayers(){
        return aiPlayers;
    }
}
