using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.ConsoleUi;
using Pos.Domain;
using Pos.Infrastructure;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var isDemo = args.Contains("--demo");
var dataDirectory = isDemo
    ? Path.Combine(Path.GetTempPath(), "pos-demo-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"))
    : "pos-data";

var services = new ServiceCollection();
services.AddSingleton(new StorageSettings(dataDirectory));
services.AddSingleton(new ExportSettings(Path.Combine(dataDirectory, "itog.csv")));

services.AddSingleton<EvaluationFactory>();
services.AddSingleton<FinalGradeCalculator>();
services.AddSingleton<IFinalGradePolicy, WeightedTotalPolicy>();
services.AddSingleton<IFinalGradePolicy, CurrentProgressPolicy>();

services.AddSingleton<IStudentRepository, JsonStudentRepository>();
services.AddSingleton<IRubricRepository, JsonRubricRepository>();
services.AddSingleton<IEvaluationRepository, JsonEvaluationRepository>();
services.AddSingleton<IReportExporter, CsvReportExporter>();

services.AddSingleton<PosFacade>();
services.AddSingleton<MainMenu>();
services.AddSingleton<Demo>();

using var provider = services.BuildServiceProvider();

Console.WriteLine("Данные: " + Path.GetFullPath(dataDirectory));
try
{
    if (isDemo)
        provider.GetRequiredService<Demo>().Run();
    else
        provider.GetRequiredService<MainMenu>().Run();
}
catch (Exception ex) when (ex is JsonException or DomainException or IOException)
{
    Console.WriteLine("Не удалось загрузить данные: " + ex.Message);
    Console.WriteLine("Проверьте файлы в папке " + Path.GetFullPath(dataDirectory));
}
