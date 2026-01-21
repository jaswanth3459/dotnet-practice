using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminPortal.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeService _employeeService;

        public EmployeesController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var allEmployees = await _employeeService.GetAllEmployeesAsync();
            return Ok(allEmployees);
        }

        [HttpGet]
        [Route("{name}")]
        public async Task<IActionResult> GetEmployeeByName(string name)
        {
            var employees = await _employeeService.GetEmployeeByNameAsync(name);
            return Ok(employees);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(AddEmployeeDto addEmployeeDto)
        {
            var employee = await _employeeService.AddEmployeeAsync(addEmployeeDto);
            return Ok(employee);
        }

        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            var employee = await _employeeService.UpdateEmployeeAsync(id, updateEmployeeDto);
            return Ok(employee);
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var employee = await _employeeService.DeleteEmployeeAsync(id);
            return Ok(new { message = "Employee deleted successfully", employee });
        }
    }
}
