using GeoFieldsApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавим FieldService с путями к KML-файлам
builder.Services.AddSingleton<FieldService>(provider =>
    new FieldService("Data/fields.kml", "Data/centroids.kml")); // Укажи корректные пути!

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();