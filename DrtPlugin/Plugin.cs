/*
  Plugin.cs -- abstract base Plugin class 1.0.0, August 30, 2015
 
  Copyright (c) 2013-2015 Kudryashov Andrey aka Dr
 
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

 */

using DrOpen.DrCommon.DrData;
using DrOpen.DrCommon.DrLog.DrLogClient;
using System;

namespace DrOpen.DrTask.DrtPlugin
{
    /// <summary>
    /// Abstract base Plugin class
    /// </summary>
    public abstract class Plugin : IPlugin
    {
        /// <summary>
        /// Access to single tone Logger object
        /// </summary>
        protected Logger log = Logger.GetInstance;
        /// <summary>
        /// Raise event before execute plugin
        /// </summary>
        public event DDEventHandler BeforeExecute;
        /// <summary>
        /// Raise event after execute plugin
        /// </summary>
        public event DDEventHandler AfterExecute;
        /// <summary>
        /// Invokes execution of plugin with given config and additional data
        /// </summary>
        /// <param name="config"></param>
        /// <param name="nodes"></param>
        /// <returns>This method returns result as Data abstraction layer</returns>
        public abstract DDNode Execute(DDNode config, params DDNode[] nodes);
        /// <summary>
        /// Facade for raise event before execute plugin
        /// </summary>
        /// <param name="eventArgs">arguments</param>
        public virtual void DoBeforeExecute(DDEventArgs eventArgs)
        {
            try
            {
                if (BeforeExecute != null) BeforeExecute(this, eventArgs);
            }
            catch (Exception e)
            {
                log.WriteError(e, Res.Msg.CANNOT_RAISE_EVENT, "BeforeExecute");
            }
        }
        /// <summary>
        /// Facade for raise event after execute plugin
        /// </summary>
        /// <param name="eventArgs">arguments</param>
        public virtual void DoAfterExecute(DDEventArgs eventArgs)
        {
            try
            {
                if (AfterExecute != null)
                    AfterExecute(this, eventArgs);
            }
            catch (Exception e)
            {
                log.WriteError(e, Res.Msg.CANNOT_RAISE_EVENT, "AfterExecute");
            }
        }

        #region ResultNodes
        /// <summary>
        /// Generates node that indicate successfull execution 
        /// </summary>
        /// <param name="info">Optional information node</param>
        /// <returns>Node "ExecutionResult"[Attributes:Success=true,IsConpleted=true] with child node [info]</returns>
        public static DDNode GoodResult(DDNode info = null)
        {
            return ExecutionResult(info, true, true);
        }

        /// <summary>
        /// Generates node that indicate failed execution
        /// </summary>
        /// <param name="info">Optional information node</param>
        /// <returns>Node "ExecutionResult"[Attributes:Success=false,IsConpleted=false] with child node [info]</returns>
        public static DDNode BadResult(DDNode info = null)
        {
            return ExecutionResult(info, false, false);
        }

        /// <summary>
        /// Generates "ExecutionResult" node with specified attributes
        /// </summary>
        /// <param name="info">Optional information node</param>
        /// <param name="success">Indicates whether there was failed or successfull execution</param>
        /// <param name="isCompleted">[True] indicates that plugin completed it's job and doesn't need to be executed again</param>
        /// <returns>Node "ExecutionResult"[Attributes:Success=[success],IsConpleted=[isCompleted]] with child node [info]</returns>
        public static DDNode ExecutionResult(DDNode info, bool success, bool isCompleted)
        {
            var resultNode = new DDNode("ExecutionResult");
            if (info != null)
                resultNode.Add(info);
            resultNode.Attributes.Add("Success", success);
            resultNode.Attributes.Add("IsCompleted", isCompleted);

            return resultNode;
        }
        #endregion

    }
}