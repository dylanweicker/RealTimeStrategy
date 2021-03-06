using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject archerSpawnerPrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

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

    public override void OnServerSceneChanged(string sceneName)
    {
        if(SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            GameOverHandler gameOverHandler = Instantiate(gameOverHandlerPrefab);

            NetworkServer.Spawn(gameOverHandler.gameObject);

        }
    }
}
