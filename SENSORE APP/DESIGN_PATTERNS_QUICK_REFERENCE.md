# GoF Design Patterns - QUICK REFERENCE CARD

## ?? ONE-PAGE CHEAT SHEET

---

## ?? FACTORY PATTERN

### Quick Definition
Creates objects without exposing creation logic to the client.

### Problem
```
? New Message(...) scattered everywhere
? Same validation code duplicated
? Hard to add new message types
```

### Solution
```csharp
? _factory.CreateMessage("Patient", "text")
? Validation in one place
? Easy to add new factories
```

### Key Classes
- `IMessageFactory` - Interface
- `MessageFactory` - Standard messages
- `AlertMessageFactory` - Alert messages

### Usage
```csharp
var msg = _messageFactory.CreateMessage("Patient", "Hi");
var alert = _alertFactory.CreatePressureAlert(240);
```

### When to Use
- Multiple object types
- Complex creation logic
- Want to centralize instantiation
- Need validation

---

## ?? ADAPTER PATTERN

### Quick Definition
Converts incompatible interfaces so they can work together.

### Problem
```
? Legacy sensor: 24×24, 0-1000 range
? New system: 32×32, 0-255 range
? Incompatible formats
```

### Solution
```
Legacy Data ? Adapter ? Standard Format
24×24, 0-1000 ? Resize & Scale ? 32×32, 0-255
```

### Key Classes
- `ILegacyPressureSensor` - External interface
- `IStandardPressureSensor` - Our interface
- `LegacyPressureSensorAdapter` - Conversion logic

### Usage
```csharp
IStandardPressureSensor sensor = 
    new LegacyPressureSensorAdapter(legacySensor);
byte[,] data = sensor.GetNormalizedReadings();
```

### When to Use
- Integrate legacy systems
- Incompatible interfaces
- Format conversion needed
- Don't want to modify existing code

---

## ?? STRATEGY PATTERN

### Quick Definition
Defines algorithms as interchangeable objects; select at runtime.

### Problem
```
? Large switch/if-else statements
? Hard to add new analysis methods
? Algorithms mixed with business logic
```

### Solution
```
Strategy Interface
    ?    ?    ?
Peak  Contact  Hotspot  Distribution
 ?      ?        ?         ?
All implement IRiskAnalysisStrategy
Switch at runtime without touching client code
```

### Key Classes
- `IRiskAnalysisStrategy` - Interface
- `PeakPressureStrategy` - Implementation 1
- `ContactAreaStrategy` - Implementation 2
- `HotspotConcentrationStrategy` - Implementation 3
- `PressureDistributionStrategy` - Implementation 4
- `StrategyBasedRiskAnalyzer` - Context/Manager

### Usage
```csharp
_analyzer.SetStrategy(new PeakPressureStrategy());
var result = _analyzer.Analyze(heatmap);

// Switch strategies
_analyzer.SetStrategy(new ContactAreaStrategy());
result = _analyzer.Analyze(heatmap);
```

### When to Use
- Multiple algorithms for same problem
- Want to switch algorithms at runtime
- Each algorithm should be independent
- Need to test algorithms separately

---

## ?? QUICK COMPARISON

| Aspect | Factory | Adapter | Strategy |
|--------|---------|---------|----------|
| **Type** | Creational | Structural | Behavioral |
| **Purpose** | Create objects | Connect incompatible | Choose algorithm |
| **Solves** | Creation logic | Interface mismatch | Conditional logic |
| **Main Class** | Factory | Adapter | Strategy |
| **Interface** | IMessageFactory | IStandard PressureSensor | IRiskAnalysisStrategy |
| **Benefit** | Encapsulation | Legacy support | Runtime flexibility |

---

## ?? KEY BENEFITS

### Factory
? Centralized creation  
? Input validation  
? Easy extension  
? Encapsulation  

### Adapter
? Legacy integration  
? Format conversion  
? No code modification  
? Clean separation  

### Strategy
? No conditionals  
? Easy to add  
? Runtime switching  
? Easy testing  

---

## ?? REAL-WORLD ANALOGY

### Factory
```
?? Call a pizza restaurant (factory)
   ?
They ask: What size? Toppings?
   ?
They make it correctly
   ?
You get perfect pizza
```

### Adapter
```
?? European plug (legacy) ?? US outlet (new system)
   ?
Need an adapter
   ?
Plug adapter transforms voltage/shape
   ?
Everything works!
```

### Strategy
```
?? Navigation to work
   ?
Options: Drive, Walk, Bus, Bike
   ?
Choose strategy based on weather
   ?
Execute chosen route
```

---

## ?? HOW TO TEST

### Factory Testing
```csharp
[TestMethod]
public void CreateMessage_Valid_ReturnsMessage()
{
    var msg = _factory.CreateMessage("Patient", "Hi");
    Assert.AreEqual("Patient", msg.Sender);
}
```

### Adapter Testing
```csharp
[TestMethod]
public void Adapter_ResizesCorrectly()
{
    var result = adapter.GetNormalizedReadings();
    Assert.AreEqual(32, result.GetLength(0));
}
```

### Strategy Testing
```csharp
[TestMethod]
public void Peak_HighPressure_HighRisk()
{
    var strategy = new PeakPressureStrategy();
    var risk = strategy.CalculateRiskScore(heatmap);
    Assert.IsTrue(risk > 7);
}
```

---

## ?? FILE LOCATIONS

| Pattern | File | Interface(s) |
|---------|------|------------|
| Factory | `Services/Factories/MessageFactory.cs` | `IMessageFactory` |
| Adapter | `Services/Adapters/PressureSensorAdapter.cs` | `ILegacyPressureSensor`, `IStandardPressureSensor` |
| Strategy | `Services/Strategies/RiskAnalysisStrategy.cs` | `IRiskAnalysisStrategy` |

---

## ?? REGISTRATION (Program.cs)

```csharp
// Factory
builder.Services.AddSingleton<IMessageFactory, MessageFactory>();
builder.Services.AddSingleton<AlertMessageFactory>();

// Strategy
builder.Services.AddSingleton<StrategyBasedRiskAnalyzer>();

// Adapter - Create on demand (no registration)
```

---

## ? USAGE CHECKLIST

- [ ] Use factory for object creation
- [ ] Use adapter for legacy integration
- [ ] Use strategy for algorithm selection
- [ ] Inject dependencies in constructor
- [ ] Write unit tests for each
- [ ] Document which pattern does what

---

## ?? REMEMBER

1. **Patterns are Solutions** - Don't use if problem doesn't exist
2. **Keep It Simple** - Don't over-engineer
3. **One Job Each** - Each pattern does one thing well
4. **Combine When Needed** - Patterns work together
5. **Code Quality** - Makes code more maintainable

---

## ?? QUICK DECISION GUIDE

**Multiple object types?** ? Use **Factory**

**Incompatible interfaces?** ? Use **Adapter**

**Multiple algorithms?** ? Use **Strategy**

**Uncertain?** ? Check real-world problem first, then pick pattern

---

*This card summarizes 3 essential GoF patterns.*  
*For details, see: DESIGN_PATTERNS_COMPREHENSIVE_GUIDE.md*
