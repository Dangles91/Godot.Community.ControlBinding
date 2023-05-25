using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Godot.Community.ControlBinding.Utilities
{
    public class Configuration
    {
        private const string _suppressAllLogMessagesKey = "ControlBinding/logging/suppressAllLogMessages";
        private const string _suppressWarnLogMessagesKey = "ControlBinding/logging/suppressWarningLogMessages";
        private const string _suppressErrLogMessagesKey = "ControlBinding/logging/suppressErrorLogMessages";
        private const string _suppressInfoLogMessagesKey = "ControlBinding/logging/suppressInfoLogMessages";

        private bool _suppressAllLogMessages = false;
        public bool SuppressAllLogMessages
        {
            get { return _suppressAllLogMessages; }
            private set { _suppressAllLogMessages = value; }
        }

        private bool _suppressWarnLogMessages = false;
        public bool SuppressWarnLogMessage
        {
            get => _suppressWarnLogMessages;
            private set => _suppressWarnLogMessages = value;
        }

        private bool _suppressErrLogMessages = false;
        public bool SuppressErrLogMessages
        {
            get => _suppressErrLogMessages;
            private set => _suppressErrLogMessages = value;
        }

        private bool _suppressInfoLogMessages = false;
        public bool SuppressInfoLogMessages
        {
            get => _suppressInfoLogMessages;
            private set => _suppressInfoLogMessages = value;
        }


        private bool _configurationChanged = false;
        public Configuration()
        {
            loadConfiguration();
            saveConfiguration();
        }

        private void loadConfiguration()
        {
            loadConfigurationKey(_suppressAllLogMessagesKey, ref _suppressAllLogMessages, false);
            loadConfigurationKey(_suppressErrLogMessagesKey, ref _suppressErrLogMessages, false);
            loadConfigurationKey(_suppressInfoLogMessagesKey, ref _suppressInfoLogMessages, false);
            loadConfigurationKey(_suppressWarnLogMessagesKey, ref _suppressWarnLogMessages, false);
        }

        private void saveConfiguration()
        {
            if(_configurationChanged)
            {
                ProjectSettings.Save();
            }
        }

        private void loadConfigurationKey<[MustBeVariant]T>(string key, ref T target, Variant defaultValue)
        {
            if(ProjectSettings.HasSetting(key))
            {
                target = ProjectSettings.GetSetting(key).As<T>();
            }
            else
            {
                createConfigurationItem(key, defaultValue);
            }
        }

        private void createConfigurationItem(string key, Variant initialValue)
        {
            ProjectSettings.SetSetting(key, initialValue);
            _configurationChanged = true;
        }
    }
}