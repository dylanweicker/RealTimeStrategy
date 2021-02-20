using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovementBackup : NetworkBehaviour
{

    [SerializeField] private NavMeshAgent agent = null;
    private Camera mainCamera;
    private Vector3 previousPosition; //new
    private float speed; //new
    private Animator anim; //new

    #region Server

    [Command]
    private void CmdMove(Vector3 position)
    {
        if(!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            return;
        }

        agent.SetDestination(hit.position);
    }

    #endregion

    #region Client

    [ClientCallback]
    private void Start(){       //new
        previousPosition = transform.position;
        anim = GetComponent<Animator> ();
    }

    public override void OnStartAuthority()
    {
        mainCamera = Camera.main;
    }

    [ClientCallback]
    private void Update()
    {
        CheckAnimation(); //new

        //Player belongs to us
        if(!hasAuthority) {return;}

        //Clicked right moust button
        if(!Mouse.current.rightButton.wasPressedThisFrame) { return; }

        //Clicked on an object in the scene
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) { return; }

        //Move
        CmdMove(hit.point);
    }

    private void CheckAnimation(){  //new
        speed = ((transform.position - previousPosition).magnitude) / Time.deltaTime;
        previousPosition = transform.position;
        anim.SetBool("Run", speed > 1);
    }

    #endregion
}
