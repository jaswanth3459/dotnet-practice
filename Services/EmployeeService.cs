using EmployeeAdminPortal.Constants;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Exceptions;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Text.RegularExpressions;

namespace EmployeeAdminPortal.Services
{
    public class EmployeeService
    {
        private readonly ApplicationDbContex _dbContex;
        private readonly Container _container;

        public EmployeeService(ApplicationDbContex dbContex, [FromKeyedServices("Employees")] Container container)
        {
            _dbContex = dbContex;
            _container = container;
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            QueryDefinition query = new QueryDefinition("SELECT * FROM c");

            var iterator = _container.GetItemQueryIterator<Employee>(query);
            var allEmployees = new List<Employee>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                allEmployees.AddRange(response);
            }

            return allEmployees;
        }

        public async Task<List<Employee>> GetEmployeeByNameAsync(string name)
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
                throw new NotFoundException($"No employee found with name '{name}'");
            }

            return employees;
        }

        public async Task<Employee> AddEmployeeAsync(AddEmployeeDto addEmployeeDto)
        {
            var allErrors = new List<ErrorDetail>();

            ValidateName(addEmployeeDto.Name, null, allErrors);
            ValidateEmail(addEmployeeDto.Email, null, allErrors);
            ValidatePhone(addEmployeeDto.Phone, null, allErrors);
            ValidateSalary(addEmployeeDto.Salary, allErrors);
            if (allErrors.Any())
            {
                throw new ValidationException(allErrors);
            }
            var employeeEntity = new Employee()
            {
                Name = addEmployeeDto.Name!,
                Email = addEmployeeDto.Email!,
                Phone = addEmployeeDto.Phone,
                Salary = addEmployeeDto.Salary!.Value
            };

            _dbContex.Employees.Add(employeeEntity);
            await _dbContex.SaveChangesAsync();

            return employeeEntity;
        }

        public async Task<Employee> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            var employee = await _dbContex.Employees.FindAsync(id);
            if (employee is null)
            {
                throw new NotFoundException("Employee", id.ToString());
            }

            var allErrors = new List<ErrorDetail>();

            ValidateName(updateEmployeeDto.Name, id, allErrors);
            ValidateEmail(updateEmployeeDto.Email, id, allErrors);
            ValidatePhone(updateEmployeeDto.Phone, id, allErrors);
            ValidateSalary(updateEmployeeDto.Salary, allErrors);
            if (allErrors.Any())
            {
                throw new ValidationException(allErrors);
            }
            employee.Name = updateEmployeeDto.Name!;
            employee.Email = updateEmployeeDto.Email!;
            employee.Phone = updateEmployeeDto.Phone;
            employee.Salary = updateEmployeeDto.Salary!.Value;

            await _dbContex.SaveChangesAsync();

            return employee;
        }
        public async Task<Employee> DeleteEmployeeAsync(Guid employeeId)
        {
            var employee = await _dbContex.Employees.FindAsync(employeeId);
            if (employee is null)
            {
                throw new NotFoundException("Employee", employeeId.ToString());
            }

            _dbContex.Employees.Remove(employee);
            await _dbContex.SaveChangesAsync();

            return employee;
        }

        private void ValidatePhone(string? phone, Guid? excludeEmployeeId, List<ErrorDetail> errors)
        {
            if (string.IsNullOrEmpty(phone))
            {
                errors.Add(new ErrorDetail
                {
                    Element = "phone",
                    Code = "E14001",
                    Message = ErrorCodes.GetErrorMessage("E14001"),
                    Value = phone ?? "",
                    Location = "body"
                });
                return; 
            }
            if (!Regex.IsMatch(phone, @"^\d{10}$"))
            {
                errors.Add(new ErrorDetail
                {
                    Element = "phone",
                    Code = "E14002",
                    Message = ErrorCodes.GetErrorMessage("E14002"),
                    Value = phone,
                    Location = "body"
                });
                return;
            }
            var existingEmployee = _dbContex.Employees
                .FirstOrDefault(e => e.Phone == phone && (excludeEmployeeId == null || e.EmployeeId != excludeEmployeeId));

            if (existingEmployee != null)
            {
                errors.Add(new ErrorDetail
                {
                    Element = "phone",
                    Code = "E14003",
                    Message = ErrorCodes.GetErrorMessage("E14003"),
                    Value = phone,
                    Location = "body"
                });
            }
        }

        private void ValidateName(string? name, Guid? excludeEmployeeId, List<ErrorDetail> errors)
        {
            if (string.IsNullOrEmpty(name))
            {
                errors.Add(new ErrorDetail
                {
                    Element = "name",
                    Code = "E14004",
                    Message = ErrorCodes.GetErrorMessage("E14004"),
                    Value = name ?? "",
                    Location = "body"
                });
                return;
            }
            if (name.Length < 3)
            {
                errors.Add(new ErrorDetail
                {
                    Element = "name",
                    Code = "E14005",
                    Message = ErrorCodes.GetErrorMessage("E14005"),
                    Value = name,
                    Location = "body"
                });
                return;
            }
            if (name.Length > 50)
            {
                errors.Add(new ErrorDetail
                {
                    Element = "name",
                    Code = "E14006",
                    Message = ErrorCodes.GetErrorMessage("E14006"),
                    Value = name,
                    Location = "body"
                });
                return;
            }
            var existingEmployee = _dbContex.Employees
                .FirstOrDefault(e => e.Name == name && (excludeEmployeeId == null || e.EmployeeId != excludeEmployeeId));

            if (existingEmployee != null)
            {
                errors.Add(new ErrorDetail
                {
                    Element = "name",
                    Code = "E14012",
                    Message = ErrorCodes.GetErrorMessage("E14012"),
                    Value = name,
                    Location = "body"
                });
            }
        }

        private void ValidateEmail(string? email, Guid? excludeEmployeeId, List<ErrorDetail> errors)
        {
            if (string.IsNullOrEmpty(email))
            {
                errors.Add(new ErrorDetail
                {
                    Element = "email",
                    Code = "E14007",
                    Message = ErrorCodes.GetErrorMessage("E14007"),
                    Value = email ?? "",
                    Location = "body"
                });
                return;
            }
            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                errors.Add(new ErrorDetail
                {
                    Element = "email",
                    Code = "E14008",
                    Message = ErrorCodes.GetErrorMessage("E14008"),
                    Value = email,
                    Location = "body"
                });
                return;
            }
            var existingEmployee = _dbContex.Employees
                .FirstOrDefault(e => e.Email == email && (excludeEmployeeId == null || e.EmployeeId != excludeEmployeeId));

            if (existingEmployee != null)
            {
                errors.Add(new ErrorDetail
                {
                    Element = "email",
                    Code = "E14009",
                    Message = ErrorCodes.GetErrorMessage("E14009"),
                    Value = email,
                    Location = "body"
                });
            }
        }

        private void ValidateSalary(decimal? salary, List<ErrorDetail> errors)
        {
            if (salary == null)
            {
                errors.Add(new ErrorDetail
                {
                    Element = "salary",
                    Code = "E14010",
                    Message = ErrorCodes.GetErrorMessage("E14010"),
                    Value = "",
                    Location = "body"
                });
                return;
            }
            if (salary <= 0)
            {
                errors.Add(new ErrorDetail
                {
                    Element = "salary",
                    Code = "E14011",
                    Message = ErrorCodes.GetErrorMessage("E14011"),
                    Value = salary.ToString()!,
                    Location = "body"
                });
            }
        }
    }
}
