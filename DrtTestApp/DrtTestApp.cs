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
            //string path = @"C:\Plugins\DrTaskConfig.xml";
            //string path = AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\Debug\\", "") + "DrTaskConfig.xml";
            string xmlName = @"./DrTaskConfig.xml";
            XmlReader xmlFile = XmlReader.Create(xmlName);
            taskNode.ReadXml(xmlFile);

            var manager = new Manager();
            manager.Execute(taskNode);

            Console.WriteLine("");
        }
    }
}
