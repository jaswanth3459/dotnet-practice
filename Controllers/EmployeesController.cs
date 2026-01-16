using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entites;
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

            if (!employees.Any())
            {
                return NotFound($"No employee found with name '{name}'.");
            }

            return Ok(employees);
        }
        [HttpPost]
        public async Task<IActionResult> AddEmployee(AddEmployeeDto addEmployeeDto)
        {
            var result = await _employeeService.AddEmployeeAsync(addEmployeeDto);

            if (!result.Success)
            {
                return BadRequest(result.ErrorResponse);
            }

            return Ok(result.Employee);
        }
        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            var result = await _employeeService.UpdateEmployeeAsync(id, updateEmployeeDto);

            if (!result.Success && result.Employee == null)
            {
                return NotFound();
            }

            if (!result.Success)
            {
                return BadRequest(result.ErrorResponse);
            }

            return Ok(result.Employee);
        }

        [HttpDelete]
        [Route("{EmployeeId :guid}")]
        public async Task<IActionResult> DeleteEmployee(Guid EmployeeId)
        {
            var result = await _employeeService.DeleteEmployeeAsync(EmployeeId);

            if (!result.Success)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
