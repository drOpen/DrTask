using DrOpen.DrCommon.DrData;
using DrOpen.DrTask.DrtPlugin;
using System;
using System.Collections.Generic;

namespace DrOpen.DrtTask.FileEnumPlugin
{
    public class FileEnumPlugin : Plugin, IPlugin
    {
        /// <summary>
        /// Plugin specific configuration, containts info of folders and file pattern, that plugin has to handle
        /// </summary>
        private DDNode enumConfig;

        public FileEnumPlugin()
        {
            enumConfig = null;
        }

        /// <summary>
        /// Facade for execution of FileEnumPlugin with given config and data
        /// </summary>
        /// <param name="config">Config should contain info of folders and filename patterns to handle</param>
        /// <param name="nodes">Nodes[0] sould be a sharedData node</param>
        /// <returns></returns>
        public override DDNode Execute(DDNode config, params DDNode[] nodes)
        {
            if (enumConfig == null)
            {
                return InitLaunch(config, nodes);
            }
            else
            {
                return DoExecute(config, nodes);
            }
        }

        /// <summary>
        /// Initializates plugin fields with given config and launches first step of execution which ends up returning info of first appropriate file
        /// </summary>
        /// <param name="config">Config should contain info of folders and filename patterns to handle</param>
        /// <param name="nodes">Nodes[0] sould be a sharedData node</param>
        /// <returns></returns>
        private DDNode InitLaunch(DDNode config, params DDNode[] nodes)
        {
            string path = config.Attributes[FileEnumPlugin.FolderPath];
            string include = config.Attributes[FileEnumPlugin.Include];
            string exclude = config.Attributes[FileEnumPlugin.Exclude];

            return new DDNode();
        }

        /// <summary>
        /// Method resolves multiple occurances of same folders in given config
        /// maybe it's worth it to have list of executed files as a field - that makes this method obsolete
        /// </summary>
        /// <param name="config"></param>
        private void ResolveRecursive(DDNode config)
        {

        }

        /// <summary>
        /// Method that returns info of next appropriate (suited given config) file
        /// </summary>
        /// <param name="config">Config should contain info of folders and filename patterns to handle</param>
        /// <param name="nodes">Nodes[0] sould be a sharedData node</param>
        /// <returns></returns>
        private DDNode DoExecute(DDNode config, params DDNode[] nodes)
        {
            return new DDNode();
        }

        #region const
        private const string FolderPath = "FolderPath";
        private const string Include = "Include";
        private const string Exclude = "Exclude";
        private const string Recurrent = "Recurrent";
        private const string OutputFolder = "OutputFolder";
        #endregion
    }
}
