/*
Manager.cs -- base manager for executing plugins 1.0.0, August 30, 2015

Copyright (c) 2013-2015 Kudryashov Andrey aka Dr
Kirillov Vasiliy 
Mattis Igor 

This software is provided 'as-is', without any express or implied
warranty. In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not
claim that you wrote the original software. If you use this software
in a product, an acknowledgment in the product documentation would be
appreciated but is not required.

2. Altered source versions must be plainly marked as such, and must not be
misrepresented as being the original software.

3. This notice may not be removed or altered from any source distribution.

Kudryashov Andrey <kudryashov.andrey at gmail.com>
Kirillov Vasiliy  <vskirillov.spb at gmail.com>
Mattis Igor       <rulez1990 at gmail.com>

*/
using DrOpen.DrTask.DrtPlugin;
using System;
using System.Collections.Generic;
using DrOpen.DrTask.DrtManager.Commands;
using System.Reflection;
using DrOpen.DrCommon.DrData;

namespace DrOpen.DrTask.DrtManager
{
    /// <summary>
    /// Base manager for executing plugins
    /// </summary>
    public class Manager : Plugin, IManager, IPlugin
    {
        /// <summary>
        /// Event for request parent manager
        /// </summary>
        public event DDEventHandler CallParent;

        /// <summary>
        /// List of commands that current instance of manager supports from child managers
        /// </summary>
        private readonly List<string> supportedCommands;
        /// <summary>
        /// List of already loaded plugin objects
        /// </summary>
        private List<IPlugin> pluginList;
        /// <summary>
        /// Currently executing plugin. Could be changed by child manager request
        /// </summary>
        int currentPlugin;


        public Manager()
        {
            supportedCommands = new List<string>()
            {
                FCommands.Sample
            };

            pluginList = new List<IPlugin>();
            currentPlugin = 0; // ToDo remove int position, need to switch to queue
        }


        /// <summary>
        /// Checks if the command is supported or not
        /// </summary>
        /// <param name="command"></param>
        /// <returns>This method returns true if given command is supported, false otherwise</returns>
        public bool IsSupportedCommand(string command)
        {
            try // ToDo remove try catch
            {
                return supportedCommands.Contains(command);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Facade for execution of manager with given config and data
        /// </summary>
        /// <param name="config">config should contain a node with list of plugin for proper execution</param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public override DDNode Execute(DDNode config, params DDNode[] nodes)
        {
            try
            {
                // ToDo Event before and validate Cancel 
                currentPlugin = 0; // ToDo remove int position, need to switch to queue
                return DoExecute(config, nodes);
                // ToDo Event after 
            }
            catch (Exception e)
            {
                Console.WriteLine(e); // ToDo log.WriteError
                throw new ApplicationException(e.Message); // ToDo throw new ApplicationException("Cannot execute ....", e);
            }
        }

        /// <summary>
        /// Invokes execution of manager with given config and additional data
        /// </summary>
        /// <param name="config">config should contain a node with list of plugin for proper execution</param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private DDNode DoExecute(DDNode config, params DDNode[] nodes)
        {
            DDNode pluginListNode = config.GetNode(Manager.PluginList); // null reference?? catch exception
            IEnumerator<KeyValuePair<string, DDNode>> pluginNodeEnumerator = pluginListNode.GetEnumerator();

            while (currentPlugin < pluginListNode.Count)
            {
                // if (IPlugin) object for [currentPlugin] isn't created yet - creates it
                if (currentPlugin >= pluginList.Count)
                {
                    pluginNodeEnumerator.MoveNext();
                    DDNode pluginNode = pluginNodeEnumerator.Current.Value;
                    this.pluginList.Add(GetPluginObject(pluginNode));
                }

                var currentPluginInstance = pluginList[currentPlugin];
                DDNode pluginConfig = GetPluginConfig(pluginListNode, currentPlugin);
                currentPlugin++;

                var beforeExecuteEventArgs = new DDEventArgs();
                ProcessBeforeExecute(currentPluginInstance, beforeExecuteEventArgs);
                if (beforeExecuteEventArgs.ContainsKey(Manager.Cancel))
                    continue;

                SubscribeToManager(currentPluginInstance);
                var callParentEventArgs = new DDEventArgs();
                currentPluginInstance.Execute(pluginConfig);
                // TBD: DDNode result = currentPluginInstance.Execute(pluginConfig);

                if (currentPlugin == pluginListNode.Count)
                    this.DoCallParent(callParentEventArgs);

                ProcessAfterExecute(currentPluginInstance, new DDEventArgs());
            }
            

            return new DDNode("GoodResult"); // ToDo create stub static positive and negative Execute result and return it
        }

        #region [Execute] supporting methods
        /// <summary>
        /// Returns config for [currentPlugin] plugin in [pluginsListNode]
        /// </summary>
        /// <param name="pluginsListNode">Node with PluginList from xml-config</param>
        /// <param name="currentPlugin">Plugin for which configuration is requested</param>
        /// <returns>Node with config for [currentPlugin] plugin</returns>
        private DDNode GetPluginConfig(DDNode pluginsListNode, int currentPlugin) // ToDo remove this stupid function
        {
            try
            {
                int i = 0;
                foreach (var pluginNode in pluginsListNode)
                {
                    if (i == currentPlugin)
                        return pluginNode.Value.GetNode(Manager.Configuration + "/" + Manager.PluginSpecific);
                    i++;
                }
                return null;
            }
            catch(Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        /// <summary>
        /// Method that subscribes for [plugin]'s BeforeExecute event and raises it.
        /// May be used to determine if [plugin] execution is needed or not
        /// </summary>
        /// <param name="plugin">Currently beeing executed plugin</param>
        /// <param name="beforeEventArgs">Event arguments</param>
        private void ProcessBeforeExecute(IPlugin plugin, DDEventArgs beforeEventArgs)
        {
            plugin.BeforeExecute += this.BeforeExecuteHandler; // ToDo Why?
            plugin.DoBeforeExecute(beforeEventArgs);
            plugin.BeforeExecute -= this.BeforeExecuteHandler; // ToDo Why?
        }

        /// <summary>
        /// Method that subscribes for [plugin]'s BeforeExecute event and raises it.
        /// </summary>
        /// <param name="plugin">Currently beeing executed plugin</param>
        /// <param name="afterEventArgs">Event arguments</param>
        private void ProcessAfterExecute(IPlugin plugin, DDEventArgs afterEventArgs)
        { 
            plugin.AfterExecute += this.AfterExecuteHandler;// ToDo Why?
            plugin.DoAfterExecute(afterEventArgs);
            plugin.AfterExecute -= this.AfterExecuteHandler;// ToDo Why?

            //this.CallUp -= this.EventHandling;
        }

        /// <summary>
        /// Subscribe CallParentHandler method to CallParent event if it's Manager object boxed into IPlugin
        /// </summary>
        /// <param name="plugin">IPlugin object that may be Manager or not</param>
        private void SubscribeToManager(IPlugin plugin)
        {
            if (plugin is Manager)
            {
                Manager manager = (Manager)plugin;
                manager.CallParent += this.CallParentHandler;
            }
        }


        /// <summary>
        /// Facade for raise event of calling up for parent manager
        /// </summary>
        /// <param name="eventArgs">Event arguments</param>
        public void DoCallParent(DDEventArgs eventArgs)
        {
            try
            {
                if (CallParent != null)
                    CallParent(this, eventArgs);

                Console.WriteLine("\tCall up started.");
            }
            catch (Exception e)
            {
                // ToDo - Вщ not ever do this. Where are logging and throw?
            }
        }


        /// <summary>
        /// Method extracts plugin dll&class information from configurational DDNode
        /// and initiates proccess of creating specified object by means of System.Reflection
        /// </summary>
        /// <param name="pluginNode">Node that contains plugin configuration (both common and plugin specific)</param>
        /// <returns>Plugin object that meets [Configuration/Common] content</returns>
        private IPlugin GetPluginObject(DDNode pluginNode)
        {
            DDNode pluginConfig = pluginNode.GetNode(Manager.Configuration);
            DDNode pluginConfigCommon = pluginConfig.GetNode(Manager.Common);
            // If [Configuration/Common] node doesn't contain both DllPath and Class name attributes:
            // try to get them from [PathToConfig] node or from [Root/Plugins/<pluginNode.Name>]
            if (!pluginConfigCommon.Contains(Manager.DllPath) || !pluginConfigCommon.Contains(Manager.ClassName))
            {
                if (pluginConfigCommon.Attributes.Contains(Manager.PathToConfig))
                {
                    pluginConfigCommon = pluginNode.GetRoot().GetNode(pluginConfigCommon.Attributes[Manager.PathToConfig]);
                }
                else
                {
                    pluginConfigCommon = pluginNode.GetRoot().GetNode("Plugins/" + pluginNode.Name);
                }
                // TBD: add fail resist here (if node is still not valid
            }

            string ddlPath = pluginConfigCommon.Attributes[Manager.DllPath];
            string className = pluginConfigCommon.Attributes[Manager.ClassName];

            return (IPlugin)this.GetObject(ddlPath, className);
        }
        #endregion
        #region EventHandling

        /// <summary>
        /// Method that handles BeforeExecute event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="simpleArgs"></param>
        public void BeforeExecuteHandler(Object sender, DDEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method that handles AfterExecute event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="simpleArgs"></param>
        public void AfterExecuteHandler(Object sender, DDEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method that handles CallParent event
        /// </summary>
        /// <param name="managerInstance"></param>
        /// <param name="simpleArgs"></param>
        public void CallParentHandler(Object managerInstance, DDEventArgs eventArgs)
        {
            //log.WriteDebug("Starting EventHandling...");
            try
            {
                string commandName = FCommands.Sample; //extendedArgs.EventData.Attributes["commandName"].ToString();
                DDNode resultNode = new DDNode();

                if (supportedCommands.Contains(commandName))
                {
                    resultNode = FCommands.GetCommand(commandName).DoIt(eventArgs.EventData);
                }

                //if (commandName == FCommands.GoTo && resultNode.GetNode("GoTo").Attributes["Enabled"])
                //    currentPlugin = resultNode.GetNode("GoTo").Attributes["PluginToGo"];

                //log.WriteDebug("EventHandling started succesfull");
            }
            catch (Exception e)
            {
                //log.WriteError(e, "Cannot process EventHandling");
            }
        }

        #endregion
        #region Reflection

        /// <summary>
        /// Searches in dll class of specified type and params of constructor wirh specified number and types.
        /// </summary>
        /// <returns>Object that meets the conditions</returns>
        private Object GetObject(string dllName, Type classType, params Object[] constructorParamsArray)
        {
            try
            {
                var reflectedDLL = Assembly.LoadFrom(dllName);

                object providerObject = GetSpecifiedObject(reflectedDLL.GetTypes(), classType, constructorParamsArray);

                if (providerObject != null) return providerObject;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message); // ToDo log.WriteError + throw ... look for previous comment about the same issue
            }

            throw new ApplicationException("There are no class|dll|construcotor..."); // ToDo Why?
        }

        /// <summary>
        /// Searches in dll class of specified type and params of constructor wirh specified number and types.
        /// </summary>
        /// <returns>Object that meets the conditions</returns>
        private Object GetObject(string dllName, string className, params Object[] constructorParamsArray)
        {
            try
            {
                var reflectedDLL = Assembly.LoadFrom(dllName);

                object providerObject = GetSpecifiedObject(reflectedDLL.GetTypes(), className, constructorParamsArray);

                if (providerObject != null) return providerObject;
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }

            throw new ApplicationException("There are no class|dll|construcotor...");
        }

        /// <summary>
        /// Searches for specified type and start GetSpecifiedConstructor.
        /// </summary>
        /// <returns>Object that meets the conditions else null</returns>
        private Object GetSpecifiedObject(Type[] allClassTypes, Type desiredType, params Object[] paramsArray)
        {
            object providerObject = null;

            foreach (var tempType in allClassTypes)
            {
                if (desiredType.IsAssignableFrom(tempType))
                {
                    providerObject = GetConstructor(tempType, paramsArray);

                    if (providerObject != null) return providerObject;
                }
            }

            return null;
        }

        /// <summary>
        /// Searches for specified type and start GetSpecifiedConstructor.
        /// </summary>
        /// <returns>Object that meets the conditions else null</returns>
        private Object GetSpecifiedObject(Type[] allClassTypes, string desiredClassName, params Object[] paramsArray)
        {
            object providerObject = null;

            foreach (var tempType in allClassTypes)
            {
                if (desiredClassName == tempType.Name)
                {
                    providerObject = GetConstructor(tempType, paramsArray);

                    if (providerObject != null) return providerObject;
                }
            }

            return null;
        }

        /// <summary>
        /// Searches for specified constructor and start ConstructorHasCorrectParams
        /// </summary>
        /// <returns>Object that meets the conditions else null</returns>
        private Object GetConstructor(Type classType, params Object[] paramsArray)
        {
            ConstructorInfo[] allClassConstructors = classType.GetConstructors();

            foreach (ConstructorInfo classConstructor in allClassConstructors)
            {
                ParameterInfo[] constructorParams = classConstructor.GetParameters();

                if (constructorParams.Length == paramsArray.Length)
                {
                    if (ConstructorHasCorrectParams(constructorParams, paramsArray)) return classConstructor.Invoke(paramsArray);
                }
            }

            return null;
        }

        /// <summary>
        /// Checks for types of params of constructor. 
        /// </summary>
        /// <returns>Returns true if types are OK. And false if not.</returns>
        private bool ConstructorHasCorrectParams(ParameterInfo[] constructorParams, params Object[] paramsArray)
        {
            for (int i = 0; i < constructorParams.Length; i++)
            {
                if (constructorParams[i].ParameterType != paramsArray[i].GetType()) return false;
            }

            return true;
        }

        #endregion
        #region const
        private const string PluginList = "PluginList";
        private const string Configuration = "Configuration";
        private const string Common = "Common";
        private const string DllPath = "DllPath";
        private const string ClassName = "ClassName";
        private const string PathToConfig = "PathToConfig";
        private const string PluginSpecific = "PluginSpecific";
        public const string Cancel = "Cancel";
        #endregion
    }
}
