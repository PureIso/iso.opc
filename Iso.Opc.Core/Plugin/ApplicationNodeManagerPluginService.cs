using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Iso.Opc.Core.Implementations;

namespace Iso.Opc.Core.Plugin
{
    public class ApplicationNodeManagerPluginService
    {
        #region Constants
        private const string AssemblyBaseTypeFullName = "Iso.Opc.Interface.AbstractApplicationNodeManagerPlugin";
        #endregion

        #region Fields
        private readonly List<AbstractApplicationNodeManagerPlugin> _pluginBaseNodeManagers = new List<AbstractApplicationNodeManagerPlugin>();
        #endregion

        #region Properties
        public List<AbstractApplicationNodeManagerPlugin> PluginBaseNodeManagers
        {
            get
            {
                _pluginBaseNodeManagers.Sort((x, y) => string.CompareOrdinal(y.ApplicationName, x.ApplicationName));
                return _pluginBaseNodeManagers;
            }
        }
        #endregion

        /// <summary>
        ///     Plug-in service constructor
        /// </summary>
        /// <param name="folderPath"> The plug-in folder path </param>
        public ApplicationNodeManagerPluginService(string folderPath)
        {
            try
            {
                foreach (string plugin in Directory.GetFiles(folderPath))
                {
                    FileInfo file = new FileInfo(plugin);
                    if (file.Extension.Equals(".dll"))
                    {
                        AddPlugin(plugin);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        #region Methods
        private void AddPlugin(string pluginPath)
        {
            try
            {
                Assembly pluginAssembly = Assembly.LoadFrom(pluginPath); //Load assembly given its full name and path

                foreach (Type pluginType in pluginAssembly.GetTypes())
                {
                    if (!pluginType.IsPublic) continue; //break the for each loop to next iteration if any
                    if (pluginType.IsAbstract) continue; //break the for each loop to next iteration if any
                    //search for specified interface while ignoring case sensitivity
                    if (pluginType.BaseType == null ||
                        pluginType.BaseType.FullName != AssemblyBaseTypeFullName)
                        continue;
                    //New plug-in information setting
                    AbstractApplicationNodeManagerPlugin pluginInterfaceInstance =
                        (AbstractApplicationNodeManagerPlugin)(Activator.CreateInstance(pluginAssembly.GetType(pluginType.ToString())));
                    _pluginBaseNodeManagers.Add(pluginInterfaceInstance);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        #endregion
    }
}
