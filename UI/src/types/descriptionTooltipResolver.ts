﻿import { ReactElement, PropsWithChildren } from "react";
import { getModule } from "cs2/modding";
import { TooltipProps } from "cs2/ui";

interface DescriptionTooltipProps extends Omit<TooltipProps, 'tooltip'>
{
    title: string | null;
    description: string | null;
}

export class DescriptionTooltipResolver
{
    private static _instance: DescriptionTooltipResolver = new DescriptionTooltipResolver();
    private readonly _descriptionTooltip: (props: PropsWithChildren<DescriptionTooltipProps>) => ReactElement;

    private constructor()
    {
        this._descriptionTooltip = getModule("game-ui/common/tooltip/description-tooltip/description-tooltip.tsx", "DescriptionTooltip");
    }

    public static get instance()
    {
        return this._instance
    }

    public get DescriptionTooltip(): (props: PropsWithChildren<DescriptionTooltipProps>) => ReactElement
    {
        return this._descriptionTooltip
    }
}