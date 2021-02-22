using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using System.Text.RegularExpressions;

public class RTSPlayer : NetworkBehaviour
{ 
    //Name and Color
    [SerializeField] private TMP_Text displayNameText = null;
    [SerializeField] private Renderer displayColorRenderer = null;

    [SyncVar(hook=nameof(HandleDisplayNameUpdated))]
    [SerializeField]
    private string displayName = "Missing Name";

    [SyncVar(hook=nameof(HandleDisplayColorUpdated))]
    [SerializeField]
    private Color displayColor = new Color(0, 0, 0);

    //Units
    private List<Unit> myUnits = new List<Unit>();

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    #region Server

    [Server]

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
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

    public void SetDisplayName(string newDisplayName)
    {
        displayName = newDisplayName;
    }

    [Server]
    public void SetPlayerColor(Color newDisplayColor)
    {
        displayColor = newDisplayColor;
    }

    [Command]
    private void CmdSetDisplayName(string newDisplayName)
    {
        if (
            !Regex.IsMatch(newDisplayName, "^[a-zA-Z0-9\\s]*$") ||
            newDisplayName.Length < 3 ||
            newDisplayName.Length > 12
            )
        {
          return;  
        }

        string[] bannedWords = {"fuck", "shit", "cunt", "piss", "bitch", "fag", "nigger", "nigga", "cock", "clit", "cum", "dildo", "whore", "jizz", "masturbate", "wank", "titty", "titti", "vagina"};

        foreach(string bannedWord in bannedWords) 
        {
            if (newDisplayName.ToLower().Contains(bannedWord)) return;
        }

        RpcBroadcastNewName(newDisplayName);
        SetDisplayName(newDisplayName);
    }

    #endregion

    #region Client

     public override void OnStartAuthority()
    {
        if(NetworkServer.active) return;
        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
    }

    public override void OnStopClient()
    {
        if(!isClientOnly || !hasAuthority) return;
        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
    }

    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    private void HandleDisplayColorUpdated(Color oldColor, Color newColor)
    {
        displayColorRenderer.material.SetColor("_BaseColor", newColor);
    }

    private void HandleDisplayNameUpdated(string oldName, string newName)
    {
        displayNameText.text = newName;
    }

    [ContextMenu("Set My Name")]
    private void SetMyName()
    {
        CmdSetDisplayName("My New Name");
    }

    [ClientRpc]
    private void RpcBroadcastNewName(string newDisplayName)
    {
        Debug.Log($"A player has been renamed to {newDisplayName}");
    }

    #endregion

}
