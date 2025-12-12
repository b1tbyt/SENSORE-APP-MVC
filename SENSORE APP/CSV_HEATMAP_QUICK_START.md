# ? CSV HEATMAP SETUP - QUICK START

## 5-Minute Setup Guide

---

## ?? CHECKLIST

- [?] HeatmapCsvLoader service created
- [?] PressureDataService updated
- [?] Program.cs configured
- [?] PatientController enhanced
- [?] Build successful

---

## ?? WHAT TO DO NOW

### Step 1: Create Data Folder

1. Open your project folder: `SENSORE APP`
2. Create a new folder: `Data`
3. Inside `Data`, create: `Heatmaps`

**Result**: `SENSORE APP/Data/Heatmaps/`

### Step 2: Add CSV Files

From your file list, copy these CSV files to `Data/Heatmaps/`:

```
1cfd777_20251011.csv
1cfd777_20251012.csv
1cfd777_20251013.csv
71e6bab3_20251011.csv
71e6bab3_20251012.csv
71e6bab3_20251013.csv
543d4676_20251011.csv
543d4676_20251012.csv
543d4676_20251013.csv
d1304d3_20251011.csv
d1304d3_20251012.csv
d1304d3_20251013.csv
(and any others you have)
```

### Step 3: Rebuild Project

1. In Visual Studio: `Build` ? `Rebuild Solution`
2. Wait for build to complete
3. ? Should show "Build successful"

### Step 4: Test It!

1. Run the application (F5)
2. Go to Dashboard
3. ? Heatmap should display real CSV data
4. ? Data patterns should change every few seconds

---

## ?? HOW IT WORKS

**Automatic**: The service automatically:
- ? Finds your CSV files on startup
- ? Loads them into memory
- ? Displays them in the heatmap
- ? Switches between them every few seconds

**No Code Changes Needed**: Everything is already set up!

---

## ?? EXPECTED BEHAVIOR

### Before (Random Data)
```
Heatmap displayed random pressure patterns
Each refresh: completely new random data
```

### After (CSV Data)
```
Heatmap displays real sensor readings
Each refresh: next CSV file in sequence
Patterns match your actual sensor data!
```

---

## ?? VERIFY IT'S WORKING

### In Dashboard
1. Open dashboard
2. Look at pressure heatmap
3. Watch the colors change
4. ? Pattern should show real sensor data

### In Console
1. Open VS Debug console
2. Should NOT see errors like:
   - "Error loading CSV files"
   - "File not found"
3. ? Should work silently

### Available Heatmaps
```
GET http://localhost:5000/Patient/GetAvailableHeatmaps

Response:
{
  "heatmaps": [
    "1cfd777_20251011.csv",
    "1cfd777_20251012.csv",
    ...
  ],
  "count": 12
}
```

---

## ?? TROUBLESHOOTING

### Heatmap Still Shows Random Data
1. Check folder path: `SENSORE APP/Data/Heatmaps/`
2. Check CSV files are there
3. Rebuild solution (Ctrl+Shift+B)
4. Restart application

### Getting Errors
1. Check CSV format: 32 values per line, comma-separated
2. Check file permissions (should be readable)
3. Check file names have `.csv` extension

### Files Not Found
1. Verify path: `Data/Heatmaps/` in project root
2. NOT in Solution items
3. NOT in obj or bin folders
4. Should be directly in project

---

## ?? FOLDER STRUCTURE

```
C:\Users\...\SENSORE APP MVC\SENSORE APP\
??? Data/                          ? CREATE THIS
?   ??? Heatmaps/                  ? CREATE THIS
?       ??? 1cfd777_20251011.csv   ? PUT FILES HERE
?       ??? 1cfd777_20251012.csv
?       ??? ... (more CSV files)
??? Controllers/
??? Models/
??? Services/
??? Views/
??? ... (rest of project)
```

---

## ? FEATURES NOW AVAILABLE

### 1. Automatic CSV Loading
- Service finds and loads CSV files automatically
- No configuration needed

### 2. Random Selection
- System randomly picks from available CSVs
- Ensures variety in displayed data

### 3. Fallback Support
- If no CSVs found: falls back to random generation
- Never crashes or fails

### 4. Manual Loading (Optional)
```
POST /Patient/LoadHeatmapFromCsv?fileName=filename.csv
```

### 5. View Available Files
```
GET /Patient/GetAvailableHeatmaps
```

---

## ?? WHAT HAPPENS UNDER THE HOOD

1. **Startup**: Service scans `Data/Heatmaps/` for CSV files
2. **Initialization**: Loads first CSV file into memory
3. **Live Updates**: Every 100ms, randomly switches CSV files
4. **Display**: Heatmap shows real sensor data patterns
5. **Fallback**: If no CSVs found, uses random generation

---

## ?? YOU'RE DONE!

Your CSV heatmap integration is complete!

**Next time you run the app**:
1. ? CSV files automatically loaded
2. ? Real heatmap data displayed
3. ? No further setup needed

---

## ?? MORE INFO

- See `CSV_HEATMAP_INTEGRATION_GUIDE.md` for detailed documentation
- See `Services/HeatmapCsvLoader.cs` for implementation
- See `Controllers/PatientController.cs` for API endpoints

---

## ? SUMMARY

| Task | Status |
|------|--------|
| Create Data folder | ? TODO |
| Create Heatmaps subfolder | ? TODO |
| Copy CSV files | ? TODO |
| Rebuild project | ? TODO |
| Test in dashboard | ? TODO |

**Once done**: Heatmaps display real CSV data! ??

---

*Quick Start Complete*  
*Ready to add CSV files!*
