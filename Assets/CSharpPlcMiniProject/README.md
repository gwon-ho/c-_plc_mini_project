# C# PLC Mini Project

This folder contains a PLC-style control layer for the realvirtual `DemoRealvirtualOld` scene.

The original scene and realvirtual components are used as the equipment model. The new scripts focus on PLC-style control logic that is easier to explain in an interview:

- `CanConveyorController`: feeds cans until the pick sensor is occupied.
- `HandlingController`: controls the gantry pick-and-place sequence with an enum-based state machine.
- `BoxConveyorController`: manages box feeding, row movement, and box change requests.
- `RobotController`: runs the robot unload cycle with an enum-based state machine.

## Create the Clean Scene

Use the Unity menu:

`Tools > C# PLC Mini Project > Create DemoRealvirtualOld PLC Scene`

This creates:

`Assets/Scenes/DemoRealvirtualOld_CleanControl.unity`

The original `Assets/realvirtual/Scenes/DemoRealvirtualOld.unity` scene is not modified.

## Interview Talking Point

The realvirtual scene provides industrial simulation components such as sensors, drives, grippers, MU handling, and PLC signals. The custom C# layer shows how to structure PLC-style sequence control using readable state machines, explicit inputs/outputs, and safer null checks.
