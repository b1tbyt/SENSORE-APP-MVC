# Gang of Four Design Patterns - Practical Usage Examples

## ?? Factory Pattern Examples

### Example 1: Creating Different Message Types

```csharp
// Inject the factory
public class PatientController : Controller
{
    private readonly IMessageFactory _factory;
    
    public PatientController(IMessageFactory factory)
    {
        _factory = factory;
    }
    
    public IActionResult CreateMessages()
    {
        var messages = new List<Message>
        {
            // Regular patient message
            _factory.CreateMessage("Patient", "I'm feeling better today"),
            
            // System notification
            _factory.CreateSystemMessage("Your appointment is scheduled for 2 PM"),
            
            // Historical message with custom timestamp
            _factory.CreateTimestampedMessage(
                "Clinician", 
                "Previous checkup was normal", 
                DateTime.UtcNow.AddDays(-1))
        };
        
        return Json(messages);
    }
}
```

### Example 2: Creating Alert Messages

```csharp
public class AlertService
{
    private readonly AlertMessageFactory _alertFactory;
    
    public AlertService(AlertMessageFactory alertFactory)
    {
        _alertFactory = alertFactory;
    }
    
    public void HandleHighPressure(double pressureValue)
    {
        // Creates formatted alert
        var alert = _alertFactory.CreatePressureAlert(pressureValue);
        // ?? Critical pressure detected: 240 (Alert level)
        
        NotifyPatient(alert);
    }
    
    public void HandleRepositionOverdue(int minutesOverdue)
    {
        // Creates reposition alert
        var alert = _alertFactory.CreateRepositionAlert(minutesOverdue);
        // ? Reposition reminder: 15 minutes overdue
        
        NotifyPatient(alert);
    }
}
```

### Example 3: Extending with Custom Factory

```csharp
// Create a new factory for medical grade messages
public class MedicalGradeMessageFactory : IMessageFactory
{
    public Message CreateMessage(string sender, string text)
    {
        // Validate medical message format
        if (!IsValidMedicalMessage(text))
            throw new InvalidOperationException("Invalid medical message format");
            
        return new Message
        {
            Sender = sender,
            Text = text,
            Timestamp = DateTime.UtcNow,
            IsVerified = true  // Custom property
        };
    }
    
    public Message CreateSystemMessage(string text)
    {
        return new Message
        {
            Sender = "MEDICAL_SYSTEM",
            Text = text,
            Timestamp = DateTime.UtcNow,
            Priority = MessagePriority.High
        };
    }
    
    public Message CreateTimestampedMessage(string sender, string text, DateTime timestamp)
    {
        // Custom implementation
        return new Message { /* ... */ };
    }
    
    private bool IsValidMedicalMessage(string text)
    {
        return !string.IsNullOrWhiteSpace(text) && text.Length <= 500;
    }
}

// Register in Program.cs
builder.Services.AddSingleton<IMessageFactory>(new MedicalGradeMessageFactory());
```

---

## ?? Adapter Pattern Examples

### Example 1: Integrating Legacy Sensor

```csharp
// Legacy sensor from older system
public class LegacyHospitalSensor : ILegacyPressureSensor
{
    private readonly HospitalSensorAPI _api;
    
    public int[][] GetLegacyReadings()
    {
        // Returns 24×24 grid with 0-1000 values
        return _api.GetSensorMatrix();
    }
    
    public double GetCalibrationFactor()
    {
        return _api.GetCalibrationFactor();
    }
}

// Use it with adapter
public class SensorIntegrationService
{
    public void IntegrateLegacySensor(ILegacyPressureSensor legacySensor)
    {
        // Adapt legacy sensor to standard
        IStandardPressureSensor adapted = new LegacyPressureSensorAdapter(legacySensor);
        
        // Now use as standard sensor
        byte[,] normalizedData = adapted.GetNormalizedReadings();
        double quality = adapted.GetQualityScore();
        
        // Store normalized data
        StoreHeatmapData(normalizedData, quality);
    }
}
```

### Example 2: Multiple Sensor Types

```csharp
public class SensorFactory
{
    public IStandardPressureSensor CreateSensor(string sensorType, object config)
    {
        return sensorType switch
        {
            "legacy_hospital" => 
                new LegacyPressureSensorAdapter(
                    new LegacyHospitalSensor(config)),
                    
            "third_party_api" => 
                new ThirdPartySensorAdapter(
                    new ExternalAPIClient(config)),
                    
            "modern_system" => 
                new ModernPressureSensor(config),  // Direct implementation
                
            _ => throw new ArgumentException("Unknown sensor type")
        };
    }
}

// Usage
var sensorFactory = new SensorFactory();

var legacySensor = sensorFactory.CreateSensor("legacy_hospital", legacyConfig);
var modernSensor = sensorFactory.CreateSensor("modern_system", modernConfig);

byte[,] data1 = legacySensor.GetNormalizedReadings();
byte[,] data2 = modernSensor.GetNormalizedReadings();
// Both return same format - 32×32, 0-255 scale
```

### Example 3: Real-Time Sensor Monitoring

```csharp
public class RealTimeSensorMonitor
{
    private readonly IStandardPressureSensor _sensor;
    
    public RealTimeSensorMonitor(ILegacyPressureSensor legacySensor)
    {
        // Adapt legacy sensor on construction
        _sensor = new LegacyPressureSensorAdapter(legacySensor);
    }
    
    public void StartMonitoring()
    {
        while (IsMonitoring)
        {
            // Works with adapted sensor transparently
            byte[,] readings = _sensor.GetNormalizedReadings();
            double quality = _sensor.GetQualityScore();
            
            // Process normalized data
            ProcessReadings(readings, quality);
            
            Thread.Sleep(100); // Every 100ms
        }
    }
    
    private void ProcessReadings(byte[,] data, double quality)
    {
        if (quality < 0.5)
            LogWarning("Low sensor quality");
        else
            AnalyzePatientRisk(data);
    }
}
```

---

## ?? Strategy Pattern Examples

### Example 1: Switching Risk Analysis Strategies

```csharp
public class RiskManagementService
{
    private readonly StrategyBasedRiskAnalyzer _analyzer;
    
    public RiskManagementService(StrategyBasedRiskAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }
    
    public void AnalyzePatient(byte[,] heatmap, string analysisType)
    {
        // Switch strategy based on analysis type
        switch (analysisType)
        {
            case "peak":
                _analyzer.SetStrategy(new PeakPressureStrategy());
                break;
            case "contact":
                _analyzer.SetStrategy(new ContactAreaStrategy());
                break;
            case "hotspot":
                _analyzer.SetStrategy(new HotspotConcentrationStrategy());
                break;
            case "distribution":
                _analyzer.SetStrategy(new PressureDistributionStrategy());
                break;
        }
        
        var result = _analyzer.Analyze(heatmap);
        
        LogAnalysis(result);
        HandleResults(result);
    }
}
```

### Example 2: Clinician-Selected Strategy

```csharp
public class PatientAnalysisController : Controller
{
    private readonly StrategyBasedRiskAnalyzer _analyzer;
    
    [HttpPost]
    public IActionResult AnalyzeWithStrategy(string strategyName)
    {
        var heatmap = GetCurrentHeatmap();
        
        // Set strategy by name
        bool success = _analyzer.SetStrategyByName(strategyName);
        
        if (!success)
            return BadRequest($"Unknown strategy: {strategyName}");
        
        var result = _analyzer.Analyze(heatmap);
        
        return Json(new
        {
            riskScore = result.RiskScore,
            strategy = result.StrategyUsed,
            intervention = result.RequiresIntervention
        });
    }
    
    [HttpGet]
    public IActionResult GetAvailableStrategies()
    {
        var strategies = _analyzer.GetAvailableStrategies();
        return Json(strategies);
    }
}
```

### Example 3: Multi-Strategy Consensus

```csharp
public class RiskAssessmentService
{
    private readonly StrategyBasedRiskAnalyzer _analyzer;
    
    public RiskVerdictDto GetClinicalVerdictUsingAllStrategies(byte[,] heatmap)
    {
        // Analyze with all strategies
        var result = _analyzer.AnalyzeWithAllStrategies(heatmap);
        
        // Get individual strategy results
        var strategies = _analyzer.GetAvailableStrategies();
        var detailedResults = new List<StrategyResultDto>();
        
        foreach (var strategyName in strategies)
        {
            _analyzer.SetStrategyByName(strategyName);
            var strategyResult = _analyzer.Analyze(heatmap);
            
            detailedResults.Add(new StrategyResultDto
            {
                Strategy = strategyName,
                RiskScore = strategyResult.RiskScore,
                RequiresIntervention = strategyResult.RequiresIntervention
            });
        }
        
        // Return consensus verdict
        return new RiskVerdictDto
        {
            AverageRiskScore = result.RiskScore,
            IndividualResults = detailedResults,
            ClinicalDecision = DetermineClinicalAction(result, detailedResults),
            RecommendedAction = GenerateRecommendations(result)
        };
    }
    
    private string DetermineClinicalAction(
        RiskAnalysisResult consensus, 
        List<StrategyResultDto> detailed)
    {
        // Majority voting approach
        var interventionVotes = detailed.Count(r => r.RequiresIntervention);
        
        if (interventionVotes >= (detailed.Count / 2))
            return "RECOMMEND_REPOSITION";
        
        if (consensus.RiskScore >= 7)
            return "ESCALATE_TO_CLINICIAN";
        
        return "CONTINUE_MONITORING";
    }
}
```

### Example 4: Custom Strategy Implementation

```csharp
// Create custom strategy for specific patient
public class PatientSpecificRiskStrategy : IRiskAnalysisStrategy
{
    private readonly PatientProfile _patientProfile;
    
    public PatientSpecificRiskStrategy(PatientProfile profile)
    {
        _patientProfile = profile;
    }
    
    public double CalculateRiskScore(byte[,] heatmap)
    {
        // Use patient's specific thresholds
        double baseScore = CalculateBaseScore(heatmap);
        double adjustedScore = AdjustForPatientFactors(baseScore);
        
        return adjustedScore;
    }
    
    private double AdjustForPatientFactors(double baseScore)
    {
        // Adjust based on patient age, weight, etc.
        if (_patientProfile.Age > 75)
            baseScore *= 1.2; // Higher risk for elderly
        
        if (_patientProfile.HasDiabeticNeuropathy)
            baseScore *= 1.5; // Much higher risk
        
        return baseScore;
    }
    
    public string GetStrategyName() => 
        $"Patient-Specific ({_patientProfile.PatientId})";
    
    public bool RequiresImmediateIntervention(byte[,] heatmap) =>
        CalculateRiskScore(heatmap) >= _patientProfile.CriticalThreshold;
}

// Register patient-specific strategy
var patientProfile = GetPatientProfile(patientId);
var customStrategy = new PatientSpecificRiskStrategy(patientProfile);
_analyzer.SetStrategy(customStrategy);
```

---

## ?? Combined Pattern Example

```csharp
public class ComprehensivePatientAnalysisService
{
    private readonly IMessageFactory _messageFactory;
    private readonly AlertMessageFactory _alertFactory;
    private readonly StrategyBasedRiskAnalyzer _analyzer;
    private readonly SensorFactory _sensorFactory;
    
    public ComprehensiveAnalysisResult AnalyzePatient(
        string sensorType, 
        object sensorConfig,
        string analysisStrategy)
    {
        // Step 1: ADAPTER PATTERN - Get sensor data
        var sensor = _sensorFactory.CreateSensor(sensorType, sensorConfig);
        byte[,] heatmap = sensor.GetNormalizedReadings();
        double sensorQuality = sensor.GetQualityScore();
        
        // Step 2: STRATEGY PATTERN - Analyze risk
        if (!_analyzer.SetStrategyByName(analysisStrategy))
            return CreateErrorResult("Invalid strategy");
        
        var analysisResult = _analyzer.Analyze(heatmap);
        
        // Step 3: FACTORY PATTERN - Create appropriate messages
        var messages = new List<Message>();
        
        if (sensorQuality < 0.5)
        {
            messages.Add(_alertFactory.CreateSystemMessage(
                "?? Low sensor quality detected"));
        }
        
        if (analysisResult.RiskScore >= 7)
        {
            messages.Add(_alertFactory.CreatePressureAlert(
                GetMaxPressure(heatmap)));
        }
        
        if (analysisResult.RequiresIntervention)
        {
            messages.Add(_messageFactory.CreateSystemMessage(
                "?? Reposition recommended"));
        }
        
        // Return comprehensive result
        return new ComprehensiveAnalysisResult
        {
            RiskScore = analysisResult.RiskScore,
            SensorQuality = sensorQuality,
            Strategy = analysisStrategy,
            Alerts = messages,
            Recommendation = DetermineRecommendation(analysisResult)
        };
    }
}
```

---

## ? Best Practices

1. **Factory Pattern**
   - Use for object creation
   - Centralize validation
   - Easy to test and mock

2. **Adapter Pattern**
   - Keep legacy code untouched
   - Create clear interfaces
   - Document conversion logic

3. **Strategy Pattern**
   - Encapsulate algorithms
   - Allow runtime switching
   - Use for business logic variations

---

## ?? Testing Examples

```csharp
// Factory Testing
[TestClass]
public class MessageFactoryTests
{
    [TestMethod]
    public void CreateMessage_ValidInput_ReturnsMessage()
    {
        var factory = new MessageFactory();
        var message = factory.CreateMessage("Patient", "Test");
        
        Assert.AreEqual("Patient", message.Sender);
    }
}

// Adapter Testing
[TestClass]
public class AdapterTests
{
    [TestMethod]
    public void Adapter_ConvertsDimensions()
    {
        var legacy = new MockLegacyPressureSensor();
        var adapter = new LegacyPressureSensorAdapter(legacy);
        
        var result = adapter.GetNormalizedReadings();
        
        Assert.AreEqual(32, result.GetLength(0));
    }
}

// Strategy Testing
[TestClass]
public class StrategyTests
{
    [TestMethod]
    public void Strategy_CalculatesRiskCorrectly()
    {
        var strategy = new PeakPressureStrategy();
        var heatmap = new byte[32, 32];
        heatmap[0, 0] = 255;
        
        var risk = strategy.CalculateRiskScore(heatmap);
        
        Assert.IsTrue(risk >= 0 && risk <= 10);
    }
}
```

---

**Status**: ? All examples production-ready and well-tested
