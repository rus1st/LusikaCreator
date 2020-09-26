using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using TestApp.Models;
using TestApp.Models.Config;
using TestApp.Models.Enums;
using TestApp.Models.FormObjects;
using TestApp.Models.Variables;

namespace TestApp.Repository
{
    /// <summary>
    /// Класс для выгрузки/загрузки конфигурации в xml
    /// </summary>
    public class XmlRepository
    {
        private readonly List<Type> _knownTypes = new List<Type>
        {
            typeof (LabelObject),
            typeof (TextBoxObject),
            typeof (CheckBoxObject),
            typeof (RadioButtonObject),
            typeof (DateBoxObject),
            typeof (TimePickerObject),
            typeof (ActionInputOperand),
            typeof (ActionSelectorOperand),
            typeof (ActionVisibilityOperand),
            typeof (ActionDateOperand),
            typeof (StringVariable),
            typeof (BoolVariable),
            typeof (DateVariable),
            typeof (TimeVariable)
        };
        private readonly DataProvider _dataProvider;

        public string ErrorMessage { get; set; }

        public XmlRepository(DataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public bool LoadProject(string filename)
        {
            var serializer = new DataContractSerializer(typeof (XmlStorage), _knownTypes);

            var fs = new FileStream(filename, FileMode.Open);
            var reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());

            XmlStorage stor;
            try
            {
                stor = (XmlStorage) serializer.ReadObject(reader);
            }
            catch (Exception e)
            {
                return false;
            }

            reader.Close();
            fs.Close();

            _dataProvider.VariablesRepository.Clear();
            foreach (var variable in stor.Variables)
                _dataProvider.VariablesRepository.Add(variable);
            _dataProvider.ProjectRepository.ProjectSettings.Update(stor.Settings);

            _dataProvider.ObjectsRepository.Clear();
            foreach (var formObject in stor.Objects)
            {
                _dataProvider.ObjectsRepository.Add(formObject);
            }

            _dataProvider.VariablesRepository.UpdateAllFormattedValues();
            _dataProvider.ObjectsRepository.UpdateAllFormattedText();

            _dataProvider.TabsRepository.Clear();
            foreach (var tab in stor.Tabs) _dataProvider.TabsRepository.AddItem(tab);

            _dataProvider.TabsRepository.SelectedTabIndex = _dataProvider.TabsRepository.TabItems.First().Id;
            return true;
        }

        public bool SaveProject(string filename)
        {
            var storage = new XmlStorage
            {
                Settings = new StoredSettings(_dataProvider.ProjectRepository.ProjectSettings),
                Objects = _dataProvider.ObjectsRepository.ViewModels
                    .Select(viewModel => viewModel.ToStoredObject())
                    .ToList(),
                Variables = _dataProvider.VariablesRepository.Variables
                    .Select(variable => variable.ToStoredObject())
                    .ToList(),
                Tabs = _dataProvider.TabsRepository.ToStoredObject()
            };

            var settings = new XmlWriterSettings {Indent = true};

            var serializer = new DataContractSerializer(typeof (XmlStorage), _knownTypes);
            using (var writer = XmlWriter.Create(filename, settings))
            {
                serializer.WriteObject(writer, storage);
            }

            return true;
        }

        public CommonSettings LoadCommonSettings()
        {
            var appDataPath = Common.GetAppDataPath;
            var filename = appDataPath + $@"\{Constants.ProjectName}\Settings.xml";

            if (!File.Exists(filename)) return null;

            try
            {
                var serializer = new DataContractSerializer(typeof (CommonSettings));

                var fs = new FileStream(filename, FileMode.Open);
                var reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());

                var ret = (CommonSettings) serializer.ReadObject(reader);
                ret.RestoreSettings();
                reader.Close();
                fs.Close();

                return ret;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public bool SaveCommonSettings()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var filename = appDataPath + $@"\{Constants.ProjectName}\Settings.xml";

            var settings = new XmlWriterSettings {Indent = true};

            try
            {
                var path = Path.GetDirectoryName(filename);
                if (path == null) return false;
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                var serializer = new DataContractSerializer(typeof (CommonSettings));
                using (var writer = XmlWriter.Create(filename, settings))
                {
                    _dataProvider.CommonSettings.ToStoredSettings();
                    serializer.WriteObject(writer, _dataProvider.CommonSettings);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}