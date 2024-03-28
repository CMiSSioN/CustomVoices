using BattleTech;
using BattleTech.Data;
using BattleTech.Portraits;
using BattleTech.UI;
using HarmonyLib;
using HBS;
using HBS.Data;
using Localize;
using ManagedBass;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace CustomVoices {
  [HarmonyPatch(typeof(CombatHUDButtonBase))]
  [HarmonyPatch("OnClick")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDActionButton_ExecuteClick {
    private static GenericPopup popup = null;
    public static bool Prefix(CombatHUDActionButton __instance) {
      Log.M?.TWL(0, "CombatHUDActionButton.ExecuteClick '" + __instance.GUID + "'/'" + CombatHUD.ButtonID_Sprint + "' " + (__instance.GUID == CombatHUD.ButtonID_Sprint) + "\n");
      CombatHUD HUD = (CombatHUD)typeof(CombatHUDActionButton).GetProperty("HUD", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance, null);
      if (__instance.GUID == CombatHUD.ButtonID_Sprint) {
        Log.M?.WL(1, "button is sprint");
        bool modifyers = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        if (modifyers) {
          Log.M?.WL(1, "ctrl is pressed");
          if (HUD.SelectedActor != null) {
            Log.M?.WL(1, "actor is selected");
            if (HUD.SelectedActor is Mech) {
              Log.M?.WL(1, "mech is selected");
              Dictionary<AudioSwitch_dialog_lines_pilots, VOEvents> voEvents = new Dictionary<AudioSwitch_dialog_lines_pilots, VOEvents>();
              foreach (VOEvents voEvent in Enum.GetValues(typeof(VOEvents))) {
                VOEntryData voEntryData = AudioEventManager.AudioConstants.GetVOEntryData(voEvent);
                if (voEvents.ContainsKey(voEntryData.switchValue) == false) { voEvents.Add(voEntryData.switchValue, voEvent); };
              };
              List<AudioSwitch_dialog_lines_pilots> dlgs = new List<AudioSwitch_dialog_lines_pilots>();
              foreach (AudioSwitch_dialog_lines_pilots dlgEvent in Enum.GetValues(typeof(AudioSwitch_dialog_lines_pilots))) {
                dlgs.Add(dlgEvent);
              };
              StringBuilder text = new StringBuilder();
              int curIndex = 0;
              int pageSize = 10;
              for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                if (index >= dlgs.Count) { break; }
                if (index == curIndex) { text.Append("->"); };
                text.Append(dlgs[index].ToString());
                if (voEvents.ContainsKey(dlgs[index])) { text.Append(" - " + voEvents[dlgs[index]].ToString()); };
                text.AppendLine();
              }

              popup = GenericPopupBuilder.Create("audio pack", text.ToString())
                .AddButton("X", (Action)(() => { }), true)
                .AddButton("->", (Action)(() => {
                  if (curIndex < (dlgs.Count - 1)) {
                    ++curIndex;
                    text.Clear();
                    for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                      if (index >= dlgs.Count) { break; }
                      if (index == curIndex) { text.Append("->"); };
                      text.Append(dlgs[index].ToString());
                      if (voEvents.ContainsKey(dlgs[index])) { text.Append(" - " + voEvents[dlgs[index]].ToString()); };
                      text.AppendLine();
                    }
                    if (popup != null) popup.TextContent = text.ToString();
                  }
                }), false)
                .AddButton("<-", (Action)(() => {
                  if (curIndex > 0) {
                    --curIndex;
                    text.Clear();
                    for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                      if (index >= dlgs.Count) { break; }
                      if (index == curIndex) { text.Append("->"); };
                      text.Append(dlgs[index].ToString());
                      if (voEvents.ContainsKey(dlgs[index])) { text.Append(" - " + voEvents[dlgs[index]].ToString()); };
                      text.AppendLine();
                    }
                    if (popup != null) popup.TextContent = text.ToString();
                  }
                }), false).AddButton("P", (Action)(() => {
                  if (voEvents.ContainsKey(dlgs[curIndex])) {
                    AudioEventManager.PlayPilotVO(voEvents[dlgs[curIndex]], HUD.SelectedActor, (AkCallbackManager.EventCallback)null, (object)null, true);
                  }
                }), false).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
            }
          }
          return false;
        }
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(SGBarracksMWCustomizationPopup))]
  [HarmonyPatch("LoadTextSelectorOptions")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SGBarracksMWCustomizationPopup_LoadTextSelectorOptions {
    private static Action<SGBarracksMWCustomizationPopup> SGBarracksMWCustomizationPopup_LoadTextSelectorOptionsBase = null;
    public static bool Prepare() {
      var method = typeof(SGCharacterCreationPortraitCustomization).GetMethod("LoadTextSelectorOptions", BindingFlags.Instance | BindingFlags.NonPublic);
      var dm = new DynamicMethod("CACLoadTextSelectorOptionsBase", null, new Type[] { typeof(SGBarracksMWCustomizationPopup) }, typeof(SGBarracksMWCustomizationPopup));
      var gen = dm.GetILGenerator();
      gen.Emit(OpCodes.Ldarg_0);
      gen.Emit(OpCodes.Call, method);
      gen.Emit(OpCodes.Ret);
      SGBarracksMWCustomizationPopup_LoadTextSelectorOptionsBase = (Action<SGBarracksMWCustomizationPopup>)dm.CreateDelegate(typeof(Action<SGBarracksMWCustomizationPopup>));
      return true;
    }
    public static bool Prefix(SGBarracksMWCustomizationPopup __instance, ref HorizontalScrollSelectorText ___voiceSelector, ref Dictionary<string, int> ___voiceIdMap) {
      Log.M?.TWL(0, "SGBarracksMWCustomizationPopup.LoadTextSelectorOptions");
      SGBarracksMWCustomizationPopup_LoadTextSelectorOptionsBase.Invoke(__instance);
      if (___voiceSelector.options.Count != 0) { return false; }
      List<string> voicesNames = new List<string>();
      List<string> voicesUINames = new List<string>();
      foreach (var defVoice in Core.settings.defaultVoices) {
        voicesNames.Add(defVoice.Key);
        voicesUINames.Add(new Text(defVoice.Value).ToString());
      }
      foreach (var custVoice in Core.extVoicePacks) {
        voicesNames.Add(custVoice.Key);
        if (string.IsNullOrEmpty(custVoice.Value.uiname)) {
          voicesUINames.Add(custVoice.Key);
        } else {
          voicesUINames.Add(new Text(custVoice.Value.uiname).ToString());
        }
      }
      for (int index = 0; index < voicesNames.Count; ++index) {
        ___voiceIdMap.Add(voicesNames[index], index);
        Log.M?.WL(1, voicesNames[index] + ":" + voicesUINames[index]);
      }
      ___voiceSelector.SetOptions(voicesUINames.ToArray());
      return false;
    }
  }
  [HarmonyPatch(typeof(SGCharacterCreationWidget))]
  [HarmonyPatch("CreatePilot")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SGCharacterCreationWidget_CreatePilot {
    public static void Postfix(SGCharacterCreationWidget __instance, ref Pilot __result) {
      if (__result == null) { return; }
      string voice = SGCharacterCreationNamePanel_Awake.GetSelectedVoice();
      if (string.IsNullOrEmpty(voice) == false) {
        __result.pilotDef.SetVoice(SGCharacterCreationNamePanel_Awake.GetSelectedVoice());
        Log.M?.TWL(0, "SGCharacterCreationWidget.CreatePilot voice:"+voice);
      }
    }
  }
  [HarmonyPatch(typeof(SGBarracksMWDetailPanel))]
  [HarmonyPatch("CustomizePilot")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SGBarracksMWDetailPanel_CustomizePilot {
    private class RenderedPortraitResultDelegate {
      private SGBarracksDossierPanel dossier;
      public RenderedPortraitResultDelegate(SGBarracksDossierPanel dossier) { this.dossier = dossier; }
      public void UpdatePortrait(RenderedPortraitResult renderResult) {
        Log.M?.TWL(0, "SGBarracksDossierPanel.UpdatePortrait");
        typeof(SGBarracksDossierPanel).GetMethod("UpdatePortrait", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this.dossier, new object[] { renderResult });
      }
    }
    public static bool Prefix(SGBarracksMWDetailPanel __instance, Pilot ___curPilot, SGBarracksWidget ___barracks, SGBarracksDossierPanel ___dossier) {
      SGBarracksMWCustomizationPopup popupModule = LazySingletonBehavior<UIManager>.Instance.GetOrCreatePopupModule<SGBarracksMWCustomizationPopup>("", true);
      popupModule.LoadPilot(___curPilot, true);
      popupModule.SetRenderedPortraitCallback(new Action<RenderedPortraitResult>(new RenderedPortraitResultDelegate(___dossier).UpdatePortrait));
      popupModule.AddOnPooledAction((Action)(() => {
        __instance.DisplayPilot(___curPilot);
        ___barracks.Reset(___curPilot);
      }));
      return false;
    }
  }
  [HarmonyPatch(typeof(SGCharacterCreationNamePanel))]
  [HarmonyPatch("Awake")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SGCharacterCreationNamePanel_Awake {
    private static GameObject voiceSelectorObj = null;
    public static Dictionary<int, string> voiceIdMap = new Dictionary<int, string>();
    public static Component FindComponentParentRecurcive<T>(this Transform tr) where T : Component {
      if (tr.parent == null) { return null; }
      T result = tr.parent.gameObject.GetComponent<T>();
      if (result != null) { return result; }
      return FindComponentParentRecurcive<T>(tr.parent);
    }
    public static string GetSelectedVoice() {
      if (voiceSelectorObj == null) { return string.Empty; }
      HorizontalScrollSelectorText voiceSelector = voiceSelectorObj.GetComponent<HorizontalScrollSelectorText>();
      if (voiceIdMap.TryGetValue(voiceSelector.selectionIdx, out string voice)) { return voice; };
      return string.Empty;
    }
    public static void Postfix(SGCharacterCreationNamePanel __instance) {
      Log.M?.TWL(0, "SGCharacterCreationNamePanel.Awake");
      if (__instance.pronounSelector != null) {
        Log.M?.WL(1, "pronounSelector is not null");
        if (voiceSelectorObj != null) { GameObject.Destroy(voiceSelectorObj); voiceSelectorObj = null; };
        if (__instance.transform.FindComponentParentRecurcive<SGBarracksMWCustomizationPopup>() != null) { return; }
        voiceSelectorObj = GameObject.Instantiate(__instance.pronounSelector.gameObject, __instance.pronounSelector.gameObject.transform.parent);
        voiceSelectorObj.name = "voiceSelector";
        voiceSelectorObj.transform.SetParent(__instance.pronounSelector.gameObject.transform.parent, false);
        //voiceSelectorObj.transform.localPosition = __instance.pronounSelector.transform.localPosition;
        voiceSelectorObj.transform.localRotation = __instance.pronounSelector.transform.localRotation;
        voiceSelectorObj.layer = __instance.pronounSelector.gameObject.layer;
        RectTransform rectTr = voiceSelectorObj.GetComponent<RectTransform>();
        Transform bracket_btm_tr = __instance.pronounSelector.gameObject.transform.parent.transform.parent.transform.parent.Find("bracket-btm");
        if(rectTr != null) {
          Log.M?.WL(1, "rect:" + rectTr.rect);
          if (bracket_btm_tr != null) {
            Log.M?.WL(1, "bracket_btm_tr:" + bracket_btm_tr.localPosition);
            bracket_btm_tr.localPosition += Vector3.down * rectTr.rect.height * 1.5f;
          }
        }
        //Log.M?.printComponents(__instance.pronounSelector.gameObject.transform.parent.transform.parent.gameObject, 2);
        //voiceSelectorObj.transform.position += Vector3.down * Core.settings.voiceSelectorDownOffset;
        //Vector3 curScale = __instance.pronounSelector.transform.parent.localScale;
        //curScale.y *= 1.25f;
        //curScale = __instance.firstName.transform.localScale; curScale.y *= 0.8f; __instance.firstName.transform.localScale = curScale;
        //curScale = __instance.lastName.transform.localScale; curScale.y *= 0.8f; __instance.lastName.transform.localScale = curScale;
        //curScale = __instance.callsign.transform.localScale; curScale.y *= 0.8f; __instance.callsign.transform.localScale = curScale;
        //curScale = __instance.pronounSelector.transform.localScale; curScale.y *= 0.8f; __instance.pronounSelector.transform.localScale = curScale;
        //curScale = voiceSelectorObj.transform.localScale; curScale.y *= 0.8f; voiceSelectorObj.transform.localScale = curScale;
        voiceSelectorObj.SetActive(true);
        List<string> voicesNames = new List<string>();
        List<string> voicesUINames = new List<string>();
        foreach (var defVoice in Core.settings.defaultVoices) {
          voicesNames.Add(defVoice.Key);
          voicesUINames.Add(new Text(defVoice.Value).ToString());
        }
        foreach (var custVoice in Core.extVoicePacks) {
          voicesNames.Add(custVoice.Key);
          if (string.IsNullOrEmpty(custVoice.Value.uiname)) {
            voicesUINames.Add(custVoice.Key);
          } else {
            voicesUINames.Add(new Text(custVoice.Value.uiname).ToString());
          }
        }
        voiceIdMap.Clear();
        for (int index = 0; index < voicesNames.Count; ++index) {
          Log.M?.WL(1, voicesNames[index] + ":" + voicesUINames[index]);
          voiceIdMap.Add(index, voicesNames[index]);
        }
        HorizontalScrollSelectorText voiceSelector = voiceSelectorObj.GetComponent<HorizontalScrollSelectorText>();
        voiceSelector.headerTextUI.SetText(new Text(Core.settings.voiceSelectorName).ToString());
        voiceSelector.AddOptions(voicesUINames.ToArray());
        voiceSelector.onValueChanged += (UnityAction)(() => {
          string voice = SGCharacterCreationNamePanel_Awake.GetSelectedVoice();
          if (string.IsNullOrEmpty(voice) == false) {
            SGBarracksDossierPanel.PlayVO(voice);
          }
        });
      } else {
        Log.M?.WL(1, "pronounSelector is null");
      }
    }
  }
  [HarmonyPatch(typeof(WwiseManager))]
  [HarmonyPatch("SetSwitchById")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(uint), typeof(uint), typeof(AkGameObj) })]
  public static class WwiseManager_SetSwitchById {
    private static HashSet<AkGameObj> darklightSwitches = new HashSet<AkGameObj>();
    private static uint darkLightGroup = uint.MaxValue;
    private static uint darkValue = uint.MaxValue;
    private static uint lightValue = uint.MaxValue;
    public static bool isDarkTheme(this AkGameObj obj) { return darklightSwitches.Contains(obj); }
    public static void Postfix(WwiseManager __instance, uint switchGroup, uint switchId, AkGameObj sourceObject) {
      //Log.M?.TWL(0, "WwiseManager.SetSwitchById grp:"+ switchGroup+" id:"+switchId);
      if (darkLightGroup == uint.MaxValue) { darkLightGroup = __instance.EnumTypeToSwitchGroup<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.dark); }
      if (darkValue == uint.MaxValue) { darkValue = __instance.EnumValueToSwitchId<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.dark); }
      if (lightValue == uint.MaxValue) { lightValue = __instance.EnumValueToSwitchId<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.light); }
      //Log.M?.WL(1, "darklight group:"+ darkLightGroup+" darkId:"+darkValue+" lightId:"+lightValue);
      if (switchGroup == darkLightGroup) {
        if (switchId == darkValue) {
          Log.M?.TWL(0, sourceObject.name + ":darkTheme");
          darklightSwitches.Add(sourceObject);
        } else {
          Log.M?.TWL(0, sourceObject.name + ":lightTheme");
          darklightSwitches.Remove(sourceObject);
        }
      }
    }
  }
  [HarmonyPatch(typeof(Pilot))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(AbstractActor) })]
  public static class Pilot_Init {
    private static PropertyInfo p_Combat = typeof(Pilot).GetProperty("Combat", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo f__parentActor = typeof(Pilot).GetField("_parentActor", BindingFlags.Instance | BindingFlags.NonPublic);
    private static MethodInfo m_InitStats = typeof(Pilot).GetMethod("InitStats", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void Combat(this Pilot pilot, CombatGameState Combat) { p_Combat.SetValue(pilot, Combat); }
    public static void _parentActor(this Pilot pilot, AbstractActor _parentActor) { f__parentActor.SetValue(pilot, _parentActor); }
    public static void InitStats(this Pilot pilot) { m_InitStats.Invoke(pilot, new object[] { }); }
    public static bool Prefix(Pilot __instance, CombatGameState Combat, AbstractActor pilotedActor) {
      //float scale = CustomAmmoCategories.Settings.bloodSettings.DecalScale[];
      Log.M?.TWL(0, "Pilot.Init " + __instance.pilotDef.Description.Id);
      try {
        __instance.Combat(Combat);
        __instance._parentActor(pilotedActor);
        __instance.InitStats();
        __instance.pilotVoice = AudioSwitch_dialog_character_type_pilots.f_pro01;
        if (string.IsNullOrEmpty(__instance.pilotDef.Voice) || Utilities.EnumTryParse<AudioSwitch_dialog_character_type_pilots>(__instance.pilotDef.Voice, out __instance.pilotVoice)) {
        } else {
          if (Core.TryGetExtVoice(__instance.pilotDef.Voice, out VoicePackDef extVoice)) {
            __instance.pilotVoice = extVoice.baseVoice;
          } else {
            Log.M?.TWL(0, "Can't find external voice " + __instance.pilotDef.Voice, true);
          }
        }
        return false;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  public static class VOSoundBankLoadHelper {
    private static FieldInfo f_loadedBanks = typeof(WwiseManager).GetField("loadedBanks", BindingFlags.Instance | BindingFlags.NonPublic);
    public static List<LoadedAudioBank> loadedBanks(this WwiseManager manager) {
      return (List<LoadedAudioBank>)f_loadedBanks.GetValue(manager);
    }
    public static void LoadVOSoundBank(ModTek.SoundBankDef def) {
      if (!SceneSingletonBehavior<WwiseManager>.HasInstance) { return; }
      SceneSingletonBehavior<WwiseManager>.Instance.loadedBanks().Add(new LoadedAudioBank(def.name, true, false));
      SceneSingletonBehavior<WwiseManager>.Instance.voBanks.Add(def.name);
    }
  }
  [HarmonyPatch(typeof(SGBarracksDossierPanel), "PlayPilotSelectionVO", new Type[] { typeof(Pilot) })]
  public static class SGBarracksDossierPanel_PlayPilotSelectionVO_Patch {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      int startIndex = -1;
      var codes = new List<CodeInstruction>(instructions);
      for (int i = 0; i < codes.Count; i++) {
        if (codes[i].opcode == OpCodes.Callvirt && (codes[i].operand as MethodInfo)?.Name == "get_IsPlayerCharacter") {
          startIndex = i;
          break;
        }
      }
      if (startIndex > -1) {
        codes.RemoveRange(startIndex - 1, 3);
      }
      return codes.AsEnumerable();
    }
  }
  [HarmonyPatch(typeof(SGBarracksDossierPanel))]
  [HarmonyPatch("PlayVO")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class SGBarracksDossierPanel_PlayVO {
    private static FieldInfo f_lastVOWasLight = typeof(SGBarracksDossierPanel).GetField("lastVOWasLight", BindingFlags.Static | BindingFlags.NonPublic);
    public static bool lastVOWasLight() { return (bool)f_lastVOWasLight.GetValue(null); }
    public static void lastVOWasLight(bool value) { f_lastVOWasLight.SetValue(null, value); }
    private static string prevStopEvent = string.Empty;
    public static bool Prefix(string voice, ref bool __result) {
      try {
        //float scale = CustomAmmoCategories.Settings.bloodSettings.DecalScale[];
        Log.M?.TWL(0, "SGBarracksDossierPanel.PlayVO " + voice);
        uint ret = uint.MaxValue;
        AudioEngine.Instance?.VoiceOverBus?.StopAll();
        if (string.IsNullOrEmpty(prevStopEvent) == false) {
          ret = WwiseManager.PostEvent(prevStopEvent, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.M?.WL(1, "playing prev stop event:" + prevStopEvent + ":" + ret);
          prevStopEvent = string.Empty;
        }
        if (string.IsNullOrEmpty(voice)) { Log.M?.WL(1, "empty voice"); return true; }
        if (voice == "pilot_player_computer") {
          Log.M?.WL(1, "voice is pilot_player_computer");
          WwiseManager.PostEvent(AudioEventList_vo.vo_stop_pilots, WwiseManager.GlobalAudioObject);
          WwiseManager.SetSwitch(AudioSwitch_dialog_lines_computer_ai.welcome_commander, WwiseManager.GlobalAudioObject);
          WwiseManager.PostEvent(AudioEventList_vo.vo_play_computer_ai, WwiseManager.GlobalAudioObject);
          return false;
        }
        if (Utilities.EnumTryParse<AudioSwitch_dialog_character_type_pilots>(voice, out AudioSwitch_dialog_character_type_pilots enumValue)) { Log.M?.WL(1, "voice is default"); return true; }
        if (Core.TryGetExtVoice(voice, out VoicePackDef voicePack)) {
          if (string.IsNullOrEmpty(voicePack.vobank) == false) {
            if (ModTek.ModTek.soundBanks.ContainsKey(voicePack.vobank) == false) { Log.M?.WL(1, "unknown sound bank"); return true; }
            if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
              Log.M?.WL(1, "bank is not loaded");
              VOSoundBankLoadHelper.LoadVOSoundBank(ModTek.ModTek.soundBanks[voicePack.vobank]);
              if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
                Log.M?.WL(1, "still bank is not loaded");
                return true;
              } else {
                Log.M?.WL(1, "bank is loaded");
              }
            }
            if (string.IsNullOrEmpty(voicePack.stop_event) == false) {
              ret = WwiseManager.PostEvent(voicePack.stop_event, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
              Log.M?.WL(1, "playing stop event:" + voicePack.stop_event + ":" + ret);
              prevStopEvent = voicePack.stop_event;
            }
          }
          WwiseManager.PostEvent<AudioEventList_vo>(AudioEventList_vo.vo_stop_pilots, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
          WwiseManager.SetSwitch<AudioSwitch_dialog_character_type_pilots>(voicePack.baseVoice, WwiseManager.GlobalAudioObject);
          //WwiseManager.SetSwitch<AudioSwitch_dialog_lines_pilots>(AudioSwitch_dialog_lines_pilots.chosen, WwiseManager.GlobalAudioObject);
          bool isDark = SGBarracksDossierPanel_PlayVO.lastVOWasLight();
          if (SGBarracksDossierPanel_PlayVO.lastVOWasLight()) {
            WwiseManager.SetSwitch<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.dark, WwiseManager.GlobalAudioObject);
          } else {
            WwiseManager.SetSwitch<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.light, WwiseManager.GlobalAudioObject);
          }
          SGBarracksDossierPanel_PlayVO.lastVOWasLight(!SGBarracksDossierPanel_PlayVO.lastVOWasLight());
          string eventId = voicePack.getPhrase(isDark, AudioSwitch_dialog_lines_pilots.chosen);
          if ((string.IsNullOrEmpty(eventId) == false) && (string.IsNullOrEmpty(voicePack.vobank) == false)) {
            ret = WwiseManager.PostEvent(eventId, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
            Log.M?.WL(1, "playing event:" + eventId + ":" + ret);
          } else if (string.IsNullOrEmpty(voicePack.vobank)) {
            Log.M?.WL(1, "playing event external audio " + eventId);
            AudioEngine.Instance?.VoiceOverBus?.Play(eventId, false);
          } else {
            WwiseManager.SetSwitch<AudioSwitch_dialog_lines_pilots>(AudioSwitch_dialog_lines_pilots.chosen, WwiseManager.GlobalAudioObject);
            int num2 = (int)WwiseManager.PostEvent<AudioEventList_vo>(AudioEventList_vo.vo_play_pilots, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
          }
        } else {
          return true;
        }
        __result = true;
        return false;
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        return true;
      }
    }
    
  }
  [HarmonyPatch(typeof(SG_HiringHall_DetailPanel))]
  [HarmonyPatch("PlayPilotSelectionVO")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Pilot) })]
  public static class SG_HiringHall_DetailPanel_PlayPilotSelectionVO {
    private static FieldInfo f_lastVOWasLight = typeof(SG_HiringHall_DetailPanel).GetField("lastVOWasLight", BindingFlags.Instance | BindingFlags.NonPublic);
    public static bool lastVOWasLight(this SG_HiringHall_DetailPanel panel) { return (bool)f_lastVOWasLight.GetValue(panel); }
    public static void lastVOWasLight(this SG_HiringHall_DetailPanel panel, bool value) { f_lastVOWasLight.SetValue(panel, value); }
    private static string prevStopEvent = string.Empty;
    public static bool Prefix(SG_HiringHall_DetailPanel __instance, Pilot p) {
      try {
        //float scale = CustomAmmoCategories.Settings.bloodSettings.DecalScale[];
        Log.M?.TWL(0, "SG_HiringHall_DetailPanel.PlayPilotSelectionVO " + p.Description.Id + ":" + p.pilotDef.Voice);
        uint ret = uint.MaxValue;
        AudioEngine.Instance?.VoiceOverBus?.StopAll();
        if (string.IsNullOrEmpty(prevStopEvent) == false) {
          ret = WwiseManager.PostEvent(prevStopEvent, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.M?.WL(1, "playing prev stop event:" + prevStopEvent + ":" + ret);
          prevStopEvent = string.Empty;
        }
        //if (p.pilotDef.Description.Id == "pilot_backer_SwansonA") { p.pilotDef.SetVoice("tex_voice"); };
        string voice = p.pilotDef.Voice;
        if (string.IsNullOrEmpty(voice)) { Log.M?.WL(1, "empty voice"); return true; }
        if (voice == "pilot_player_computer") {
          Log.M?.WL(1, "voice is pilot_player_computer");
          WwiseManager.PostEvent(AudioEventList_vo.vo_stop_pilots, WwiseManager.GlobalAudioObject);
          WwiseManager.SetSwitch(AudioSwitch_dialog_lines_computer_ai.welcome_commander, WwiseManager.GlobalAudioObject);
          WwiseManager.PostEvent(AudioEventList_vo.vo_play_computer_ai, WwiseManager.GlobalAudioObject);
          return false;
        }
        if (Utilities.EnumTryParse<AudioSwitch_dialog_character_type_pilots>(voice, out AudioSwitch_dialog_character_type_pilots enumValue)) { Log.M?.WL(1, "voice is default"); return true; }
        if (Core.TryGetExtVoice(voice, out VoicePackDef voicePack)) {
          if (string.IsNullOrEmpty(voicePack.vobank) == false) {
            if (ModTek.ModTek.soundBanks.ContainsKey(voicePack.vobank) == false) { Log.M?.WL(1, "unknown sound bank"); return true; }
            if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
              Log.M?.WL(1, "bank is not loaded");
              VOSoundBankLoadHelper.LoadVOSoundBank(ModTek.ModTek.soundBanks[voicePack.vobank]);
              if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
                Log.M?.WL(1, "still bank is not loaded");
                return true;
              } else {
                Log.M?.WL(1, "bank is loaded");
              }
            }
          }
          if (string.IsNullOrEmpty(voicePack.stop_event) == false) {
            ret = WwiseManager.PostEvent(voicePack.stop_event, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
            Log.M?.WL(1, "playing stop event:" + voicePack.stop_event + ":" + ret);
            prevStopEvent = voicePack.stop_event;
          }
          WwiseManager.PostEvent<AudioEventList_vo>(AudioEventList_vo.vo_stop_pilots, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
          WwiseManager.SetSwitch<AudioSwitch_dialog_character_type_pilots>(voicePack.baseVoice, WwiseManager.GlobalAudioObject);
          //WwiseManager.SetSwitch<AudioSwitch_dialog_lines_pilots>(AudioSwitch_dialog_lines_pilots.chosen, WwiseManager.GlobalAudioObject);
          bool isDark = __instance.lastVOWasLight();
          if (__instance.lastVOWasLight()) {
            Log.M?.WL(1, "switch dark");
            WwiseManager.SetSwitch<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.dark, WwiseManager.GlobalAudioObject);
          } else {
            Log.M?.WL(1, "switch light");
            WwiseManager.SetSwitch<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.light, WwiseManager.GlobalAudioObject);
          }
          __instance.lastVOWasLight(!__instance.lastVOWasLight());
          Log.M?.WL(1, "lastVOWasLight:" + __instance.lastVOWasLight() + " isDarkTheme:" + WwiseManager.GlobalAudioObject.isDarkTheme());
          string eventId = voicePack.getPhrase(isDark, AudioSwitch_dialog_lines_pilots.chosen);
          if ((string.IsNullOrEmpty(eventId) == false) && (string.IsNullOrEmpty(voicePack.vobank) == false)) {
            //AKRESULT res = AkSoundEngine.SetRTPCValue(254523064, 100f);
            //Log.M?.WL(1, "SetRTPCValue result:" + res);
            ret = WwiseManager.PostEvent(eventId, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
            Log.M?.WL(1, "playing event:" + eventId + ":" + ret);
          } else if (string.IsNullOrEmpty(voicePack.vobank)) {
            Log.M?.WL(1, "playing event external audio " + eventId);
            AudioEngine.Instance?.VoiceOverBus?.Play(eventId, false);
          } else {
            WwiseManager.SetSwitch<AudioSwitch_dialog_lines_pilots>(AudioSwitch_dialog_lines_pilots.chosen, WwiseManager.GlobalAudioObject);
            int num2 = (int)WwiseManager.PostEvent<AudioEventList_vo>(AudioEventList_vo.vo_play_pilots, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
          }
        } else {
          return true;
        }
        return false;
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(AudioEventManager))]
  [HarmonyPatch("InterruptPilotVOForTeam")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Team), typeof(AbstractActor) })]
  public static class AudioEventManager_InterruptPilotVOForTeam {
    public static void InterruptPilotVO(string voice, AkGameObj audioObject) {
      Log.M?.TWL(0, "AudioEventManager.InterruptPilotVO " + voice);
      if (string.IsNullOrEmpty(voice)) { Log.M?.WL(1, "empty voice"); return; }
      AudioEngine.Instance?.VoiceOverBus?.StopAll();
      if (Utilities.EnumTryParse<AudioSwitch_dialog_character_type_pilots>(voice, out AudioSwitch_dialog_character_type_pilots enumValue)) { Log.M?.WL(1, "voice is default"); return; }
      if (Core.TryGetExtVoice(voice, out VoicePackDef voicePack)) {
        if (string.IsNullOrEmpty(voicePack.vobank) == false) {
          if (ModTek.ModTek.soundBanks.ContainsKey(voicePack.vobank) == false) { Log.M?.WL(1, "unknown sound bank"); return; }
          if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
            Log.M?.WL(1, "bank is not loaded");
            VOSoundBankLoadHelper.LoadVOSoundBank(ModTek.ModTek.soundBanks[voicePack.vobank]);
            if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
              Log.M?.WL(1, "still bank is not loaded");
              return;
            } else {
              Log.M?.WL(1, "bank is loaded");
            }
          }
          if (string.IsNullOrEmpty(voicePack.stop_event) == false) {
            uint ret = WwiseManager.PostEvent(voicePack.stop_event, audioObject, (AkCallbackManager.EventCallback)null, (object)null);
            Log.M?.WL(1, "playing stop event:" + voicePack.stop_event + ":" + ret);
          }
        }
      }
    }
    public static void Postfix(Team team, AbstractActor actorToIgnore) {
      //float scale = CustomAmmoCategories.Settings.bloodSettings.DecalScale[];
      Log.M?.TWL(0, "AudioEventManager.InterruptPilotVOForTeam " + team.DisplayName);
      if (!team.LocalPlayerControlsTeam) { return; };
      for (int index = 0; index < team.unitCount; ++index) {
        AbstractActor unit = team.units[index];
        if (unit != actorToIgnore && (UnityEngine.Object)unit.GameRep != (UnityEngine.Object)null && (UnityEngine.Object)unit.GameRep.audioObject != (UnityEngine.Object)null) {
          AudioEventManager_InterruptPilotVOForTeam.InterruptPilotVO(unit.GetPilot().pilotDef.Voice, unit.GameRep.audioObject);
        }
      }
    }
  }
  public static class PlayPilotVOPatch {
    private static MethodInfo m_IsPlayingVO = typeof(PilotRepresentation).GetProperty("IsPlayingVO", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true);
    private static MethodInfo m_CurrentVOPriority = typeof(PilotRepresentation).GetProperty("CurrentVOPriority", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true);
    public static void IsPlayingVO(this PilotRepresentation pr, bool value) {
      m_IsPlayingVO.Invoke(pr, new object[] { value });
    }
    public static void CurrentVOPriority(this PilotRepresentation pr, int value) {
      m_CurrentVOPriority.Invoke(pr, new object[] { value });
    }
    public class AudioCallbackDelegate {
      public PilotRepresentation __instance;
      public object in_cookie;
      public AudioCallbackDelegate(PilotRepresentation __instance, object in_cookie) {
        this.__instance = __instance; this.in_cookie = in_cookie;
      }
      public void AudioCallback() {
        Log.M?.TWL(0, __instance.pilot.Description.Id + " end VO event");
        __instance.AudioCallback(in_cookie, AkCallbackType.AK_EndOfEvent, null);
      }
    }
    public static bool Prefix(PilotRepresentation __instance, ref bool ___startedVOStatic, AudioSwitch_dialog_lines_pilots VOEnumValue, int priority, AkCallbackManager.EventCallback callback, object in_cookie) {
      try {
        Log.M?.TWL(0, "PilotRepresentation.PlayPilotVO " + __instance.pilot.pilotDef.Description.Id + " voice:" + __instance.pilot.pilotDef.Voice + " phrase:" + VOEnumValue);
        string voice = __instance.pilot.pilotDef.Voice;
        if (string.IsNullOrEmpty(voice)) { Log.M?.WL(1, "empty voice"); return true; }
        if (voice == "pilot_player_computer") {
          Log.M?.WL(1, "voice is pilot_player_computer");
          if (!___startedVOStatic)
          {
            ___startedVOStatic = true;
            WwiseManager.PostEvent(AudioEventList_vo.vo_static_start_pilot, __instance.audioObject);
          }
          AudioEventManager.InterruptPilotVOForTeam(__instance.pilot.ParentActor.team, null);
          WwiseManager.SetSwitch(VOEnumValue, __instance.audioObject);
          WwiseManager.PostEvent(AudioEventList_vo.vo_play_pilot_player_character, __instance.audioObject, (callback != null) ? callback : new AkCallbackManager.EventCallback(__instance.AudioCallback), in_cookie);
          __instance.IsPlayingVO(true);
          __instance.CurrentVOPriority(priority);
          return false;
        }
        if (Utilities.EnumTryParse<AudioSwitch_dialog_character_type_pilots>(voice, out AudioSwitch_dialog_character_type_pilots enumValue)) { Log.M?.WL(1, "voice is default"); return true; }
        if (Core.TryGetExtVoice(voice, out VoicePackDef voicePack)) {
          if (string.IsNullOrEmpty(voicePack.vobank) == false) {
            if (ModTek.ModTek.soundBanks.ContainsKey(voicePack.vobank) == false) { Log.M?.WL(1, "unknown sound bank"); return true; }
            if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
              Log.M?.WL(1, "bank is not loaded");
              VOSoundBankLoadHelper.LoadVOSoundBank(ModTek.ModTek.soundBanks[voicePack.vobank]);
              if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
                Log.M?.WL(1, "still bank is not loaded");
                return true;
              } else {
                Log.M?.WL(1, "bank is loaded");
              }
            }
          }
          uint ret = uint.MaxValue;
          string eventId = voicePack.getPhrase(__instance.audioObject.isDarkTheme(), VOEnumValue);
          if (string.IsNullOrEmpty(eventId) == false) {
            //ret = WwiseManager.PostEvent(voicePack.dark_phrases[AudioSwitch_dialog_lines_pilots.chosen], WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
            if (!___startedVOStatic) {
              ___startedVOStatic = true;
              int num = (int)WwiseManager.PostEvent<AudioEventList_vo>(AudioEventList_vo.vo_static_start_pilot, __instance.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
            }
            AudioEventManager.InterruptPilotVOForTeam(__instance.pilot.ParentActor.team, (AbstractActor)null);
            //AKRESULT res = AkSoundEngine.SetRTPCValue(254523064, 100f);
            //Log.M?.WL(1, "SetRTPCValue result:" + res);
            if (string.IsNullOrEmpty(voicePack.vobank)) {
              AudioEngine.AudioChannel channel = AudioEngine.Instance?.VoiceOverBus?.Play(eventId, false, new List<AudioEngine.AudioChannelEvent>() { new AudioEngine.AudioChannelEvent(float.NaN,new AudioCallbackDelegate(__instance, in_cookie).AudioCallback) });
              ret = (uint)channel.handle;
            } else { 
              ret = WwiseManager.PostEvent(eventId, __instance.audioObject, callback != null ? callback : new AkCallbackManager.EventCallback(__instance.AudioCallback), in_cookie);
              Log.M?.WL(1, "playing event:" + eventId + ":" + ret);
            }
          } else {
            return true;
          }
          if (ret != 0) {
            __instance.IsPlayingVO(true);
            __instance.CurrentVOPriority(priority);
            Log.M?.WL(1, "event has been played");
          } else {
            __instance.IsPlayingVO(true);
            __instance.CurrentVOPriority(priority);
            __instance.AudioCallback(null, AkCallbackType.AK_EndOfEvent, null);
            Log.M?.WL(1, "event has not been played");
          }
        } else {
          Log.M?.WL(1, "can't found voice pack");
          return true;
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        return true;
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(PilotRepresentation))]
  [HarmonyPatch("LoadSoundbanks")]
  [HarmonyPatch(MethodType.Normal)]
  public static class PilotRepresentation__LoadSoundbanks__Patch
  {
    public static bool Prefix(ref PilotRepresentation __instance, ref bool ___startedVOStatic)
    {
      string voice = __instance.pilot.pilotDef.Voice;
      if (string.IsNullOrEmpty(voice) || voice != "pilot_player_computer") { return true; }
      AudioBankList bankId = AudioBankList.vo_pilot_player_character;
      HBS.SceneSingletonBehavior<WwiseManager>.Instance.LoadBank(bankId);
      HBS.SceneSingletonBehavior<WwiseManager>.Instance.voBanks.Add("vo_pilot_player_character");
      ___startedVOStatic = true;
      return false;
    }
  }
  public class VoicePackDef {
    [JsonIgnore]
    private string fuiname;
    public string name { get; set; }
    public string uiname { get { if (string.IsNullOrEmpty(fuiname)) { return name; } else { return fuiname; } } set { fuiname = value; } }
    public string vobank { get; set; }
    public AudioSwitch_dialog_character_type_pilots baseVoice { get; set; }
    public string stop_event { get; set; }
    public Gender gender { get; set; }
    public Dictionary<AudioSwitch_dialog_lines_pilots, List<string>> dark_phrases { get; set; }
    public Dictionary<AudioSwitch_dialog_lines_pilots, List<string>> light_phrases { get; set; }
    public override string ToString() { return uiname; }
    //public static int[] randomSeed = { 114, 17, 24,  };
    public string getPhrase(bool darkMood, AudioSwitch_dialog_lines_pilots val) {
      Log.M?.TWL(0, "VoicePackDef.getPhrase mood:" + darkMood + " phrase:" + val);
      Dictionary<AudioSwitch_dialog_lines_pilots, List<string>> phrases = darkMood ? dark_phrases : light_phrases;
      bool swithcMood = true;
      if (phrases.ContainsKey(val)) {
        if (phrases[val].Count > 0) {
          foreach (string res in phrases[val]) { if (string.IsNullOrEmpty(res) == false) { swithcMood = false; break; } }
        }
      }
      if (swithcMood) { darkMood = !darkMood; phrases = darkMood ? dark_phrases : light_phrases; };
      Log.M?.WL(1, "resulting mood:" + darkMood);
      int watchdog = 0;
      if (phrases.ContainsKey(val) == false) { Log.M?.WL(1, "can't find"); return string.Empty; }
      do {
        int roll = Random.Range(0, phrases[val].Count);
        Log.M?.WL(1, "roll:" + roll + "/" + phrases[val].Count);
        if (roll < phrases[val].Count) {
          if (string.IsNullOrEmpty(phrases[val][roll]) == false) { return phrases[val][roll]; };
        }
        ++watchdog;
      } while (watchdog < 10);
      return string.Empty;
    }
    public VoicePackDef() {
      dark_phrases = new Dictionary<AudioSwitch_dialog_lines_pilots, List<string>>();
      light_phrases = new Dictionary<AudioSwitch_dialog_lines_pilots, List<string>>();
      vobank = string.Empty;
      gender = Gender.Male;
      baseVoice = AudioSwitch_dialog_character_type_pilots.m_bear01;
    }
  }
  public class BASSDistFactor {
    public float distanceFactor = 1f;
    public override string ToString() {
      if (Mathf.Abs(distanceFactor - 1f) < 0.01f) { return Strings.CurrentCulture == Strings.Culture.CULTURE_RU_RU? "метры":"meters"; }
      if (Mathf.Abs(distanceFactor - 0.9144f) < 0.01f) { return Strings.CurrentCulture == Strings.Culture.CULTURE_RU_RU ? "ярды" : "yards"; }
      if (Mathf.Abs(distanceFactor - 0.3048f) < 0.01f) { return Strings.CurrentCulture == Strings.Culture.CULTURE_RU_RU ? "футы" : "feet"; }
      return $"{distanceFactor}";
    }
  }
  [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
  public class NextBASSDistFactor : CustomSettings.NextSettingValue {
    public override void Next(object settings) {
      Log.M?.TWL(0, $"NextBASSDistFactor.Next {settings.GetType()}");
      if (settings is Volumes volumes) {
        try {
          if (Mathf.Abs(volumes.bassDistFactor.distanceFactor - 1f) < 0.01f) { volumes.bassDistFactor.distanceFactor = 0.9144f; }else
          if (Mathf.Abs(volumes.bassDistFactor.distanceFactor - 0.9144f) < 0.01f) { volumes.bassDistFactor.distanceFactor = 0.3048f; }else
          { volumes.bassDistFactor.distanceFactor = 1f; }
          Bass.Set3DFactors(volumes.bassDistFactor.distanceFactor, -1f,-1f);
        } catch (Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
          UIManager.logger.LogException(e);
        }
      }
    }
    public NextBASSDistFactor() { }
  }
  [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
  public class NextBASSRolloffFactor : CustomSettings.NextSettingValue {
    public override void Next(object settings) {
      Log.M?.TWL(0, $"NextBASSRolloffFactor.Next {settings.GetType()}");
      if (settings is Volumes volumes) {
        try {
          volumes.bassRollOffFactor += 0.1f;
          if (volumes.bassRollOffFactor > 2.0f) { volumes.bassRollOffFactor = 0.1f; }
          Bass.Set3DFactors(-1f, volumes.bassRollOffFactor, -1f);
        } catch (Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
          UIManager.logger.LogException(e);
        }
      }
    }
    public NextBASSRolloffFactor() { }
  }

  public class Volumes {
    public float MusicBus { get; set; } = 1f;
    public float AmbientBus { get; set; } = 1f;
    public float CombatBus { get; set; } = 1f;
    public float VoiceOverBus { get; set; } = 1f;
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Audio output")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Аудио выход")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Audio output for custom audio engine")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Аудио выход для кастмного айдио движка")]
    [NextAudioOutput]
    public CustomOutputId outputId { get; set; } = new CustomOutputId();
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "LibBASS 3D position distance factor")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Фактор расстояния для 3D звука libBASS")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "LibBASS 3D position distance factor")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Фактор расстояния для 3D звука libBASS")]
    [NextBASSDistFactor]
    public BASSDistFactor bassDistFactor { get; set; } = new BASSDistFactor();
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "LibBASS 3D position rolloff factor")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Фактор убывания громкости с расстоянием для 3D звука libBASS")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "LibBASS 3D position rolloff factor")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Фактор убывания громкости с расстоянием для 3D звука libBASS")]
    [NextBASSRolloffFactor]
    public float bassRollOffFactor { get; set; } = 1f;
  }
  public class CustomOutputId {
    public static Dictionary<int, string> outputs = new Dictionary<int, string>();
    public int outputId { get; set; } = -1;
    public override string ToString() {
      if (outputs.TryGetValue(outputId, out var result)) { return result; }
      return $"({this.outputId})not exists";
    }
  }
  [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
  public class NextAudioOutput : CustomSettings.NextSettingValue {
    public override void Next(object settings) {
      Log.M?.TWL(0, $"NextAudioOutput.Next {settings.GetType()}");
      if(settings is Volumes volumes) {
        try {
          var iterator = CustomOutputId.outputs.GetEnumerator();
          int prevOut = volumes.outputId.outputId;
          ++volumes.outputId.outputId;
          if (CustomOutputId.outputs.ContainsKey(volumes.outputId.outputId) == false) {
            volumes.outputId.outputId = -1;
          }
          if(prevOut != volumes.outputId.outputId) {
            if ((prevOut != -2) && (AudioEngine.EngineInited)) {
              AudioEngine.Instance?.VoiceOverBus?.StopAll();
              AudioEngine.Instance?.MusicBus?.StopAll();
              AudioEngine.Instance?.AmbientBus?.StopAll();
              AudioEngine.Instance?.CombatBus?.StopAll();
              AudioEngine.Instance?.Free();
              AudioEngine.EngineInited = false;
            }
            AudioEngine.Init(false);
            AudioEngine.Instance?.VoiceOverBus?.Play("nominal", false);
          }
        }catch(Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
          UIManager.logger.LogException(e);
        }
      }
    }
    public NextAudioOutput() { }
  }
  public class Settings {
    public bool debugLog { get; set; }
    public Volumes volumes { get; set; } = new Volumes();
    public string voiceSelectorName { get; set; }
    public float voiceSelectorDownOffset { get; set; }
    public HashSet<uint> suppressWwiseIds { get; set; } = new HashSet<uint>();
    public Dictionary<string, string> defaultVoices { get; set; }
    public Settings() { debugLog = false; defaultVoices = new Dictionary<string, string>(); voiceSelectorName = "voice"; voiceSelectorDownOffset = 60f; }
  }
  public static class Core {
    public static Dictionary<string, VoicePackDef> extVoicePacks = new Dictionary<string, VoicePackDef>();
    public static bool TryGetExtVoice(string name, out VoicePackDef voicePack) {
      return extVoicePacks.TryGetValue(name, out voicePack);
    }
    public static Settings settings { get; set; }
    public static Volumes global_volumes { get; set; }
    public static string resetVolumes() {
      return JsonConvert.SerializeObject(global_volumes, Formatting.Indented);
    }
    public static void readVolumes(string json) {
      Core.settings.volumes = JsonConvert.DeserializeObject<Volumes>(json);
    }
    public static Volumes DefaultSettings() {
      return Core.global_volumes;
    }
    public static Volumes CurrentSettings() {
      return Core.settings.volumes;
    }
    public static string SaveSettings(object settings) {
      Volumes set = settings as Volumes;
      if (set == null) { set = Core.global_volumes; }
      JObject jsettigns = JObject.FromObject(set);
      return jsettigns.ToString(Formatting.Indented);
    }

    public static readonly float Epsilon = 0.001f;
    public static string FindGameRoot(string directory) {
      string temp = Path.GetDirectoryName(directory);
      while (string.IsNullOrEmpty(temp) == false) {
        if (Directory.Exists(Path.Combine(temp, ".modtek"))) { return Path.GetDirectoryName(temp); }
        temp = Path.GetDirectoryName(temp);
      }
      return string.Empty;
    }
    public static void FinishedLoading(List<string> loadOrder, Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources) {
      Log.M?.TWL(0, "FinishedLoading", true);
      try {
        try {
          string root = FindGameRoot(Log.BaseDirectory);
          if (string.IsNullOrEmpty(root) == false) {
            Log.M?.WL(1, $"Game root folder:{root}", true);
            if (Directory.Exists(Path.Combine(root, "Mods_sfx")) == false) { Directory.CreateDirectory(Path.Combine(root, "Mods_sfx")); }
            if (Directory.Exists(Path.Combine(root, "Mods_sfx", "voicepacks")) == false) { Directory.CreateDirectory(Path.Combine(root, "Mods_sfx", "voicepacks")); }
            if (Directory.Exists(Path.Combine(root, "Mods_sfx", "audio")) == false) { Directory.CreateDirectory(Path.Combine(root, "Mods_sfx", "audio")); }
            if (Directory.Exists(Path.Combine(root, "Mods_sfx", "audio_settings")) == false) { Directory.CreateDirectory(Path.Combine(root, "Mods_sfx", "audio_settings")); }
            foreach (string filepath in Directory.GetFiles(Path.Combine(root, "Mods_sfx", "voicepacks"), "*.json", SearchOption.AllDirectories)) {
              try {
                VoicePackDef def = JsonConvert.DeserializeObject<VoicePackDef>(File.ReadAllText(filepath));
                if (extVoicePacks.ContainsKey(def.name) == false) {
                  extVoicePacks.Add(def.name, def);
                } else {
                  extVoicePacks[def.name] = def;
                }
              } catch (Exception e) {
                Log.M?.TWL(0, filepath + "\n" + e.ToString(), true);
              }
            }
            AudioEngine.getAudioFiles(Directory.GetFiles(Path.Combine(root, "Mods_sfx", "audio"), "*.ogg", SearchOption.AllDirectories));
            AudioEngine.getAudioFiles(Directory.GetFiles(Path.Combine(root, "Mods_sfx", "audio"), "*.wav", SearchOption.AllDirectories));
            AudioEngine.getAudioFiles(Directory.GetFiles(Path.Combine(root, "Mods_sfx", "audio"), "*.mp3", SearchOption.AllDirectories));
            AudioEngine.getAudioFilesSettings(Directory.GetFiles(Path.Combine(root, "Mods_sfx", "audio_settings"), "*.json", SearchOption.AllDirectories));
          }
        }catch(Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
        }
        foreach (var customResource in customResources) {
          Log.M?.WL(1, "customResource:" + customResource.Key);
          if (customResource.Key == nameof(VoicePackDef)) {
            foreach (var resource in customResource.Value) {
              Log.M?.WL(2, "resource:" + resource.Key + "=" + resource.Value.FilePath);
              try {
                VoicePackDef def = JsonConvert.DeserializeObject<VoicePackDef>(File.ReadAllText(resource.Value.FilePath));
                if (extVoicePacks.ContainsKey(def.name) == false) {
                  extVoicePacks.Add(def.name, def);
                } else {
                  extVoicePacks[def.name] = def;
                }
              } catch (Exception e) {
                Log.M?.TWL(0, resource.Value.FilePath + "\n" + e.ToString(), true);
              }
            }
          } else if (customResource.Key == "AudioFile") {
            AudioEngine.getAudioFiles(customResource.Value);
          } else if (customResource.Key == "AudioFileSettings") {
            AudioEngine.getAudioFilesSettings(customResource.Value);
          }
        }
        CustomSettings.ModsLocalSettingsHelper.RegisterLocalSettings("CustomVoices", "Custom Voices", AudioEngine.ResetSettings, AudioEngine.ReadSettings);
        CustomSettings.ModsLocalSettingsHelper.RegisterLocalSettings("CustomAudio", "Custom audio", Core.resetVolumes, Core.readVolumes, Core.DefaultSettings, Core.CurrentSettings, Core.SaveSettings);
        CustomMusicHelper.Init(Log.BaseDirectory);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    //[DllImport("kernel32.dll")]
    //private static extern IntPtr LoadLibrary(String fileName);

    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      Core.settings = JsonConvert.DeserializeObject<CustomVoices.Settings>(settingsJson);
      Core.global_volumes = Core.settings.volumes;
      Log.InitLog();
      Log.M?.TWL(0, "Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n", true);
      try {
        var harmony = new Harmony("ru.mission.customvoices");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        MethodInfo bPlayPilotVO = typeof(PilotRepresentation).GetMethod("PlayPilotVO");
        MethodInfo miPlayPilotVO = bPlayPilotVO.MakeGenericMethod(new Type[] { typeof(AudioSwitch_dialog_lines_pilots) });
        MethodInfo pPlayPilotVO = typeof(PlayPilotVOPatch).GetMethod("Prefix");
        harmony.Patch(miPlayPilotVO, new HarmonyMethod(pPlayPilotVO));
        HBS.SceneSingletonBehavior<WwiseManager>.Instance.LoadBank((AudioBankList)Enum.Parse(typeof(AudioBankList), "vo_f_kamea", true));
        HBS.SceneSingletonBehavior<WwiseManager>.Instance.LoadBank((AudioBankList)Enum.Parse(typeof(AudioBankList), "vo_m_raju", true));
        HBS.SceneSingletonBehavior<WwiseManager>.Instance.voBanks.Add("vo_f_kamea");
        HBS.SceneSingletonBehavior<WwiseManager>.Instance.voBanks.Add("vo_m_raju");
        List<string> PERSISTENT_BANK_IDS = new List<string>();
        PERSISTENT_BANK_IDS.AddRange(WwiseDefinitions.PERSISTENT_BANK_IDS);
        PERSISTENT_BANK_IDS.Add("vo_f_kamea");
        PERSISTENT_BANK_IDS.Add("vo_m_raju");
        typeof(WwiseDefinitions).GetField("PERSISTENT_BANK_IDS", BindingFlags.Static | BindingFlags.Public).SetValue(null,PERSISTENT_BANK_IDS.ToArray());
      } catch (Exception e) {
        Log.LogWrite(e.ToString(),true);
      }
      AudioEngine.Init(true);
    }
  }
}
