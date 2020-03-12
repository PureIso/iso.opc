using System;
using Iso.Opc.Interface;
using Opc.Ua;

namespace XMLServerNodeManagerPlugin
{
    public class EntryPoint : AbstractApplicationNodeManagerPlugin
    {
        public EntryPoint()
        {
            base.ApplicationName = "XML Server Node Manager";
            base.Author = "Ola";
            base.Description = "XML Plugin Test";
            base.Version = "1.0.0.0";
            base.ResourcePath = "xml_example.xml";
        }

        public override void BindMethod(MethodState methodState)
        {
            Console.WriteLine(methodState.NodeId);
            //methodState.OnCallMethod = OnGeneratedEmptyMethod;
            //methodState.OnCallMethod2 = OnGeneratedEmptyMethod2;
        }
    }
}
