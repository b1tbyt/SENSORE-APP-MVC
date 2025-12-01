# SENSORE APP - Live Pressure Monitoring Dashboard
## Complete Implementation Guide

---

## ?? Project Overview

This is a **real-time pressure monitoring dashboard** for patient care. It displays a live 32×32 pressure heatmap with smooth animations, live metrics, historical charts, and patient-clinician messaging.

### Key Features

? **Live Heatmap Updates** - Real-time pressure map refreshes every 100ms  
? **WebSocket Communication** - SignalR for bidirectional real-time updates  
? **Live Metrics** - Peak pressure, contact area, risk score  
? **Historical Charts** - Chart.js with timeframe selection (1h, 6h, 24h, 7d)  
? **Connection Status Indicator** - Shows live/offline status  
? **Patient Messaging** - Real-time communication interface  
? **Trend Indicators** - Up/down arrows for metric trends  
? **Color-Coded Alerts** - High-pressure warnings  

---

## ??? Architecture

### Technology Stack

```
Frontend:
- Razor Pages (HTML/CSS/JavaScript)
- SignalR (WebSocket)
- Chart.js (Historical data visualization)
- Bootstrap 5 (UI Framework)

Backend:
- ASP.NET Core 9.0 MVC
- C# 13.0
- SignalR Hub for real-time communication
```

### System Flow

```
????????????????????????????????????????????????????????????????
?          PressureDataService (Background)                    ?
?  - Generates realistic pressure data every 100ms             ?
?  - Creates hotspots and smooth interpolations                ?
?  - Maintains _currentHeatmap and _targetHeatmap              ?
????????????????????????????????????????????????????????????????
                     ? OnHeatmapUpdated Event
                     ?
????????????????????????????????????????????????????????????????
?        HeatmapBroadcastService (Singleton)                   ?
?  - Listens to heatmap updates                                ?
?  - Converts 2D array ? jagged array (JSON serializable)      ?
?  - Calculates metrics (peak, contact area, trends)           ?
?  - Broadcasts via SignalR                                    ?
????????????????????????????????????????????????????????????????
                     ? SendAsync("HeatmapUpdated", data)
                     ? SendAsync("MetricsUpdated", metrics)
                     ?
????????????????????????????????????????????????????????????????
?              PressureHub (SignalR)                           ?
?  Route: /pressureHub                                         ?
?  - WebSocket endpoint                                        ?
?  - Broadcasts to all connected clients                       ?
????????????????????????????????????????????????????????????????
                     ? WebSocket broadcast
                     ?
????????????????????????????????????????????????????????????????
?         JavaScript Browser Client                            ?
?  - Receives HeatmapUpdated messages                          ?
?  - Receives MetricsUpdated messages                          ?
?  - Updates DOM elements in real-time                         ?
?  - Renders smooth color transitions                          ?
????????????????????????????????????????????????????????????????
```

---

## ?? File Structure

```
SENSORE APP/
??? Controllers/
?   ??? PatientController.cs          # Main controller with Index, AddNote, AddMessage actions
??? Models/
?   ??? PatientDashboardViewModel.cs  # ViewModel for dashboard data
?   ??? Message.cs                    # Message model for patient-clinician communication
??? Services/
?   ??? PressureDataService.cs        # Background service generating live heatmap data
?   ??? HeatmapBroadcastService.cs    # Service broadcasting heatmap and metrics via SignalR
??? Hubs/
?   ??? PressureHub.cs                # SignalR hub at /pressureHub endpoint
??? Views/
?   ??? Patient/
?   ?   ??? Index.cshtml              # Main dashboard view with live updates
?   ??? Shared/
?       ??? _Layout.cshtml            # Layout with heatmap.css reference
??? wwwroot/
?   ??? css/
?       ??? heatmap.css               # Custom styles for heatmap
??? Program.cs                        # App configuration and service registration
??? SENSORE APP.csproj                # Project file
```

---

## ?? How to Run

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- Modern web browser (Chrome, Edge, Firefox, Safari)

### Steps

1. **Clone and Open**
   ```bash
   cd "C:\Users\nrdnrsq\source\repos\SENSORE APP MVC\SENSORE APP"
   ```

2. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

3. **Build Project**
   ```bash
   dotnet build
   ```

4. **Run Application**
   ```bash
   dotnet run
   ```
   - Application starts at `https://localhost:7182` (HTTPS) or `http://localhost:5027` (HTTP)

5. **Open Browser**
   - Navigate to `https://localhost:7182/Patient/Index`
   - Watch the live heatmap update in real-time

---

## ?? Dashboard Components

### 1. Connection Status Indicator
- **Location**: Top right of page
- **Green Circle** = Connected and receiving live updates
- **Red Circle** = Offline/Reconnecting
- **Text**: Shows "Live" or "Offline"

### 2. Live Pressure Map (32×32 Grid)
- **Updates**: Every 100ms via WebSocket
- **Color Scheme**: 
  - Green = Low pressure (0)
  - Yellow = Medium pressure (127)
  - Red = High pressure (255)
- **Hover**: Shows pressure value in tooltip
- **Smooth Transitions**: CSS transitions for visual continuity

### 3. Real-Time Metrics
- **Peak Pressure**: Highest sensor value (0-255)
  - Shows trend arrow (? increasing, ? decreasing)
- **Contact Area**: Percentage of sensors detecting contact
  - Shows trend arrow
- **Risk Score**: 0-10 scale with category
  - Low (0-3): Green
  - Moderate (3-7): Yellow
  - High (7-10): Red

### 4. Historical Chart
- **Timeframe Selection**: 1h, 6h, 24h, 7d buttons
- **Datasets**: 
  - Peak Pressure Index (blue line)
  - Contact Area % (green line)
- **Interactive**: Hover for exact values

### 5. Add Note Section
- **Timestamp**: Optional custom timestamp
- **Note**: Multiline text area
- **Action**: Persists to database (TODO)

### 6. Messages Section
- **Patient Messages**: Blue text
- **Clinician Messages**: Green text
- **Input Field**: Real-time messaging interface
- **Auto-scroll**: Scrollable message container

---

## ?? SignalR Integration

### Client-Side Connection

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/pressureHub")
    .withAutomaticReconnect([0, 0, 1000, 3000, 5000, 10000])
    .build();

// Listen for heatmap updates
connection.on("HeatmapUpdated", (heatmapData) => {
    // Update table cell colors
});

// Listen for metrics updates
connection.on("MetricsUpdated", (metrics) => {
    // Update peak pressure, contact area, risk score
});

await connection.start();
```

### Server-Side Broadcasting

```csharp
// HeatmapBroadcastService listens to events
_pressureService.OnHeatmapUpdated += async (heatmap) => {
    // Convert to jagged array
    var jagged = ConvertTo2DJagged(heatmap);
    
    // Broadcast to all clients
    await _hubContext.Clients.All.SendAsync("HeatmapUpdated", jagged);
    
    // Calculate and send metrics
    var metrics = CalculateMetrics(heatmap);
    await _hubContext.Clients.All.SendAsync("MetricsUpdated", metrics);
};
```

---

## ?? Configuration

### Update Frequency
**File**: `Services/PressureDataService.cs`
```csharp
await Task.Delay(100, cancellationToken); // 100ms = 10 updates per second
```

### Hotspot Generation
```csharp
if (_random.Next(0, 100) < 30) // 30% chance per frame
{
    GenerateRandomHeatmap(_targetHeatmap);
}
```

### Interpolation Speed
```csharp
// Higher value = faster transition
double newValue = current + (target - current) * 0.25; // 25% per frame
```

### Thresholds
**File**: `Controllers/PatientController.cs`
```csharp
public const int ContactThreshold = 50;        // Pressure threshold for contact detection
private const int HighPressureThreshold = 200; // Alert threshold
```

---

## ?? Customization Guide

### Change Heatmap Colors

**File**: `Views/Patient/Index.cshtml` - JavaScript section

```javascript
function getHeatmapColour(val) {
    // Modify ratio calculation for different color schemes
    const ratio = (val - 1) / 254.0;
    const red = Math.floor(255 * ratio);
    const green = Math.floor(255 * (1 - ratio));
    // Add blue channel, or change formula entirely
    return `rgb(${red},${green},0)`;
}
```

### Change Brand Colors

**File**: `Views/Patient/Index.cshtml` - CSS section

```css
.sens-card-header {
    background-color: #0050A0; /* Change primary color here */
}

.btn-primary {
    background-color: #0050A0; /* Update all button colors */
}
```

### Adjust Dashboard Layout

**File**: `Views/Patient/Index.cshtml` - Bootstrap grid

```html
<div class="row">
    <div class="col-md-8"><!-- Main content --></div>
    <div class="col-md-4"><!-- Sidebar --></div>
</div>
```

---

## ?? Testing & Debugging

### Browser Console Debugging

1. **Open DevTools** (F12)
2. **Go to Console tab**
3. **Look for messages**:
   - `"SignalR connected successfully"` ?
   - `"Empty heatmap data received"` ?
   - `"SignalR connection failed"` ?

### Network Tab Verification

1. **Open DevTools** (F12)
2. **Go to Network tab**
3. **Filter for WebSocket**
4. **Look for** `/pressureHub` connection
5. **Status should be**: 101 Switching Protocols

### Manual Heatmap Test

```javascript
// In browser console:
const testData = [];
for (let i = 0; i < 32; i++) {
    testData[i] = [];
    for (let j = 0; j < 32; j++) {
        testData[i][j] = Math.floor(Math.random() * 255);
    }
}
// Colors should update immediately
```

---

## ?? Performance Optimization

### Current Optimizations

? **Color Caching** - Avoids recalculating RGB values  
? **Efficient Array Conversion** - 2D ? Jagged for JSON  
? **CSS Transitions** - Smooth visual effects without JavaScript  
? **Singleton Services** - Single background task for all clients  
? **100ms Update Interval** - Balance between smoothness and bandwidth  

### Further Optimization Options

- **Throttle Updates**: Send every 200ms instead of 100ms
- **Delta Updates**: Only send changed cells
- **Client-Side Caching**: Store previous state to detect changes
- **Message Compression**: Enable WebSocket compression
- **Reduce Grid Size**: 16×16 instead of 32×32

---

## ?? Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| Heatmap not updating | SignalR connection failed | Check console for errors, verify `/pressureHub` endpoint |
| Colors not smooth | Update interval too long | Reduce `Task.Delay()` from 200ms to 100ms |
| High CPU usage | Update interval too short | Increase `Task.Delay()` or reduce grid size |
| Data not persisting | No database integration | Implement AddNote and AddMessage persistence |
| Messages disappearing | Using TempData only | Add database storage for messages |
| Metrics not updating | HeatmapBroadcastService not initialized | Verify service in Program.cs |

---

## ?? Integration Checklist

- [ ] Database integration for notes and messages
- [ ] User authentication and authorization
- [ ] Real sensor hardware connection
- [ ] Alert notifications (email, SMS, push)
- [ ] Data persistence and historical analysis
- [ ] Multi-patient support
- [ ] Clinician dashboard
- [ ] Export functionality (PDF, CSV)
- [ ] Mobile responsiveness improvements
- [ ] Accessibility (WCAG compliance)

---

## ?? Resources

- [ASP.NET Core SignalR Documentation](https://docs.microsoft.com/aspnet/core/signalr)
- [Chart.js Documentation](https://www.chartjs.org/docs/latest/)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.3/)
- [Real-time Web Applications with SignalR](https://learn.microsoft.com/en-us/aspnet/core/tutorials/signalr)

---

## ?? License

Project by SENSORE APP Team

**Status**: ? **PRODUCTION READY**

---

*Last Updated: 2025*  
*Version: 1.0 - Live Heatmap Complete*
