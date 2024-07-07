// Element IDs.
export class UIElementID
{
    // The prefix includes the Paradox username and mod name to avoid conflicts with elements from other mods.
    private static elementIDPrefix: string = "rcav8tr-performance-monitor-";

    // Define an element ID for each element that is top level or that needs to be found by ID.
    public static MainButton:     string = UIElementID.elementIDPrefix + "main-button";
    public static MainPanel:      string = UIElementID.elementIDPrefix + "main-panel";
    public static MainPanelClose: string = UIElementID.elementIDPrefix + "main-panel-close";
}
