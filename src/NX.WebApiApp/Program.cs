using NX.AspNetCore.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// 依存サービスの追加

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// HTTPリクエストパイプラインの設定

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// 追加
app.UseRemoveControlCharacters();

app.MapControllers();

app.Run();
