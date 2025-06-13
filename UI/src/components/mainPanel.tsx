import { MouseEvent, CSSProperties } from "react";
import { trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { Panel } from "cs2/ui";
import { UIElementID } from "types/elementIDs";
import { UIEventName, UITranslationKey } from "types/uiConstants";
import { bindingMainPanelVisible, bindingMainPanelPositionX, bindingMainPanelPositionY, bindingActiveLocale } from "../bindings";
import { checkPositionOnWindow } from "../utility";
import { PanelContent } from "./panelContent";
import stopwatchIcon from "images/stopwatchIcon.svg";

export const MainPanel = () =>
{
    // Main panel visibility and position.
    const valueMainPanelVisible:   boolean = useValue(bindingMainPanelVisible);
    const valueMainPanelPositionX: number  = useValue(bindingMainPanelPositionX);
    const valueMainPanelPositionY: number  = useValue(bindingMainPanelPositionY);

    // Localization.
    const valueActiveLocale: string = useValue(bindingActiveLocale);
    const { translate } = useLocalization();

    // Define the panel width for each locale ID so that the panel heading text
    // and row labels fit without ellipses, wrapping, or too much extra space.
    const panelWidths: Record<string, number> = {
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

    // Get the panel width based on locale ID and apply a default if needed.
    const panelWidth: number = panelWidths[valueActiveLocale] || 350;

    // Main panel style.
    const mainPanelStyle: Partial<CSSProperties> =
    {
        position: "absolute",
        left: valueMainPanelPositionX + "px",
        top:  valueMainPanelPositionY + "px",
        opacity: "1",
        margin: "0px",

        // Set panel width to automatically scale according to font size.
        // Panel height automatically adjusts itself according to font size and content.
        width: "calc(" + panelWidth + "rem + " + (panelWidth * 1.7) + "rem * (var(--fontScale) - 1))"
    }

    // Header style.
    const headerStyle: Partial<CSSProperties> =
    {
        width: "100%",
        display: "flex",
        flexDirection: "row"
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

    // The main panel is displayed only when the visibile value is true.
    // The main panel consists of the header and content.
    // The header consists of an image, a div for the title, and a close button.
    // The header elements and class names are adapated from info view panels.
    // Click on close button is same as click on main button.
    return (
        <>
        {
            valueMainPanelVisible &&
                (
                    <Panel
                        id={UIElementID.MainPanel}
                        style={mainPanelStyle}
                        header={(
                            <div style={headerStyle} onMouseDown={(e) => onMouseDown(e)}>
                                <img className="icon_OVK" src={stopwatchIcon} />
                                <div className="title_SVH title_zQN">{translate(UITranslationKey.Title)}</div>
                                <button id={UIElementID.MainPanelClose} className="button_bvQ close-button_wKK"
                                    onClick={() => trigger(UIEventName.GroupName, UIEventName.MainButtonClicked)}>
                                    <div className="tinted-icon_iKo icon_PhD" style={{ maskImage: "url(Media/Glyphs/Close.svg)" }} ></div>
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