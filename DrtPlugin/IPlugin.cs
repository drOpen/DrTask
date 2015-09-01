/*
  IPlugin.cs -- Interface describes the base Plugin class 1.0.0, August 30, 2015
 
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
using System;

namespace DrOpen.DrTask.DrtPlugin
{
    /// <summary>
    /// Interface describes the base Plugin class
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// raise event before execute plugin
        /// </summary>
        event EventHandler BeforeExecute;
        /// <summary>
        /// raise event after execute plugin
        /// </summary>
        event EventHandler AfterExecute;
        /// <summary>
        /// Invokes execution of plugin with given config and additional data
        /// </summary>
        /// <param name="config"></param>
        /// <param name="nodes"></param>
        /// <returns>This method returns result as Data abstraction layer</returns>
        DDNode Execute(DDNode config, params DDNode[] nodes);
        void DoBeforeExecute(DDEventArgs beforeEventArgs);
        void DoAfterExecute(DDEventArgs afterEventArgs);
    }
}
