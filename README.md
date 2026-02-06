# Ostrich emulator

A simple Game Boy emulator written in C# using SDL2 and ImGUI.

### Display

The display uses only integer scaling in relation to the emulators display buffer. This is so that the rendered image
better reflects the hardware being emulated.

Window Pipeline:
```
    Window
    ├─ Padding
    │   └─ Content Area
    │       └─ Integer-scaled emulator image
    └─ ImGui overlay
```

Padding has been added around the content area. This was present on original hardware and games would draw to the edge
of the screen with the knowledge that the padding was there.