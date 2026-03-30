# Dartillery

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)
[![C#](https://img.shields.io/badge/C%23-12-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![CI](https://github.com/TZY-1/Dartillery/actions/workflows/ci.yml/badge.svg)](https://github.com/TZY-1/Dartillery/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/TZY-1/Dartillery/graph/badge.svg)](https://codecov.io/gh/TZY-1/Dartillery)

Dartillery is a dart throw simulation engine built with C# and .NET 8. The repository serves as an example of applying SOLID principles, Clean Code practices, and architectural patterns like Builder, Decorator, and Dependency Injection to a domain-specific problem.

Rather than calculating a static hit-or-miss probability, Dartillery computes target deviation by combining different models for human and physical behaviors. The solution includes the simulation backend as well as a Blazor WebAssembly frontend for visualization.

---

## Features & Simulation Engine

At its core, Dartillery constructs a simulation session using a fluent builder API. A virtual thrower is configured by setting a base precision and layering multiple deviation calculators representing different factors:

### Behavioral & Physical Deviation Models
- **Fatigue:** Simulates physical exhaustion over the course of a session or match, gradually degrading baseline accuracy based on a calculation curve.
- **Momentum:** Tracks hot and cold streaks by analyzing recent throw consistency. Consecutive accurate throws stabilize precision, while poor throws reduce accuracy.
- **Pressure:** Models situational psychological stress, applying an accuracy penalty when aiming for critical targets such as match checkouts.
- **Grouping:** Replicates the physical behavior of dart clustering. Subsequent darts in a single visit may be deflected or guided toward the impact points of earlier darts.
- **Target Difficulty:** Adjusts the base deviation dynamically depending on the physical size of the aimed segment.

Additional engine mechanics include **Player Profiles** (configuring skill, fatigue resistance, and pressure thresholds), deviation distribution methods (Gaussian spread with optional truncation to prevent statistical outliers), and **Event Listeners** for throw broadcasting.

### Blazor Web UI
The included Blazor Web application serves as an interactive frontend for the simulation engine:
- **Visual Dartboard:** An interactive SVG dartboard plotting coordinates for intended aim points vs. actual hit points.
- **Real-Time Statistics:** Tracks lifetime hit rate, average score, and visualizes recent throws and spread radiuses graphically.
- **Live Configuration:** Provides an interface to tweak player profiles, behavioral model parameters, and target selections mid-session.

---

## Project Architecture

The solution uses a multi-project structure to separate domains:

- **`Dartillery.Core`**: Foundational domain abstractions. Contains core models, interfaces, and dartboard mathematical geometry.
- **`Dartillery.Simulation`**: Contains the spread calculation logic, behavior models (Fatigue, Momentum, Pressure), and simulation session handlers.
- **`Dartillery`**: The main library wrapper. Exposes the fluent builder API, session state management, and event-dispatching mechanics.
- **`Dartillery.Web`**: The Blazor WebAssembly frontend acting as the graphical dashboard.
- **`Dartillery.Shared`**: Shared functionality, constants, and utilities.
- **`Dartillery.Tests`**: Unit and integration testing suite to validate statistical outcomes and simulation logic.

---

## Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022, JetBrains Rider, or VS Code

### Installation & Build

Clone this repository and compile the solution:

```bash
git clone https://github.com/TZY-1/Dartillery.git
cd Dartillery
dotnet build
```

Run the Blazor Web UI:

```bash
cd Source/Dartillery.Web
dotnet run
```

---

## Code Examples

The engine provides a fluent Builder API. You can use standard configurations, fine-tune existing models, or inject custom logic.

### 1. Quick Start (Standard Presets)
The easiest way to get started is by using the integrated presets that automatically apply a variety of behavioral models:

```csharp
// Build an advanced simulation pipeline using predefined logic
var session = new EnhancedDartboardSimulatorBuilder()
    .WithAmateurPlayer("Alice")     // Sets base skill and physical traits
    .WithRealisticFatigue()         // Exhaustion over time
    .WithStandardMomentum()         // Enables hot/cold streaks
    .WithCheckoutPsychology()       // Adds pressure on doubles
    .WithSimpleGrouping()           // Clustering within the same visit
    .WithTruncation()               // Caps maximum statistical scatter
    .BuildSession();

// Aim for Triple 20
var target = Target.Triple(20);

// Throw the dart
var result = session.Throw(target);
```

### 2. Fine-Tuning the Simulation
If granular control is required, the Builder allows manual override of thresholds, windows, and modifiers for the integrated models:

```csharp
var fineTunedSession = new EnhancedDartboardSimulatorBuilder()
    // Configure a custom player profile
    .WithPlayerProfile(new PlayerProfile 
    {
        Name = "Custom Bot",
        BaseSkill = 0.08,             // Custom base standard deviation
        FatigueRate = 0.005,          // Slower fatigue buildup
        PressureResistance = 0.8      // High resistance to stress
    })
    // Adjust Momentum window and bonuses
    .WithStandardMomentum(windowSize: 10, hotHandBonus: 0.1, coldStreakPenalty: 0.2)
    // Cap maximum deviation to prevent extreme outliers
    .WithTruncation()
    .BuildSession();
```

### 3. Extending the Engine (Custom Behaviors & Events)
Custom logic can be implemented by satisfying the core interfaces (`IFatigueModel`, `IPressureModel`, `IGroupingModel`, `IThrowEventListener`, etc.) without altering the library source code:

```csharp
public class MyCustomRecalibrationModel : IFatigueModel 
{
    // Your custom implementation here...
}

public class MyDatabaseLogger : IThrowEventListener
{
    // E.g., Save results to a DB after every throw...
}

// Injecting your custom implementations into the engine pipeline
var customSession = new EnhancedDartboardSimulatorBuilder()
    .WithProfessionalPlayer("Bob")
    .WithFatigueModel(new MyCustomRecalibrationModel()) // Applying custom model
    .AddEventListener(new MyDatabaseLogger())           // Listening to events
    .BuildSession();
```

---

## License

This project is licensed under the [MIT License](LICENSE).
