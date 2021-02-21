using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
public class Targeter : NetworkBehaviour
{
    private Targetable target = null;

    public Targetable GetTarget()
    {
        return target;
    }

    #region server

    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        if (!targetGameObject.TryGetComponent<Targetable>(out Targetable targetable)) return;

        target = targetable;
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
    }

    #endregion

}
