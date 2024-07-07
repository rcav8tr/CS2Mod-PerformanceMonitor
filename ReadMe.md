# Performance Monitor

Monitor your computer's performance data.

# Main Window
To view the performance data, click the icon (stopwatch) in the upper left of the screen or press Ctrl+Shift+P.
To close the window, click the icon again, press Ctrl+Shift+P again, or click the close (X) button on the window.
The window display status is saved between games.
The activation key sequence can be changed in Options.

The window starts in the upper left of the screen, but can be dragged to a new position.
The window position is saved between games.

# Performance Data
The main window shows the following performance data.

## Current Game Minute
## Previous Game Minute
The real time in seconds is displayed for both the current game minute and the previous game minute (i.e. the game minute most recently ended).
- The standard durations are about 3.00, 1.50, 0.75 seconds for sim speeds 1, 2, 3 respectively.
  If your game is running slower than the standard duration, it means your CPU cannot keep up with the requested sim speed.
  There is nothing wrong with running a game at a slower duration than standard (I do it all the time), but now you are aware of it.
- Changing the sim speed will cause both counters to reset.
- After a game pause, the current game minute timer will continue where it left off.

## Frame Rate
Frame Rate is the number of frames displayed per second (FPS).
- A lower Frame Rate (less than about 30) results in stuttering or choppy animation but may be easier on the GPU.
  Reduce your Graphics settings in Options to increase your frame rate.
- A higher Frame Rate results in smoother animation but may be harder on the GPU.
  See GPU Usage below.
- The value is accumulated over one second and updated once per second.

## GPU Usage
GPU Usage is the amount the Graphics Processing Unit (GPU) is utilized as a percent of its capacity.
- A value approaching 100% may indicate that the GPU cannot keep up with the requested frame rate and may result in a lower frame rate.
  Reduce your Graphics settings in Options to decrease your GPU usage.
- The value is accumulated over one second and updated once per second.
- The value is for your entire computer, not just for the game running on your computer.
- Displayed only for computers running Windows with NVidia graphics card(s).

## CPU Usage
CPU Usage is the amount the (Central Processing Unit) CPU is utilized as a percent of its capacity.
- A value approaching 100% may indicate that the CPU cannot keep up with the requested simulation speed.
  Close other programs running on your computer so more of the CPU can be devoted to the game.
- The value may differ slightly from Windows task manager due to differences in the way the data is obtained.
- The value is a weighted average over the previous three seconds (to reduce volatility) and updated once per second.
- The value is for your entire computer, not just for the game running on your computer.
- Displayed only for computers running Windows.

## Memory Usage
Memory Usage is the amount of memory utilized as a percent of its capacity.
- A value approaching 100% may indicate that your computer needs to use slower disk space to make up for insufficient high-speed memory.
  Close other programs running on your computer so more of the memory can be devoted to the game.
- The value is a snapshot at the time the data is displayed once per second.
- The value is for your entire computer, not just for the game running on your computer.
- Displayed only for computers running Windows.

# Options
The Activation Key allows you to set the keyboard sequence to show and hide the window.

The Reset Window Position button sets the window to its default position in case the window somehow ends up off screen.

The current version of the mod is displayed.

# Compatibility
The mod is translated into all the languages supported by the base game.

There are no known mods with which this mod is incompatible.