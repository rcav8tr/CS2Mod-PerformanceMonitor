import { bindValue } from "cs2/api";
import { UIEventName, ModSettingsDefaults } from "./types/uiConstants";

// Main panel visibility and position.
export const bindingMainPanelVisible   = bindValue<boolean>(UIEventName.GroupName, UIEventName.MainPanelVisible,   ModSettingsDefaults.MainPanelVisible);
export const bindingMainPanelPositionX = bindValue<number >(UIEventName.GroupName, UIEventName.MainPanelPositionX, ModSettingsDefaults.MainPanelPositionX);
export const bindingMainPanelPositionY = bindValue<number >(UIEventName.GroupName, UIEventName.MainPanelPositionY, ModSettingsDefaults.MainPanelPositionY);

// Data row values.
export const bindingCurrentGameMinute  = bindValue<string >(UIEventName.GroupName, UIEventName.CurrentGameMinute,  "");
export const bindingPreviousGameMinute = bindValue<string >(UIEventName.GroupName, UIEventName.PreviousGameMinute, "");
export const bindingFrameRate          = bindValue<string >(UIEventName.GroupName, UIEventName.FrameRate,          "");
export const bindingGPUUsage           = bindValue<string >(UIEventName.GroupName, UIEventName.GPUUsage,           "");
export const bindingCPUUsage           = bindValue<string >(UIEventName.GroupName, UIEventName.CPUUsage,           "");
export const bindingMemoryUsage        = bindValue<string >(UIEventName.GroupName, UIEventName.MemoryUsage,        "");

// Whether or not to show data rows.
export const bindingShowGPUUsage       = bindValue<boolean>(UIEventName.GroupName, UIEventName.ShowGPUUsage,       false);
export const bindingShowCPUUsage       = bindValue<boolean>(UIEventName.GroupName, UIEventName.ShowCPUUsage,       false);
export const bindingShowMemoryUsage    = bindValue<boolean>(UIEventName.GroupName, UIEventName.ShowMemoryUsage,    false);

// Localization
export const bindingActiveLocale       = bindValue<string >("app",                 "activeLocale",                 "en-US");
