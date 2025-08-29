using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

public sealed class PicoManager : MonoBehaviour
{

    /// <summary>
    /// 用户带上头盔事件
    /// </summary>
    public static event Action HMDMounted;

    /// <summary>
    /// 用户取下头盔事件
    /// </summary>
    public static event Action HMDUnmounted;

    private static bool videoSeeThroughOpened;

    private bool hmdDetector;

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            ////大厅配置检测
            //if (IOHelper.IsExistFile(IOHelper.PicoRootPath + "config.txt") == false)
            //{
            //    IOHelper.CopyFile("config.txt", () =>
            //    {
            //        PXR_Enterprise.ControlSetDeviceAction(DeviceControlEnum.DEVICE_CONTROL_REBOOT, (int test) => { });
            //    });
            //}

            //IOHelper.CreateFolder(IOHelper.PicoRootPath + "bootanimation");

            //IOHelper.CopyFile("bootanimation/bootanimation.zip", () => { });
            //IOHelper.CopyFile("bootanimation/shutdownanimation.zip", () => { });

            //IOHelper.CopyFile("LOGO.png", () =>
            //{
            //    PXR_Enterprise.SetPowerOnOffLogo(PowerOnOffLogoEnum.PLPowerOnLogo, IOHelper.PicoRootPath + "LOGO.png", (x) => { Debug.Log(x); });
            //});
        }
    }

    void Update()
    {
        bool hmdstate;
        InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(CommonUsages.userPresence, out hmdstate);

        if (hmdDetector != hmdstate)
        {
            if (hmdstate == true)
                HMDMounted?.Invoke();
            else
                HMDUnmounted?.Invoke();
            hmdDetector = hmdstate;
        }
    }

    public static void Calibration()
    {
        if(Application.platform == RuntimePlatform.Android)
            InputDevices.GetDeviceAtXRNode(XRNode.Head).subsystem.TryRecenter();
    }

    public static void SetVideoSeeThrough(bool isOpen)
    {
        //if (videoSeeThroughOpened == isOpen) return;
        Camera camera = Camera.main;
        if (isOpen)
        {
            ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset).supportsHDR = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0, 0, 0, 0);
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            camera.cullingMask = 1 << 15;
            PXR_Manager.EnableVideoSeeThrough = true;
        }
        else
        {
            ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset).supportsHDR = true;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            camera.cullingMask = -1;
            PXR_Manager.EnableVideoSeeThrough = false;
        }
        videoSeeThroughOpened = isOpen;
    }

    public static void SetVideoSeeThroughForLayer(bool isOpen,int layer = ~0)//1 << 9 | 1 << 5 | 1 << 0
    {
        //if (videoSeeThroughOpened == isOpen) return;
        Camera camera = Camera.main;
        if (isOpen)
        {
            ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset).supportsHDR = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0, 0, 0, 0);
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            camera.cullingMask = layer;
            PXR_Manager.EnableVideoSeeThrough = true;
        }
        else
        {
            ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset).supportsHDR = true;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            camera.cullingMask = layer;
            PXR_Manager.EnableVideoSeeThrough = false;
        }
        videoSeeThroughOpened = isOpen;
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause && videoSeeThroughOpened)
        {
            PXR_Manager.EnableVideoSeeThrough = true;
        }
    }
}
