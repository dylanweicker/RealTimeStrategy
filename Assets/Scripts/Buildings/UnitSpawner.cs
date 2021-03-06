using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Health))]
public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    private Health health = null;
    [SerializeField] private GameObject unitPrefab = null;
    [SerializeField] private Transform spawnPoint = null;


    #region Server

    public override void OnStartServer()
    {
        health = gameObject.GetComponent<Health>();
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdSpawnUnit(){
        GameObject unitInstance = Instantiate(
            unitPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        NetworkServer.Spawn(unitInstance, connectionToClient);

    }

    #endregion

    #region Client
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) {
            return;
        }

        if (!hasAuthority) return;

        CmdSpawnUnit();
        
    }

    #endregion
}
