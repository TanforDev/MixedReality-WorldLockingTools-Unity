using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Experimental.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.WorldLocking.Core;
using Microsoft.MixedReality.WorldLocking.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA;

public class PlaceItems : InputSystemGlobalHandlerListener, IMixedRealityPointerHandler
{
    [SerializeField] private ItemInventory inventory;
    private WorldAnchorManager worldAnchorManager;

    #region Private Fields
    private enum BuildMode
    {
        Idle = 0,
        PlaceObject = 1,
        RemoveObject = 2
    };

    private int ModeToIndex(BuildMode mode)
    {
        return (int)mode;
    }

    private InteractableToggleCollection radioSet = null;

    private BuildMode mode = BuildMode.Idle;

    private int objectToPlace;

    #endregion Private Fields

    #region InputSystemGlobalHandlerListener Implementation
    protected override void RegisterHandlers()
    {
        MixedRealityToolkit.Instance?.GetService<IMixedRealityInputSystem>()?.RegisterHandler<IMixedRealityPointerHandler>(this);
    }

    protected override void UnregisterHandlers()
    {
        MixedRealityToolkit.Instance?.GetService<IMixedRealityInputSystem>()?.UnregisterHandler<IMixedRealityPointerHandler>(this);
    }
    #endregion InputSystemGlobalHandlerListener Implementation

    #region IMixedRealityPointerHandler

    /// <summary>
    /// Process pointer clicked event if ray cast has result.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        var pointerResult = eventData.Pointer.Result;
        var rayHit = new RayHit(pointerResult);
        int uiLayer = LayerMask.GetMask("UI");
        if (rayHit.gameObject == null || ((1 << rayHit.gameObject.layer) & uiLayer) == 0)

            HandleHit(rayHit);
    }

    /// <summary>
    /// No-op on pointer up.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {

    }

    /// <summary>
    /// No-op on pointer down.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {

    }

    /// <summary>
    /// No-op on pointer drag.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {

    }

    #endregion IMixedRealityPointerHandler

    #region Unity Methods

    /// <summary>
    /// Override InputSystemGlobalListener Start() method for additional one-time setup.
    /// </summary>
    protected override void Start()
    {
        worldAnchorManager = FindObjectOfType<WorldAnchorManager>();

        base.Start();

        radioSet = gameObject.GetComponent<InteractableToggleCollection>();

        SyncRadioSet();
    }

    #endregion Unity Methods

    private void HandleHit(RayHit rayHit)
    {
        switch (mode)
        {
            case BuildMode.Idle:
                break;
            case BuildMode.PlaceObject:
                PlaceObject(rayHit, objectToPlace);
                break;
            case BuildMode.RemoveObject:
                RemoveObject(rayHit);
                break;
            default:
                break;
        }
    }

    private void PlaceObject(RayHit rayHit, int index)
    {
        var hitPos = rayHit.hitPosition;
        var toRay = rayHit.rayStart - hitPos;
        var hitDirProj = toRay;
        hitDirProj.y = 0;
        hitDirProj.Normalize();
        var hitUp = new Vector3(0.0f, 1.0f, 0.0f);
        var hitRot = Quaternion.LookRotation(hitDirProj, hitUp);

        var newObj = inventory.SpawnItem(index);

        newObj.transform.position = hitPos;
        newObj.transform.rotation = hitRot;

        worldAnchorManager.AttachAnchor(newObj);

        //Pose pose = new Pose(hitPos, hitRot);
        //newObj.GetComponent<SpacePin>().SetFrozenPose(pose);

        //var twa = newObj.AddComponent<ToggleWorldAnchor>();
        //twa.AlwaysLock = true;


        EnterIdleMode();

        radioSet.CurrentIndex = ModeToIndex(BuildMode.Idle);
    }

    private void RemoveObject(RayHit rayHit)
    {
        if (rayHit.gameObject != null)
        {
            RemovableGroup removal = rayHit.gameObject.GetComponentInParent<RemovableGroup>();
            if (removal != null)
            {
                GameObject.Destroy(removal.gameObject);
            }
        }
    }

    private void SyncRadioSet()
    {
        radioSet.CurrentIndex = ModeToIndex(mode);
        radioSet.SetSelection(radioSet.CurrentIndex);
    }


    private struct RayHit
    {
        public readonly Vector3 rayStart;
        public readonly Vector3 hitPosition;
        public readonly Vector3 hitNormal;
        public readonly GameObject gameObject;

        public RayHit(Vector3 rayStart, RaycastHit hitInfo)
        {
            this.rayStart = rayStart;
            this.hitPosition = hitInfo.point;
            this.hitNormal = hitInfo.normal;
            this.gameObject = hitInfo.collider?.gameObject;
        }

        public RayHit(IPointerResult pointerResult)
        {
            this.rayStart = pointerResult.StartPoint;
            this.hitPosition = pointerResult.Details.Point;
            this.hitNormal = pointerResult.Details.Normal;
            this.gameObject = pointerResult.CurrentPointerTarget;
        }
    };

    #region Mode transitions
    public void EnterIdleMode()
    {
        mode = BuildMode.Idle;
    }

    public void EnterPlaceObjectMode(int index)
    {
        mode = BuildMode.PlaceObject;
    }

    public void EnterRemoveMode()
    {
        mode = BuildMode.RemoveObject;
    }

    #endregion Mode transitions
}
