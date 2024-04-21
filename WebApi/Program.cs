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
// Добавляем контекст БД в коллекцию сервисов приложения
builder.Services.AddDbContext<TodoContext>(options => options.UseInMemoryDatabase("TodoList"));
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "SwaggerAPI",
        Description = "Swagger-проект для интеграции БД с API",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Дмитрий",
            Url = new Uri("https://github.com/dimayych02")
        },
        License = new OpenApiLicense
        {
            Name = "Лицензия",
            Url = new Uri("https://example.com/license")
        }
    });


    // Создаем путь к xml-файлу для интеграции в SwaggerAPI
    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
    // Казалось этот метод должен фильтровать http запросы но нет...
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
