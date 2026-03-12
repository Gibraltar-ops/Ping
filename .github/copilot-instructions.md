# Crypt of the AmnBat - AI Coding Agent Instructions

This document provides guidance for AI coding agents to effectively assist in the development of "Crypt of the AmnBat".

## Project Overview & Architecture

This is a 3D stealth horror game developed in Unity. The player is seemingly blind and must navigate using a "sonar ping" mechanic. The primary antagonist is a monster that hunts the player by detecting these sonar pings.

- **Engine:** Unity. All development and testing happens within the Unity Editor.
- **Core Mechanic:** The player emits sonar pings to visualize the environment. These pings also attract the monster.
- **Player:** The player's logic is primarily located in `Assets/Scripts/Player/`.
  - `SonarScript.cs`: Manages the sonar ping ability, including cooldowns.
  - `RaycastSonar.cs`: Implements the sonar ping itself. It performs a sphere cast, identifies colliders, and notifies them of the ping by calling `RegisterSonarPing`.
- **Monster AI:** The monster's behavior is controlled by `Assets/Scripts/Enemies/MonsterAI.cs`.
  - It uses a `NavMeshAgent` for navigation.
  - It has a `suspicionLevel` that increases when it detects a sonar ping via the `RegisterSonarPing` method.
  - **Behavior States:**
    - **Patrolling:** The default state. The monster wanders within a defined radius.
    - **Investigating:** When a sonar ping is detected but the `suspicionLevel` is below the `detectionThreshold`, the monster moves to the source of the ping.
    - **Charging:** If `suspicionLevel` exceeds the `detectionThreshold`, the monster enters a "charging" state and actively chases the player for a short duration.
- **Animation:** Monster animations are managed by `Assets/Scripts/Enemies/MonsterAnimation/MonsterAnimationManager.cs`, which is controlled by the `MonsterAI`.

## Key Data Flow: Sonar Ping Detection

1.  The `Player` triggers a sonar ping (managed by `SonarScript.cs`).
2.  `RaycastSonar.cs` casts a sphere and finds all colliders on the `Hittable` layer.
3.  For each `Hittable` collider, it uses `GetComponentInParent<MonsterAI>()` to find the `MonsterAI` script.
4.  It calls the `RegisterSonarPing(Vector3 sonarPosition, float strength)` method on the `MonsterAI` instance.
5.  The `MonsterAI` updates its `suspicionLevel` and changes its behavior state accordingly.

## Development Workflow

- **Primary Tool:** All development, scene editing, and playtesting is done through the **Unity Editor**.
- **Scripts:** All game logic is written in C# and located in the `Assets/Scripts/` directory.
- **Dependencies:** The project uses the standard Unity `InputSystem` package. Player actions are defined in `Assets/InputSystem_Actions.inputactions`.

## Coding Conventions

- **Component Access:** Use `[SerializeField]` to expose fields to the Unity Editor for references (e.g., `NavMeshAgent`, `Transform`). Fallback to `GetComponent<T>()` or `GameObject.FindGameObjectWithTag()` in `Awake()` if a reference is not assigned in the editor.
- **State Management:** The monster's AI uses boolean flags (`isCharging`, `isInvestigating`, `isWaiting`) and a `suspicionLevel` float to manage its state. State transitions are handled within the `Update()` and `RegisterSonarPing()` methods.
- **Coroutines:** Asynchronous or timed behaviors, like the monster's "charging" phase or waiting at a patrol point, are managed using Coroutines.
- **Layers:** The project uses Unity's Layer system for physics and detection. Key layers include `groundLayer` and `playerLayerMask`.

Further instructions:

Jag är en programmeringsstudent. När jag ber dig om assistans om hur man kodar vill jag att du gör det på ett pedagogiskt sätt, sakligt och praktiskt. Ingen snygga workarounds eller bytande av funktionsnamn utan all kod skall gå att få in i den ursprungliga koden utan att behöva skriva om hela databasen. Jag vill dessutom att du gör grundlig research varje gång jag ber om assistans och ställer frågor innan du ger mig ett svar för att dubbelsäkra att vi delar samma uppfattning. Innan du ger mig kod, berätta vad den gör och identifiera vad den eventuellt skulle kunna ha sönder. Skriv ditt svar som om jag har chefen tittandes över min axel. Det skall vara korrekt kod och korrekt beskrivning.
