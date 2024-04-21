using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using WebApi.Models;


namespace WebApi.Controllers;

[Route("employees/[controller]")]
[ApiController]
public class ApiController : ControllerBase
{
    private readonly TodoContext _context;

    public ApiController(TodoContext context) => _context = context;

    /// <summary>
    /// Получение всех сотрудников
    /// </summary>
    /// <returns></returns>
    // GET-запрос: api/Api
    [HttpGet("GetAllEmployees")]
    [Consumes((MediaTypeNames.Application.Json))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<EmployeeModel>>> GetTodoItems() =>
        await _context.EmployeesItems.ToListAsync();


    /// <summary>
    /// Получение сотрудника по идентификатору
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // GET-запрос: api/Api/{id}
    [HttpGet("{id}")]
    [Consumes((MediaTypeNames.Application.Json))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeModel>> GetEmployeeItem(long id)
    {
        var todoItem = await _context.EmployeesItems.FindAsync(id);

        if (todoItem is null)
            return NotFound();

        return todoItem;
    }
    /// <summary>
    /// Получение зарплаты всех сотрудников одной компании 
    /// </summary>
    /// <param company = "company"></param>
    /// <returns></returns>
    // GET-запрос: api/Api/{company}
    [HttpGet("GetEmployeesSalary/{company}")]
    [Consumes((MediaTypeNames.Application.Json))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Int32>> GetEmployeesSalaryByCompany(string company)
    {
        // В контексте БД видимо хранятся null значения для самозанятых - поэтому и делаем обработку на null
        var companyName = company == "Самозанятый" ? null : company;

        if (await EmployeesExistByCompany(companyName))
            return _context.EmployeesItems.Where(param => param.CompanyName == companyName).Sum(acc => acc.Salary);     
             
        return BadRequest($"Нету сотрудников из компании {company}!");
    }
    /// <summary>
    /// Добавляет нового сотрудника
    /// </summary>
    /// <param name="todoItem"></param>
    /// <returns></returns>
    // POST-запрос: api/Api
    [HttpPost("AddEmployee")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeModel>> PostTodoItem(EmployeeModel todoItem)
    {
        // Если сотрудник существует - то возвращаем BadRequest
        if (await EmployeeExistById(todoItem.userId))
            return BadRequest($"Запись с id-{todoItem.userId} существует в БД!");

        if (todoItem.userId < 0)
            return BadRequest($"Невозможно создать сотрудника с невалидным идентификатором");

        _context.EmployeesItems.Add(todoItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(PostTodoItem), new { id = todoItem.userId }, todoItem);
    }

    /// <summary>
    /// Изменение параметров сотрудника по идентификатору
    /// </summary>
    /// <param name="id"></param>
    /// <param name="todoItem"></param>
    /// <returns></returns>
    // PUT-запрос: api/Api/{id}
    // Защищаем от выполнение оверзапросов
    [HttpPut("ChangeEmployee/{id}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutTodoItem(long id, EmployeeModel todoItem)
    {
        if (id != todoItem.userId)
            return BadRequest();

        _context.Entry(todoItem).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await EmployeeExistById(id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Изменение имени всех сотрудников
    /// </summary>
    /// <param name="id"></param>
    /// <param name="todoItem"></param>
    /// <returns></returns>
    [HttpPut("ChangeAllEmployeesNames")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<EmployeeModel>>> ChangeAllItemsNames()
    {
        var allItems = await _context.EmployeesItems.ToListAsync();

        if (allItems.Count == 0)
            return Ok("Нету сотрудника для изменения!");
        // Символы для генерации рандомной строки
        char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
        Random random = new Random();
        int length = random.Next(5, 100);
        allItems.ForEach(item =>
        {
            // Генерация рандомной строки
            string randStr = new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

            item.userName = randStr;

        });
        await _context.SaveChangesAsync();
        return Ok(allItems);

    }
    /// <summary>
    /// Удаляет сотрудника из компании по id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // DELETE-запрос: api/Api/{id}
    [HttpDelete("DeleteEmployee/{id}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteTodoItem(long id)
    {
        var todoItem = await _context.EmployeesItems.FindAsync(id);
        if (todoItem is null)
            return NotFound($"Сущности по id-{id} не существует в БД!");

        _context.EmployeesItems.Remove(todoItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Удаляет всех сотрудников из БД
    /// </summary>
    /// <returns></returns>
    // Delete-запрос на ВСЕ записи в БД: api/API/DeleteAllItems
    [HttpDelete("DeleteAllEmployees")]
    [Consumes((MediaTypeNames.Application.Json))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> DeleteAllTodoItems()
    {
        try
        {
            string message;
            // Получаем список всех записей в БД
            var allItems = await _context.EmployeesItems.ToListAsync();
            if (allItems.Count == 0)
            {
                message = "Не было сотрудников для удаления из БД!";
            }
            else
            {
                _context.EmployeesItems.RemoveRange(allItems);
                // Сохраняем изменения в БД
                await _context.SaveChangesAsync();
                message = "Все сотрудники были удалены из БД!";
            }

            return Results.Ok(message);
        }

        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return Results.BadRequest("Возникли проблемы при удалении всех сотрудников из БД");
        }
    }


    // Асинхронно проверяет  существование сотрудника в БД по его айдишке
    private async Task<bool> EmployeeExistById(long id) =>
        await _context.EmployeesItems.AnyAsync(val => val.userId == id);


    // Асинхронно проверяет существование сотрудников в компании БД по наименованию компании
    private async Task<bool> EmployeesExistByCompany(string companyName) =>
        await _context.EmployeesItems.AnyAsync(val => val.CompanyName == companyName);
}
