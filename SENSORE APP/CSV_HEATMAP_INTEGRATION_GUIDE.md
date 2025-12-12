# ?? CSV HEATMAP DATA INTEGRATION GUIDE

## Adding Real Heatmap Data to Your SENSORE APP

---

## ?? WHAT'S NEW

You can now:
? Load real heatmap data from CSV files  
? Use historical pressure readings  
? Display authentic sensor data patterns  
? Manage multiple heatmap datasets  
? Dynamically switch between heatmaps  

---

## ?? FILE STRUCTURE

### Directory Setup

```
SENSORE APP/
??? Data/
?   ??? Heatmaps/
?       ??? 1cfd777_20251011.csv
?       ??? 1cfd777_20251012.csv
?       ??? 1cfd777_20251013.csv
?       ??? 71e6bab3_20251011.csv
?       ??? 71e6bab3_20251012.csv
?       ??? 71e6bab3_20251013.csv
?       ??? 543d4676_20251011.csv
?       ??? 543d4676_20251012.csv
?       ??? 543d4676_20251013.csv
?       ??? d1304d3_20251011.csv
?       ??? d1304d3_20251012.csv
?       ??? d1304d3_20251013.csv
??? ... (rest of project)
```

---

## ?? CSV FILE FORMAT

### Expected Format
- **Grid Size**: 32x32 (matching sensor array)
- **Data Type**: Byte values (0-255)
- **Separator**: Comma (,)
- **Line Structure**: Each row represents one row of the heatmap

### Example CSV Content
```csv
10,15,20,18,22,25,30,28,32,35,38,40,42,44,46,48,50,52,54,56,58,60,62,64,66,68,70,72,74,76,78,80
12,17,22,20,24,27,32,30,34,37,40,42,44,46,48,50,52,54,56,58,60,62,64,66,68,70,72,74,76,78,80,82
...
(32 rows total)
```

### Validation Rules
- Each row must have exactly 32 comma-separated values
- All values must be valid integers between 0-255
- Empty cells are treated as 0
- Invalid values are skipped

---

## ?? HOW IT WORKS

### Architecture

```
CSV Files (in Data/Heatmaps/)
        ?
HeatmapCsvLoader Service
        ?
PressureDataService
        ?
PatientController
        ?
Views (Display)
```

### Services

#### HeatmapCsvLoader
**Purpose**: Load and manage CSV heatmap files

**Key Methods**:
- `GetRandomHeatmap()` - Gets random heatmap from available files
- `GetHeatmapByName(fileName)` - Gets specific heatmap
- `GetAvailableHeatmaps()` - Lists all available files
- `RefreshAvailableFiles()` - Reloads file list

#### PressureDataService
**Updated to support CSV data**

**New Methods**:
- `LoadHeatmapFromCsv(fileName)` - Loads specific CSV
- `GetAvailableHeatmaps()` - Gets list of CSV files

---

## ?? IMPLEMENTATION DETAILS

### 1. New Service: HeatmapCsvLoader

**Location**: `Services/HeatmapCsvLoader.cs`

```csharp
public class HeatmapCsvLoader
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _dataDirectory;
    
    public byte[,] GetRandomHeatmap() { ... }
    public byte[,] GetHeatmapByName(string fileName) { ... }
    public List<string> GetAvailableHeatmaps() { ... }
}
```

### 2. Updated PressureDataService

**Key Changes**:
- Constructor accepts `HeatmapCsvLoader` (optional dependency)
- Automatically detects CSV files on startup
- Alternates between CSV data and random generation
- Falls back gracefully if no CSV files available

```csharp
public PressureDataService(HeatmapCsvLoader csvLoader = null)
{
    // Loads CSV files if available
}

public void LoadHeatmapFromCsv(string fileName = null) { ... }
```

### 3. Dependency Injection (Program.cs)

```csharp
builder.Services.AddSingleton<HeatmapCsvLoader>();
```

### 4. Controller Endpoints

**New Actions in PatientController**:

- `GET /Patient/GetAvailableHeatmaps`
  - Returns list of available CSV files
  - Response: `{ heatmaps: [...], count: N }`

- `POST /Patient/LoadHeatmapFromCsv`
  - Parameters: `fileName` (optional)
  - Loads specific or random heatmap
  - Sets confirmation message

- `GET /Patient/HeatmapManagement`
  - Returns management information
  - Shows available heatmap count

---

## ?? HOW TO USE

### Step 1: Prepare CSV Files

1. **Create folder**: `SENSORE APP/Data/Heatmaps/`
2. **Add your CSV files** with heatmap data
3. **Ensure format**: 32x32 grid, comma-separated values

### Step 2: Build & Run

1. Rebuild project
2. Service automatically discovers CSV files on startup
3. Heatmaps ready to use!

### Step 3: Load Heatmaps

#### Option A: Automatic (Recommended)
- Service automatically uses CSV data
- Live heatmap display switches between CSV files
- Fallback to random generation if needed

#### Option B: Manual (via Controller)
```csharp
// Programmatic loading
_pressureService.LoadHeatmapFromCsv("1cfd777_20251011.csv");
_pressureService.LoadHeatmapFromCsv(); // Random CSV
```

#### Option C: Via API Endpoint
```javascript
// Load specific heatmap
POST /Patient/LoadHeatmapFromCsv?fileName=1cfd777_20251011.csv

// Load random heatmap
POST /Patient/LoadHeatmapFromCsv

// Get available heatmaps
GET /Patient/GetAvailableHeatmaps
```

---

## ?? EXAMPLE USAGE

### Display Available Heatmaps (JavaScript)

```javascript
fetch('/Patient/GetAvailableHeatmaps')
    .then(response => response.json())
    .then(data => {
        console.log(`${data.count} heatmaps available:`);
        data.heatmaps.forEach(file => console.log(file));
    });
```

### Load Specific Heatmap

```javascript
const fileName = '1cfd777_20251011.csv';
fetch(`/Patient/LoadHeatmapFromCsv?fileName=${fileName}`, { 
    method: 'POST' 
})
.then(response => response.json())
.then(data => console.log('Heatmap loaded:', data));
```

### Load Random Heatmap

```javascript
fetch('/Patient/LoadHeatmapFromCsv', { method: 'POST' })
    .then(response => response.json())
    .then(data => console.log('Random heatmap loaded'));
```

---

## ?? INTEGRATION WITH DASHBOARD

### How It Displays

The heatmap data is automatically integrated with the dashboard:

1. **Service loads CSV data** on application startup
2. **PressureDataService simulates live data** using CSV values
3. **Heatmap display shows real patterns** from sensor data
4. **Live updates continue** with smooth interpolation

### No Changes Required
- Dashboard views work unchanged
- Controller actions use CSV automatically
- UI displays heatmap without modifications
- Real data appears in pressure map!

---

## ?? LIVE UPDATE BEHAVIOR

### With CSV Files Available

```
CSV Random Selection (50% probability)
         ?
Load New CSV ? Interpolate ? Display
         ?
Update every 100ms with smooth transitions
```

### Without CSV Files

```
Random Generation (30% probability)
         ?
Generate Hotspots ? Interpolate ? Display
         ?
Update every 100ms with smooth transitions
```

---

## ?? CONFIGURATION

### Change CSV Directory
```csharp
// In HeatmapCsvLoader constructor
_dataDirectory = Path.Combine(
    _environment.ContentRootPath, 
    "YourCustomPath", 
    "HeatmapFiles"
);
```

### Adjust Update Frequency
```csharp
// In PressureDataService.StartLiveUpdates()
await Task.Delay(100, cancellationToken); // Change this value
```

### Change Selection Probability
```csharp
// Increase frequency of CSV data loads (currently 50%)
if (_random.Next(0, 100) < 70) // 70% instead of 50%
{
    _targetHeatmap = _csvLoader.GetRandomHeatmap();
}
```

---

## ?? TROUBLESHOOTING

### CSV Files Not Found
1. Check folder path: `Data/Heatmaps/`
2. Verify file extensions: `.csv`
3. Ensure files are readable

### Invalid Data Values
- Non-numeric values are skipped
- Invalid rows use zeros
- Check CSV format (comma-separated)

### Performance Issues
- Reduce update frequency (increase delay)
- Limit number of CSV files
- Use smaller grid size (if possible)

### Fallback to Random
- Service automatically falls back if CSV unavailable
- Check debug console for error messages
- Verify file permissions

---

## ?? MONITORING

### Debug Output
```csharp
System.Diagnostics.Debug.WriteLine(
    $"Error loading CSV files: {ex.Message}"
);
```

### Check Available Heatmaps
```csharp
var count = _pressureService.GetAvailableHeatmaps().Count;
System.Diagnostics.Debug.WriteLine($"Heatmaps loaded: {count}");
```

---

## ?? FILE HANDLING

### Safety Features
? Validates grid size (32x32)  
? Validates value range (0-255)  
? Handles missing files gracefully  
? Falls back to random generation  
? Thread-safe operations  

### Error Handling
- Invalid CSV format: Skips invalid rows
- Missing files: Falls back to random
- Read errors: Uses default heatmap
- No exceptions thrown to user

---

## ?? CSV FILE NAMING

### Suggested Convention
```
{DeviceID}_{YYYYMMDD}.csv
1cfd777_20251011.csv
71e6bab3_20251012.csv
543d4676_20251013.csv
```

### Why This Naming?
- Device ID tracks which sensor collected data
- Date helps organize chronologically
- Easy to batch process files
- Human-readable format

---

## ?? NEXT STEPS

### Immediate
1. ? Create `Data/Heatmaps/` folder
2. ? Add CSV files with heatmap data
3. ? Rebuild project
4. ? Test loading heatmaps

### Future Enhancements
- [ ] Database integration for CSV data
- [ ] Batch CSV import feature
- [ ] Heatmap comparison/analysis
- [ ] CSV download from dashboard
- [ ] Real-time data validation UI
- [ ] Historical heatmap playback

---

## ?? DATA STATISTICS

### File Size Examples
- CSV File (32x32, bytes): ~9-10 KB
- Typical Dataset: 12-15 files
- Total Storage: ~120-150 KB
- Memory per heatmap: 1 KB (1024 bytes)

---

## ? VERIFICATION CHECKLIST

- [?] HeatmapCsvLoader service created
- [?] PressureDataService updated
- [?] Program.cs dependency injection added
- [?] PatientController enhanced
- [?] Build successful
- [?] No errors or warnings
- [?] Ready for CSV files

---

## ?? TIPS & BEST PRACTICES

### Best Practices
1. **Organize files**: Use device ID + date in filename
2. **Validate data**: Check CSV format before adding
3. **Test thoroughly**: Verify heatmaps display correctly
4. **Monitor performance**: Check update frequency
5. **Keep backups**: Save original CSV files

### Performance Tips
1. Use reasonable number of CSV files (20-30 max)
2. Store files locally (not network drives)
3. Pre-validate CSV format
4. Monitor memory usage
5. Refresh file list occasionally

---

## ?? SUPPORT

**Issues with CSV loading?**

1. Check CSV format (32x32, comma-separated)
2. Verify folder path: `Data/Heatmaps/`
3. Check file permissions
4. Review debug console output
5. Test with sample CSV file

---

*CSV Heatmap Integration Complete*  
*Status: ? Ready for Use*  
*Build: ? Successful*
