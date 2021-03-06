using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using System.Text.RegularExpressions;
using System;

public class RTSPlayer : NetworkBehaviour
{ 
    [SerializeField] private Building[] buildings = new Building[0];
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();

    [SyncVar (hook = nameof(ClientHandleGoldUpdated))] private int gold = 10;
    [SyncVar (hook = nameof(ClientHandleWoodUpdated))] private int wood = 50;
    [SyncVar (hook = nameof(ClientHandleStoneUpdated))] private int stone = 0;
    [SyncVar (hook = nameof(ClientHandleIronUpdated))] private int iron = 0;
    [SyncVar (hook = nameof(ClientHandleFoodUpdated))] private int food = 500;

    public event Action<int> ClientOnGoldUpdated;
    public event Action<int> ClientOnWoodUpdated;
    public event Action<int> ClientOnStoneUpdated;
    public event Action<int> ClientOnIronUpdated;
    public event Action<int> ClientOnFoodUpdated;

    private float woodPerMinute = 0;

    public event Action<float> ClientOnWoodPerMinuteUpdated;


    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }
    
    public List<Building> GetMyBuildings()
    {
        return myBuildings;
    }

    public int GetGold() { return gold; }
    public int GetWood() { return wood; }
    public int GetStone() { return stone; }
    public int GetIron() { return iron; }
    public int GetFood() { return food; }
    
    [Server] public void SetGold(int newValue) { gold = newValue; }
    [Server] public void SetWood(int newValue) { wood = newValue; }
    [Server] public void SetStone(int newValue) { stone = newValue; }
    [Server] public void SetIron(int newValue) { iron = newValue; }
    [Server] public void SetFood(int newValue) { food = newValue; }

    public float GetWoodPerMinute() { return woodPerMinute;}

    #region Server

    [Server]
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;
    }


    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
    }


    private void ServerHandleUnitSpawned(Unit unit)
    {
        //check if the unit that spawned belong to the client?
        if(unit.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myUnits.Add(unit);
    }

    private void ServerHandleUnitDespawned(Unit unit)
    {
        //check if the unit that spawned belong to the client?
        if(unit.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myUnits.Remove(unit);
    }
    
    private void ServerHandleBuildingSpawned(Building building)
    {
        if(building.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myBuildings.Add(building);
    }
    
    private void ServerHandleBuildingDespawned(Building building)
    {
        if(building.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myBuildings.Remove(building);
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 location, Quaternion rotation)
    {
        Building buildingToPlace = null;

        foreach(Building building in buildings)
        {
            if(building.GetId() == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }
        if(buildingToPlace == null) 
        {
            Debug.LogError($"Tried to place a building with an unknown building Id {buildingId}");
            return;
        }

        GameObject buildingInstance = 
            Instantiate(buildingToPlace.gameObject, location, rotation);
        NetworkServer.Spawn(buildingInstance, connectionToClient);
    }


    #endregion

    #region Client

     public override void OnStartAuthority()
    {

        if(!NetworkServer.active) 
        {
            Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
            Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
            Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
            Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
        }

        ResourceGenerator.ResourceGeneratorsChanged += AuthorityHandleResourceGeneratorsChanged;
    }

    public override void OnStopClient()
    {

        if(isClientOnly && hasAuthority) 
        {
            Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
            Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
            Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
            Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
        }
        
        ResourceGenerator.ResourceGeneratorsChanged -= AuthorityHandleResourceGeneratorsChanged;
    }

    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }
    
    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }

    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    private void ClientHandleGoldUpdated(int oldValue, int newValue)
    {
        ClientOnGoldUpdated?.Invoke(newValue);
    }
    
    private void ClientHandleWoodUpdated(int oldValue, int newValue)
    {
        ClientOnWoodUpdated?.Invoke(newValue);
    }
    
    private void ClientHandleIronUpdated(int oldValue, int newValue)
    {
        ClientOnIronUpdated?.Invoke(newValue);
    }
    
    private void ClientHandleStoneUpdated(int oldValue, int newValue)
    {
        ClientOnStoneUpdated?.Invoke(newValue);
    }

    private void ClientHandleFoodUpdated(int oldValue, int newValue)
    {
        ClientOnFoodUpdated?.Invoke(newValue);
    }

    private void AuthorityHandleResourceGeneratorsChanged()
    {
        CalculateWoodPerMinute();
    }

    private void CalculateWoodPerMinute()
    {
        float wpm = 0;
        foreach (Building building in myBuildings)
        {
            ResourceGenerator resourceGenerator = building.GetComponent<ResourceGenerator>();
            if (!resourceGenerator) continue;
            wpm += resourceGenerator.GetWoodPerMinute();
        }
        woodPerMinute = wpm;
        Debug.Log($"Our new wood per minute is {wpm}");
        ClientOnWoodPerMinuteUpdated.Invoke(woodPerMinute);
    }

    #endregion

}
