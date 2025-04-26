using Autofac;
using EQTool.UI.MobInfoComponents;
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
        private readonly Dictionary<MobInfoItemType, Type> modinfoComponentTypes = new Dictionary<MobInfoItemType, Type>();
        public UserComponentSettingsManagementFactory(ILifetimeScope container)
        {
            this.container = container;
            userComponentTypes.Add(TreeViewItemType.General, typeof(SettingsGeneral));
            userComponentTypes.Add(TreeViewItemType.Server, typeof(SettingsServer));
            userComponentTypes.Add(TreeViewItemType.Player, typeof(SettingsPlayer));
            userComponentTypes.Add(TreeViewItemType.Zone, typeof(SettingsGeneral));
            userComponentTypes.Add(TreeViewItemType.Global, typeof(SettingsGeneral));

            // todo - what is this intended to do?  do we need a SettingsTrigger defined??
            userComponentTypes.Add(TreeViewItemType.Trigger, typeof(SettingsTrigger));

            modinfoComponentTypes.Add(MobInfoItemType.Mob, typeof(MobComponent));
            modinfoComponentTypes.Add(MobInfoItemType.Pet, typeof(PetComponent));
        }

        public UserControl CreateComponent(TreeViewItemType userComponentSettingsManagementType)
        {
            var t = userComponentTypes[userComponentSettingsManagementType];
            return container.Resolve(t) as UserControl;
        }

        public UserControl CreateComponent(MobInfoItemType modInfoItemType)
        {
            var t = modinfoComponentTypes[modInfoItemType];
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
