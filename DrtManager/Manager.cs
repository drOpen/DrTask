using DrOpen.DrCommon.DrData;
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
        public event EventHandler CallParent;

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
            currentPlugin = 0;
        }


        /// <summary>
        /// Checks if the command is supported or not
        /// </summary>
        /// <param name="command"></param>
        /// <returns>This method returns true if given command is supported, false otherwise</returns>
        public bool IsSupportedCommand(string command)
        {
            try
            {
                return supportedCommands.Contains(command);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Invokes execution of manager with given config and additional data
        /// </summary>
        /// <param name="config">config should contain a node with list of plugin for proper execution</param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public override DDNode Execute(DDNode config, params DDNode[] nodes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Facade for raise event of calling up for parent manager
        /// </summary>
        /// <param name="eventArgs"></param>
        public void DoCallParent(DrtEventArgs eventArgs)
        {
            try
            {
                if (CallParent != null)
                    CallParent(this, eventArgs);

                Console.WriteLine("\tCall up started.");
            }
            catch (Exception e)
            {
            }
        }




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
                throw new ApplicationException(e.Message);
            }

            throw new ApplicationException("There are no class|dll|construcotor...");
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
    }
}
// test comment 1001