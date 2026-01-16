using EmployeeAdminPortal.Constants;
using EmployeeAdminPortal.Data;
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

            return employees;
        }

        public async Task<(bool Success, ErrorResponse? ErrorResponse, Employee? Employee)> AddEmployeeAsync(AddEmployeeDto addEmployeeDto)
        {
            var allErrors = new List<ErrorDetail>();

            var nameValidation = ValidateName(addEmployeeDto.Name);
            if (nameValidation != null)
            {
                allErrors.AddRange(nameValidation.Errors);
            }

            var emailValidation = ValidateEmail(addEmployeeDto.Email);
            if (emailValidation != null)
            {
                allErrors.AddRange(emailValidation.Errors);
            }

            var phoneValidation = ValidatePhone(addEmployeeDto.Phone);
            if (phoneValidation != null)
            {
                allErrors.AddRange(phoneValidation.Errors);
            }

            var salaryValidation = ValidateSalary(addEmployeeDto.Salary);
            if (salaryValidation != null)
            {
                allErrors.AddRange(salaryValidation.Errors);
            }

            if (allErrors.Any())
            {
                return (false, new ErrorResponse
                {
                    Message = "Errors in the request.",
                    Errors = allErrors
                }, null);
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

            return (true, null, employeeEntity);
        }

        public async Task<(bool Success, ErrorResponse? ErrorResponse, Employee? Employee)> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            var employee = await _dbContex.Employees.FindAsync(id);
            if (employee is null)
            {
                return (false, null, null);
            }

            var allErrors = new List<ErrorDetail>();

            var nameValidation = ValidateName(updateEmployeeDto.Name, id);
            if (nameValidation != null)
            {
                allErrors.AddRange(nameValidation.Errors);
            }

            var emailValidation = ValidateEmail(updateEmployeeDto.Email, id);
            if (emailValidation != null)
            {
                allErrors.AddRange(emailValidation.Errors);
            }

            var phoneValidation = ValidatePhone(updateEmployeeDto.Phone, id);
            if (phoneValidation != null)
            {
                allErrors.AddRange(phoneValidation.Errors);
            }

            var salaryValidation = ValidateSalary(updateEmployeeDto.Salary);
            if (salaryValidation != null)
            {
                allErrors.AddRange(salaryValidation.Errors);
            }

            if (allErrors.Any())
            {
                return (false, new ErrorResponse
                {
                    Message = "Errors in the request.",
                    Errors = allErrors
                }, null);
            }

            employee.Name = updateEmployeeDto.Name!;
            employee.Email = updateEmployeeDto.Email!;
            employee.Phone = updateEmployeeDto.Phone;
            employee.Salary = updateEmployeeDto.Salary!.Value;

            await _dbContex.SaveChangesAsync();

            return (true, null, employee);
        }

        public async Task<(bool Success, Employee? Employee)> DeleteEmployeeAsync(Guid employeeId)
        {
            var employee = await _dbContex.Employees.FindAsync(employeeId);
            if (employee is null)
            {
                return (false, null);
            }

            _dbContex.Employees.Remove(employee);
            await _dbContex.SaveChangesAsync();

            return (true, employee);
        }

        private ErrorResponse? ValidatePhone(string? phone, Guid? excludeEmployeeId = null)
        {
            var errors = new List<ErrorDetail>();

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
            }
            else if (!Regex.IsMatch(phone, @"^\d{10}$"))
            {
                errors.Add(new ErrorDetail
                {
                    Element = "phone",
                    Code = "E14002",
                    Message = ErrorCodes.GetErrorMessage("E14002"),
                    Value = phone,
                    Location = "body"
                });
            }
            else
            {
                var existingEmployee = _dbContex.Employees
                    .FirstOrDefault(e => e.Phone == phone && (excludeEmployeeId == null || e.EmployeeId != excludeEmployeeId));

                if (existingEmployee != null)
                {
                    return new ErrorResponse
                    {
                        Message = "Phone number already exists.",
                        Errors = new List<ErrorDetail>
                        {
                            new ErrorDetail
                            {
                                Element = "phone",
                                Code = "E14003",
                                Message = ErrorCodes.GetErrorMessage("E14003"),
                                Value = phone,
                                Location = "body"
                            }
                        }
                    };
                }
            }

            if (errors.Any())
            {
                return new ErrorResponse
                {
                    Message = "Errors in the request.",
                    Errors = errors
                };
            }

            return null;
        }

        private ErrorResponse? ValidateName(string? name, Guid? excludeEmployeeId = null)
        {
            var errors = new List<ErrorDetail>();

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
            }
            else if (name.Length < 3)
            {
                errors.Add(new ErrorDetail
                {
                    Element = "name",
                    Code = "E14005",
                    Message = ErrorCodes.GetErrorMessage("E14005"),
                    Value = name,
                    Location = "body"
                });
            }
            else if (name.Length > 50)
            {
                errors.Add(new ErrorDetail
                {
                    Element = "name",
                    Code = "E14006",
                    Message = ErrorCodes.GetErrorMessage("E14006"),
                    Value = name,
                    Location = "body"
                });
            }
            else
            {
                var existingEmployee = _dbContex.Employees
                    .FirstOrDefault(e => e.Name == name && (excludeEmployeeId == null || e.EmployeeId != excludeEmployeeId));

                if (existingEmployee != null)
                {
                    return new ErrorResponse
                    {
                        Message = "Name already exists.",
                        Errors = new List<ErrorDetail>
                        {
                            new ErrorDetail
                            {
                                Element = "name",
                                Code = "E14012",
                                Message = ErrorCodes.GetErrorMessage("E14012"),
                                Value = name,
                                Location = "body"
                            }
                        }
                    };
                }
            }

            if (errors.Any())
            {
                return new ErrorResponse
                {
                    Message = "Errors in the request.",
                    Errors = errors
                };
            }

            return null;
        }

        private ErrorResponse? ValidateEmail(string? email, Guid? excludeEmployeeId = null)
        {
            var errors = new List<ErrorDetail>();

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
            }
            else if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                errors.Add(new ErrorDetail
                {
                    Element = "email",
                    Code = "E14008",
                    Message = ErrorCodes.GetErrorMessage("E14008"),
                    Value = email,
                    Location = "body"
                });
            }
            else
            {
                var existingEmployee = _dbContex.Employees
                    .FirstOrDefault(e => e.Email == email && (excludeEmployeeId == null || e.EmployeeId != excludeEmployeeId));

                if (existingEmployee != null)
                {
                    return new ErrorResponse
                    {
                        Message = "Email already exists.",
                        Errors = new List<ErrorDetail>
                        {
                            new ErrorDetail
                            {
                                Element = "email",
                                Code = "E14009",
                                Message = ErrorCodes.GetErrorMessage("E14009"),
                                Value = email,
                                Location = "body"
                            }
                        }
                    };
                }
            }

            if (errors.Any())
            {
                return new ErrorResponse
                {
                    Message = "Errors in the request.",
                    Errors = errors
                };
            }

            return null;
        }

        private ErrorResponse? ValidateSalary(decimal? salary)
        {
            var errors = new List<ErrorDetail>();

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
            }
            else if (salary <= 0)
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

            if (errors.Any())
            {
                return new ErrorResponse
                {
                    Message = "Errors in the request.",
                    Errors = errors
                };
            }

            return null;
        }
    }
}
