using System.Collections.Generic;

namespace TestApp.Models.FormObjects
{
    public class ComboBoxObject
    {
        public List<ComboBoxItem> Items { get; set; } = new List<ComboBoxItem>();

        public ComboBoxObject()
        {
        }
    }
}