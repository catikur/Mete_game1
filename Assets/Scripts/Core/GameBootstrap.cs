using MeteGame.CameraRig;
using MeteGame.City;
using MeteGame.Controls;
using MeteGame.Missions;
using MeteGame.Traffic;
using MeteGame.UI;
using MeteGame.Vehicle;
using UnityEngine;

namespace MeteGame.Core
{
    /// <summary>
    /// City sahnesinin tek kök bileşeni. Sahne dosyası neredeyse boştur;
    /// şehir, araç, ışık, kamera, HUD ve görev sistemi burada koddan kurulur.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        void Awake()
        {
            Application.targetFrameRate = 60;
            SaveManager.RefreshDaily();
            GarageShop.Normalize(SaveManager.Data);
            DriveInput.Locked = false;
            DriveInput.ResetTouch();
            SceneFlow.ResumeTime();

            var layout = CityBuilder.Build(transform);
            Vector3 spawn = layout.PlayerSpawnPosition;
            Quaternion facing = Quaternion.identity;
            var session = SaveManager.Data.city;
            if (session != null && session.active)
            {
                float h = layout.HalfExtent - 6f;
                spawn = new Vector3(
                    Mathf.Clamp(session.x, -h, h),
                    0.5f,
                    Mathf.Clamp(session.z, -h, h));
                facing = Quaternion.Euler(0f, session.yaw, 0f);
            }

            var vehicle = VehicleFactory.CreatePlayerVehicle(spawn, facing);

            CreateSun();
            CreateCamera(vehicle);

            var hud = HudController.Build(vehicle.transform);

            TrafficSystem.Spawn(transform, layout, vehicle, hud);

            var missions = gameObject.AddComponent<MissionManager>();
            missions.Init(layout, vehicle, hud);
            hud.SetSessionHandlers(
                missions.CaptureSession,
                () =>
                {
                    missions.CaptureSession();
                    SceneFlow.OpenMenu();
                });
            hud.SetGarageHandler(() =>
            {
                if (missions.IsOnDuty)
                {
                    hud.ShowToast("Önce görevi bitir!");
                    return;
                }
                missions.CaptureSession();
                SceneFlow.OpenGarage("City");
            });
        }

        static void CreateSun()
        {
            var go = new GameObject("Sun");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.88f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
        }

        static void CreateCamera(VehicleController vehicle)
        {
            var go = new GameObject("GameCamera");
            go.tag = "MainCamera";
            var camera = go.AddComponent<Camera>();
            camera.fieldOfView = 50f;
            camera.nearClipPlane = 1f;
            camera.farClipPlane = 500f;
            go.AddComponent<AudioListener>();

            var follow = go.AddComponent<FollowCamera>();
            follow.SetTarget(vehicle.transform, vehicle.Body);
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
                Persist();
        }

        void OnApplicationFocus(bool focused)
        {
            if (!focused)
                Persist();
        }

        void OnApplicationQuit()
        {
            Persist();
        }

        void Persist()
        {
            var missions = GetComponent<MissionManager>();
            if (missions != null)
                missions.CaptureSession();
            SaveManager.Save();
        }
    }
}
