﻿using BangazonWorkforceManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforceManagement.ViewModels
{
    public class EmployeeDetailViewModel
    {
        public Employee employee { get; set; }
        public List<Computer>ComputerList { get; set; }
        public EmployeeDetailViewModel()
        {
            ComputerList = new List<Computer>();
        }
    }
}
