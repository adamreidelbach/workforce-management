﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforceManagement.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }
        
        [Required]
        public string Name { get; set; }

        [Required]
        public int DepartmentId { get; set; }
        public Department Departments { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        public bool Supervisor { get; set; }

        public ICollection<TrainingPgmEmpl> TrainingPgmEmpls;

        public Employee()
        {
            Supervisor = false;
            StartDate = DateTime.Now;
        }
    }
}
