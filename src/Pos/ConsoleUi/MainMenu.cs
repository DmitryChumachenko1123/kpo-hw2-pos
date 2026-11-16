using Pos.Application;
using Pos.Domain;

namespace Pos.ConsoleUi;

public class MainMenu
{
    private readonly PosFacade _pos;
    private readonly string _exportPath;

    public MainMenu(PosFacade pos, ExportSettings exportSettings)
    {
        _pos = pos;
        _exportPath = exportSettings.Path;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== ПОС ===");
            Console.WriteLine("1. Список студентов");
            Console.WriteLine("2. Добавить студента");
            Console.WriteLine("3. Удалить студента");
            Console.WriteLine("4. Рубрикатор (критерии)");
            Console.WriteLine("5. Добавить критерий");
            Console.WriteLine("6. Удалить критерий");
            Console.WriteLine("7. Выставить оценку");
            Console.WriteLine("8. Посмотреть оценки студента");
            Console.WriteLine("9. Исправить оценку");
            Console.WriteLine("10. Отменить оценку");
            Console.WriteLine("11. Итоговая ведомость");
            Console.WriteLine("12. Правило подсчёта итога");
            Console.WriteLine("13. Экспорт итога в CSV");
            Console.WriteLine("0. Выход");

            var choice = Input.ReadLine("Выбор");
            if (choice == "0")
                return;

            try
            {
                switch (choice)
                {
                    case "1": ShowStudents(); break;
                    case "2": AddStudent(); break;
                    case "3": RemoveStudent(); break;
                    case "4": ShowCriteria(); break;
                    case "5": AddCriterion(); break;
                    case "6": RemoveCriterion(); break;
                    case "7": Grade(); break;
                    case "8": ShowEvaluations(); break;
                    case "9": CorrectEvaluation(); break;
                    case "10": CancelEvaluation(); break;
                    case "11": ShowFinalGrades(); break;
                    case "12": ChoosePolicy(); break;
                    case "13": Export(); break;
                    default: Console.WriteLine("Нет такого пункта."); break;
                }
            }
            catch (DomainException ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }
    }

    private void ShowStudents()
    {
        var students = _pos.GetStudents();
        if (students.Count == 0)
            Console.WriteLine("Студентов пока нет.");
        foreach (var s in students)
            Console.WriteLine($"- {s.Name} {s.Group}");
    }

    private void AddStudent()
    {
        var name = Input.ReadRequired("ФИО");
        var group = Input.ReadLine("Группа (можно пусто)");
        var student = _pos.AddStudent(name, group);
        Console.WriteLine($"Добавлен студент {student.Name}.");
    }

    private void RemoveStudent()
    {
        var student = ChooseStudent();
        if (student == null)
            return;
        _pos.RemoveStudent(student.Id);
        Console.WriteLine("Студент и его оценки удалены.");
    }

    private void ShowCriteria()
    {
        var criteria = _pos.GetCriteria();
        if (criteria.Count == 0)
            Console.WriteLine("Критериев пока нет.");
        var totalWeight = criteria.Sum(c => c.Weight.Value);
        foreach (var c in criteria)
            Console.WriteLine($"- {c.Title}: макс. {c.MaxScore}, вес {c.Weight} ({c.Weight.Value / totalWeight:P0} итога)");
    }

    private void AddCriterion()
    {
        var title = Input.ReadRequired("Название");
        var maxScore = Input.ReadDecimal("Максимальный балл");
        var weight = Input.ReadDecimal("Вес");
        _pos.AddCriterion(title, maxScore, weight);
        Console.WriteLine("Критерий добавлен.");
    }

    private void RemoveCriterion()
    {
        var criterion = Input.Choose("Критерий:", _pos.GetCriteria(), c => c.Title);
        if (criterion == null)
            return;
        _pos.RemoveCriterion(criterion.Id);
        Console.WriteLine("Критерий удалён.");
    }

    private void Grade()
    {
        var student = ChooseStudent();
        if (student == null)
            return;
        var criterion = Input.Choose("Критерий:", _pos.GetCriteria(), c => $"{c.Title} (0..{c.MaxScore})");
        if (criterion == null)
            return;

        var score = Input.ReadDecimal("Балл");
        var comment = Input.ReadLine("Комментарий (можно пусто)");
        _pos.Grade(student.Id, criterion.Id, score, comment);
        Console.WriteLine("Оценка выставлена.");
    }

    private void ShowEvaluations()
    {
        var student = ChooseStudent();
        if (student == null)
            return;

        var evaluations = _pos.GetEvaluations(student.Id);
        if (evaluations.Count == 0)
            Console.WriteLine("Оценок нет.");
        foreach (var e in evaluations)
            Console.WriteLine($"- {Describe(e)} ({e.Date:dd.MM.yyyy}) {e.Comment}");

        var grade = _pos.GetFinalGrade(student.Id);
        Console.WriteLine($"Итог: {grade.Value:0.00} -> {grade.Rounded}");
    }

    private void CorrectEvaluation()
    {
        var evaluation = ChooseEvaluation();
        if (evaluation == null)
            return;

        var score = Input.ReadDecimal("Новый балл");
        var comment = Input.ReadLine("Комментарий (можно пусто)");
        _pos.CorrectEvaluation(evaluation.Id, score, comment);
        Console.WriteLine("Оценка исправлена.");
    }

    private void CancelEvaluation()
    {
        var evaluation = ChooseEvaluation();
        if (evaluation == null)
            return;
        _pos.CancelEvaluation(evaluation.Id);
        Console.WriteLine("Оценка отменена.");
    }

    private void ShowFinalGrades()
    {
        Console.WriteLine($"Правило: {_pos.CurrentPolicy.Name}");
        PrintFinalGrades(_pos.GetFinalGrades());
    }

    private void ChoosePolicy()
    {
        var policy = Input.Choose("Правило подсчёта итога:", _pos.Policies, p => p.Name);
        if (policy == null)
            return;
        _pos.SelectPolicy(policy);
        Console.WriteLine($"Теперь итог считается так: {policy.Name}");
    }

    private void Export()
    {
        var count = _pos.ExportFinalGrades(_exportPath);
        Console.WriteLine($"Выгружено {count} строк в {Path.GetFullPath(_exportPath)}");
    }

    public static void PrintFinalGrades(List<FinalGradeRow> rows)
    {
        if (rows.Count == 0)
            Console.WriteLine("Студентов нет.");
        foreach (var r in rows)
            Console.WriteLine($"- {r.Student,-25} {r.Value,6:0.00} -> {r.Rounded,2}   (оценок {r.GradedCriteria}/{r.TotalCriteria})");
    }

    private Student? ChooseStudent() => Input.Choose("Студент:", _pos.GetStudents(), s => s.Name);

    private Evaluation? ChooseEvaluation()
    {
        var student = ChooseStudent();
        if (student == null)
            return null;
        return Input.Choose("Оценка:", _pos.GetEvaluations(student.Id), Describe);
    }

    private string Describe(Evaluation e)
    {
        var criterion = _pos.GetCriteria().FirstOrDefault(c => c.Id == e.CriterionId);
        return $"{criterion?.Title ?? "?"}: {e.Score}/{criterion?.MaxScore}";
    }
}
