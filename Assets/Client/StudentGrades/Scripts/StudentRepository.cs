using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Client.StudentGrades.Scripts
{
    public sealed class StudentLoadResult
    {
        public List<Student> Students { get; } = new List<Student>();

        public List<string> Warnings { get; } = new List<string>();

        public int TotalRecords { get; set; }

        public string Error { get; set; }
    }

    public static class StudentRepository
    {
        public static StudentLoadResult Load(string path)
        {
            try
            {
                return Parse(File.ReadAllText(path, Encoding.UTF8));
            }
            catch (Exception exception) when (
                exception is IOException ||
                exception is UnauthorizedAccessException ||
                exception is System.Security.SecurityException)
            {
                return Failure(
                    "No se pudo leer estudiantes.json. " +
                    "Comprueba que exista y que pueda abrirse.");
            }
        }

        public static StudentLoadResult Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return Failure(
                    "El archivo estudiantes.json está vacío.");
            }

            JToken root;

            try
            {
                using (var textReader = new StringReader(json))
                using (var reader = new JsonTextReader(textReader))
                {
                    reader.FloatParseHandling = FloatParseHandling.Decimal;
                    reader.DateParseHandling = DateParseHandling.None;
                    reader.MaxDepth = 32;

                    root = JToken.Load(
                        reader,
                        new JsonLoadSettings
                        {
                            DuplicatePropertyNameHandling =
                                DuplicatePropertyNameHandling.Error
                        });

                    while (reader.Read())
                    {
                        if (reader.TokenType != JsonToken.Comment)
                        {
                            throw new JsonReaderException(
                                "Hay contenido adicional después del JSON.");
                        }
                    }
                }
            }
            catch (JsonException exception)
            {
                return Failure($"JSON inválido: {exception.Message}");
            }

            JArray array = root as JArray;

            if (array == null && root is JObject document)
            {
                array = document["estudiantes"] as JArray;
            }

            if (array == null)
            {
                return Failure(
                    "Se esperaba un array o un objeto " +
                    "con el array 'estudiantes'.");
            }

            var result = new StudentLoadResult
            {
                TotalRecords = array.Count
            };

            var usedCodes = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            for (int index = 0; index < array.Count; index++)
            {
                try
                {
                    if (!(array[index] is JObject record))
                    {
                        throw new FormatException(
                            "El registro debe ser un objeto.");
                    }

                    string first = ReadText(record, "nombre");
                    string last = ReadText(record, "apellido");
                    string code = ReadText(record, "codigo");
                    string email = ReadText(record, "correo");
                    decimal grade = ReadGrade(record);

                    if (!usedCodes.Add(code))
                    {
                        throw new FormatException(
                            $"El código '{code}' está duplicado.");
                    }

                    result.Students.Add(
                        new Student(
                            first,
                            last,
                            code,
                            email,
                            grade));
                }
                catch (FormatException exception)
                {
                    result.Warnings.Add(
                        $"Registro {index + 1} omitido: " +
                        exception.Message);
                }
            }

            return result;
        }

        private static string ReadText(
            JObject record,
            string field)
        {
            JToken token = record[field];

            string value =
                token != null && token.Type == JTokenType.String
                    ? token.Value<string>()?.Trim()
                    : null;

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FormatException(
                    $"'{field}' debe contener un texto no vacío.");
            }

            return value;
        }

        private static decimal ReadGrade(JObject record)
        {
            JToken token = record["notaFinal"];

            if (token == null ||
                token.Type != JTokenType.Integer &&
                 token.Type != JTokenType.Float)
            {
                throw new FormatException(
                    "'notaFinal' debe existir y ser un número.");
            }

            bool parsed = decimal.TryParse(
                token.ToString(Formatting.None),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out decimal grade);

            if (!parsed || grade < 0m || grade > 5m)
            {
                throw new FormatException(
                    "'notaFinal' debe estar entre 0.0 y 5.0.");
            }

            return grade;
        }

        private static StudentLoadResult Failure(string message)
        {
            return new StudentLoadResult
            {
                Error = message
            };
        }
    }
}