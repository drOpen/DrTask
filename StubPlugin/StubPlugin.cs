using System;
using System.Collections.Generic;
using DrOpen.DrCommon.DrData;
using System.IO;
using System.Xml.Serialization;
using DrOpen.DrTask.DrtPlugin;

namespace StubPlugin
{
    public class StubPlugin : Plugin, IPlugin
    {
        public override DDNode Execute(DDNode config, params DDNode[] nodes)
        {
            return Plugin.GoodResult(new DDNode("Stub")); ;
        }
        /*
        DDNode stubEnum;
        IEnumerator<KeyValuePair<string, DDNode>> enumEnumerator;
        //List<DDNode> stubNodeList;
        //List<DDNode>.Enumerator listEnumerator;
        

        public StubPlugin()
        {
            stubEnum = null;
            //stubNodeList = new List<DDNode>();

            if (Enumer.Gener().TryGetNode("PluginList", out stubEnum))
                enumEnumerator = stubEnum.GetEnumerator();
            else
                stubEnum = ExecutionResult(null, false, false);
        }

        public override DDNode Execute(DDNode config, params DDNode[] nodes)
        {
            Console.WriteLine("StubPlugIn executed succesfully.");

            var result = new DDNode("Result");
            var sharedData = result.Add("SharedData");
            var isOver = false;
            DDNode shared = GetNewDDNode();
            if (shared == null)
                isOver = true;
            else
                sharedData.Add(shared.Clone(true));
            var status = result.Add("Status");
            status.Add(ExecutionResult(null, false, isOver));

            return result;
        }

        private DDNode GetNewDDNode()
        {
            enumEnumerator.MoveNext();
            return enumEnumerator.Current.Value;
            //listEnumerator.MoveNext();
            //return listEnumerator.Current;
        }

        private DDNode Result(bool completed = true)
        {
            DDNode resultNode = new DDNode("ExecutionResult");
            resultNode.Attributes.Add("Error", false);
            resultNode.Attributes.Add("Completed", completed);
            return resultNode;
        }

        private DDNode BadResult()
        {
            DDNode resultNode = new DDNode("ExecutionResult");
            resultNode.Attributes.Add("Error", true);
            resultNode.Attributes.Add("Completed", false);
            return resultNode;
        }
        */
    }


    //public static class Enumer
    //{

    //    public static DDNode Gener()
    //    {
    //        string path = @"C:\testConfig\enum.xml";

    //        XmlSerializer serializer = new XmlSerializer(typeof(DDNode));

    //        StreamReader reader = new StreamReader(path);
    //        DDNode stubEnumConfig = (DDNode)serializer.Deserialize(reader);
    //        reader.Close();

    //        return stubEnumConfig;
    //    }
    //}
}
