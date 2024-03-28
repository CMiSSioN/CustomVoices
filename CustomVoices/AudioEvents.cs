using BattleTech;
using BattleTech.UI;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomVoices {
  [HarmonyPatch(typeof(Briefing))]
  [HarmonyPatch("SetStartButtonActive")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  internal static class Briefing_SetStartButtonActive {
    private static void _SetStartButtonActive(this Briefing instance, bool isActive) {
      Log.M?.TWL(0, $"Briefing_SetStartButtonActive.SetStartButtonActive {isActive}", true);
      if (isActive)
        instance?.startButton?.SetState(ButtonState.Enabled);
      else
        instance?.startButton?.SetState(ButtonState.Disabled);
    }
    public static IEnumerator PlayStartAudio(Briefing __instance) {
      AudioEngine.Instance.VoiceOverBus.StopAll();
      var sample = AudioEngine.Instance.VoiceOverBus.Play("nominal", false);
      if (sample == null) {
        Log.M?.TWL(0, "starting sample not found. waiting fallback 10 seconds", true);
        yield return new WaitForSeconds(10);
      } else {
        Log.M?.TWL(0, $"starting sample found {sample.length}",true);
        if (sample.length < 5f) { yield return new WaitForSeconds(10); } else { yield return new WaitForSeconds(sample.length); }
      }
      __instance._SetStartButtonActive(true);
      yield break;
    }
    public static bool Prefix(Briefing __instance,bool isActive) {
      Log.M?.TWL(0, "Briefing.SetStartButtonActive");
      try {
        if (isActive == false) { return true; }
        try {
          GameInstance battleTechGame = __instance.battleTechGame;
          UnityGameInstance.Instance.StartCoroutine(PlayStartAudio(__instance));
          Contract activeContract = battleTechGame.Combat.ActiveContract;
          SpawnableUnit[] units = activeContract.Lances.GetLanceUnits(battleTechGame.Combat.LocalPlayerTeamGuid);
          foreach (SpawnableUnit unit in units) {
            if (unit.Pilot == null) { continue; }
            if (Core.TryGetExtVoice(unit.Pilot.Voice, out VoicePackDef voicePack)) {
              if (string.IsNullOrEmpty(voicePack.vobank)) {
                Log.M?.TWL(0,"found external voice. caching samples");
                foreach(var phrase in voicePack.light_phrases) {
                  foreach (string id in phrase.Value) {
                    try {
                      Log.M?.WL(1, id);
                      AudioEngine.Instance?.VoiceOverBus?.Preload(id);
                    }catch(Exception e) {
                      Log.M?.TWL(0,e.Message);
                    }
                  }
                }
                foreach (var phrase in voicePack.dark_phrases) {
                  foreach (string id in phrase.Value) {
                    try {
                      Log.M?.WL(1, id);
                      AudioEngine.Instance?.VoiceOverBus?.Preload(id);
                    } catch (Exception e) {
                      Log.M?.TWL(0, e.Message);
                    }
                  }
                }
              }
            }
          }
        } catch(Exception e) {
          Log.M?.TWL(0,e.ToString(),true);
        }
        return false;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
}