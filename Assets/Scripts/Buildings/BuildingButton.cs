using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Building building = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text description = null;
    [SerializeField] private GameObject toolTip = null;
    [SerializeField] private LayerMask floorMask = new LayerMask();

    private Camera mainCamera;
    private RTSPlayer player;
    private GameObject buildingPreviewInstance;
    private Renderer buildingRendererInstance;
    private int price;
    private string buildingName;


    private void Start()
    {
        mainCamera = Camera.main;
        iconImage.sprite = building.GetIcon();
        price = building.GetPrice();
        buildingName = building.GetbuildingName();
        description.text = buildingName + "\n" + price.ToString();
    }

    private void Update()
    {
        if (player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        if(buildingPreviewInstance)
        {
            UpdateBuildingPreview();
        }
    }    

    public void OnPointerEnter(PointerEventData eventData)
    {
        toolTip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        toolTip.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) return;
        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();
        buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (buildingPreviewInstance == null) {return;}
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) 
        {
            //place building
            player.CmdTryPlaceBuilding(building.GetId(), hit.point, buildingPreviewInstance.transform.rotation);
            Destroy(buildingPreviewInstance);
        }
    }

    private void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) {return;}
        buildingPreviewInstance.transform.position = hit.point;

        buildingPreviewInstance.transform.Rotate(0, Input.mouseScrollDelta.y * 15, 0);

        if(!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }
    }
}
