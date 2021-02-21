using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitAnimation : NetworkBehaviour
{
    private Vector3 previousPosition; //new
    private float speed; //new
    private Animator anim; //new

    #region Client

    [ClientCallback]
    private void Start(){       //new
        previousPosition = transform.position;
        anim = GetComponent<Animator> ();
    }

    [ClientCallback]
    private void Update()
    {
        CheckMovement(); //new
    }

    private void CheckMovement(){  //new
        speed = ((transform.position - previousPosition).magnitude) / Time.deltaTime;
        previousPosition = transform.position;
        anim.SetBool("Run", speed > 1);
    }

    [ClientRpc]
    public void RpcShootArrow()
    {
        anim.SetBool("Longbow Shoot Attack 01", true);
    }

    #endregion
}
