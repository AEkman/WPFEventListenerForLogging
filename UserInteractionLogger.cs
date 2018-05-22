using DirigoClient.DirigoLoggingCore;
using DirigoClient.View;
using System;

namespace DirigoClient
{
    /// <summary>
    /// Log events to file and console
    /// </summary>
    public class UserInteractionLogger
    {
        private readonly UserInteractionEventManager _eventManager;
        private bool _started;

        /// <summary>
        /// Create a user interaction logger. Remember to Start() it.
        /// </summary>
        public UserInteractionLogger()
        {
            _eventManager = new UserInteractionEventManager();
        }

        /// <summary>
        /// Start logging user interaction events.
        /// </summary>
        public void Start()
        {
            if (_started) return;

            _eventManager.ButtonClicked += ButtonClicked;
            _eventManager.TextBoxInput += TextBoxInput;
            _eventManager.MenuSelected += MenuSelected;

            _started = true;
        }

        /// <summary>
        /// Stop logging user interaction events.
        /// </summary>
        public void Stop()
        {
            if (!_started) return;

            _eventManager.ButtonClicked -= ButtonClicked;
            _eventManager.TextBoxInput -= TextBoxInput;
            _eventManager.MenuSelected -= MenuSelected;

            _started = false;
        }

        private void MenuSelected(object selectedMenuItem)
        {
            Console.WriteLine("Menu selection: {0}", selectedMenuItem);
            Logger.WriteUserInteraction("Menubar", "Menu selected", selectedMenuItem.ToString());
        }

        private void TextBoxInput(string senderName, string senderTypeName, string parentWindowName, string textBoxValue)
        {
            string message = (parentWindowName + "." + senderTypeName + " input: " + senderName + " " + textBoxValue);

            Console.WriteLine(message);
            Logger.WriteUserInteraction(parentWindowName, senderTypeName + " " + senderName, textBoxValue);
        }

        private void ButtonClicked(string parentWindowName, string senderTypeName, string senderName, string type)
        {
            string message = (parentWindowName + "." + senderTypeName + " " + type + ": " + senderName);

            Console.WriteLine(message);
            Logger.WriteUserInteraction(parentWindowName, senderTypeName + " " + type, senderName);
        }
    }
}