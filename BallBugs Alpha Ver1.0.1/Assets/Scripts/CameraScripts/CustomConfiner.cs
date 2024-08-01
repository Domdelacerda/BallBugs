using Cinemachine.Utility;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Credit for CustomConfiner goes to user SudoCat on the Unity Forum: https://forum.unity.com/threads/unrelaible-results-using-cinemachine-confiner-with-group-framing-transposer.1490167/
public class CustomConfiner : CinemachineExtension
{
    [Range(0, 5)]
    [SerializeField] private float damping;

    [SerializeField] private float bottomScreenOmitZone;
    [SerializeField] private Bounds bounds;

    class VcamExtraState
    {
        public Vector3 dampedDisplacement;
        public Vector3 previousDisplacement;
    };

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize) return;
        if (vcam.State.Lens.Orthographic)
        {
            // currently not needed!
        }
        else
        {
            var extra = GetExtraState<VcamExtraState>(vcam);
            var constraint = bounds;
            var fov = state.Lens.FieldOfView;
            var position = state.CorrectedPosition;

            var distance = Mathf.Abs(constraint.center.z - position.z);
            var frustumHeight = 2.0f * distance * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            var frustumWidth = frustumHeight * state.Lens.Aspect;
            var center = new Vector3(position.x, position.y, constraint.center.z);
            var size = new Vector3(frustumWidth, frustumHeight, 1f);
            var cameraBounds = new Bounds(center, size);
            var newBounds = ConstrainBounds(constraint, cameraBounds, state.Lens.Aspect);

            var prev = extra.previousDisplacement;
            var displacement = newBounds.center - position;
            displacement.z = -(newBounds.extents.y / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) - distance);
            extra.previousDisplacement = displacement;

            if (!VirtualCamera.PreviousStateIsValid || deltaTime < 0 || damping <= 0)
                extra.dampedDisplacement = Vector3.zero;
            else
            {
                // If a big change from previous frame's desired displacement is detected,
                // extract difference for damping
                if (prev.sqrMagnitude > 0.01f && Vector2.Angle(prev, displacement) > 10)
                    extra.dampedDisplacement += displacement - prev;

                extra.dampedDisplacement -= Damper.Damp(extra.dampedDisplacement, damping, deltaTime);
                displacement -= extra.dampedDisplacement;
            }

            state.PositionCorrection += displacement;
        }
    }

    private Bounds ConstrainBounds(Bounds constraint, Bounds target, float aspect)
    {
        // first check if too big
        var height = Mathf.Min(constraint.extents.y, Mathf.Max(target.extents.x / aspect, target.extents.y));
        target.extents = new Vector3(height * aspect, height);

        // next check if over the bounds
        var xMinOverhang = Mathf.Max(0, constraint.min.x - target.min.x);
        var yMinOverhang = Mathf.Max(0, constraint.min.y - target.min.y);
        var xMaxOverhang = Mathf.Min(0, constraint.max.x - target.max.x);
        var yMaxOverhang = Mathf.Min(0, constraint.max.y - target.max.y);
        target.center += new Vector3(xMinOverhang + xMaxOverhang, yMinOverhang + yMaxOverhang);

        return target;
    }

    // This function is copied straight out of Cinemachine's built-in confiner class.
    static Bounds GetScreenSpaceGroupBoundingBox(
        ICinemachineTargetGroup group, ref Vector3 pos, Quaternion orientation)
    {
        var observer = Matrix4x4.TRS(pos, orientation, Vector3.one);
        group.GetViewSpaceAngularBounds(observer, out var minAngles, out var maxAngles, out var zRange);
        var shift = (minAngles + maxAngles) / 2;

        var q = Quaternion.identity.ApplyCameraRotation(new Vector2(-shift.x, shift.y), Vector3.up);
        pos = q * new Vector3(0, 0, (zRange.y + zRange.x) / 2);
        pos.z = 0;
        pos = observer.MultiplyPoint3x4(pos);
        observer = Matrix4x4.TRS(pos, orientation, Vector3.one);
        group.GetViewSpaceAngularBounds(observer, out minAngles, out maxAngles, out zRange);

        // For width and height (in camera space) of the bounding box, we use the values at the center of the box.
        // This is an arbitrary choice.  The gizmo drawer will take this into account when displaying
        // the frustum bounds of the group
        var d = zRange.y + zRange.x;
        Vector2 angles = new Vector2(89.5f, 89.5f);
        if (zRange.x > 0)
        {
            angles = Vector2.Max(maxAngles, UnityVectorExtensions.Abs(minAngles));
            angles = Vector2.Min(angles, new Vector2(89.5f, 89.5f));
        }
        angles *= Mathf.Deg2Rad;
        return new Bounds(
            new Vector3(0, 0, d / 2),
            new Vector3(Mathf.Tan(angles.y) * d, Mathf.Tan(angles.x) * d, zRange.y - zRange.x));
    }
}
