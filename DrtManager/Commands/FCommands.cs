/*
  FCommands.cs -- factory of commands 1.0.0, August 30, 2015
 
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

using DrOpen.DrCommon.DrLog.DrLogClient;
using System;
using System.Collections.Generic;
namespace DrOpen.DrTask.DrtManager.Commands
{
    /// <summary>
    /// factory of commands
    /// </summary>
    public static class FCommands
    {
        static FCommands()
        {
            cmd = new Dictionary<string, Type> 
            { 
                { Sample, typeof(CStub) },
            };
        }

        #region Commands name
        public const string Sample = "Stub";
        #endregion

        static readonly Dictionary<string, Type> cmd;

        public static ICommand GetCommand(string name)
        {
            Logger.GetInstance.WriteDebug(Res.Msg.START_CREATE_OBJECT_OF_COMMAND_MANAGER, name);
            try
            {
                return (ICommand)Activator.CreateInstance(cmd[name]);
            }
            catch(Exception e)
            {
                Logger.GetInstance.WriteError(e,Res.Msg.CANNOT_CREATE_OBJECT_OF_COMMAND_MANAGER, name);
                return null;
            }
        }


    }
}
