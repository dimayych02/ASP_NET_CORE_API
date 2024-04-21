namespace WebApi.Models;
    
using Microsoft.EntityFrameworkCore;

    public class TodoContext : DbContext

{
    public TodoContext(DbContextOptions<TodoContext> options) : base(options)
    {
    }

    public DbSet<EmployeeModel> EmployeesItems { set; get; } = null!;
}

