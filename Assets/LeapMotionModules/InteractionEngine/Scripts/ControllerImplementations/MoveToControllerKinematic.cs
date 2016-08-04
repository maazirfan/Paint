﻿using System;
using UnityEngine;

namespace Leap.Unity.Interaction {

  public class MoveToControllerKinematic : IMoveToController {

    public override void MoveTo(ReadonlyList<Hand> hands, PhysicsMoveInfo info, Vector3 solvedPosition, Quaternion solvedRotation) {
      if (info.shouldTeleport) {
        _obj.warper.Teleport(solvedPosition, solvedRotation);
      } else {
        _obj.rigidbody.MovePosition(solvedPosition);
        _obj.rigidbody.MoveRotation(solvedRotation);
      }
    }

    public override void SetGraspedState() {
      _obj.rigidbody.isKinematic = true;
    }

    public override void OnGraspBegin() { }

    public override void OnGraspEnd() { }
  }
}
