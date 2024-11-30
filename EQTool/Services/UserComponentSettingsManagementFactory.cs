using Autofac;
using EQTool.UI.SettingsComponents;
using EQTool.ViewModels.SettingsComponents;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace EQTool.Services
{
    public class UserComponentSettingsManagementFactory
    { 
        private readonly ILifetimeScope container;
        private readonly Dictionary<TreeViewItemType, Type> userComponentTypes = new Dictionary<TreeViewItemType, Type>();
        public UserComponentSettingsManagementFactory(ILifetimeScope container)
        {
            this.container = container;
            userComponentTypes.Add(TreeViewItemType.General, typeof(SettingsGeneral));
            userComponentTypes.Add(TreeViewItemType.Server, typeof(SettingsServer));
            userComponentTypes.Add(TreeViewItemType.Player, typeof(SettingsPlayer));
        }

        public UserControl CreateComponent(TreeViewItemType userComponentSettingsManagementType)
        {
            var t = userComponentTypes[userComponentSettingsManagementType];
            return container.Resolve(t) as UserControl;
        }

        public UserControl CreateComponent(TreeViewItemType userComponentSettingsManagementType, params object[] parameters)
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
