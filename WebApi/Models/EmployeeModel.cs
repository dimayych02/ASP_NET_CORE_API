using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

// Модель DTO для API-Контроллера
public class EmployeeModel
{
    // Обязательный параметр
    [Required]
    // Первичный ключ БД
    [Key]
    // Работаем с nullable-context чтобы id
    // создавался нулевым
    public long userId { set; get; }
    public string userName { set; get; }

    private string? _companyName;

    // Может сотрудник Самозанятый - поэтому может быть null 
    public string? CompanyName
    {
        set
        {
            _companyName = value;
        }
        get
        {
            return _companyName ?? "Самозанятый";
        }
    }

    public int Salary { set; get; }
    public bool IsSecretEmployee { set; get; }
}
