using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using WebApi.Models;
using WebApi.Helpers;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers()
    .AddXmlSerializerFormatters();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
// ��������� �������� �� � ��������� �������� ����������
builder.Services.AddDbContext<TodoContext>(options => options.UseInMemoryDatabase("TodoList"));
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "SwaggerAPI",
        Description = "Swagger-������ ��� ���������� �� � API",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "�������",
            Url = new Uri("https://github.com/dimayych02")
        },
        License = new OpenApiLicense
        {
            Name = "��������",
            Url = new Uri("https://example.com/license")
        }
    });


    // ������� ���� � xml-����� ��� ���������� � SwaggerAPI
    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
    // �������� ���� ����� ������ ����������� http ������� �� ���...
    options.DocumentFilter<OperationOrderFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger(options => options.SerializeAsV2 = true);
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
