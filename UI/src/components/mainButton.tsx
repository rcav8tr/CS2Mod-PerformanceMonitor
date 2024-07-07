import { useValue, trigger } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { Button } from "cs2/ui";
import { DescriptionTooltipResolver } from "types/descriptionTooltipResolver";
import { UIElementID } from "types/elementIDs";
import { UIEventName, UITranslationKey } from "types/uiConstants";
import { bindingMainPanelVisible } from "../bindings";
import stopwatchIcon from "images/stopwatchIcon.svg";

export const MainButton = () =>
{
    const valueMainPanelVisible: boolean = useValue(bindingMainPanelVisible);
    const { translate } = useLocalization();
    const { DescriptionTooltip } = DescriptionTooltipResolver.instance;

    return (
        <>
            <DescriptionTooltip title={translate(UITranslationKey.Title)} description={translate(UITranslationKey.Description)}>
                <Button
                    id={UIElementID.MainButton}
                    src={stopwatchIcon}
                    variant="floating"
                    selected={valueMainPanelVisible}
                    onSelect={() => trigger(UIEventName.GroupName, UIEventName.MainButtonClicked)}
                />
            </DescriptionTooltip>
        </>
    );
}