using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class RTSNetworkManager : NetworkManager
{
    [SerializeField] 
    private GameObject archerSpawnerPrefab = null;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        GameObject archerSpawnerInstance = Instantiate(
            archerSpawnerPrefab, 
            conn.identity.transform.position, 
            conn.identity.transform.rotation
        );

        NetworkServer.Spawn(archerSpawnerInstance, conn);
    }
}
