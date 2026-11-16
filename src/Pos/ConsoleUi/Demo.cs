using Pos.Application;
using Pos.Domain;

namespace Pos.ConsoleUi;

public class Demo
{
    private readonly PosFacade _pos;
    private readonly string _exportPath;

    public Demo(PosFacade pos, ExportSettings exportSettings)
    {
        _pos = pos;
        _exportPath = exportSettings.Path;
    }

    public void Run()
    {
        Console.WriteLine("1) Добавляем студентов и критерии");
        var anna = _pos.AddStudent("Иванова Анна", "БПИ-241");
        var boris = _pos.AddStudent("Петров Борис", "БПИ-241");
        var gleb = _pos.AddStudent("Сидоров Глеб", "БПИ-242");
        var hw = _pos.AddCriterion("Домашние задания", 10, 4);
        var test = _pos.AddCriterion("Контрольная", 20, 3);
        var exam = _pos.AddCriterion("Экзамен", 10, 3);

        Console.WriteLine("2) Выставляем оценки");
        _pos.Grade(anna.Id, hw.Id, 9, null);
        _pos.Grade(anna.Id, test.Id, 17, null);
        _pos.Grade(anna.Id, exam.Id, 8, null);
        _pos.Grade(boris.Id, hw.Id, 6.5m, null);
        _pos.Grade(boris.Id, test.Id, 12, null);
        _pos.Grade(gleb.Id, hw.Id, 10, null);
        _pos.Grade(gleb.Id, test.Id, 14, null);
        var glebExam = _pos.Grade(gleb.Id, exam.Id, 5, null);
        _pos.CorrectEvaluation(glebExam.Id, 6, "после апелляции");

        Console.WriteLine("3) Пробуем нарушить правила");
        TryAndPrint("12 баллов из 10", () => _pos.Grade(boris.Id, exam.Id, 12, null));
        TryAndPrint("критерий не из рубрикатора", () => _pos.Grade(boris.Id, Guid.NewGuid(), 5, null));
        TryAndPrint("вторая оценка по тому же критерию", () => _pos.Grade(anna.Id, exam.Id, 10, null));
        TryAndPrint("удалить критерий с оценками", () => _pos.RemoveCriterion(exam.Id));

        Console.WriteLine("4) Итог: " + _pos.CurrentPolicy.Name);
        MainMenu.PrintFinalGrades(_pos.GetFinalGrades());

        var progress = _pos.Policies.First(p => p is CurrentProgressPolicy);
        _pos.SelectPolicy(progress);
        Console.WriteLine("   Итог: " + progress.Name);
        MainMenu.PrintFinalGrades(_pos.GetFinalGrades());
        _pos.SelectPolicy(_pos.Policies[0]);

        Console.WriteLine("5) Экспорт");
        _pos.ExportFinalGrades(_exportPath);
        Console.WriteLine(File.ReadAllText(_exportPath));
    }

    private static void TryAndPrint(string what, Action action)
    {
        try
        {
            action();
            Console.WriteLine($"   {what}: прошло (так быть не должно)");
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"   {what}: ошибка - {ex.Message}");
        }
    }
}
