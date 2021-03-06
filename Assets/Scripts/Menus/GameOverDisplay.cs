using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{

    [SerializeField] private TMP_Text winnerNameText = null;
    [SerializeField] private GameObject gameOverDisplayParent = null;
    

    private void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void ClientHandleGameOver(string winner)
    {
        winnerNameText.text = $"{winner} Wins!";
        gameOverDisplayParent.SetActive(true);
    }

    public void LeaveGame()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else{
            NetworkManager.singleton.StopClient();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
