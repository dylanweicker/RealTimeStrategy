using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{

    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 5f;

    //How frequently the fire cycle is allowed to happen
    [SerializeField] private float fireDuration = 3f;

    //How late into the fire cycle the projectile is spawned 
    [SerializeField] private float fireDelay = 1.5f;
    [SerializeField] private float rotationSpeed = 20f;

    private UnitAnimation anim = null; 
    private Targeter targeter = null;
    
    private float lastFireTime;


    #region Server

    [ServerCallback]
    private void Start() {
        anim = gameObject.GetComponent<UnitAnimation>();
        targeter = gameObject.GetComponent<Targeter>();
    }

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if(target == null) return;
        if (!CanFireAtTarget()) return;

        Quaternion targetRotation = 
            Quaternion.LookRotation(target.transform.position - transform.position);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if(Time.time > (fireDuration) + lastFireTime)
        {
            anim.RpcShootArrow();

            lastFireTime = Time.time; 
            Invoke(nameof(FireProjectile), fireDelay);
        }

    }

    [Server]
    private void FireProjectile()
    {
        Quaternion projectileRotation = Quaternion.LookRotation(
            targeter.GetTarget().GetAimAtPoint().position - projectileSpawnPoint.position);

        GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);

        NetworkServer.Spawn(projectileInstance, connectionToClient);
    }

    [Server]
    private bool CanFireAtTarget()
    {
        return  (transform.position - targeter.GetTarget().transform.position).sqrMagnitude 
            < fireRange * fireRange;
    }

    #endregion

    #region client

    #endregion
}
