using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int currentHealth;

    public event Action ServerOnDie;
    public event Action<int, int> ClientOnHealthUpdated;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if(currentHealth == 0) return;  //already dead

        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);
        Debug.Log($"Took {damageAmount} damage");

        if (currentHealth != 0) return; //not dead yet

        //now dead
        ServerOnDie?.Invoke();
        Debug.Log("We Died");
    }

    #endregion


    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(currentHealth, maxHealth);
    }

    #endregion
}
