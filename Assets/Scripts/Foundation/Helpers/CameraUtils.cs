using Cinemachine;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BB
{
    public static class CameraUtils
    {
        public const int MainCameraPriority = 15;
        public const int DisabledCameraPriority = 10;
        public const float DefaultFieldOfView = 60f;
        public static Camera MainCamera
        {
            get
            {
                if (!_mainCamera)
                    _mainCamera = Camera.main;
                return _mainCamera;
            }
        }
        public static CinemachineVirtualCamera FreeCamera;
        public static CinemachineVirtualCamera CurrentCamera => Brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        private static Camera _mainCamera;
        public static CinemachineBrain Brain
        {
            get
            {
                if (!_brain)
                    _brain = MainCamera.GetComponent<CinemachineBrain>();
                return _brain;
            }
        }
        private static CinemachineBrain _brain;
        private static void DoWithBlend(CinemachineBlendDefinition.Style blend, Action action)
        {
            var defaultBlend = _brain.m_DefaultBlend.m_Style;
            _brain.m_DefaultBlend.m_Style = blend;
            action();
            _brain.m_DefaultBlend.m_Style = defaultBlend;
        }
        //public static void AlignThirdPersonCameraToCurrentView() => DefaultCameraThirdPersonExtension.Rotation.y = _mainCamera.transform.eulerAngles.y;
        public static void CutToCamera(ICinemachineCamera camera) => DoWithBlend(CinemachineBlendDefinition.Style.Cut, () => EnableCamera(camera));
        public static void EnableCamera(this ICinemachineCamera camera)
        {
            camera.Priority = Brain.ActiveVirtualCamera != null && Brain.ActiveVirtualCamera != camera ? Math.Max(Brain.ActiveVirtualCamera.Priority + 1, MainCameraPriority) : MainCameraPriority;
        }
        public static void DisableCamera(this ICinemachineCamera camera)
        {
            camera.Priority = DisabledCameraPriority;
        }
        public static float GetCameraWidth(float distance, float fov, float aspect)
        {
            var height = GetCameraHeight(distance, fov);
            var width = height * aspect;
            return width;
        }
        public static float GetWidth(this Camera camera, float distance)
        {
            return GetCameraWidth(distance, camera.fieldOfView, camera.aspect);
        }
        public static float GetHeight(this Camera camera, float distance)
        {
            return GetCameraHeight(distance, camera.fieldOfView);
        }
        public static float GetCameraHeight(float distance, float fov)
        {
            var result = 2.0f * distance * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            return result;
        }
        public static float GetCameraDistanceForHeight(float height, float fov)
        {
            var result = height * 0.5f / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            return result;
        }
        public static float GetCameraFOVForDistanceAndHeight(float distance, float height)
        {
            var result = 2.0f * Mathf.Atan(height * 0.5f / distance) * Mathf.Rad2Deg;
            return result;
        }
        public static Vector3 GetCameraAlignedHorizontal(Vector2 direction)
        {
            var cameraRot = Quaternion.Euler(0, MainCamera.transform.rotation.eulerAngles.y, 0);
            var result = cameraRot * direction.Horizontal();
            return result;
        }
        //public static bool FollowsPlayer(this CinemachineVirtualCamera camera)
        //{
        //    var follow = camera.GetFollowPlayerExtension();
        //    return follow;
        //}
        //public static void SetPlayerFollow(this CinemachineVirtualCamera camera, bool value)
        //{
        //    var follow = camera.GetFollowPlayerExtension();
        //    if (value)
        //    {
        //        if (!follow)
        //            camera.gameObject.AddComponent<RestrictCameraMovementExtension>();
        //        if (!camera.GetCinemachineComponent<CinemachineFramingTransposer>())
        //            camera.AddDefaultFramingTransposer();
        //        var player = PlayerGS.Get();
        //        if (player)
        //            camera.Follow = player.transform;
        //    }
        //    else
        //    {
        //        if (follow)
        //            UnityHelper.Destroy(follow);
        //        camera.DestroyCinemachineComponent<CinemachineFramingTransposer>();
        //        camera.Follow = null;
        //    }
        //}
        public static void AddDefaultFramingTransposer(this CinemachineVirtualCamera camera)
        {
            //position camera
            var composer = camera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (!composer)
                composer = camera.AddCinemachineComponent<CinemachineFramingTransposer>();
            composer.m_ScreenY = 0.95f;
            composer.m_CameraDistance = 6;
        }
        //private static RestrictCameraMovementExtension GetFollowPlayerExtension(this CinemachineVirtualCamera camera)
        //{
        //    return camera.GetComponent<RestrictCameraMovementExtension>();
        //}
        public static Vector3 WorldToScreen(Vector3 position) => MainCamera.WorldToScreenPoint(position);
        public static Vector3 ScreenToWorld(Vector3 position) => MainCamera.ScreenToWorldPoint(position);
        public static Vector3 WorldToScreenCenter(Vector3 position) => WorldToScreen(position) - ScreenCenter.Z(0);
        public static Vector2 ScreenCenter => new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        public static void Align(this CinemachineVirtualCamera camera, CinemachineVirtualCamera target) => camera.transform.rotation = target.transform.rotation;
        public static Ray ScreenCenterRay => MainCamera.ScreenPointToRay(ScreenCenter);
		public static Ray GetMouseWorldRay()
		{
			var position =Mouse.current.position.ReadValue();
			return MainCamera.ScreenPointToRay(position);
		}
        //public static void AlignThirdPersonCameras(this CinemachineVirtualCamera camera, CinemachineVirtualCamera target) => Align(camera.GetComponent<ThirdPersonCinemachineExtension>(), target.GetComponent<ThirdPersonCinemachineExtension>());
        //public static void Align(this ThirdPersonCinemachineExtension camera, ThirdPersonCinemachineExtension target) => camera.Rotation = target.Rotation;
    }
}