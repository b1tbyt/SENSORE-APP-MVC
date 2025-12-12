# Gang of Four (GoF) Design Patterns Implementation

## ?? Overview

This document describes the implementation of three fundamental Gang of Four (GoF) design patterns in the SENSORE APP:

1. **Factory Pattern** (Creational)
2. **Adapter Pattern** (Structural)
3. **Strategy Pattern** (Behavioral)

---

## ?? 1. Factory Pattern (Creational)

### Purpose
Provide a unified interface for creating different types of objects without exposing the creation logic.

### Location
`Services/Factories/MessageFactory.cs`

### Implementation

#### Interfaces
```csharp
public interface IMessageFactory
{
    Message CreateMessage(string sender, string text);
    Message CreateSystemMessage(string text);
    Message CreateTimestampedMessage(string sender, string text, DateTime timestamp);
}
```

#### Concrete Factories

**MessageFactory**
- Creates standard messages with current timestamp
- Creates system messages for alerts and notifications
- Creates timestamped messages for historical data

```csharp
var message = _messageFactory.CreateMessage("Patient", "I feel better today");
```

**AlertMessageFactory**
- Specialized factory for high-priority alerts
- Creates pressure alerts with emoji indicators
- Creates reposition reminder alerts

```csharp
var pressureAlert = _alertFactory.CreatePressureAlert(250);
```

### Benefits
? Encapsulates object creation logic  
? Centralizes validation  
? Easy to extend with new message types  
? Reduces coupling between client and objects  

### Usage in Controller

```csharp
public class PatientController : Controller
{
    private readonly IMessageFactory _messageFactory;
    private readonly AlertMessageFactory _alertFactory;

    public PatientController(IMessageFactory messageFactory, AlertMessageFactory alertFactory)
    {
        _messageFactory = messageFactory;
        _alertFactory = alertFactory;
    }

    public IActionResult Messages()
    {
        var messages = new List<Message>
        {
            _messageFactory.CreateMessage("Clinician", "How are you feeling?"),
            _messageFactory.CreateSystemMessage("Daily check-in reminder"),
            _alertFactory.CreatePressureAlert(210)
        };
        return View(messages);
    }
}
```

### Real-World Scenarios
- Different message types (patient, clinician, system)
- Alert messages with special formatting
- Historical messages with custom timestamps
- System notifications

---

## ?? 2. Adapter Pattern (Structural)

### Purpose
Convert the interface of a class into another interface clients expect, allowing incompatible interfaces to work together.

### Location
`Services/Adapters/PressureSensorAdapter.cs`

### Problem Solved
- Integrate legacy sensor systems (24×24 grid ? 32×32 grid)
- Support different external APIs
- Normalize data from various sources to standard format

### Implementation

#### Interfaces

**ILegacyPressureSensor** (External/Legacy Interface)
```csharp
public interface ILegacyPressureSensor
{
    int[][] GetLegacyReadings();  // 0-1000 scale
    double GetCalibrationFactor();
}
```

**IStandardPressureSensor** (Our Standard Interface)
```csharp
public interface IStandardPressureSensor
{
    byte[,] GetNormalizedReadings();  // 0-255 scale, 32×32
    double GetQualityScore();
}
```

#### Adapter Implementation

```csharp
public class LegacyPressureSensorAdapter : IStandardPressureSensor
{
    private readonly ILegacyPressureSensor _legacySensor;

    public byte[,] GetNormalizedReadings()
    {
        // Convert legacy format to standard format
        var legacyReadings = _legacySensor.GetLegacyReadings();
        var calibrationFactor = _legacySensor.GetCalibrationFactor();
        
        // Resize from any size to 32×32
        // Convert from 0-1000 scale to 0-255 scale
        // Apply calibration factor
        
        return normalized;
    }
}
```

### Key Features

1. **Format Conversion**
   - Converts 0-1000 scale to 0-255 scale
   - Applies calibration factors

2. **Dimension Resizing**
   - Supports any legacy grid size
   - Resamples/interpolates to 32×32 standard

3. **Quality Assessment**
   - Provides quality score of conversion
   - Validates calibration data

### Concrete Adapters

**LegacyPressureSensorAdapter**
- Adapts old sensor systems
- Handles dimension mismatch
- Applies calibration

**ThirdPartySensorAdapter**
- Adapts third-party APIs
- Converts from external data formats
- Normalizes accuracy scores

### Usage

```csharp
// Use legacy sensor as if it were standard
ILegacyPressureSensor legacySensor = GetLegacySensor();
IStandardPressureSensor standardSensor = new LegacyPressureSensorAdapter(legacySensor);

byte[,] normalizedData = standardSensor.GetNormalizedReadings();
double quality = standardSensor.GetQualityScore();
```

### Benefits
? Works with incompatible interfaces  
? No modification to legacy code needed  
? Clean integration of external systems  
? Maintains abstraction boundaries  

### Real-World Scenarios
- Integrating older pressure sensor hardware
- Connecting to third-party medical devices
- Supporting multiple sensor manufacturers
- Legacy system migration

---

## ?? 3. Strategy Pattern (Behavioral)

### Purpose
Define a family of algorithms, encapsulate each one, and make them interchangeable. Strategy lets the algorithm vary independently from clients that use it.

### Location
`Services/Strategies/RiskAnalysisStrategy.cs`

### Problem Solved
Different clinical approaches need different risk calculation methods. Switch between strategies without changing client code.

### Strategies Implemented

#### 1. Peak Pressure Strategy
Focuses on maximum pressure values
```csharp
var strategy = new PeakPressureStrategy();
double riskScore = strategy.CalculateRiskScore(heatmap);
// 0-3: Low, 3-7: Moderate, 7-10: High
```

#### 2. Contact Area Strategy
Focuses on percentage of cells detecting pressure
```csharp
var strategy = new ContactAreaStrategy();
double riskScore = strategy.CalculateRiskScore(heatmap);
// Based on % of sensors above threshold
```

#### 3. Hotspot Concentration Strategy
Focuses on high-pressure areas
```csharp
var strategy = new HotspotConcentrationStrategy();
double riskScore = strategy.CalculateRiskScore(heatmap);
// Combines hotspot count + average pressure
```

#### 4. Pressure Distribution Strategy
Focuses on variance and uniformity
```csharp
var strategy = new PressureDistributionStrategy();
double riskScore = strategy.CalculateRiskScore(heatmap);
// Analyzes standard deviation of pressure
```

### Strategy Interface

```csharp
public interface IRiskAnalysisStrategy
{
    double CalculateRiskScore(byte[,] heatmap);
    string GetStrategyName();
    bool RequiresImmediateIntervention(byte[,] heatmap);
}
```

### Context: StrategyBasedRiskAnalyzer

```csharp
public class StrategyBasedRiskAnalyzer
{
    private IRiskAnalysisStrategy _strategy;

    public void SetStrategy(IRiskAnalysisStrategy strategy)
    {
        _strategy = strategy;
    }

    public RiskAnalysisResult Analyze(byte[,] heatmap)
    {
        return new RiskAnalysisResult
        {
            RiskScore = _strategy.CalculateRiskScore(heatmap),
            StrategyUsed = _strategy.GetStrategyName(),
            RequiresIntervention = _strategy.RequiresImmediateIntervention(heatmap)
        };
    }

    public RiskAnalysisResult AnalyzeWithAllStrategies(byte[,] heatmap)
    {
        // Calculate using all strategies and average
    }
}
```

### Usage in Controller

```csharp
public class PatientController : Controller
{
    private readonly StrategyBasedRiskAnalyzer _riskAnalyzer;

    public IActionResult Index(string timeframe = "1h")
    {
        var heatmap = _pressureService.GetCurrentHeatmap();
        
        // Uses default strategy
        var analysis = _riskAnalyzer.Analyze(heatmap);
        vm.RiskScore = analysis.RiskScore;
        vm.RiskCategory = DetermineCategory(analysis.RiskScore);
        
        return View(vm);
    }

    public IActionResult RiskAnalysis()
    {
        var heatmap = _pressureService.GetCurrentHeatmap();
        
        // Get all available strategies
        var strategies = _riskAnalyzer.GetAvailableStrategies();
        
        // Analyze with all strategies and return average
        var result = _riskAnalyzer.AnalyzeWithAllStrategies(heatmap);
        
        return Json(result);
    }
}
```

### Benefits
? Encapsulates algorithms  
? Switch algorithms at runtime  
? Eliminates conditional logic  
? Easy to add new analysis methods  
? Single responsibility per strategy  

### Real-World Scenarios
- Different clinical approaches to risk assessment
- Multiple patient condition categories
- Customizable analysis per clinician
- A/B testing different algorithms
- Clinical research flexibility

---

## ?? How Patterns Work Together

### Data Flow Example

```
1. Patient Dashboard Loads
   ?
2. Factory creates initial messages
   ?
3. Legacy sensor data received
   ?
4. Adapter converts to standard format
   ?
5. Strategy analyzes risk
   ?
6. Results displayed to patient
```

### Service Registration (Program.cs)

```csharp
// Factory Pattern
builder.Services.AddSingleton<IMessageFactory, MessageFactory>();
builder.Services.AddSingleton<AlertMessageFactory>();

// Strategy Pattern
builder.Services.AddSingleton<StrategyBasedRiskAnalyzer>();

// Adapter Pattern (instantiated as needed)
```

---

## ?? Testing Strategies

### Unit Testing Factory
```csharp
[Test]
public void CreateMessage_WithValidInput_ReturnsMessage()
{
    var factory = new MessageFactory();
    var message = factory.CreateMessage("Patient", "Test message");
    
    Assert.That(message.Sender, Is.EqualTo("Patient"));
    Assert.That(message.Text, Is.EqualTo("Test message"));
}
```

### Unit Testing Adapter
```csharp
[Test]
public void ConvertLegacySensor_ResizesAndNormalizes()
{
    var legacySensor = new MockLegacyPressureSensor();
    var adapter = new LegacyPressureSensorAdapter(legacySensor);
    
    var normalized = adapter.GetNormalizedReadings();
    
    Assert.That(normalized.GetLength(0), Is.EqualTo(32));
    Assert.That(normalized.GetLength(1), Is.EqualTo(32));
}
```

### Unit Testing Strategy
```csharp
[Test]
public void PeakPressureStrategy_CalculatesCorrectRiskScore()
{
    var strategy = new PeakPressureStrategy();
    var heatmap = new byte[32, 32];
    heatmap[0, 0] = 255;
    
    var risk = strategy.CalculateRiskScore(heatmap);
    
    Assert.That(risk, Is.GreaterThan(0).And.LessThanOrEqualTo(10));
}
```

---

## ?? Extension Points

### Adding New Message Type
```csharp
public class CustomMessageFactory : IMessageFactory
{
    // Implement interface with custom logic
}
```

### Adding New Sensor Adapter
```csharp
public class CustomSensorAdapter : IStandardPressureSensor
{
    // Adapt your specific sensor format
}
```

### Adding New Risk Analysis Strategy
```csharp
public class MyAnalysisStrategy : IRiskAnalysisStrategy
{
    public double CalculateRiskScore(byte[,] heatmap)
    {
        // Your custom algorithm
    }
    
    public string GetStrategyName() => "My Analysis";
    
    public bool RequiresImmediateIntervention(byte[,] heatmap)
    {
        // Your intervention logic
    }
}
```

---

## ?? Design Pattern Benefits Summary

| Pattern | Benefit | Use Case |
|---------|---------|----------|
| **Factory** | Centralized creation, extensibility | Multiple message types |
| **Adapter** | Legacy system integration | Sensor compatibility |
| **Strategy** | Runtime algorithm switching | Risk analysis algorithms |

---

## ?? Code Organization

```
Services/
??? Factories/
?   ??? MessageFactory.cs          # Factory Pattern
??? Adapters/
?   ??? PressureSensorAdapter.cs   # Adapter Pattern
??? Strategies/
    ??? RiskAnalysisStrategy.cs    # Strategy Pattern
```

---

## ? Checklist for Using These Patterns

- [ ] Use `IMessageFactory` to create messages
- [ ] Use `AlertMessageFactory` for high-priority alerts
- [ ] Use `LegacyPressureSensorAdapter` for old sensors
- [ ] Use `StrategyBasedRiskAnalyzer` for risk analysis
- [ ] Register all factories in `Program.cs`
- [ ] Inject strategies into controllers
- [ ] Write unit tests for each pattern
- [ ] Document custom implementations

---

## ?? Production Readiness

? All patterns properly implemented  
? Dependency injection configured  
? Clear interfaces defined  
? Extensible architecture  
? Well-documented code  
? Unit test ready  

**Status**: Ready for production use and further extension.
