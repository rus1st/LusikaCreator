using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jint;
using TestApp.Models.Enums;
using TestApp.ViewModels.Interfaces;
using TestApp.ViewModels.Variables;

namespace TestApp.Models.Helpers
{
    public class ScriptRunner
    {
        private readonly Engine _engine;
        private readonly string _fullFileName;
        public const string ScriptFileName = "functions.js";

        private static string TypeMismatchError(string funcName, string returnType, IVariableWrapper variable) =>
            $"Несоответствие типов: метод \"{funcName}\" " +
            $"возвращает значение типа \"{returnType}\", " +
            $"а переменная \"{variable.Name}\" - {Common.GetEnumDescription(variable.Type).ToLower()}";

        public string ErrorMessage { get; set; }

        public List<string> FuncNames { get; }

        public ScriptRunner(string scriptsPath)
        {
            _fullFileName = Path.Combine(scriptsPath, ScriptFileName);
            if (!File.Exists(_fullFileName))
            {
                ErrorMessage = $"Файл пользовательских скриптов не найден: \"{_fullFileName}\"";
                return;
            }

            var scriptData = File.ReadAllText(_fullFileName);
            _engine = new Engine();

            var before = _engine.Global.GetOwnProperties().ToList();
            _engine.Execute(scriptData);
            var after = _engine.Global.GetOwnProperties();

            var uniqueValues = after.Except(before);
            FuncNames = uniqueValues.Select(t => t.Key).ToList();
        }

        private static dynamic GetValue(IVariableWrapper variable)
        {
            switch (variable.Type)
            {
                case VariableType.String:
                    return ((StringVariableWrapper)variable).Value;

                case VariableType.Bool:
                    return ((BoolVariableWrapper)variable).IsSet;

                case VariableType.Date:
                    return ((DateVariableWrapper)variable).Value;

                case VariableType.Time:
                    return ((TimeVariableWrapper)variable).Value;
            }
            return null;
        }

        public bool Execute(string funcName, IVariableWrapper variable)
        {
            var jsFunction = _engine.GetValue(funcName);
            var value = GetValue(variable);
            if (jsFunction.IsUndefined())
            {
                ErrorMessage = $"Метод \"{funcName}({value})\" не найден в файле \"{_fullFileName}\"";
                return false;
            }

            try
            {
                var result = (Jint.Native.JsValue)jsFunction.Invoke(value);

                var variableType = variable.Type;
                if (variableType == VariableType.Bool)
                {
                    if (result.IsBoolean())
                    {
                        variable.Set(result.AsBoolean());
                        return true;
                    }
                    ErrorMessage = TypeMismatchError(funcName, "логическая переменная", variable);
                    return false;
                }

                if (variableType == VariableType.Date || variableType == VariableType.Time)
                {
                    if (result.IsDate())
                    {
                        variable.Set(result.AsDate());
                        return true;
                    }
                    if (result.IsUndefined())
                    {
                        variable.Set(null);
                        return true;
                    }
                    ErrorMessage = TypeMismatchError(funcName, "дата/время", variable);
                    return false;
                }

                if (variableType == VariableType.String)
                {
                    if (result.IsString())
                    {
                        variable.Set(result.AsString());
                        return true;
                    }
                    if (result.IsUndefined())
                    {
                        variable.Set(string.Empty);
                        return true;
                    }
                    ErrorMessage = TypeMismatchError(funcName, "строка", variable);
                    return false;
                }

                variable.Set(result);
                return true;
            }
            catch (Jint.Runtime.JavaScriptException e)
            {
                ErrorMessage = $"Ошибка вызова метода \"{funcName}({value})\": {e.Message}";
                return false;
            }
        }

    }
}