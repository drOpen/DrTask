using DrOpen.DrCommon.DrData;
using DrOpen.DrTask.DrtManager;
using System;
using System.Xml;

namespace DrtTestApp
{
    class DrtTestApp
    {
        static void Main(string[] args)
        {
            DDNode taskNode = new DDNode();
            string path = @"C:\Plugins\DrTaskConfig.xml";
            //var currentFolder = AppDomain.CurrentDomain.BaseDirectory;
            XmlReader xmlFile = XmlReader.Create(path);
            taskNode.ReadXml(xmlFile);

            var manager = new Manager();
            manager.Execute(taskNode.GetNode("Tasks/Task1"));

            Console.WriteLine("");
        }
    }
}
