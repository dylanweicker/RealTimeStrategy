using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text woodText = null;
    [SerializeField] private TMP_Text woodPerMinuteText = null;
    [SerializeField] private TMP_Text goldText = null;
    [SerializeField] private TMP_Text stoneText = null;
    [SerializeField] private TMP_Text ironText = null;
    [SerializeField] private TMP_Text foodText = null;
    private RTSPlayer player;

    private void Update()
    {
        if (player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            if (player != null) 
            {
                ClientHandleGoldUpdated(player.GetGold());
                ClientHandleWoodUpdated(player.GetWood());
                ClientHandleStoneUpdated(player.GetStone());
                ClientHandleIronUpdated(player.GetIron());
                ClientHandleFoodUpdated(player.GetFood());

                player.ClientOnGoldUpdated += ClientHandleGoldUpdated;
                player.ClientOnWoodUpdated += ClientHandleWoodUpdated;
                player.ClientOnStoneUpdated += ClientHandleStoneUpdated;
                player.ClientOnIronUpdated += ClientHandleIronUpdated;
                player.ClientOnFoodUpdated += ClientHandleFoodUpdated;

                player.ClientOnWoodPerMinuteUpdated += ClientHandleWoodPerMinuteUpdated;
            }
        }
    }

    private void OnDestroy()
    {
        player.ClientOnWoodUpdated -= ClientHandleWoodUpdated;
        player.ClientOnWoodUpdated -= ClientHandleWoodUpdated;
        player.ClientOnStoneUpdated -= ClientHandleStoneUpdated;
        player.ClientOnIronUpdated -= ClientHandleIronUpdated;
        player.ClientOnFoodUpdated -= ClientHandleFoodUpdated;
        player.ClientOnWoodPerMinuteUpdated -= ClientHandleWoodPerMinuteUpdated;
    }

    private void ClientHandleGoldUpdated(int gold)
    {
        goldText.text = gold.ToString();
    }

    private void ClientHandleWoodUpdated(int wood)
    {
        woodText.text = wood.ToString();
    }

    private void ClientHandleStoneUpdated(int stone)
    {
        stoneText.text = stone.ToString();
    }

    private void ClientHandleIronUpdated(int iron)
    {
        ironText.text = iron.ToString();
    }

    private void ClientHandleFoodUpdated(int food)
    {
        foodText.text = food.ToString();
    }

    private void ClientHandleWoodPerMinuteUpdated(float wpm)
    {
        string prefix = (wpm > 0) ? "+" : "";
        woodPerMinuteText.color = (wpm >= 0) ?  Color.green : Color.red;
        woodPerMinuteText.text = prefix + wpm.ToString("n1");
    }
}
