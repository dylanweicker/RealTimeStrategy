using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask = new LayerMask();
    [SerializeField] private RectTransform unitSelectionArea = null;

    private RTSPlayer player;

    private Vector2 startPosition;

   private Camera mainCamera;
   public List<Unit> SelectedUnits {get;} = new List<Unit>();

   //Todo move these to a config file
   private KeyControl multiselectKey1 = Keyboard.current.leftShiftKey;
   private KeyControl multiselectKey2 = Keyboard.current.rightShiftKey;
   private KeyControl deselectKey1 = Keyboard.current.leftCtrlKey;
   private KeyControl deselectKey2 = Keyboard.current.rightCtrlKey;

   private void Start() 
   {
       mainCamera = Camera.main;
   }

   private void Update(){

       if (player == null) {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
       }

       if(Mouse.current.leftButton.wasPressedThisFrame)
       {
           StartSelectionArea();
       }
       else if (Mouse.current.leftButton.wasReleasedThisFrame)
       {
           ClearSelectionArea();
       }
       else if(Mouse.current.leftButton.isPressed)
       {
           UpdateSelectionArea();
       }
   }

   private void StartSelectionArea()
   {
        if (!(
            multiselectKey1.isPressed || 
            multiselectKey2.isPressed || 
            deselectKey1.isPressed ||  //new
            deselectKey2.isPressed  //new
        )) 
        {
            DeselectAllUnits();
        }
        unitSelectionArea.gameObject.SetActive(true);
        startPosition = Mouse.current.position.ReadValue();
        UpdateSelectionArea();
   }

    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPosition + 
            new Vector2(areaWidth /2, areaHeight /2);

    }

    private void ClearSelectionArea()
    {
        //Hide selection box
        unitSelectionArea.gameObject.SetActive(false);

        //Single click select
        if(unitSelectionArea.sizeDelta.magnitude == 0) {
            if (deselectKey1.isPressed || deselectKey2.isPressed) DeselectUnit();
            else SelectUnit();
            return;
        }

        //Box area select
        if (deselectKey1.isPressed || deselectKey2.isPressed) DeselectUnits();
        else SelectUnits();
    }

    private void SelectUnit() {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;

        if(!hit.collider.TryGetComponent<Unit>(out Unit unit)) return;

        if(!unit.hasAuthority) return;

        SelectedUnits.Add(unit);
        unit.Select();
    }

    private void DeselectUnit() {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;

        if(!hit.collider.TryGetComponent<Unit>(out Unit unit)) return;

        if(!unit.hasAuthority) return;

        if (SelectedUnits.Contains(unit)){
            SelectedUnits.Remove(unit);
            unit.Deselect();
        }
    }

    private void SelectUnits() {
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach(Unit unit in player.GetMyUnits())
        {
            if (SelectedUnits.Contains(unit)) continue;

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
            if(screenPosition.x > min.x && 
                screenPosition.x < max.x &&
                screenPosition.y > min.y && 
                screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    private void DeselectUnits() {
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach(Unit unit in player.GetMyUnits())
        {
            if (!SelectedUnits.Contains(unit)) continue;

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
            if(screenPosition.x > min.x && 
                screenPosition.x < max.x &&
                screenPosition.y > min.y && 
                screenPosition.y < max.y)
            {
                SelectedUnits.Remove(unit);
                unit.Deselect();
            }
        }
    }

    private void DeselectAllUnits() {
        foreach(Unit selectedUnit in SelectedUnits)
        {
            selectedUnit.Deselect();
        }
        SelectedUnits.Clear();
    }
}
