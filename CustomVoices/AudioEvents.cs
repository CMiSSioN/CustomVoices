using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;

namespace CustomVoices {
  [HarmonyPatch(typeof(Briefing))]
  [HarmonyPatch("SetStartButtonActive")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  internal static class Briefing_SetStartButtonActive {
    private static void SetStartButtonActive(this Briefing instance, bool isActive) {
      if (isActive)
        Traverse.Create(instance).Field<HBSDOTweenButton>("startButton").Value.SetState(ButtonState.Enabled);
      else
        Traverse.Create(instance).Field<HBSDOTweenButton>("startButton").Value.SetState(ButtonState.Disabled);
    }
    public class SetStartButtonActiveDelegate {
      public Briefing instance;
      public SetStartButtonActiveDelegate(Briefing i) { this.instance = i; }
      public void SetStartButtonActive() {
        Log.M?.TWL(0, "Briefing_SetStartButtonActive.SetStartButtonActive");
        this.instance.SetStartButtonActive(true);
      }
    }
    public static bool Prefix(Briefing __instance,bool isActive) {
      Log.M?.TWL(0, "Briefing.SetStartButtonActive");
      try {
        if (isActive == false) { return true; }
        AudioEngine.Instance.VoiceOverBus.StopAll();
        AudioEngine.Instance.VoiceOverBus.Play("nominal", false, new List<AudioEngine.AudioChannelEvent>() { new AudioEngine.AudioChannelEvent(float.NaN, new SetStartButtonActiveDelegate(__instance).SetStartButtonActive) });
        try {
          GameInstance battleTechGame = Traverse.Create(__instance).Field<GameInstance>("battleTechGame").Value;
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