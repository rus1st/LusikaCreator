using System.Linq;
using System.Windows;
using TestApp.Models;
using TestApp.Models.Enums;

namespace TestApp
{
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var appMode = e.Args.Any(arg => arg.Trim().ToLower() == "--editor")
                ? AppMode.Editor
                : AppMode.Viewer;

            var dataProvider = new DataProvider(appMode);


//            var script = new UserScript
//            {
//                Description = "Преобразовать ФИО в Фамилию И.О.",
//                InputType = VariableType.String,
//                OutputType = VariableType.String,
//                Code = @"function makeFamIO(FIO){ 
//  var splitted = FIO.split(' ').filter(function(elem){
//    return elem.trim() !== '';
//  });
//  var len = splitted.length;
//  var ret = `${splitted[0]} ${splitted[1].substring(0,1)}.`
//  if (len == 2) return ret;
//  else if (len == 3) return ret + ` ${splitted[2].substring(0,1)}.`;
//  else return FIO;
//}"
//            };
//            dataProvider.ScriptsRepository.AddScript(script);
//            var ret = dataProvider.XmlRepository.SaveScripts();



            dataProvider.WindowsManager.CreateWindows();
        }
    }
}