using System.ComponentModel;

namespace TestApp.ViewModels.Helpers
{
    public enum WindowType
    {
        [Description("Root")]
        Root,

        [Description("ToolsPanel")]
        ToolsPanel,

        [Description("ObjectBrowser")]
        ObjectBrowser,

        [Description("VariablesViewer")]
        VariablesViewer,

        [Description("VariableEditor")]
        VariableEditor,

        [Description("ActionEditor")]
        ActionEditor,

        [Description("ConfigDialog")]
        ConfigDialog,

        [Description("MessageDialog")]
        MessageDialog,

        [Description("TreeMenu")]
        TreeMenu
    }
}