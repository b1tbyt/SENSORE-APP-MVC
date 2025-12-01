# Heatmap Debugging Guide

## How to verify the heatmap is moving:

### 1. **Browser Console Debugging**
- Open DevTools (F12)
- Go to **Console** tab
- Watch for these messages:
  - `"SignalR connected successfully"` - Connection established ?
  - `"HeatmapUpdated"` messages - Updates being received
  - Any error messages about connection failures ?

### 2. **Network Tab Debugging**
- Open **Network** tab in DevTools
- Look for WebSocket connection to `/pressureHub`
- You should see messages being sent/received every ~100ms
- Green status = connection active ?

### 3. **Visual Verification**
- The heatmap table should show **smooth color transitions**
- Colors should change from **green (low pressure) ? yellow ? red (high pressure)**
- Movement should be **continuous and smooth** (not jumpy)
- Look for **hotspots** (bright red areas) that move around

### 4. **Common Issues & Solutions**

| Issue | Cause | Solution |
|-------|-------|----------|
| No movement | SignalR not connected | Check browser console for errors |
| Movement stops | Connection lost | Page will auto-reconnect (see logs) |
| Colors not changing | Event not firing | Check that `HeatmapBroadcastService` is initialized |
| JavaScript errors | Syntax issues | Check console tab for exact error |

### 5. **What's Happening Behind the Scenes**

```
PressureDataService (runs every 100ms)
  ?
  Generates/interpolates heatmap data
  ?
  Fires OnHeatmapUpdated event
  ?
HeatmapBroadcastService
  ?
  Converts 2D array to jagged array
  ?
  Broadcasts via SignalR to all clients
  ?
JavaScript (connection.on("HeatmapUpdated"))
  ?
  Updates table cell colors
  ?
Browser renders smooth color transitions
```

### 6. **Performance Tips**

- Heatmap updates: **100ms interval** (10 FPS equivalent)
- Color cache: Reduces RGB calculation overhead
- CSS transitions: Provides smooth visual interpolation
- Jagged array: Optimizes JSON serialization

### 7. **To Manually Test**

Add this to your browser console:
```javascript
// Simulate a manual heatmap update
const testData = [];
for (let i = 0; i < 32; i++) {
    testData[i] = [];
    for (let j = 0; j < 32; j++) {
        testData[i][j] = Math.floor(Math.random() * 255);
    }
}
connection.invoke("RequestHeatmapUpdate", testData).catch(err => console.error(err));
```

---

**Expected behavior**: You should see the pressure map colors update smoothly every ~100ms, with hotspots (red areas) appearing, moving, and fading as the simulated pressure sensor data changes.
