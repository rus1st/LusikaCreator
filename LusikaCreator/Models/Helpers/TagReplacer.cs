using System.Linq;
using TestApp.Models.Enums;
using TestApp.Repository;
using TestApp.ViewModels.Variables;

namespace TestApp.Models.Helpers
{
    public class TagStruct
    {
        private readonly string _tagName;

        /// <summary>
        /// Чистое имя тега без концевых символов
        /// </summary>
        public string Name { get; set; }

        public TagType Type { get; set; }

        public string Option { get; set; }

        public bool IsValid { get; set; }

        public bool HasOption => Option != null;

        public TagStruct(string tagName)
        {
            _tagName = tagName;
            IsValid = TryParse();
        }

        public bool Validate()
        {
            var tagName = _tagName.Trim();
            if (tagName.Length < 4) return false;

            var left = tagName.Substring(0, 2);
            if (left != "{#" && left != "{$") return false;
            if (tagName.Last() != '}') return false;

            return true;
        }

        public bool TryParse()
        {
            if (!Validate()) return false;

            switch (_tagName[1])
            {
                case '#':
                    Type = TagType.Single;
                    break;
                case '$':
                    Type = TagType.Paragraph;
                    break;
            }

            var index = _tagName.LastIndexOf(':');
            if (index == -1)
            {
                // No options
                Name = _tagName.Substring(2, _tagName.Length - 3);
                Option = null;
            }
            else
            {
                // Has options
                var options = _tagName.Substring(index + 1, _tagName.Length - index - 2);
                Name = _tagName.Substring(2, _tagName.Length - options.Length - 4);
                Option = options.Trim();
            }

            return true;
        }
    }

    public class TagReplacer
    {
        private readonly VariablesRepository _variablesRepository;
        private TagStruct _tag;

        public string ErrorMessage { get; set; }

        public TagType Type { get; set; }

        public TagReplacer(VariablesRepository variablesRepository)
        {
            _variablesRepository = variablesRepository;
        }

        public bool TryParse(string tagName)
        {
            _tag = new TagStruct(tagName);
            if (!_tag.IsValid) return false;
            Type = _tag.Type;

            return true;
        }

        /// <summary>
        /// Возвращает текст для замены тега
        /// </summary>
        /// <returns>
        /// null         - тег не найден
        /// string.Empty - тег найден, но значение false
        /// not null     - тег найден и значение true
        /// </returns>
        public bool GetValue(out string replacedText)
        {
            replacedText = string.Empty;
            var targetVariable = _variablesRepository.Find(_tag.Name);
            if (targetVariable == null) return false;

            if (_tag.Type == TagType.Paragraph && !(targetVariable is BoolVariableWrapper))
            {
                ErrorMessage = "Тег параграфа применим только к логическим переменным (тип переменной " +
                               $"\"{targetVariable.Name}\": {Common.GetEnumDescription(targetVariable.Type).ToLower()})";
                return false;
            }

            if (targetVariable is StringVariableWrapper)
                replacedText = GetStringReplacedText((StringVariableWrapper) targetVariable);

            else if (targetVariable is BoolVariableWrapper)
                replacedText = GetBoolReplacedText((BoolVariableWrapper) targetVariable);

            else if (targetVariable is DateVariableWrapper)
                replacedText = GetDateReplacedText((DateVariableWrapper) targetVariable);

            else
            {
                ErrorMessage = "Неизвестный тип переменной.";
                return false;
            }

            return true;
        }

        private string GetStringReplacedText(StringVariableWrapper variable)
        {
            // {#str:length}
            // {#str:invert}
            var value = variable.ToString();
            if (!_tag.HasOption) return value;

            var option = _tag.Option.ToLower();
            if (option == "length") return value.Length.ToString();
            if (option == "invert") return Common.Reverse(value);

            return value;
        }

        private string GetBoolReplacedText(BoolVariableWrapper variable)
        {
            // {#bool:Да;Нет}
            // {#bool:not}
            if (!_tag.HasOption) return variable.IsSet ? variable.ToString(variable.IsSet) : string.Empty;

            if (_tag.Option.ToLower() == "not")
                return variable.IsSet ? string.Empty : variable.ToString(!variable.IsSet);

            var array = _tag.Option.Replace(',', ';').Split(';');
            if (array.Length != 2)
                return variable.ToString();

            var trueValue = array[0];
            var falseValue = array[1];
            return variable.IsSet ? trueValue : falseValue;
        }

        private string GetDateReplacedText(DateVariableWrapper variable)
        {
            // {#date:"%h"}
            if (!_tag.HasOption) return variable.ToString();

            var format = ReplaceQuotes(_tag.Option);
            var ret = variable.Value?.ToString(format) ?? string.Empty;
            return ret;
        }

        private static string ReplaceQuotes(string buffer)
        {
            if (buffer.IndexOf('\u2018') > -1) buffer = buffer.Replace('\u2018', '\"');
            if (buffer.IndexOf('\u2019') > -1) buffer = buffer.Replace('\u2019', '\"');
            if (buffer.IndexOf('\u201b') > -1) buffer = buffer.Replace('\u201b', '\"');
            if (buffer.IndexOf('\u201c') > -1) buffer = buffer.Replace('\u201c', '\"');
            if (buffer.IndexOf('\u201d') > -1) buffer = buffer.Replace('\u201d', '\"');
            if (buffer.IndexOf('\u201e') > -1) buffer = buffer.Replace('\u201e', '\"');
            if (buffer.IndexOf('\u2032') > -1) buffer = buffer.Replace('\u2032', '\"');
            if (buffer.IndexOf('\u2033') > -1) buffer = buffer.Replace('\u2033', '\"');

            return buffer.Replace("\"", "");
        }

        public string GetFormattedString(string text)
        {
            var tags = Common.GetTextTags(text);
            if (tags.Count == 0) return text;

            var ret = (string) text.Clone();
            foreach (var tag in tags)
            {
                if (!TryParse(tag)) continue;

                string formattedValue;
                if (!GetValue(out formattedValue)) continue;

                ret = ret.Replace(tag, formattedValue);
            }

            return ret;
        }
    }
}