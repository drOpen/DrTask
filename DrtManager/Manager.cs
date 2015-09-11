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
using DrOpen.DrCommon.DrData;
using System.Reflection;

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
        //private List<IPlugin> pluginList;
        /// <summary>
        /// Collection of loaded plugin objects with specified IDs
        /// </summary>
        private Dictionary<string, IPlugin> taskList;
        private string currentTask;
        private string[] taskOrder;
        /// <summary>
        /// Currently executing plugin. Could be changed by child manager request
        /// </summary>
        //private int currentPlugin;


        public Manager()
        {
            supportedCommands = new List<string>()
            {
                FCommands.Sample
            };

            //pluginList = new List<IPlugin>();
            taskList = new Dictionary<string, IPlugin>();
            currentTask = null;
            //currentPlugin = 0;
        }


        /// <summary>
        /// Checks if the command is supported or not
        /// </summary>
        /// <param name="command"></param>
        /// <returns>This method returns true if given command is supported, false otherwise</returns>
        public bool IsSupportedCommand(string command)
        {
            return supportedCommands.Contains(command);
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
                var beforeExecuteArgs = new DDEventArgs();
                DoBeforeExecute(beforeExecuteArgs);
                if (beforeExecuteArgs.ContainsKey(Manager.Cancel))
                    throw new NotImplementedException();

                //currentPlugin = 0;
                currentTask = null;
                var result = DoExecute(config, nodes);
                DoAfterExecute(new DDEventArgs());
                return result;
            }
            catch (Exception e)
            {
                log.WriteError(e, "Execute failure");
                throw new ApplicationException("Cannot execute Manager", e);
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
            DDNode taskListNode = GetTaskList(config);
            BuildTaskOrder(taskListNode);

            while(currentTask != null)
            {
                IPlugin currentTaskInstance = getNextTask(taskListNode);

                DDNode taskConfig = GetTaskConfig(taskListNode, currentTask);
                currentTaskInstance.Execute(taskConfig);

                setCurrentTask();
            }

            return new DDNode("GoodResult"); // ToDo create stub static positive and negative Execute result and return it
        }


        #region [Execute] supporting methods
        /// <summary>
        /// Returns plugin specific config for task [taskName]
        /// </summary>
        /// <param name="taskListNode">Node with TaskList</param>
        /// <param name="taskName">Name (unique at its level ID) of task</param>
        /// <returns></returns>
        private DDNode GetTaskConfig(DDNode taskListNode, string taskName)
        {
            if (taskListNode.Contains(taskName))
                return taskListNode.GetNode(taskName + "/" + Manager.Configuration + "/" + Manager.PluginSpecific);
            else
                return null; // TODO: to be discussed
        }

        /// <summary>
        /// Returns TaskList if given config contains one
        /// </summary>
        /// <param name="config">Config for manager</param>
        /// <returns></returns>
        private DDNode GetTaskList(DDNode config)
        {
            if (config.Contains(Manager.TaskList))
                return config.GetNode(Manager.TaskList);
            throw new NotImplementedException(); // ToDo handling if there isn't any TaskList node
        }

        /// <summary>
        /// Creates array of task names using the same task order as [taskListNode]
        /// and sets currentPlugin to first in this array
        /// </summary>
        private void BuildTaskOrder(DDNode taskListNode)
        {
            taskOrder = new string[taskListNode.Count];
            int i = 0;
            foreach(var taskNode in taskListNode)
            {
                taskOrder[i] = taskNode.Value.Name;
                i++;
            }
            setCurrentTask(0);
        }

        /// <summary>
        /// Sets [this].[currentTask] to next element in [taskOrder] array
        /// </summary>
        private void setCurrentTask()
        {
            int currentIndex = Array.IndexOf(taskOrder, currentTask);
            if (currentIndex == taskOrder.Length)
                currentTask = null;
            else
                currentTask = taskOrder[currentIndex + 1];
        }

        /// <summary>
        /// Sets [this].[currentTask] to <taskName> value if it exists in taskOrder array
        /// </summary>
        /// <param name="taskName">Name (unique at the level ID) of task</param>
        private void setCurrentTask(string taskName)
        {
            if (Array.IndexOf(taskOrder, taskName) != -1)
                currentTask = taskName;
            else
                currentTask = null; // TODO: to be discussed
        }

        /// <summary>
        /// Sets [this].[currentTask] to [taskOrder][index] if it's not out of range
        /// </summary>
        /// <param name="index">Index of task is [taskOrder] array</param>
        private void setCurrentTask(int index)
        {
            if (index < taskOrder.Length)
                currentTask = taskOrder[index];
            else
                currentTask = null; // TODO: to be discussed
        }

        /// <summary>
        /// Returns [currentTask] plugin object for further execution
        /// If it isn't created yet - creates it first and subscribes for its events
        /// </summary>
        /// <param name="taskListNode"></param>
        /// <returns></returns>
        private IPlugin getNextTask(DDNode taskListNode)
        {
            if(!taskList.ContainsKey(currentTask))
            {
                if (!taskListNode.Contains(currentTask))
                    throw new NotImplementedException(); // TODO: not existant node handling
                DDNode pluginNode = taskListNode.GetNode(currentTask);
                IPlugin newTask = GetPluginObject(pluginNode);

                taskList.Add(currentTask, newTask);
                newTask.BeforeExecute += BeforeExecuteHandler;
                newTask.AfterExecute += AfterExecuteHandler;
                SubscribeToManager(newTask);
            }

            return taskList[currentTask];
        }
        
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
        private const string TaskList = "TaskList";
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
