﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TestApp.ViewModels.Helpers
{
    public class TreeViewHelper
    {
        private static readonly Dictionary<DependencyObject, TreeViewSelectedItemBehavior> Behaviors =
            new Dictionary<DependencyObject, TreeViewSelectedItemBehavior>();

        public static object GetSelectedItem(DependencyObject obj)
        {
            return obj.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(DependencyObject obj, object value)
        {
            obj.SetValue(SelectedItemProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached("SelectedItem", typeof (object), typeof (TreeViewHelper),
                new UIPropertyMetadata(null, SelectedItemChanged));

        private static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is TreeView))
                return;

            if (!Behaviors.ContainsKey(obj))
                Behaviors.Add(obj, new TreeViewSelectedItemBehavior(obj as TreeView));

            TreeViewSelectedItemBehavior view = Behaviors[obj];
            view.ChangeSelectedItem(e.NewValue);
        }

        private class TreeViewSelectedItemBehavior
        {
            readonly TreeView _view;

            public TreeViewSelectedItemBehavior(TreeView view)
            {
                this._view = view;
                view.SelectedItemChanged += (sender, e) => SetSelectedItem(view, e.NewValue);
            }

            internal void ChangeSelectedItem(object p)
            {
                TreeViewItem item = (TreeViewItem) _view.ItemContainerGenerator.ContainerFromItem(p);
                item.IsSelected = true;
            }
        }
    }
}