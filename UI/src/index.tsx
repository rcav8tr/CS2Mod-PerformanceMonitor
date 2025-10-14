import { ModRegistrar   } from "cs2/modding";

import { MainButton     } from "mainButton";
import { MainPanel      } from "mainPanel";
import   mod              from "../mod.json";

const register: ModRegistrar = (moduleRegistry) => 
{
    // Append mod's main button to info view button list in game's top left.
    moduleRegistry.append("GameTopLeft", MainButton);

    // Append mod's main panel to the game's main panel.
    moduleRegistry.append("Game", MainPanel);

    // Registration is complete.
    console.log(mod.id + " registration complete.");
}

export default register;