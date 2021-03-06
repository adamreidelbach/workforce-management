using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BangazonWorkforceManagement.Models;
using BangazonWorkforceManagement.ViewModels;
using BangazonWorkforceManagement.Models.ViewModels;

namespace BangazonWorkforceManagement.Controllers
{
    /* 
    Class: EmployeesController 
    Purpose: Control the Employee views and interact with the database as necessary
    Author:  Adam + Ollie + Krissy 8/21 - 8-24 
    */

    public class EmployeesController : Controller
    {
        private readonly BangazonWorkforceManagementContext _context;

        public EmployeesController(BangazonWorkforceManagementContext context)
        {
            _context = context;    
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var bangazonWorkforceManagementContext = _context.Employee.Include(e => e.Departments);
            return View(await bangazonWorkforceManagementContext.ToListAsync());
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .Include(e => e.Departments)
                .Include(e => e.EmployeeComputers)
                .Include(e => e.TrainingPgmEmps)
                .SingleOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            var employeeView = new EmployeeDetailViewModel();

            employeeView.Employee = employee;

            foreach (var item in employee.EmployeeComputers)
            {
                var empComputer = _context.Computer.SingleOrDefault(c => c.ComputerId == item.ComputerId);

                if (item.EndDate == null)
                {
                    employeeView.ComputerList.Add(empComputer);
                }
               
            }

            foreach (var item in employee.TrainingPgmEmps)
            {
                // Queries DB for training program employee plans to attend
                var fp = from tp in _context.TrainingPgmEmp
                         join t in _context.TrainingProgram
                         on tp.TrainingProgramId equals t.TrainingProgramId
                         where t.StartDate > DateTime.Now && tp.EmployeeId == id
                         select tp.TrainingProgram;

                employeeView.FuturePrograms = fp.ToList();

                // Queries DB for training program employee has attended
                var ap = from tp in _context.TrainingPgmEmp
                         join t in _context.TrainingProgram
                         on tp.TrainingProgramId equals t.TrainingProgramId
                         where t.StartDate < DateTime.Now && tp.EmployeeId == id
                         select tp.TrainingProgram;

                employeeView.AttendedPrograms = ap.ToList();
            }

            // Gets a list of all training programs. Removes training programs employee plans to attend from the list
            foreach (var item in _context.TrainingProgram)
            {
                var nap = from t in _context.TrainingProgram
                          where t.StartDate > DateTime.Now
                          select t;

                var napList = nap.ToList();

                foreach (var pgm in employeeView.FuturePrograms)
                {
                    napList.Remove(pgm);
                }

                employeeView.NotAttendingPrograms = napList;
            }

            return View(employeeView);
        }

        private object EmployeeDetailViewModel()
        {
            throw new NotImplementedException();
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Department, "DepartmentId", "Name");
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,FirstName,LastName,DepartmentId,StartDate,Supervisor")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["DepartmentId"] = new SelectList(_context.Department, "DepartmentId", "Name", employee.DepartmentId);
            return View(employee);
        }

     
        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            EmployeeEditViewModel viewModel = new EmployeeEditViewModel();

            var employee = await _context.Employee.SingleOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }
                viewModel.Employee = employee;

                //new instance of the employeeComputer model with Computer attached to access. 
                var empComputer = await _context.EmployeeComputer.Include("Computer").ToListAsync();

                var currentEmpComputer = await _context.EmployeeComputer.Include("Computer").Where(e => e.EmployeeId == id && e.EndDate == null).ToListAsync();

                var allComputers = await _context.Computer.ToListAsync();

                foreach (EmployeeComputer emp in currentEmpComputer)
                {
                    allComputers.Add(emp.Computer);
                }

                foreach (EmployeeComputer x in empComputer)
                {
                    //Checking to see if this computer is being used.
                    if (x.EndDate == null)
                    {
                        allComputers.Remove(x.Computer);

                    }
                }

                viewModel.Computers = allComputers;
                ViewData["ComputerId"] = new SelectList(viewModel.Computers, "ComputerId", "Make", "Manufacturer");

                ViewData["DepartmentId"] = new SelectList(_context.Department, "DepartmentId", "Name", employee.DepartmentId);
            return View(viewModel);
        }


        // POST: Employees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeEditViewModel model)
        {
            if (id != model.Employee.EmployeeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    EmployeeComputer newEmpComp = new EmployeeComputer()
                    {
                        //Creating a New Instance of EMpID from the EditViewModel and a new Instance of computerId from the model
                        EmployeeId = id,
                        ComputerId = model.ComputerId,
                        // Add today's date as the StartDate - Ollie
                        StartDate = DateTime.Now
                    };

                    // Get Employees current Computer - Ollie
                    var currentEmpComputer = await _context.EmployeeComputer.SingleOrDefaultAsync(e => e.EmployeeId == id && e.EndDate == null);

                    // Checks if they have a computer assigned
                    if(currentEmpComputer != null)
                    {
                        // If they do. It then checks if the selected computer is different from their currently assigned computer
                        if (currentEmpComputer.ComputerId != model.ComputerId)
                        {
                            // If it is not the same
                            // Change the EndDate from NULL to the current Date - Ollie
                            currentEmpComputer.EndDate = DateTime.Now;
                            // Update the EndDate in the DB - Ollie
                            _context.Update(currentEmpComputer);
                            // Add the new computer
                            _context.Add(newEmpComp);
                        }
                    }
                    else
                    {
                        // If they don't have a computer in the DB just add the new one
                        _context.Add(newEmpComp);
                    }
                   
                    _context.Update(model.Employee);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(model.Employee.EmployeeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }


            ViewData["DepartmentId"] = new SelectList(_context.Department, "DepartmentId", "Name", model.Employee.DepartmentId);
            return View(model);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .Include(e => e.Departments)
                .SingleOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employee.SingleOrDefaultAsync(m => m.EmployeeId == id);
            _context.Employee.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employee.Any(e => e.EmployeeId == id);
        }
    }
}
