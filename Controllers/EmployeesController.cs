using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entites;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContex dbContex;
        private readonly Container _container;

        public EmployeesController(ApplicationDbContex dbContex, [FromKeyedServices("Employees")] Container container)
        {
            this.dbContex = dbContex;
            _container = container;
        }

       [HttpGet]
public async Task<IActionResult> GetAllEmployees()
{
    QueryDefinition query = new QueryDefinition("SELECT * FROM c");

    var iterator = _container.GetItemQueryIterator<Employee>(query);
    var allEmployees = new List<Employee>();

    while (iterator.HasMoreResults)
    {
        var response = await iterator.ReadNextAsync();
        allEmployees.AddRange(response);
    }

    return Ok(allEmployees);
}

    [HttpGet]
    [Route("{name}")]
public async Task<IActionResult> GetEmployeeByName(string name)
{
    QueryDefinition query = new QueryDefinition("SELECT * FROM c WHERE LOWER(c.Name) = @name")
        .WithParameter("@name", name.ToLower());

    var iterator = _container.GetItemQueryIterator<Employee>(query);
    var employees = new List<Employee>();

    while (iterator.HasMoreResults)
    {
        var response = await iterator.ReadNextAsync();
        employees.AddRange(response);
    }

    if (!employees.Any())
    {
        return NotFound($"No employee found with name '{name}'.");
    }

    return Ok(employees);
}
        [HttpPost]
        public IActionResult AddEmployee(AddEmployeeDto addEmployeeDto)
        {
            var employeeEntity = new Employee()
            {
                Name = addEmployeeDto.Name!,
                Email = addEmployeeDto.Email!,
                Phone = addEmployeeDto.Phone,
                Salary = addEmployeeDto.Salary!.Value
            };
            dbContex.Employees.Add(employeeEntity);
            dbContex.SaveChanges();

            return Ok(employeeEntity);
        }
        [HttpPut]
        [Route("{id:guid}")]
        public IActionResult UpdateEmployee(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            var employee = dbContex.Employees.Find(id);
            if (employee is null)
            {
                    return NotFound();
            }
            employee.Name = updateEmployeeDto.Name!;
            employee.Email = updateEmployeeDto.Email!;
            employee.Phone = updateEmployeeDto.Phone;
            employee.Salary = updateEmployeeDto.Salary!.Value;
            dbContex.SaveChanges();
            return Ok(employee);
        }

        [HttpDelete]
        [Route("{EmployeeId :guid}")]
        public IActionResult DeleteEmployee(Guid EmployeeId)
        {
            var employee = dbContex.Employees.Find(EmployeeId);
            if (employee is null)
            {
                return NotFound();
            }
            dbContex.Employees.Remove(employee);
            dbContex.SaveChanges();
            return Ok();
        }
    }
}
