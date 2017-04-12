using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine;

namespace HoloToolkitExtensions.Utilities
{
    public static class LookingDirectionHelpers
    {
        public static Vector3 GetPostionInLookingDirection(float maxDistance = 2, BaseRayStabilizer stabilizer = null )
        {
            RaycastHit hitInfo;

            var ray = stabilizer != null
                ? stabilizer.StableRay
                : new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            if (SpatialMappingManager.Instance != null &&
                Physics.Raycast(ray, out hitInfo, maxDistance, SpatialMappingManager.Instance.LayerMask))
            {
                return hitInfo.point;
            }

            return CalculatePositionDeadAhead(maxDistance);
        }

        public static Vector3 CalculatePositionDeadAhead(float distance = 2, BaseRayStabilizer stabilizer = null)
        {
            return stabilizer != null
                ? stabilizer.StableRay.origin + stabilizer.StableRay.direction.normalized * distance
                : Camera.main.transform.position + Camera.main.transform.forward.normalized * distance;
        }
    }
}
