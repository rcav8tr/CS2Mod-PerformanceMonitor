import { MouseEvent, CSSProperties } from "react";

import { trigger, useValue  } from "cs2/api";
import { useLocalization    } from "cs2/l10n";
import { Panel              } from "cs2/ui";

import { MainPanelUISettings, bindingMainPanelUISettings, bindingActiveLocale, bindingTextScale } from "bindings";
import { UIElementID                    } from "elementIDs";
import   styles                           from "mainPanel.module.scss";
import { ModuleResolver                 } from "moduleResolver";
import { PanelContent                   } from "panelContent";
import   stopwatchIcon                    from "images/stopwatchIcon.svg";
import { UIEventName, UITranslationKey  } from "uiConstants";
import { checkPositionOnWindow          } from "utility";

// Panel to display performance monitor data.
export const MainPanel = () =>
{
    // Get main panel UI settings.
    const mainPanelUISettings: MainPanelUISettings = useValue(bindingMainPanelUISettings);

    // Localization.
    const { translate } = useLocalization();
    const headingText: string = translate(UITranslationKey.Title) || "Performance Monitor";

    // Define the base panel width for each locale ID so that the panel heading text
    // and row labels fit without ellipses, wrapping, or too much extra space.
    // Fixed width is used instead of variable width so that data width does not affect panel width.
    const basePanelWidths: Record<string, number> = {
        "en-US":   300,
        "de-DE":   320,
        "es-ES":   410,
        "fr-FR":   360,
        "it-IT":   400,
        "ja-JP":   300,
        "ko-KR":   260,
        "pl-PL":   390,
        "pt-BR":   350,
        "ru-RU":   420,
        "zh-HANS": 220,
        "zh-HANT": 240,
    };

    // Get the base panel width according to locale ID.
    const valueActiveLocale: string = useValue(bindingActiveLocale);
    const basePanelWidth: number = basePanelWidths[valueActiveLocale] || 350;

    // Adjust panel width according to text scale.
    const valueTextScale: number = useValue(bindingTextScale);
    const adjustedPanelWidth: number = basePanelWidth + basePanelWidth * 1.7 * (valueTextScale - 1);

    // Verify panel position.
    let verifiedPanelPosition = { x: mainPanelUISettings.panelPositionX, y: mainPanelUISettings.panelPositionY };
    const panel: HTMLElement | null = document.getElementById(UIElementID.MainPanel);
    if (panel)
    {
        // Prevent any part of panel from going outside the window.
        const panelRect = panel.getBoundingClientRect();
        verifiedPanelPosition = checkPositionOnWindow(
            mainPanelUISettings.panelPositionX, mainPanelUISettings.panelPositionY, adjustedPanelWidth, panelRect.height);

        // Check for any change in panel position.
        if (verifiedPanelPosition.x != mainPanelUISettings.panelPositionX ||
            verifiedPanelPosition.y != mainPanelUISettings.panelPositionY)
        {
            // Move panel to verified position.
            trigger(UIEventName.GroupName, UIEventName.MainPanelMoved, verifiedPanelPosition.x, verifiedPanelPosition.y);
        }
    }

    // Set panel to the verified position and adjusted width using a dynamic style.
    const mainPanelStyle: Partial<CSSProperties> =
    {
        left:   verifiedPanelPosition.x + "px",
        top:    verifiedPanelPosition.y + "px",
        width:  adjustedPanelWidth + "rem",
    }

    // Variables for dragging.
    let mainPanel: HTMLElement | null = null;
    let relativePositionX = 0.0;
    let relativePositionY = 0.0;

    // Start dragging.
    // Dragging is initiated by mouse down on the panel header, but it is the whole panel that is moved.
    function onMouseDown(e: MouseEvent<HTMLDivElement, globalThis.MouseEvent>)
    {
        // Ignore mouse down if other than left mouse button.
        if (e.button !== 0)
        {
            return;
        }

        // Get close button.
        const closeButton = document.getElementById(UIElementID.MainPanelClose);
        if (closeButton)
        {
            // Ignore mouse down if over the close button.
            const closeButtonRect = closeButton.getBoundingClientRect();
            if (e.clientX >= closeButtonRect.left && e.clientX <= closeButtonRect.left + closeButtonRect.width &&
                e.clientY >= closeButtonRect.top && e.clientY <= closeButtonRect.top + closeButtonRect.height)
            {
                return;
            }
        }

        // Get main panel.
        mainPanel = document.getElementById(UIElementID.MainPanel);
        if (mainPanel)
        {
            // Save the position of the mouse relative to the main panel.
            const mainPanelRect = mainPanel.getBoundingClientRect();
            relativePositionX = e.clientX - mainPanelRect.left;
            relativePositionY = e.clientY - mainPanelRect.top;

            // Add mouse event listeners.
            window.addEventListener("mousemove", onMouseMove);
            window.addEventListener("mouseup", onMouseUp);

            // Stop event propagation.
            e.stopPropagation();
            e.preventDefault();
        }
    }

    // Move the main panel while dragging.
    function onMouseMove(e: { clientX: number; clientY: number; stopPropagation: () => void; preventDefault: () => void; })
    {
        // Check if main panel is valid.
        if (mainPanel)
        {
            // Compute new panel position based on current mouse position.
            // Adjusting by relative position while dragging keeps the panel in the same location
            // under the pointer as when the panel was originally clicked to start dragging.
            const newPosition = { x: e.clientX - relativePositionX, y: e.clientY - relativePositionY };

            // Prevent any part of panel from going outside the window.
            const mainPanelRect = mainPanel.getBoundingClientRect();
            const checkedPosition = checkPositionOnWindow(newPosition.x, newPosition.y, mainPanelRect.width, mainPanelRect.height);

            // Move panel to checked position.
            mainPanel.style.left = checkedPosition.x + "px";
            mainPanel.style.top  = checkedPosition.y + "px";

            // Stop event propagation.
            e.stopPropagation();
            e.preventDefault();
        }
    }

    // Finish dragging.
    function onMouseUp(e: { stopPropagation: () => void; preventDefault: () => void; })
    {
        // Check if main panel is valid.
        if (mainPanel)
        {
            // Remove mouse event listeners.
            window.removeEventListener("mousemove", onMouseMove);
            window.removeEventListener("mouseup", onMouseUp);

            // Trigger main panel moved event.
            const mainPanelRect = mainPanel.getBoundingClientRect();
            trigger(UIEventName.GroupName, UIEventName.MainPanelMoved, mainPanelRect.left, mainPanelRect.top);

            // Stop event propagation.
            e.stopPropagation();
            e.preventDefault();
        }
    }

    // Handle click on close button.
    // Click on close button is same as click on activation button.
    function onCloseClick()
    {
        trigger("audio", "playSound", ModuleResolver.instance.UISound.selectItem, 1);
        trigger(UIEventName.GroupName, UIEventName.MainButtonClicked)
    }

    // Function to join classes.
    function joinClasses(...classes: any) { return classes.join(" "); }

    // The main panel is displayed only when the visibile value is true.
    // The main panel consists of the header and content.
    // The header consists of an image, a div for the title, and a close button.
    return (
        <>
            {
                mainPanelUISettings.panelVisible &&
                (
                    <Panel
                        id={UIElementID.MainPanel}
                        className={styles.mainPanel}
                        style={mainPanelStyle}
                        header={(
                            <div className={styles.mainPanelHeader} onMouseDown={(e) => onMouseDown(e)}>
                                <img className={ModuleResolver.instance.PanelClasses.icon} src={stopwatchIcon} />
                                <div className={joinClasses(ModuleResolver.instance.PanelThemeClasses.title,
                                                            styles.mainPanelHeaderTitle)}>
                                    {headingText}
                                </div>
                                <button
                                    id={UIElementID.MainPanelClose}
                                    className={joinClasses(ModuleResolver.instance.PanelClasses.closeButton,
                                                           ModuleResolver.instance.RoundHighlightButtonClasses.button,
                                                           styles.mainPanelHeaderClose)}
                                    onClick={() => onCloseClick()}
                                >
                                    <div
                                        className={joinClasses(ModuleResolver.instance.TintedIconClasses.tintedIcon,
                                                               ModuleResolver.instance.IconButtonClasses.icon)}
                                        style={{ maskImage: "url(Media/Glyphs/Close.svg)" }}
                                    >
                                    </div>
                                </button>
                            </div>
                        )}
                    >
                        <PanelContent />
                    </Panel >
                )
            }
        </>
    )
}