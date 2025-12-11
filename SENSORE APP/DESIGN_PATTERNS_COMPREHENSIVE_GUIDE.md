# Gang of Four (GoF) Design Patterns in SENSORE APP

## ?? Complete Guide to Design Patterns Implementation

---

## ?? Overview

This document provides a complete reference for the three Gang of Four design patterns implemented in SENSORE APP:

1. **Factory Pattern** (Creational)
2. **Adapter Pattern** (Structural)  
3. **Strategy Pattern** (Behavioral)

Each pattern solves a specific architectural problem and makes the code more maintainable, testable, and extensible.

---

## ?? 1. FACTORY PATTERN (Creational)

### What Is It?

The Factory Pattern provides a unified interface for creating objects without exposing the creation logic to the client. Instead of using `new` directly, you call a factory method that handles object construction.

### Purpose

- **Centralize object creation logic**
- **Encapsulate creation complexity**
- **Enable easy extension with new types**
- **Reduce coupling between client and concrete classes**

### Implementation in SENSORE APP

**File**: `Services/Factories/MessageFactory.cs`

#### Interface
```csharp
public interface IMessageFactory
{
    Message CreateMessage(string sender, string text);
    Message CreateSystemMessage(string text);
    Message CreateTimestampedMessage(string sender, string text, DateTime timestamp);
}
```

#### Concrete Factory 1: MessageFactory
```csharp
public class MessageFactory : IMessageFactory
{
    // Creates standard messages with current timestamp
    // Creates system messages for alerts
    // Creates timestamped messages for history
}
```

#### Concrete Factory 2: AlertMessageFactory
```csharp
public class AlertMessageFactory : IMessageFactory
{
    // Specializes in creating alert messages
    // Adds emoji prefixes automatically
    // Creates high-priority alerts
    // Creates reposition reminders
}
```

### How It's Used

#### In PatientController.cs
```csharp
public class PatientController : Controller
{
    private readonly IMessageFactory _messageFactory;
    private readonly AlertMessageFactory _alertFactory;

    public IActionResult Messages()
    {
        var messages = new List<Message>
        {
            // Standard message from factory
            _messageFactory.CreateMessage("Clinician", "How are you feeling?"),
            
            // System message from factory
            _messageFactory.CreateSystemMessage("Daily reminder"),
            
            // Alert message from alert factory
            _alertFactory.CreatePressureAlert(240)
        };
        return View(messages);
    }
}
```

### Benefits

| Benefit | Explanation |
|---------|------------|
| **Encapsulation** | Creation logic is hidden from client |
| **Validation** | Input validation happens at factory |
| **Flexibility** | Easy to add new message types |
| **Testability** | Can mock factory for unit tests |
| **Consistency** | All messages created same way |
| **Maintainability** | Change creation logic in one place |

### Real-World Example

```csharp
// Bad approach (tight coupling)
var msg = new Message 
{ 
    Sender = "Patient", 
    Text = text,
    Timestamp = DateTime.UtcNow 
};

// Good approach (factory pattern)
var msg = _messageFactory.CreateMessage("Patient", text);
```

### Extension Example

To add a new message type:

```csharp
public class MedicalGradeMessageFactory : IMessageFactory
{
    public Message CreateMessage(string sender, string text)
    {
        // Validate medical message format
        if (!IsValidMedicalMessage(text))
            throw new InvalidOperationException("Invalid medical format");
            
        return new Message 
        { 
            Sender = sender, 
            Text = text, 
            Timestamp = DateTime.UtcNow,
            IsVerified = true  // Custom property
        };
    }
}

// Register in Program.cs
builder.Services.AddSingleton<IMessageFactory>(
    new MedicalGradeMessageFactory());
```

---

## ?? 2. ADAPTER PATTERN (Structural)

### What Is It?

The Adapter Pattern converts the interface of a class into another interface that clients expect. It allows incompatible interfaces to work together without modifying existing code.

### Purpose

- **Integrate incompatible systems**
- **Legacy system integration**
- **Format conversion**
- **API compatibility layer**

### The Problem It Solves

You have a legacy pressure sensor that outputs:
- Grid size: 24×24 (not 32×32)
- Pressure range: 0-1000 (not 0-255)
- Calibration factor: Must be applied

Your system expects:
- Grid size: 32×32
- Pressure range: 0-255
- Normalized data

**Solution**: Use an adapter!

### Implementation in SENSORE APP

**File**: `Services/Adapters/PressureSensorAdapter.cs`

#### External Interface (Legacy)
```csharp
public interface ILegacyPressureSensor
{
    int[][] GetLegacyReadings();      // 0-1000 scale
    double GetCalibrationFactor();
}
```

#### Our Standard Interface
```csharp
public interface IStandardPressureSensor
{
    byte[,] GetNormalizedReadings();   // 0-255 scale, 32×32
    double GetQualityScore();
}
```

#### The Adapter
```csharp
public class LegacyPressureSensorAdapter : IStandardPressureSensor
{
    private readonly ILegacyPressureSensor _legacySensor;

    public byte[,] GetNormalizedReadings()
    {
        var legacyReadings = _legacySensor.GetLegacyReadings();
        var calibrationFactor = _legacySensor.GetCalibrationFactor();

        // Resize from any dimension to 32×32
        // Convert from 0-1000 to 0-255 scale
        // Apply calibration factor
        
        return normalized;
    }
}
```

### How It Works

```
Legacy Sensor Data
     ?
[Convert: 24×24 ? 32×32]
     ?
[Scale: 0-1000 ? 0-255]
     ?
[Apply: Calibration factor]
     ?
Standard Format Data
```

### How It's Used

```csharp
// Old system - would not work directly
ILegacyPressureSensor legacySensor = new LegacyHospitalSensor();

// Adapt it to our standard interface
IStandardPressureSensor adapted = 
    new LegacyPressureSensorAdapter(legacySensor);

// Now it works with our code
byte[,] normalizedData = adapted.GetNormalizedReadings();
double quality = adapted.GetQualityScore();

// Rest of code works without knowing about legacy system
ProcessData(normalizedData, quality);
```

### Key Features

| Feature | Description |
|---------|------------|
| **Dimension Resizing** | Handles any grid size ? 32×32 |
| **Value Normalization** | Converts any range ? 0-255 |
| **Calibration** | Applies sensor calibration factors |
| **Quality Assessment** | Reports conversion quality |
| **No Legacy Modification** | Legacy code unchanged |

### Concrete Implementations

#### LegacyPressureSensorAdapter
```csharp
// Handles old hospital sensors
// Grid: 24×24, Range: 0-1000
// Output: 32×32, Range: 0-255
```

#### ThirdPartySensorAdapter
```csharp
// Handles third-party APIs
// Various formats ? Standard format
// Adapts external data structures
```

### Benefits

| Benefit | Explanation |
|---------|------------|
| **Compatibility** | Old systems work with new code |
| **No Modification** | Legacy code stays unchanged |
| **Clean Integration** | Minimal coupling |
| **Maintainability** | Changes isolated in adapter |
| **Reusability** | One adapter for many sensors |
| **Testability** | Can mock sensors in tests |

### Real-World Scenario

```csharp
// Your new system needs 32×32, 0-255
// But you have 4 different sensor types:

var sensor1 = new LegacyPressureSensorAdapter(
    new OldHospitalSensor());      // 24×24, 0-1000

var sensor2 = new ThirdPartySensorAdapter(
    new ExternalAPIClient());      // Various formats

var sensor3 = new ModernSensorAdapter(
    new ModernPressureSensor());   // Already 32×32, 0-255

// All now work identically!
byte[,] data1 = sensor1.GetNormalizedReadings();
byte[,] data2 = sensor2.GetNormalizedReadings();
byte[,] data3 = sensor3.GetNormalizedReadings();

// Same format for all
ProcessHeatmap(data1);
ProcessHeatmap(data2);
ProcessHeatmap(data3);
```

---

## ?? 3. STRATEGY PATTERN (Behavioral)

### What Is It?

The Strategy Pattern defines a family of algorithms, encapsulates each one, and makes them interchangeable. The strategy pattern lets the algorithm vary independently from clients that use it.

### Purpose

- **Multiple algorithms for same problem**
- **Runtime algorithm selection**
- **Eliminate conditional logic**
- **Easy to add new algorithms**

### The Problem It Solves

You need different ways to calculate patient risk:

1. **Peak Pressure Strategy** - Focus on maximum values
2. **Contact Area Strategy** - Focus on contact percentage
3. **Hotspot Strategy** - Focus on high-pressure zones
4. **Distribution Strategy** - Focus on pressure uniformity

Bad approach (without strategy):
```csharp
// Lots of if-else statements
if (analysisType == "peak")
{
    // peak pressure calculation
}
else if (analysisType == "contact")
{
    // contact area calculation
}
// ... more conditions
```

Good approach (with strategy):
```csharp
_analyzer.SetStrategy(new PeakPressureStrategy());
var result = _analyzer.Analyze(heatmap);
```

### Implementation in SENSORE APP

**File**: `Services/Strategies/RiskAnalysisStrategy.cs`

#### Strategy Interface
```csharp
public interface IRiskAnalysisStrategy
{
    double CalculateRiskScore(byte[,] heatmap);
    string GetStrategyName();
    bool RequiresImmediateIntervention(byte[,] heatmap);
}
```

#### Strategy 1: Peak Pressure Strategy
```csharp
public class PeakPressureStrategy : IRiskAnalysisStrategy
{
    public double CalculateRiskScore(byte[,] heatmap)
    {
        // Find maximum pressure
        byte maxPressure = 0;
        foreach (byte val in heatmap)
        {
            if (val > maxPressure) maxPressure = val;
        }

        // Score: 0-10 based on max pressure
        // 0-100: Low (0-3)
        // 100-200: Moderate (3-7)
        // 200+: High (7-10)
        return (maxPressure / 255.0) * 10.0;
    }

    public bool RequiresImmediateIntervention(byte[,] heatmap)
    {
        byte maxPressure = /* get max */;
        return maxPressure >= 240;
    }
}
```

#### Strategy 2: Contact Area Strategy
```csharp
public class ContactAreaStrategy : IRiskAnalysisStrategy
{
    public double CalculateRiskScore(byte[,] heatmap)
    {
        // Count cells with pressure ? threshold
        int contactCells = 0;
        foreach (byte val in heatmap)
        {
            if (val >= 50) contactCells++;
        }

        // Score based on contact percentage
        double contactPercentage = 
            (contactCells * 100.0) / (heatmap.GetLength(0) * heatmap.GetLength(1));

        return (contactPercentage / 100.0) * 10.0;
    }
}
```

#### Strategy 3: Hotspot Strategy
```csharp
public class HotspotConcentrationStrategy : IRiskAnalysisStrategy
{
    public double CalculateRiskScore(byte[,] heatmap)
    {
        // Analyze high-pressure areas
        int hotspotCells = 0;
        double totalPressure = 0;

        foreach (byte val in heatmap)
        {
            if (val >= 180) hotspotCells++;
            totalPressure += val;
        }

        // Combine hotspot count + average pressure
        double hotspotPercentage = 
            (hotspotCells * 100.0) / (heatmap.GetLength(0) * heatmap.GetLength(1));
        double averagePressure = 
            totalPressure / (heatmap.GetLength(0) * heatmap.GetLength(1));

        double riskFromHotspots = (hotspotPercentage / 100.0) * 5.0;
        double riskFromAverage = (averagePressure / 255.0) * 5.0;

        return riskFromHotspots + riskFromAverage;
    }
}
```

#### Strategy 4: Distribution Strategy
```csharp
public class PressureDistributionStrategy : IRiskAnalysisStrategy
{
    public double CalculateRiskScore(byte[,] heatmap)
    {
        // Analyze pressure variance
        double mean = CalculateMean(heatmap);
        double stdDev = CalculateStandardDeviation(heatmap, mean);

        // High variance = poor distribution = higher risk
        return Math.Min(10, (stdDev / 50.0) * 10.0);
    }
}
```

#### Context: Strategy Analyzer
```csharp
public class StrategyBasedRiskAnalyzer
{
    private IRiskAnalysisStrategy _strategy;

    public void SetStrategy(IRiskAnalysisStrategy strategy)
    {
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
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

    // Get all available strategies
    public IEnumerable<string> GetAvailableStrategies()
    {
        return new[]
        {
            "Peak Pressure Analysis",
            "Contact Area Analysis",
            "Hotspot Concentration Analysis",
            "Pressure Distribution Analysis"
        };
    }

    // Analyze with all strategies and return average
    public RiskAnalysisResult AnalyzeWithAllStrategies(byte[,] heatmap)
    {
        var results = _availableStrategies
            .Select(s => s.CalculateRiskScore(heatmap))
            .ToList();

        return new RiskAnalysisResult
        {
            RiskScore = results.Average(),
            StrategyUsed = "Multi-Strategy Consensus",
            RequiresIntervention = _availableStrategies
                .Any(s => s.RequiresImmediateIntervention(heatmap))
        };
    }
}
```

### How It's Used

#### In PatientController.cs
```csharp
public class PatientController : Controller
{
    private readonly StrategyBasedRiskAnalyzer _riskAnalyzer;

    public IActionResult Index(string timeframe = "1h")
    {
        var heatmap = _pressureService.GetCurrentHeatmap();

        // Use default strategy (Peak Pressure)
        var analysis = _riskAnalyzer.Analyze(heatmap);
        
        vm.RiskScore = analysis.RiskScore;
        vm.RiskCategory = DetermineCategory(analysis.RiskScore);

        return View(vm);
    }

    public IActionResult RiskAnalysis()
    {
        var heatmap = _pressureService.GetCurrentHeatmap();

        // Switch strategy based on user request
        _riskAnalyzer.SetStrategy(new ContactAreaStrategy());
        var result1 = _riskAnalyzer.Analyze(heatmap);

        _riskAnalyzer.SetStrategy(new HotspotConcentrationStrategy());
        var result2 = _riskAnalyzer.Analyze(heatmap);

        // Or get consensus from all strategies
        var consensus = _riskAnalyzer.AnalyzeWithAllStrategies(heatmap);

        return Json(new { result1, result2, consensus });
    }
}
```

### Benefits

| Benefit | Explanation |
|---------|------------|
| **Flexibility** | Switch algorithms at runtime |
| **No Conditionals** | No if-else chains |
| **Open/Closed** | Open for extension, closed for modification |
| **Testability** | Test each strategy independently |
| **Reusability** | Strategies can be used elsewhere |
| **Maintainability** | Each strategy is isolated |
| **Scalability** | Easy to add new strategies |

### Real-World Example

```csharp
// Different clinicians prefer different analysis methods

// Clinician A: "I like to focus on peak pressure"
_analyzer.SetStrategy(new PeakPressureStrategy());

// Clinician B: "I prefer contact area analysis"
_analyzer.SetStrategy(new ContactAreaStrategy());

// Both get the same interface:
var result = _analyzer.Analyze(heatmap);

// Result structure is identical, algorithm varies
// No if-else logic, clean and elegant!
```

### Extension Example

To add a new analysis strategy:

```csharp
public class WeightedRiskStrategy : IRiskAnalysisStrategy
{
    public double CalculateRiskScore(byte[,] heatmap)
    {
        // Custom weighted calculation
        double peakScore = CalculatePeakComponent(heatmap) * 0.4;
        double areaScore = CalculateAreaComponent(heatmap) * 0.35;
        double distributionScore = CalculateDistributionComponent(heatmap) * 0.25;
        
        return peakScore + areaScore + distributionScore;
    }

    public string GetStrategyName() => "Weighted Risk Analysis";

    public bool RequiresImmediateIntervention(byte[,] heatmap)
    {
        return CalculateRiskScore(heatmap) >= 7.5;
    }
}

// Register and use
_analyzer.SetStrategy(new WeightedRiskStrategy());
var result = _analyzer.Analyze(heatmap);
```

---

## ?? HOW PATTERNS WORK TOGETHER

### Data Flow Example

```
1. REQUEST ARRIVES
   ??? PatientController.Index()

2. FACTORY PATTERN
   ??? Create initial messages
   ??? _messageFactory.CreateMessage("Clinician", "...")
   ??? _alertFactory.CreatePressureAlert(240)

3. ADAPTER PATTERN (if legacy sensor)
   ??? Get legacy sensor data
   ??? LegacyPressureSensorAdapter converts
   ??? 24×24, 0-1000 ? 32×32, 0-255

4. STRATEGY PATTERN
   ??? Set analysis strategy
   ??? _analyzer.SetStrategy(new PeakPressureStrategy())
   ??? Calculate risk score
   ??? Determine if intervention needed

5. RESPONSE RENDERED
   ??? View displays:
       - Heatmap (from adapter)
       - Risk score (from strategy)
       - Messages (from factory)
       - Trend indicators
       - Historical data

6. USER SEES
   ??? Complete dashboard
       with professional risk analysis
       integrated with multiple systems
```

### Service Registration (Program.cs)

```csharp
// Factory Pattern Services
builder.Services.AddSingleton<IMessageFactory, MessageFactory>();
builder.Services.AddSingleton<AlertMessageFactory>();

// Strategy Pattern Services
builder.Services.AddSingleton<StrategyBasedRiskAnalyzer>();

// Adapter Pattern (instantiated on demand)
// No registration needed - created as required
```

---

## ?? COMPARISON: WITH vs WITHOUT PATTERNS

### Factory Pattern

```csharp
// WITHOUT Factory Pattern
var msg1 = new Message { Sender = "P1", Text = "Hi", Timestamp = DateTime.UtcNow };
var msg2 = new Message { Sender = "P2", Text = "Hi", Timestamp = DateTime.UtcNow };
var msg3 = new Message { Sender = "S", Text = "Hi", Timestamp = DateTime.UtcNow };
// If validation needed, add to 3 places!

// WITH Factory Pattern
var msg1 = _factory.CreateMessage("P1", "Hi");
var msg2 = _factory.CreateMessage("P2", "Hi");
var msg3 = _factory.CreateSystemMessage("Hi");
// Validation and logic in one place
```

### Adapter Pattern

```csharp
// WITHOUT Adapter Pattern
if (sensorType == "legacy")
{
    var legacyData = legacySensor.GetLegacyReadings();
    // Manually convert 24×24 to 32×32
    // Manually scale 0-1000 to 0-255
    // Manually apply calibration
    // ... 50 lines of conversion code
}
else if (sensorType == "thirdparty")
{
    // Different conversion code
    // ... another 50 lines
}
// Business logic mixed with conversion

// WITH Adapter Pattern
IStandardPressureSensor sensor = 
    new LegacyPressureSensorAdapter(legacySensor);
byte[,] data = sensor.GetNormalizedReadings();
// Business logic clean, conversion encapsulated
```

### Strategy Pattern

```csharp
// WITHOUT Strategy Pattern
double riskScore;
switch (analysisType)
{
    case "peak":
        // Calculate peak risk - 20 lines
        break;
    case "contact":
        // Calculate contact risk - 20 lines
        break;
    case "hotspot":
        // Calculate hotspot risk - 30 lines
        break;
    case "distribution":
        // Calculate distribution risk - 25 lines
        break;
}
// 100+ lines in one method
// Hard to test
// Hard to add new types

// WITH Strategy Pattern
_analyzer.SetStrategy(new PeakPressureStrategy());
var result = _analyzer.Analyze(heatmap);

// Each strategy is 15-20 lines
// Easy to test in isolation
// Easy to add new strategies
// No switch statements
```

---

## ?? UNIT TESTING WITH PATTERNS

### Testing Factory

```csharp
[TestClass]
public class MessageFactoryTests
{
    [TestMethod]
    public void CreateMessage_WithValidInput_ReturnsMessage()
    {
        var factory = new MessageFactory();
        var message = factory.CreateMessage("Patient", "Test");
        
        Assert.AreEqual("Patient", message.Sender);
        Assert.AreEqual("Test", message.Text);
        Assert.IsNotNull(message.Timestamp);
    }

    [TestMethod]
    public void CreateMessage_WithInvalidSender_ThrowsException()
    {
        var factory = new MessageFactory();
        
        Assert.ThrowsException<ArgumentException>(
            () => factory.CreateMessage("", "Text"));
    }
}
```

### Testing Adapter

```csharp
[TestClass]
public class AdapterTests
{
    [TestMethod]
    public void Adapter_ResizesGridCorrectly()
    {
        var legacySensor = new MockLegacyPressureSensor();
        var adapter = new LegacyPressureSensorAdapter(legacySensor);
        
        var result = adapter.GetNormalizedReadings();
        
        Assert.AreEqual(32, result.GetLength(0));
        Assert.AreEqual(32, result.GetLength(1));
    }

    [TestMethod]
    public void Adapter_NormalizesValuesCorrectly()
    {
        // Legacy: 500 (middle of 0-1000)
        // Should convert to ~127 (middle of 0-255)
        
        Assert.IsTrue(normalized[0, 0] > 100 && normalized[0, 0] < 150);
    }
}
```

### Testing Strategy

```csharp
[TestClass]
public class StrategyTests
{
    [TestMethod]
    public void PeakStrategy_HighPressure_ReturnsHighRisk()
    {
        var strategy = new PeakPressureStrategy();
        var heatmap = new byte[32, 32];
        heatmap[0, 0] = 255;  // Max pressure
        
        double risk = strategy.CalculateRiskScore(heatmap);
        
        Assert.AreEqual(10.0, risk, 0.1);  // Should be ~10
    }

    [TestMethod]
    public void ContactStrategy_LowContact_ReturnsLowRisk()
    {
        var strategy = new ContactAreaStrategy();
        var heatmap = new byte[32, 32];
        // All values < 50 (contact threshold)
        
        double risk = strategy.CalculateRiskScore(heatmap);
        
        Assert.IsTrue(risk < 3.0);
    }
}
```

---

## ?? SUMMARY TABLE

| Pattern | Type | Purpose | Location | Used For |
|---------|------|---------|----------|----------|
| **Factory** | Creational | Object creation | `Services/Factories/` | Message creation |
| **Adapter** | Structural | Interface conversion | `Services/Adapters/` | Sensor integration |
| **Strategy** | Behavioral | Algorithm selection | `Services/Strategies/` | Risk analysis |

---

## ?? KEY TAKEAWAYS

1. **Patterns Solve Problems** - Use patterns when you face specific issues
2. **Don't Over-Engineer** - Use patterns only when beneficial
3. **Combine Patterns** - Patterns work well together
4. **Easy to Extend** - Each pattern makes adding features easier
5. **Improve Testing** - Patterns make unit testing simpler
6. **Professional Code** - Shows code quality and architecture knowledge

---

## ?? FURTHER READING

- **Gang of Four Book**: "Design Patterns: Elements of Reusable Object-Oriented Software"
- **Microsoft Docs**: Design Patterns in .NET
- **Refactoring Guru**: Visual guide to design patterns

---

*Last Updated: 2025*  
*Framework: .NET 9.0*  
*Language: C# 13.0*  
*Status: Complete & Production Ready*
