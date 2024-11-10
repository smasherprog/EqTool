using Autofac;
using EQTool.UI.SettingsComponents;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace EQTool.Services
{
    public enum UserComponentSettingsManagementTypes
    {
        Landing = 1,
        Server,
        Player
    }

    public class UserComponentSettingsManagementFactory
    {
        //anti pattern below, but its the only way to do this.
        private readonly ILifetimeScope container;
        private readonly Dictionary<UserComponentSettingsManagementTypes, Type> userComponentTypes = new Dictionary<UserComponentSettingsManagementTypes, Type>();
        public UserComponentSettingsManagementFactory(ILifetimeScope container)
        {
            this.container = container;
            userComponentTypes.Add(UserComponentSettingsManagementTypes.Landing, typeof(SettingsLanding));
            userComponentTypes.Add(UserComponentSettingsManagementTypes.Server, typeof(SettingsServer));
            userComponentTypes.Add(UserComponentSettingsManagementTypes.Player, typeof(SettingsPlayer));
        }

        public UserControl CreateComponent(UserComponentSettingsManagementTypes userComponentSettingsManagementType)
        {
            var t = userComponentTypes[userComponentSettingsManagementType];
            return container.Resolve(t) as UserControl;
        }

        public UserControl CreateComponent(UserComponentSettingsManagementTypes userComponentSettingsManagementType, params object[] parameters)
        {
            var t = userComponentTypes[userComponentSettingsManagementType];
            var typedparameters = new List<TypedParameter>();
            foreach (var parameter in parameters)
            {
                typedparameters.Add(new TypedParameter(parameter.GetType(), parameter));
            }

            return container.Resolve(t, typedparameters) as UserControl;
        }
    }
}
