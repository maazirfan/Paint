﻿using UnityEngine;
using System.Collections;
using Leap.Unity;

public class WearableUI : AnchoredBehaviour, IWearable {

  private const float VELOCITY_DEPENDENT_RETURN_TO_ANCHOR_DISTANCE = 0.2F;
  private const float OVERRIDE_VELOCITY_RETURN_TO_ANCHOR_DISTANCE = 0.05F;

  [Header("Wearable UI")]
  public MeshRenderer _appearanceExplosionRenderer;

  private WearableManager _manager;
  private WearableAnchor _wearableAnchor;
  public WearableAnchor WearableAnchor {
    get { return _wearableAnchor; }
  }
  private Chirality _anchorChirality = Chirality.Left;
  public Chirality AnchorChirality {
    get { return _anchorChirality; }
  }
  private Chirality _displayingChirality = Chirality.Left;
  protected Chirality DisplayingChirality {
    get { return _displayingChirality; }
    set { _displayingChirality = value; }
  }

  private bool _isDisplayingOnAnyHand = false;

  protected void Start() {
    InitAppearVanish();

    _manager = GameObject.FindObjectOfType<WearableManager>();
    WearableAnchor anchor = _anchorTransform.GetComponent<WearableAnchor>();
    if (anchor != null) {
      _wearableAnchor = anchor;
      _wearableAnchor.OnAnchorChiralityChanged += DoOnAnchorChiralityChanged;
    }
  }

  protected void FixedUpdate() {
    FixedAppearVanishUpdate();
    FixedGrabUpdate();
    FixedWorkstationTransitionUpdate();
  }

  private void RefreshVisibility() {
    if (_isAttached) {
      if (!_isDisplayingOnAnyHand) {
        if (_isLeftHandTracked && _isLeftPalmFacingCamera && !_isLeftHandPinching) {
          ScheduleAppear(Chirality.Left);
        }
        else if (_isRightHandTracked && _isRightPalmFacingCamera && !_isRightHandPinching) {
          ScheduleAppear(Chirality.Right);
        }
        else if (_anchorTransform != _grabbingTransform) {
          ScheduleVanish();
        }
      }
      else if (_grabbingTransform != null) {
        // If currently grabbed, don't change visibility.
      }
      else {
        if (_wearableAnchor._chirality == Chirality.Left && (!_isLeftHandTracked || !_isLeftPalmFacingCamera || _isLeftHandPinching)) {
          ScheduleVanish();
        }
        else if (_wearableAnchor._chirality == Chirality.Right && (!_isRightHandTracked || !_isRightPalmFacingCamera || _isRightHandPinching)) {
          ScheduleVanish();
        }
      }
    }
  }

  private void RefreshAnchorState() {
    if (_grabbingTransform == null) {
      bool returningToAnchor = false;
      if (_anchorTransform != _wearableAnchor.transform) {
        RefreshVisibility();
        returningToAnchor = true;
      }
      _anchorTransform = _wearableAnchor.transform;
      if (returningToAnchor) {
        DoOnReturnedToAnchor();
      }
    }
    else {
      _anchorTransform = _grabbingTransform;
    }
  }

  // TODO: Make emerge action happen on hover instead of relying on passing OnTriggerStay from the marble
  public void OnFingerEnterMarble(Collider fingerCollider) {
    DoOnFingerPressedMarble();
  }

  protected virtual void DoOnAnchorChiralityChanged(Chirality whichHand) {
    if (whichHand != _anchorChirality) {
      _anchorChirality = whichHand;
    }
  }

  protected virtual void DoOnFingerPressedMarble() { }

  protected virtual void DoOnReturnedToAnchor() { }

  #region Wearable Implementation

  private bool _isLeftHandTracked = false;
  private bool _isLeftPalmFacingCamera = false;
  private bool _isLeftHandPinching = false;
  private bool _isRightHandTracked = false;
  private bool _isRightPalmFacingCamera = false;
  private bool _isRightHandPinching = false;

  public void NotifyHandTracked(bool isHandTracked, Chirality whichHand) {
    if (whichHand == Chirality.Left) {
      _isLeftHandTracked = isHandTracked;
    }
    else {
      _isRightHandTracked = isHandTracked;
    }
    RefreshVisibility();
  }

  public void NotifyPalmFacingCamera(bool isPalmFacingCamera, Chirality whichHand) {
    if (whichHand == Chirality.Left) {
      _isLeftPalmFacingCamera = isPalmFacingCamera;
    }
    else {
      _isRightPalmFacingCamera = isPalmFacingCamera;
    }
    RefreshVisibility();
  }

  public void NotifyPinchChanged(bool isPinching, Chirality whichHand) {
    if (whichHand == Chirality.Left) {
      _isLeftHandPinching = isPinching;
    }
    else {
      _isRightHandPinching = isPinching;
    }
    RefreshVisibility();
  }

  public Vector3 GetPosition() {
    return this.transform.position;
  }

  // See Grab State region for grab implementation.

  #endregion

  #region Appear / Vanish

  private TweenHandle _appearTween;
  private bool _appearScheduled = false;
  private bool _vanishScheduled = false;

  private void InitAppearVanish() {
    _appearTween = ConstructAppearTween();
    _appearTween.Progress = 0.001F;
    Vanish();
  }

  private void ScheduleAppear(Chirality whichHand) {
    _appearScheduled = true;
  }
  private void ScheduleVanish() {
    _vanishScheduled = true;
  }

  private void FixedAppearVanishUpdate() {
    if (_appearScheduled) {
      Appear();
      _appearScheduled = false;
      if (_vanishScheduled) _vanishScheduled = false;
    }
    else if (_vanishScheduled) {
      Vanish();
      _vanishScheduled = false;
    }
  }

  private void Appear() {
    _appearTween.Play(TweenDirection.FORWARD);
  }

  private void Vanish() {
    _appearTween.Play(TweenDirection.BACKWARD);
  }

  private TweenHandle ConstructAppearTween() {
    return Tween.Target(this.transform)
      .LocalScale(Vector3.one / 1000F, Vector3.one)
      .Target(_appearanceExplosionRenderer.material)
      .Color(Color.white, new Color(1F, 1F, 1F, 0F))
      .OverTime(0.2F)
      .Smooth(TweenType.SMOOTH)
      .OnLeaveStart(DoOnStartAppearing)
      .OnReachEnd(OnExplosionShouldDisappear)
      .OnLeaveEnd(OnExplosionShouldAppear)
      .OnReachStart(DoOnFinishedVanishing)
      .Keep();
  }

  private void DoOnStartAppearing() {
    _isDisplayingOnAnyHand = true;
    OnExplosionShouldAppear();
  }

  private void OnExplosionShouldAppear() {
    _appearanceExplosionRenderer.enabled = true;
  }

  private void DoOnFinishedVanishing() {
    _isDisplayingOnAnyHand = false;
    RefreshVisibility();
  }

  private void OnExplosionShouldDisappear() {
    _appearanceExplosionRenderer.enabled = false;
  }

  #endregion

  #region Grab State

  private Transform _grabbingTransform = null;

  private bool _hasGrabVelocity = false;
  private int _framesGrabbed = 0;
  private Vector3 _curPosition;
  private Vector3 _lastPosition;

  protected bool IsGrabbed {
    get { return _grabbingTransform != null; }
  }

  public bool CanBeGrabbed() {
    return _isDisplayingOnAnyHand;
  }

  protected void FixedGrabUpdate() {
    if (_grabbingTransform != null) {
      _framesGrabbed += 1;
      _lastPosition = _curPosition;
      _curPosition = this.transform.position;
      if (_framesGrabbed == 2) {
        _hasGrabVelocity = true;
      }
    }
    else {
      if (_framesGrabbed > 0) {
        _framesGrabbed = 0;
        _hasGrabVelocity = false;
        _curPosition = Vector3.zero;
        _lastPosition = Vector3.zero;
      }
    }
  }

  public bool BeGrabbedBy(Transform grabber) {
    if (_grabbingTransform != null) {

    }
    _grabbingTransform = grabber;
    _isAttached = true;
    _isWorkstation = false;
    RefreshAnchorState();
    DoOnGrabbed();
    return true;
  }

  protected virtual void DoOnGrabbed() { }

  public void ReleaseFromGrab(Transform grabber) {
    bool doOnReleased = false;
    if (_grabbingTransform != null && _grabbingTransform == grabber) {
      doOnReleased = true;
      _grabbingTransform = null;
    }

    if (doOnReleased) {
      DoOnReleasedFromGrab();
    }
  }

  private void DoOnReleasedFromGrab() {
    bool shouldActivateWorkstation = EvaluateShouldActivateWorkstation();

    if (shouldActivateWorkstation) {
      ActivateWorkstationTransition();
    }
    else {
      RefreshAnchorState();
    }
  }

  private bool EvaluateShouldActivateWorkstation() {
    bool shouldActivateWorkstation = false;

    if ((_wearableAnchor._chirality == Chirality.Left
      && _manager.LastGrabbedByLeftHand() == this)
     || (_wearableAnchor._chirality == Chirality.Right
      && _manager.LastGrabbedByRightHand() == this)) {
      return true;
    }

    if ((_wearableAnchor._chirality == Chirality.Left && _isLeftHandTracked)
     || (_wearableAnchor._chirality == Chirality.Right && _isRightHandTracked)) {

      float anchorDistance = Vector3.Distance(this.transform.position, _wearableAnchor.transform.position);
      if (anchorDistance < OVERRIDE_VELOCITY_RETURN_TO_ANCHOR_DISTANCE) {
        shouldActivateWorkstation = false;
      }
      else if (anchorDistance < VELOCITY_DEPENDENT_RETURN_TO_ANCHOR_DISTANCE) {
        if (IsVelocityAwayFromAnchor()) {
          shouldActivateWorkstation = true;
        }
        else {
          shouldActivateWorkstation = false;
        }
      }
      else {
        if (IsVelocityTowardAnchor()) {
          shouldActivateWorkstation = false;
        }
        else {
          shouldActivateWorkstation = true;
        }
      }
    }
    else {
      shouldActivateWorkstation = true;
    }
    return shouldActivateWorkstation;
  }

  private bool IsVelocityAwayFromAnchor() {
    return VelocityTowardsAnchorNormalizedDotProduct() < -0.2F;
  }

  private bool IsVelocityTowardAnchor() {
    return VelocityTowardsAnchorNormalizedDotProduct() > 0.85F;
  }

  private float VelocityTowardsAnchorNormalizedDotProduct() {
    Vector3 grabVelocity = GetGrabVelocity();
    Vector3 wearableToAnchor = _wearableAnchor.transform.position - this.transform.position;
    return Vector3.Dot(grabVelocity.normalized, wearableToAnchor.normalized);
  }

  private Vector3 GetGrabVelocity() {
    if (!_hasGrabVelocity) {
      Debug.Log("Don't have grab velocity, returning zero.");
      return Vector3.zero;
    }
    Vector3 grabVelocity = (_curPosition - _lastPosition) / Time.fixedDeltaTime;
    return grabVelocity;
  }

  #endregion

  #region Workstation Transition

  private Rigidbody _simulatedThrownBody = null;
  private Transform _workstationTargetLocation = null;
  private bool _isWorkstation = false;

  private float _optimalWorkstationDistance = 0.6F;
  private float _optimalWorkstationVerticalOffset = -0.3F;

  private bool _isRigidbodyUpdateScheduled = false;
  private Vector3 _scheduledRigidbodyPosition;
  private Quaternion _scheduledRigidbodyRotation;
  private Vector3 _scheduledRigidbodyVelocity;

  protected bool IsWorkstation {
    get { return _isWorkstation; }
  }

  private Transform _lerpFrom;
  private Transform _lerpTo;
  private TweenHandle ConstructWorkstationPlacementTween(Transform from, Transform to, float overTime) {
    _lerpFrom = from;
    _lerpTo = to;
    return Tween.Target(this.transform)
      .Rotation(from, to)
      .Value(0F, 1F, OnGetWorkstationEmergeTransformLerpAmount)
      .OverTime(overTime)
      .Smooth(TweenType.SMOOTH)
      .OnLeaveStart(DoOnMovementToWorkstationBegan)
      .OnReachEnd(DoOnMovementToWorkstationFinished);
  }

  private void OnGetWorkstationEmergeTransformLerpAmount(float lerpAmount) {
    this.transform.position = Vector3.Lerp(_lerpFrom.position, _lerpTo.position, lerpAmount);
  }

  private void ActivateWorkstationTransition() {
    Debug.Log(this.name + " is activating its workstation transition.");
    // Create rigidbody throw simulation object
    if (_simulatedThrownBody == null) {
      GameObject bodyObj = new GameObject(this.name + " Rigidbody Simulation Object");
      _simulatedThrownBody = bodyObj.AddComponent<Rigidbody>();
      _simulatedThrownBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
      _simulatedThrownBody.interpolation = RigidbodyInterpolation.Interpolate;
      bodyObj.AddComponent<DestroyAfterDuration>();
    }
    else {
      _simulatedThrownBody.GetComponent<DestroyAfterDuration>().SetTimer(5F);
    }
    Vector3 throwVelocity = GetGrabVelocity();
    ScheduleRigidbodyUpdate(this.transform.position, this.transform.rotation, throwVelocity);

    // Construct target workstation location object
    if (_workstationTargetLocation != null) {
      Destroy(_workstationTargetLocation.gameObject);
    }
    _workstationTargetLocation = new GameObject("WearableUI Workstation Target Location Object").GetComponent<Transform>();
    SetGoodWorkstationTransform(_workstationTargetLocation, this.transform.position, throwVelocity);
    
    // Determine how long the tween should take
    Vector3 targetPosition = _workstationTargetLocation.position;
    Vector3 velocityOffsetPosition = this.transform.position + 0.5F * Vector3.Distance(targetPosition, this.transform.position) * (throwVelocity.normalized);
    float timeCoefficient = 1.5F;
    float timeToTween = timeCoefficient * Vector3.Distance(velocityOffsetPosition, targetPosition);

    // Construct tween and play it.
    ConstructWorkstationPlacementTween(_simulatedThrownBody.transform, _workstationTargetLocation, timeToTween).Play();

    _isWorkstation = true;
  }

  private void ScheduleRigidbodyUpdate(Vector3 position, Quaternion rotation, Vector3 velocity) {
    _isRigidbodyUpdateScheduled = true;
    _scheduledRigidbodyPosition = position;
    _scheduledRigidbodyRotation = rotation;
    _scheduledRigidbodyVelocity = velocity;
  }

  private void FixedWorkstationTransitionUpdate() {
    if (_isRigidbodyUpdateScheduled) {
      _simulatedThrownBody.transform.position = _scheduledRigidbodyPosition;
      _simulatedThrownBody.transform.rotation = _scheduledRigidbodyRotation;
      _simulatedThrownBody.velocity = _scheduledRigidbodyVelocity;
      _isRigidbodyUpdateScheduled = false;
    }
  }

  private void SetGoodWorkstationTransform(Transform toSet, Vector3 initPosition, Vector3 initVelocity) {
    Transform centerEyeAnchor = _manager.GetCenterEyeAnchor();

    Vector3 workstationPosition;
    bool modifyHeight = true;
    if (initVelocity.magnitude < 0.7F) {
      // Just use current position as the position to choose.
      workstationPosition = initPosition;
      modifyHeight = false;
    }
    else {
      // Find projection direction
      Vector3 projectDirection;
      Vector3 groundAlignedInitVelocity = new Vector3(initVelocity.x, 0F, initVelocity.z);
      Vector3 effectiveLookDirection = centerEyeAnchor.forward;
      effectiveLookDirection = new Vector3(effectiveLookDirection.x, 0F, effectiveLookDirection.z);
      if (effectiveLookDirection.magnitude < 0.01F) {
        if (effectiveLookDirection.y > 0F) {
          projectDirection = -centerEyeAnchor.up;
        }
        else {
          projectDirection = centerEyeAnchor.up;
        }
      }
      if (initVelocity.magnitude < 0.5F || groundAlignedInitVelocity.magnitude < 0.01F) {
        projectDirection = effectiveLookDirection;
      }
      else {
        projectDirection = groundAlignedInitVelocity;
      }

      // Add a little bit of the effective look direction to the projectDirection to skew towards winding up
      // in front of the user unless they really throw it hard behind them
      float forwardSkewAmount = 1F;
      projectDirection += effectiveLookDirection * forwardSkewAmount;
      projectDirection = projectDirection.normalized;

      // Find good workstation position based on projection direction
      Vector3 workstationDirection = (initPosition + (projectDirection * 20F) - centerEyeAnchor.position);
      Vector3 groundAlignedWorkstationDirection = new Vector3(workstationDirection.x, 0F, workstationDirection.z).normalized;
      workstationPosition = centerEyeAnchor.position + _optimalWorkstationDistance * groundAlignedWorkstationDirection;
    }

    // Find a good workstation orientation
    Vector3 optimalLookVector = GetOptimalOrientationLookVector(centerEyeAnchor, workstationPosition);

    // Set the workstation target transform.
    toSet.position = new Vector3(workstationPosition.x,
      (modifyHeight ? centerEyeAnchor.position.y + _optimalWorkstationVerticalOffset : workstationPosition.y),
      workstationPosition.z);
    toSet.rotation = Quaternion.LookRotation(optimalLookVector);
  }

  protected virtual Vector3 GetOptimalOrientationLookVector(Transform centerEyeAnchor, Vector3 fromPosition) {
    Vector3 towardsCamera = centerEyeAnchor.position - fromPosition;
    return new Vector3(towardsCamera.x, 0F, towardsCamera.z).normalized;
  }

  protected virtual void DoOnMovementToWorkstationBegan() {
    Debug.Log(this.name + " got OnMovementToWorkstationBegan.");
    _isAttached = false;
  }

  protected virtual void DoOnMovementToWorkstationFinished() { }

  #endregion

}