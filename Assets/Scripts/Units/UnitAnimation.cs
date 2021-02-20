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
        CheckAnimation(); //new
    }

    private void CheckAnimation(){  //new
        speed = ((transform.position - previousPosition).magnitude) / Time.deltaTime;
        previousPosition = transform.position;
        anim.SetBool("Run", speed > 1);
    }

    #endregion
}
