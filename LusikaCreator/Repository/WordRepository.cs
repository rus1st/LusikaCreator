using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Office.Interop.Word;
using TestApp.Models;
using TestApp.Models.Config;
using TestApp.Models.Helpers;
using TestApp.Models.Interfaces;
using TestApp.ViewModels.Helpers;

namespace TestApp.Repository
{
    /// <summary>
    /// Класс для работы с документом Microsoft Word
    /// </summary>
    public class WordRepository
    {
        private readonly TagReplacer _tagReplacer;
        private readonly VariablesRepository _variablesRepository;
        private readonly ProjectSettings _projectSettings;
        private readonly DialogsManager _dialogsHelper;
        private Application _app;
        private Document _doc;

        public string ErrorMessage { get; set; }

        public string WrongTypeError(IVariable variable) =>
            $"Переменная \"{variable.Name}\" помечена как логическая переменная, но имеет недопустимое значение ({variable.GetType()}).";

        public WordRepository(DataProvider dataProvider)
        {
            _variablesRepository = dataProvider.VariablesRepository;
            _projectSettings = dataProvider.ProjectRepository.ProjectSettings;
            _dialogsHelper = dataProvider.DialogsManager;
            _tagReplacer = new TagReplacer(_variablesRepository);
        }

        public bool FillTemplate(string outPath, string fileName)
        {
            var templateFileName = _projectSettings.TemplateFileName;
            if (!File.Exists(templateFileName))
            {
                ErrorMessage = $"Файл шаблона не найден \"{templateFileName}\".";
                return false;
            }

            try
            {
                _app = new Application {Visible = false};
                _doc = _app.Documents.Open(templateFileName, ReadOnly: false, Visible: false);
                _doc.Activate();
            }
            catch (Exception e)
            {
                ErrorMessage = "Ошибка создания документа Word.";
                return false;
            }

            if (!TryUntilSuccess(FillTags))
            {
                ErrorMessage = "Превышено время ожидания формирования файла.";
                try { _app.Quit(false); }
                catch { }
                return false;
            }

            var formattedFileName = _tagReplacer.GetFormattedString(fileName) ?? fileName;
            var formattedOutPath = _tagReplacer.GetFormattedString(outPath) ?? outPath;
            if (!Directory.Exists(formattedOutPath))
            {
                try
                {
                    Directory.CreateDirectory(formattedOutPath);
                }
                catch (Exception)
                {
                    ErrorMessage = $"Невозможно создать директорию{Environment.NewLine}\"{formattedOutPath}\"";
                    _app.Quit(false);
                    return false;
                }
            }

            var ext = Path.GetExtension(templateFileName);
            var saveTo = formattedOutPath + @"\" + formattedFileName + ext;

            if (!IsValidFilename(saveTo))
            {
                ErrorMessage = "Имя файла содержит недопустимые символы.";
                return false;
            }

            try
            {
                _doc.SaveAs(saveTo);
            }
            catch (Exception e)
            {
                ErrorMessage = $"Ошибка сохранения шаблона в файл \"{saveTo}\":{Environment.NewLine}{e.Message}";
                _app.Quit(true);
                return false;
            }

            _app.Quit(true);

            if (_projectSettings.ShowTargetFile)
            {
                System.Diagnostics.Process.Start(saveTo);
            }
            else if (_projectSettings.OpenTargetFolder)
            {
                var argument = "/select, \"" + saveTo + "\"";
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
            return true;
        }

        private static bool IsValidFilename(string testName)
        {
            var containsABadCharacter = new Regex($"[{Regex.Escape(new string(Path.GetInvalidPathChars()))}]");
            return !containsABadCharacter.IsMatch(testName);
        }


        private void FillTags()
        {
            var rng = _doc.Content;
            rng.Find.ClearFormatting();
            rng.Find.Text = @"(\{[#,$])*(\})";
            rng.Find.MatchWildcards = true;
            rng.Find.Forward = true;

            while (rng.Find.Execute())
            {
                object start = rng.Start;
                object end = rng.End;
                var localRng = _doc.Range(ref start, ref end);

                if (!_tagReplacer.TryParse(localRng.Text))
                {
                    // todo error
                    continue;
                }

                string replacedText;
                if (!_tagReplacer.GetValue(out replacedText)) continue;

                if (_tagReplacer.Type == TagType.Single)
                {
                    localRng.Text = replacedText;
                }
                else if (_tagReplacer.Type == TagType.Paragraph)
                {
                    if (replacedText == string.Empty)
                    {
                        var index = GetParagraphIndexByText(localRng.Text);
                        if (index != -1) RemoveParagraph(index);
                    }
                    else if (!string.IsNullOrEmpty(replacedText))
                    {
                        localRng.Text = string.Empty;
                    }
                }
            }
        }

        private void RemoveParagraph(int index)
        {
            _doc.Paragraphs[index].Range.Select();
            _app.Selection.Delete();
        }

        private bool TryUntilSuccess(Action action)
        {
            const int tryCount = 100;
            var success = false;

            var currentTry = 0;
            while (!success)
            {
                try
                {
                    action();
                    success = true;
                }

                catch (System.Runtime.InteropServices.COMException e)
                {
                    currentTry++;
                    _dialogsHelper.ChangeProgressText($"Формирование файла (попытка № {currentTry})...");
                    
                    if (currentTry == tryCount) return false;
                    if ((e.ErrorCode & 0xFFFF) == 0xC472)
                    {
                        // Excel is busy
                        Thread.Sleep(500);
                        success = false;
                    }
                    else
                    {
                        Thread.Sleep(100);
                        success = false;
                    }
                }
            }
            return true;
        }

        private int GetParagraphIndexByText(string tag)
        {
            _app.Selection.Find.ClearFormatting();
            _app.Selection.Find.Forward = true;
            _app.Selection.Find.Wrap = 0;

            if (_app.Selection.Find.Execute(tag))
            {
                return _doc.Range(0, _app.Selection.Start).Paragraphs.Count + 1;
            }
            return -1;
        }

    }
}