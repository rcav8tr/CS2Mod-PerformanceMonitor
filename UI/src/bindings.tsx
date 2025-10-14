import { bindValue } from "cs2/api";

import { UIEventName } from "uiConstants";

// Define main panel UI settings.
export type MainPanelUISettings =
{
    activationKey:  any;
    panelVisible:   boolean;
    panelPositionX: number;
    panelPositionY: number;
}

// Main panel UI settings.
export const bindingMainPanelUISettings = bindValue<MainPanelUISettings>(UIEventName.GroupName, UIEventName.MainPanelUISettings);

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

// Game bindings.
export const bindingActiveLocale       = bindValue<string >("app",                 "activeLocale",                 "en-US");
export const bindingTextScale          = bindValue<number >("options",             "textScale",                    1);
