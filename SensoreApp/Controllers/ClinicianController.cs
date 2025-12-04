using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoreApp.Data;
using SensoreApp.Models;
using SensoreApp.Patterns;

public class ClinicianController : Controller
{
    private readonly AppDBContext _context;
    private readonly VitalSignMonitor _monitor;


    public ClinicianController(AppDBContext context, VitalSignMonitor monitor)
    {
        _context = context;
        _monitor = monitor;
    }

    public async Task<IActionResult> Dashboard()
    {
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => DateTime.Today.AddDays(-i))
            .OrderBy(d => d)
            .ToList();

        var apptsLast7 = new List<int>();
        foreach (var day in last7Days)
        {
            int count = await _context.Appointments
                .CountAsync(a => a.AppointmentDate.Date == day.Date);
            apptsLast7.Add(count);
        }

        var todaysList = await _context.Appointments
            .Where(a => a.AppointmentDate.Date == DateTime.Today)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        int completed = await _context.Appointments.CountAsync(a => a.Status == "Completed");
        int pending = await _context.Appointments.CountAsync(a => a.Status == "Pending");
        int cancelled = await _context.Appointments.CountAsync(a => a.Status == "Cancelled");

       
        _monitor.UpdateVitals(
            heartRate: 82,
            temperature: 36.4f,
            oxygen: 98
        );

        var vm = new ClinicianDashboardVM
        {
            TotalAppointments = await _context.Appointments.CountAsync(),
            TodayAppointments = todaysList.Count,
            TotalPatients = await _context.PatientRecords.CountAsync(),
            PendingAppointments = pending,
            AppointmentsLast7Days = apptsLast7,
            AppointmentStatusCounts = new List<int> { completed, pending, cancelled },
            TodaysAppointments = todaysList,

           
            LatestHeartRate = _monitor.HeartRate,
            LatestTemperature = _monitor.Temperature,
            LatestOxygen = _monitor.Oxygen
        };

        return View(vm);
    }

    public async Task<IActionResult> Appointments()
    {
        var list = await _context.Appointments.ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult AddAppointment()
    {
        return View("AddAppointment");
    }

    [HttpPost]
    public async Task<IActionResult> AddAppointment(Appointment appointment)
    {
        if (ModelState.IsValid)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Appointments");
        }

        return View("AddAppointment", appointment);
    }

    public async Task<IActionResult> PatientRecords()
    {
        var list = await _context.PatientRecords.ToListAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult AddRecord()
    {
        return View("AddRecord");
    }

    [HttpPost]
    public async Task<IActionResult> AddRecord(PatientRecord record)
    {
        if (ModelState.IsValid)
        {
            _context.PatientRecords.Add(record);
            await _context.SaveChangesAsync();
            return RedirectToAction("PatientRecords");
        }

        return View("AddRecord", record);
    }

    public async Task<IActionResult> RecordDetails(int id)
    {
        var data = await _context.PatientRecords.FindAsync(id);
        if (data == null)
            return NotFound();

        return View("PatientRecordDetails", data);
    }

  
    [HttpGet]
    public async Task<IActionResult> EditPatientRecord(int id)
    {
        var record = await _context.PatientRecords.FindAsync(id);
        if (record == null)
            return NotFound();

        return View("EditPatientRecord", record);
    }

    [HttpPost]
    public async Task<IActionResult> EditPatientRecord(PatientRecord record)
    {
        if (ModelState.IsValid)
        {
            _context.PatientRecords.Update(record);
            await _context.SaveChangesAsync();
            return RedirectToAction("PatientRecords");
        }

        return View("EditPatientRecord", record);
    }

    
    [HttpGet]
    public async Task<IActionResult> DeleteRecord(int id)
    {
        var record = await _context.PatientRecords.FindAsync(id);
        if (record == null)
            return NotFound();

        return View("DeleteRecord", record);
    }

    [HttpPost, ActionName("DeleteRecord")]
    public async Task<IActionResult> DeleteRecordConfirmed(int id)
    {
        var record = await _context.PatientRecords.FindAsync(id);

        if (record != null)
        {
            _context.PatientRecords.Remove(record);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("PatientRecords");
    }

   
    [HttpGet]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var appt = await _context.Appointments.FindAsync(id);
        if (appt == null)
            return NotFound();

        return View("DeleteAppointment", appt);
    }

    [HttpPost, ActionName("DeleteAppointment")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var appt = await _context.Appointments.FindAsync(id);
        if (appt != null)
        {
            _context.Appointments.Remove(appt);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Appointments");
    }

   
    [HttpGet]
    public async Task<IActionResult> EditAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            return NotFound();

        return View("EditAppointment", appointment);
    }

    [HttpPost]
    public async Task<IActionResult> EditAppointment(Appointment appointment)
    {
        if (!ModelState.IsValid)
            return View("EditAppointment", appointment);

        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Appointments");
    }
}
