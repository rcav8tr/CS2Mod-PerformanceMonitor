import { ModRegistrar } from "cs2/modding";
import { MainButton } from "components/mainButton";
import { MainPanel } from "components/mainPanel";

const register: ModRegistrar = (moduleRegistry) => 
{
    // Append mod's main button to info view button list in game's top left.
    moduleRegistry.append("GameTopLeft", MainButton);

    // Append mod's main panel to the game's main panel.
    moduleRegistry.append("Game", MainPanel);
}

export default register;