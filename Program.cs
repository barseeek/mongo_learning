using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string connectionString = "mongodb://localhost:27017/";
        MongoClient client = new MongoClient(connectionString);
        IMongoDatabase database = client.GetDatabase("test");
        IMongoCollection<Student> collection = database.GetCollection<Student>("students");

        // Вставка данных
        collection.InsertMany(new List<Student>
        {
            new Student
            {
                FirstName = "Иван",
                LastName = "Иванов",
                Grades = new Dictionary<string, int>
                {
                    { "Математика", 90 },
                    { "Литература", 80 },
                    { "История", 75 }
                }
            },
            new Student
            {
                FirstName = "Иван",
                LastName = "Олегов",
                Grades = new Dictionary<string, int>
                {
                    { "Математика", 100 },
                    { "Литература", 60 },
                    { "История", 77 }
                }
            },
            new Student
            {
                FirstName = "Петр",
                LastName = "Петров",
                Grades = new Dictionary<string, int>
                {
                    { "Математика", 85 },
                    { "Литература", 75 },
                    { "История", 80 }
                }
            },
            new Student
            {
                FirstName = "Сидор",
                LastName = "Сидоров",
                Grades = new Dictionary<string, int>
                {
                    { "Математика", 95 },
                    { "Литература", 70 },
                    { "История", 85 }
                }
            }
        });

        Console.WriteLine("Данные добавлены в базу данных.");
        PrintAllStudents(collection);

        // Вариации использования метода Find
        FindStudent(collection, "Иванов"); // Только фамилия
        FindStudent(collection, "Иван");

        // Обновление оценок
        var newGradesData = new Dictionary<string, Dictionary<string, int>>
        {
            { "Иванов", new Dictionary<string, int> { { "Математика", 92 }, { "Литература", 88 } } },
            { "Петров", new Dictionary<string, int> { { "Математика", 89 }, { "Литература", 79 } } },
            // ... Другие записи
        };

        BulkUpdateGrades(collection, newGradesData);

        // Удаление студента
        string student_id = "65a578f3bc18ff0bc2ff9268";
        // Отображение всех студентов
        PrintAllStudents(collection);
        DeleteStudent(collection, student_id);

    }

    static void FindStudent(IMongoCollection<Student> collection, string searchQuery)
    {
        var filter = Builders<Student>.Filter.Or(
            Builders<Student>.Filter.Eq("LastName", searchQuery),
            Builders<Student>.Filter.Eq("FirstName", searchQuery)
        );

        var students = collection.Find(filter).ToList();

        if (students.Count > 0)
        {
            Console.WriteLine($"Результаты поиска для запроса '{searchQuery}':");
            foreach (var student in students)
            {
                Console.WriteLine($"Имя: {student.FirstName} {student.LastName}");
                Console.WriteLine("Оценки:");
                foreach (var grade in student.Grades)
                {
                    Console.WriteLine($"{grade.Key}: {grade.Value}");
                }
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine($"Студент с запросом '{searchQuery}' не найден.");
        }

        Console.WriteLine();
    }

    static void BulkUpdateGrades(IMongoCollection<Student> collection, Dictionary<string, Dictionary<string, int>> newGradesData)
    {
        var bulkOps = new List<WriteModel<Student>>();

        foreach (var updateData in newGradesData)
        {
            var filter = Builders<Student>.Filter.Eq("LastName", updateData.Key);
            var update = Builders<Student>.Update.Set("Grades", updateData.Value);

            var updateModel = new UpdateOneModel<Student>(filter, update);
            bulkOps.Add(updateModel);
        }

        var result = collection.BulkWrite(bulkOps);

        Console.WriteLine($"Обновлено оценок для {result.ModifiedCount} студентов.");
    }

    static void DeleteStudent(IMongoCollection<Student> collection, string studentId)
    {
        var filter = Builders<Student>.Filter.Eq("_id", ObjectId.Parse(studentId));
        var result = collection.DeleteOne(filter);

        if (result.DeletedCount > 0)
        {
            Console.WriteLine($"Студент с ObjectId {studentId} удален.");
        }
        else
        {
            Console.WriteLine($"Студент с ObjectId {studentId} не найден.");
        }
    }

    static void PrintAllStudents(IMongoCollection<Student> collection)
    {
        List<Student> students = collection.Find(new BsonDocument()).ToList();
        Console.WriteLine("Список студентов в базе данных:");

        foreach (var s in students)
        {
            Console.WriteLine($"Имя: {s.FirstName} {s.LastName}");
            Console.WriteLine("Оценки:");
            foreach (var grade in s.Grades)
            {
                Console.WriteLine($"{grade.Key}: {grade.Value}");
            }
            Console.WriteLine();
        }
    }
}