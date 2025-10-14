import { useValue, trigger  } from "cs2/api";
import { useLocalization    } from "cs2/l10n";
import { Button             } from "cs2/ui";

import { MainPanelUISettings, bindingMainPanelUISettings    } from "bindings";
import { DescriptionTooltipWithKeyBind                      } from "descriptionTooltipWithKeyBind";
import   stopwatchIcon                                        from "images/stopwatchIcon.svg";
import { UIEventName, UITranslationKey                      } from "uiConstants";

// Button to display or hide the main panel.
export const MainButton = () =>
{
    // Get main panel UI settings.
    const mainPanelUISettings: MainPanelUISettings = useValue(bindingMainPanelUISettings);

    // Translations.
    const { translate } = useLocalization();

    return (
        <>
            <DescriptionTooltipWithKeyBind
                title={translate(UITranslationKey.Title)}
                description={translate(UITranslationKey.Description)}
                keyBind={mainPanelUISettings.activationKey}
            >
                <Button
                    src={stopwatchIcon}
                    variant="floating"
                    selected={mainPanelUISettings.panelVisible}
                    onSelect={() => trigger(UIEventName.GroupName, UIEventName.MainButtonClicked)}
                />
            </DescriptionTooltipWithKeyBind>
        </>
    );
}