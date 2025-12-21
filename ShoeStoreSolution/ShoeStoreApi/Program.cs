using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShoeStoreData.Contexts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Настраиваем Swagger для тестирования API
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ShoeStore API",
        Version = "v1"
    });

    // Добавляем кнопку "Authorize" в Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Подключаем базу данных
builder.Services.AddDbContext<ShoeStoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настраиваем JWT авторизацию
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Проверяем кто выдал токен
            ValidIssuer = AuthOptions.ISSUER, // Наш сервер

            ValidateAudience = true, // Проверяем кому токен
            ValidAudience = AuthOptions.AUDIENCE, // Наше приложение

            ValidateLifetime = true, // Проверяем срок действия

            ValidateIssuerSigningKey = true, // Проверяем подпись
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey() // Наш секретный ключ
        };
    });

// Включаем проверку ролей
builder.Services.AddAuthorization();

var app = builder.Build();

// Включаем Swagger для тестирования
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication(); // Проверяем токены
app.UseAuthorization(); // Проверяем роли

// Подключаем API методы
app.MapControllers();
app.Run();
