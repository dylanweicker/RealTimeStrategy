using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class ResourceGenerator : NetworkBehaviour
{
    private Health health = null;
   [SerializeField] private int woodPerInterval = 2;
   [SerializeField] private int foodPerInterval = -1;
   [SerializeField] private float interval = 5f;
   [SerializeField] private int maxWorkers = 1;
   [SerializeField] private int workers = 1;

   public static event Action ResourceGeneratorsChanged;

   private float woodPerMinute = 0;
   private float foodPerMinute = 0;

   private float timer;
   private RTSPlayer player;

   public float GetWoodPerMinute()
   {
       return woodPerMinute;
   }
   public float GetFoodPerMinute()
   {
       return foodPerMinute;
   }

    #region server
    public override void OnStartServer()
    {
        timer = interval;
        player = connectionToClient.identity.GetComponent<RTSPlayer>();
        health = gameObject.GetComponent<Health>();
        health.ServerOnDie += ServerHandleDie;
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update()
    {
        if (workers < 1) return;

        timer -= Time.deltaTime; 
        if (timer <= 0)
        {
            //Todo add resources
            player.SetWood(player.GetWood() + woodPerInterval * workers);

            //Todo remove food
            player.SetFood(player.GetFood() + (foodPerInterval * workers));

            //Todo if food is below zero, remove a worker and a population and notify the player. 
            if (player.GetFood() < 0) 
            {
                player.SetFood(0);
                workers -= 1;
            }

            setResourcesPerMinute();

            //Reset the timer according to the number of workers  
            timer += interval;
        };

    }

    private void setResourcesPerMinute()
    {
        woodPerMinute = woodPerInterval / interval * workers * 60;
        foodPerMinute = foodPerInterval / interval * workers * 60;
    }

    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }
    private void ServerHandleGameOver()
    {
        enabled = false;
    }

    public override void OnStartAuthority()
    {
        ResourceGeneratorsChanged?.Invoke();
    }

    #endregion
}
