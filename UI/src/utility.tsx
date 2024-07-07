// Ensure element is not outside the window.
export function checkPositionOnWindow(positionX: number, positionY: number, elementWidth: number, elementHeight: number): {x: number, y: number}
{
    // Check position against left and top.
    if (positionX < 0) { positionX = 0.0; }
    if (positionY < 0) { positionY = 0.0; }

    // Check position against right and bottom.
    if (positionX > window.innerWidth  - elementWidth ) { positionX = window.innerWidth  - elementWidth;  }
    if (positionY > window.innerHeight - elementHeight) { positionY = window.innerHeight - elementHeight; }

    // Return the checked position.
    return {x: positionX, y: positionY};
}
