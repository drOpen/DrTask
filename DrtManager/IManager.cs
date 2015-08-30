/*
  IManager.cs -- Interface describes the base manager for executing plugins 1.0.0, August 30, 2015
 
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
using DrOpen.DrTask.DrtPlugin;
using System;

namespace DrOpen.DrTask.DrtManager
{
    /// <summary>
    /// Interface describes the base manager for executing plugins
    /// </summary>
    public interface IManager : IPlugin
    {
        /// <summary>
        /// event to interact with the parent manager
        /// </summary>
        event EventHandler CallParent;
    }
}
