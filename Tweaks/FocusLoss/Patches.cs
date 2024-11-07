using System;
using HarmonyLib;
using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SulfurTweaks.Tweaks.FocusLoss;

[HarmonyPatch]
public class Patches
{
    [HarmonyPatch(typeof(AudioSettingsManager), "Initialize")]
    [HarmonyPostfix]
    static void GameManagerStart(AudioSettingsManager __instance)
    {
        __instance.gameObject.AddComponent(typeof(FocusManager));
    }

    // Yes, this could be just `Application.runInBackground` but i dont think pausing the entire player
    // is a good idea when multiplayer is planned.
    private class FocusManager : MonoBehaviour
    {
        private AudioSettingsManager audioSettingsManager;
        private SettingsManager settingsManager;
        private float masterVolume;

        private void Start()
        {
            audioSettingsManager = GetComponentInParent<AudioSettingsManager>();
            settingsManager = StaticInstance<SettingsManager>.Instance;
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                if (!FocusLossConfig.MuteOnFocusLoss.Value) return;
                
                masterVolume = settingsManager.GetSetting("Audio_MasterVolume").currentValue;
                audioSettingsManager.SetPlayerVolumeMaster(masterVolume);
            }
            else
            {
                if (FocusLossConfig.MuteOnFocusLoss.Value)
                    audioSettingsManager.SetPlayerVolumeMaster(0f);
                
                if (FocusLossConfig.PauseOnFocusLoss.Value)
                    StaticInstance<GameManager>.Instance?.PauseGame();
            }
        }
    }
}