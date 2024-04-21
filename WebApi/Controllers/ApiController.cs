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
    /// ��������� ���� �����������
    /// </summary>
    /// <returns></returns>
    // GET-������: api/Api
    [HttpGet("GetAllEmployees")]
    [Consumes((MediaTypeNames.Application.Json))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<EmployeeModel>>> GetTodoItems() =>
        await _context.EmployeesItems.ToListAsync();


    /// <summary>
    /// ��������� ���������� �� ��������������
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // GET-������: api/Api/{id}
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
    /// ��������� �������� ���� ����������� ����� �������� 
    /// </summary>
    /// <param company = "company"></param>
    /// <returns></returns>
    // GET-������: api/Api/{company}
    [HttpGet("GetEmployeesSalary/{company}")]
    [Consumes((MediaTypeNames.Application.Json))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Int32>> GetEmployeesSalaryByCompany(string company)
    {
        // � ��������� �� ������ �������� null �������� ��� ����������� - ������� � ������ ��������� �� null
        var companyName = company == "�����������" ? null : company;

        if (await EmployeesExistByCompany(companyName))
            return _context.EmployeesItems.Where(param => param.CompanyName == companyName).Sum(acc => acc.Salary);     
             
        return BadRequest($"���� ����������� �� �������� {company}!");
    }
    /// <summary>
    /// ��������� ������ ����������
    /// </summary>
    /// <param name="todoItem"></param>
    /// <returns></returns>
    // POST-������: api/Api
    [HttpPost("AddEmployee")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeModel>> PostTodoItem(EmployeeModel todoItem)
    {
        // ���� ��������� ���������� - �� ���������� BadRequest
        if (await EmployeeExistById(todoItem.userId))
            return BadRequest($"������ � id-{todoItem.userId} ���������� � ��!");

        if (todoItem.userId < 0)
            return BadRequest($"���������� ������� ���������� � ���������� ���������������");

        _context.EmployeesItems.Add(todoItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(PostTodoItem), new { id = todoItem.userId }, todoItem);
    }

    /// <summary>
    /// ��������� ���������� ���������� �� ��������������
    /// </summary>
    /// <param name="id"></param>
    /// <param name="todoItem"></param>
    /// <returns></returns>
    // PUT-������: api/Api/{id}
    // �������� �� ���������� ������������
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
    /// ��������� ����� ���� �����������
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
            return Ok("���� ���������� ��� ���������!");
        // ������� ��� ��������� ��������� ������
        char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
        Random random = new Random();
        int length = random.Next(5, 100);
        allItems.ForEach(item =>
        {
            // ��������� ��������� ������
            string randStr = new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

            item.userName = randStr;

        });
        await _context.SaveChangesAsync();
        return Ok(allItems);

    }
    /// <summary>
    /// ������� ���������� �� �������� �� id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // DELETE-������: api/Api/{id}
    [HttpDelete("DeleteEmployee/{id}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteTodoItem(long id)
    {
        var todoItem = await _context.EmployeesItems.FindAsync(id);
        if (todoItem is null)
            return NotFound($"�������� �� id-{id} �� ���������� � ��!");

        _context.EmployeesItems.Remove(todoItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// ������� ���� ����������� �� ��
    /// </summary>
    /// <returns></returns>
    // Delete-������ �� ��� ������ � ��: api/API/DeleteAllItems
    [HttpDelete("DeleteAllEmployees")]
    [Consumes((MediaTypeNames.Application.Json))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> DeleteAllTodoItems()
    {
        try
        {
            string message;
            // �������� ������ ���� ������� � ��
            var allItems = await _context.EmployeesItems.ToListAsync();
            if (allItems.Count == 0)
            {
                message = "�� ���� ����������� ��� �������� �� ��!";
            }
            else
            {
                _context.EmployeesItems.RemoveRange(allItems);
                // ��������� ��������� � ��
                await _context.SaveChangesAsync();
                message = "��� ���������� ���� ������� �� ��!";
            }

            return Results.Ok(message);
        }

        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return Results.BadRequest("�������� �������� ��� �������� ���� ����������� �� ��");
        }
    }


    // ���������� ���������  ������������� ���������� � �� �� ��� �������
    private async Task<bool> EmployeeExistById(long id) =>
        await _context.EmployeesItems.AnyAsync(val => val.userId == id);


    // ���������� ��������� ������������� ����������� � �������� �� �� ������������ ��������
    private async Task<bool> EmployeesExistByCompany(string companyName) =>
        await _context.EmployeesItems.AnyAsync(val => val.CompanyName == companyName);
}
