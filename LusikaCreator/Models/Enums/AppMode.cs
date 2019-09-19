using System.ComponentModel;

namespace TestApp.Models.Enums
{
    /// <summary>
    /// Режим запуска программы
    /// </summary>
    public enum AppMode
    {
        [Description("Запуск в режиме просмотра")]
        Viewer = 1,

        [Description("Запуск в режиме редактирования")]
        Editor = 2,

        [Description("Запуск в режиме отладки")]
        Debug = 3
    }
}