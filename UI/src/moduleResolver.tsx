import { PropsWithChildren, ReactElement, ReactNode } from "react";

import { Theme          } from "cs2/bindings";
import { ControlPath    } from "cs2/input";
import { getModule      } from "cs2/modding";
import { TooltipProps   } from "cs2/ui";


export interface DescriptionTooltipProps extends Omit<TooltipProps, 'tooltip'>
{
    title:          string | null;
    description:    string | null;
    content?:       ReactNode | string | null;
}

export interface LocalizedInputPathProps
{
    group:          string;
    binding:        ControlPath;
    modifiers:      ControlPath[];
    short:          any;
    gamepadType:    any;
    keyboardLayout: any;
    layoutMap:      any;
}

// Provide access to modules from index.js.
export class ModuleResolver
{
    // Define instance.
    private static _instance: ModuleResolver = new ModuleResolver();
    public static get instance(): ModuleResolver { return this._instance }

    // Define modules.
    private _descriptionTooltip:            any;
    private _localizedInputPath:            any;
    private _uiSound:                       any;

    // Define SCSS modules.
    private _iconButtonClasses:             any;
    private _panelClasses:                  any;
    private _panelThemeClasses:             any;
    private _roundHighlightButtonClasses:   any;
    private _tintedIconClasses:             any;

    // Provide access to modules.
    public get DescriptionTooltip():    (props: PropsWithChildren<DescriptionTooltipProps>)
                                                                          => ReactElement   { return this._descriptionTooltip   ?? (this._descriptionTooltip    = getModule("game-ui/common/tooltip/description-tooltip/description-tooltip.tsx",   "DescriptionTooltip")); }
    public get LocalizedInputPath():    (props: LocalizedInputPathProps ) => ReactElement   { return this._localizedInputPath   ?? (this._localizedInputPath    = getModule("game-ui/common/localization/localized-input-path.tsx",                 "LocalizedInputPath")); }
    public get UISound()                                                                    { return this._uiSound              ?? (this._uiSound               = getModule("game-ui/common/data-binding/audio-bindings.ts",                        "UISound"           )); }

    // Provide access to SCSS modules.
    public get IconButtonClasses():             Theme | any { return this._iconButtonClasses            ?? (this._iconButtonClasses             = getModule("game-ui/common/input/button/icon-button.module.scss",                      "classes")); }
    public get PanelClasses():                  Theme | any { return this._panelClasses                 ?? (this._panelClasses                  = getModule("game-ui/common/panel/panel.module.scss",                                   "classes")); }
    public get PanelThemeClasses():             Theme | any { return this._panelThemeClasses            ?? (this._panelThemeClasses             = getModule("game-ui/common/panel/themes/default.module.scss",                          "classes")); }
    public get RoundHighlightButtonClasses():   Theme | any { return this._roundHighlightButtonClasses  ?? (this._roundHighlightButtonClasses   = getModule("game-ui/common/input/button/themes/round-highlight-button.module.scss",    "classes")); }
    public get TintedIconClasses():             Theme | any { return this._tintedIconClasses            ?? (this._tintedIconClasses             = getModule("game-ui/common/image/tinted-icon.module.scss",                             "classes")); }
}