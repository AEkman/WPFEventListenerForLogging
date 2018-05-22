using DirigoClient.DirigoLoggingCore;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DirigoClient
{
    /// <summary>
    /// Listens to user RoutedEvents: "Buttonclicks", "Selections", "Textbox inputs", "Menu Selections"
    /// </summary>
    public class UserInteractionEventManager
    {
        public delegate void ButtonClickedHandler(string parentWindowName, string senderTypeName, string senderName, string type);
        public delegate void TextBoxInputHandler(string senderName, string senderTypeName, string parentWindowName, string textBoxValue);
        public delegate void MenuSelectionHandler(object selectedMenuItem);

        public event ButtonClickedHandler ButtonClicked;
        public event TextBoxInputHandler TextBoxInput;
        public event MenuSelectionHandler MenuSelected;

        public UserInteractionEventManager()
        {
            EventManager.RegisterClassHandler(typeof(ButtonBase), ButtonBase.ClickEvent, new RoutedEventHandler(HandleButtonClicked));
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.LostFocusEvent, new RoutedEventHandler(HandleTextBoxInput));
            EventManager.RegisterClassHandler(typeof(MenuItem), MenuItem.ClickEvent, new RoutedEventHandler(HandleMenuSelectorClicked));
        }

        #region Handling events

        private void HandleMenuSelectorClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Avoid multiple events due to bubbling. Example: A ListBox inside a TabControl will cause both to send the SelectionChangedEvent
                if (sender != e.OriginalSource) return;

                var element = sender as FrameworkElement;
                if (element == null) return;

                MenuItem menuItem = (MenuItem)e.OriginalSource;

                string selectedHeader = menuItem.Header.ToString();

                if (selectedHeader != null)
                    MenuSelected(selectedHeader);
            }
            catch (Exception ex)
            {
                //Logger.WriteError(AppInfo.GetLogError(ex.ToString()));
            }
        }

        private void HandleTextBoxInput(object sender, RoutedEventArgs e)
        {
            try
            {
                var element = sender as FrameworkElement;
                if (element == null) return;

                string eventName = GetEventName(e);
                string senderName = GetSenderName(element);
                string senderTypeName = GetSenderTypeName(sender);
                string parentWindowsName = GetParentWindowTypeName(sender);
                string textBoxValue = ((TextBox)element).Text == "" ? "empty" : ((TextBox)element).Text;

                // Don't record sensitive information such as new passwords
                if (parentWindowsName.Contains("ChangePasswordWindow")) return;

                if (TextBoxInput != null && textBoxValue != "empty")
                    TextBoxInput(senderName, senderTypeName, parentWindowsName, textBoxValue);
            }
            catch (Exception ex)
            {
                //Logger.WriteError(AppInfo.GetLogError(ex.ToString()));
            }
        }

        private void HandleButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var element = sender as FrameworkElement;
                if (element == null) return;

                string parentWindowName = GetParentWindowTypeName(sender);
                string senderTypeName = GetSenderTypeName(sender);
                string senderName = GetSenderName(element);
                string type = "clicked";

                // Check if button is radio button
                if (senderTypeName == "RadioButton")
                {
                    var radioButton = ((RadioButton)element);
                    type = (radioButton.IsChecked == false) ? "unchecked" : "checked";
                }

                // Check if button is column header
                if (senderName == "<no sender name>" && e.OriginalSource.ToString().Contains("DataGridColumnHeader"))
                {
                    if (e.OriginalSource.ToString().Length > 60)
                    {
                        senderName = e.OriginalSource.ToString().Remove(0, 60);
                    }

                    type = "filter";
                }

                // Check if button is checkbox
                if (senderTypeName == "CheckBox")
                {
                    var checkBox = ((CheckBox)element);

                    // Collect checkbox status
                    type = (checkBox.IsChecked == false) ? "unchecked" : "checked";

                    // Casting to get parent
                    var templatedParent = ((System.Windows.Controls.ContentPresenter)element.TemplatedParent);
                    var parent2 = ((Microsoft.Windows.Controls.DataGridCell)element.Parent);

                    if (templatedParent != null)
                    {
                        var parent = ((Microsoft.Windows.Controls.DataGridCell)templatedParent.Parent);
                        senderName = parent.Column.Header.ToString();
                    }

                    if (parent2 != null)
                    {
                        senderName = parent2.Column.Header.ToString();
                    }

                    if (senderName == "") { senderName = "<no sender name>"; }

                }

                if (ButtonClicked != null)
                    ButtonClicked(parentWindowName, senderTypeName, senderName, type);

            }
            catch (Exception ex)
            {
                //Logger.WriteError(ex.ToString());
            }
        }

        #endregion

        #region Private helpers

        private static string GetSenderName(FrameworkElement element)
        {
            return !String.IsNullOrEmpty(element.Name) ? element.Name : "<no sender name>";
        }

        private static string GetSenderTypeName(Object sender)
        {
            return !String.IsNullOrEmpty(sender.GetType().Name) ? sender.GetType().Name : "<no sender type name>";
        }

        private static string GetEventName(RoutedEventArgs routedEventArgs)
        {
            return !String.IsNullOrEmpty(routedEventArgs.RoutedEvent.Name) ? routedEventArgs.RoutedEvent.Name : "<no event name>";
        }

        private static string GetParentWindowTypeName(object sender)
        {
            var parent = FindParent<Window>(sender as DependencyObject);
            return parent != null ? parent.GetType().Name : "<no parent>";
        }

        private static T FindParent<T>(DependencyObject item) where T : class
        {
            if (item == null)
                return default(T);

            if (item is T)
                return item as T;

            DependencyObject parent = VisualTreeHelper.GetParent(item);
            if (parent == null)
                return default(T);

            return FindParent<T>(parent);
        }

        #endregion
    }
}