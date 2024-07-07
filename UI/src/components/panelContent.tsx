import { useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { PanelSection, PanelSectionRow } from "cs2/ui";
import { UITranslationKey } from "types/uiConstants";
import
    {
        bindingCurrentGameMinute,
        bindingPreviousGameMinute,
        bindingFrameRate,
        bindingGPUUsage,
        bindingCPUUsage,
        bindingMemoryUsage,
        bindingShowGPUUsage,
        bindingShowCPUUsage,
        bindingShowMemoryUsage
    } from "../bindings";

export const PanelContent = () =>
{
    // Translation.
    const { translate } = useLocalization();

    // Get the label for each data row.
    const rowLabelCurrentGameMinute:  string | null = translate(UITranslationKey.RowLabelCurrentGameMinute );
    const rowLabelPreviousGameMinute: string | null = translate(UITranslationKey.RowLabelPreviousGameMinute);
    const rowLabelFrameRate:          string | null = translate(UITranslationKey.RowLabelFrameRate         );
    const rowLabelGPUUsage:           string | null = translate(UITranslationKey.RowLabelGPUUsage          );
    const rowLabelCPUUsage:           string | null = translate(UITranslationKey.RowLabelCPUUsage          );
    const rowLabelMemoryUsage:        string | null = translate(UITranslationKey.RowLabelMemoryUsage       );

    // Get the value for each data row.
    const valueCurrentGameMinute:     string        = useValue(bindingCurrentGameMinute );
    const valuePreviousGameMinute:    string        = useValue(bindingPreviousGameMinute);
    const valueFrameRate:             string        = useValue(bindingFrameRate         );
    const valueGPUUsage:              string        = useValue(bindingGPUUsage          );
    const valueCPUUsage:              string        = useValue(bindingCPUUsage          );
    const valueMemoryUsage:           string        = useValue(bindingMemoryUsage       );

    // Get the tool tip for each data row.
    const toolTipCurrentGameMinute:   string | null = translate(UITranslationKey.ToolTipCurrentGameMinute );
    const toolTipPreviousGameMinute:  string | null = translate(UITranslationKey.ToolTipPreviousGameMinute);
    const toolTipFrameRate:           string | null = translate(UITranslationKey.ToolTipFrameRate         );
    const toolTipGPUUsage:            string | null = translate(UITranslationKey.ToolTipGPUUsage          );
    const toolTipCPUUsage:            string | null = translate(UITranslationKey.ToolTipCPUUsage          );
    const toolTipMemoryUsage:         string | null = translate(UITranslationKey.ToolTipMemoryUsage       );

    // Get the values for showing data rows.
    const showGPUUsage:               boolean       = useValue(bindingShowGPUUsage   );
    const showCPUUsage:               boolean       = useValue(bindingShowCPUUsage   );
    const showMemoryUsage:            boolean       = useValue(bindingShowMemoryUsage);

    // The panel content consists of 5 data rows.
    // GPU, CPU, and memory usage are displayed only when specified.
    return (
        <PanelSection>
                                 <PanelSectionRow left={<>{rowLabelCurrentGameMinute }</>} right={<>{valueCurrentGameMinute }</>} tooltip={<>{toolTipCurrentGameMinute }</>} />
                                 <PanelSectionRow left={<>{rowLabelPreviousGameMinute}</>} right={<>{valuePreviousGameMinute}</>} tooltip={<>{toolTipPreviousGameMinute}</>} />
                                 <PanelSectionRow left={<>{rowLabelFrameRate         }</>} right={<>{valueFrameRate         }</>} tooltip={<>{toolTipFrameRate         }</>} />
            {showGPUUsage    && (<PanelSectionRow left={<>{rowLabelGPUUsage          }</>} right={<>{valueGPUUsage          }</>} tooltip={<>{toolTipGPUUsage          }</>} />)}
            {showCPUUsage    && (<PanelSectionRow left={<>{rowLabelCPUUsage          }</>} right={<>{valueCPUUsage          }</>} tooltip={<>{toolTipCPUUsage          }</>} />)}
            {showMemoryUsage && (<PanelSectionRow left={<>{rowLabelMemoryUsage       }</>} right={<>{valueMemoryUsage       }</>} tooltip={<>{toolTipMemoryUsage       }</>} />)}
        </PanelSection>
    );
}
