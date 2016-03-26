﻿using Actor.Base;
using Actor.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TestActor
{
    [TestClass]
    public class SerializationTest
    {

        public class Test1 : ActionActor
        {
            public string Name { get; set; }
        }

        private class actRemoteActor : actActor 
        {
            public actTag remoteTag {get ;set ;}
        }

        private class ActorBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type outtype = null;
                Type typefound = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
                if (typefound != null)
                {
                    if (typefound.IsSubclassOf(typeof(actActor)))
                    {
                        outtype = typeof(actRemoteActor);
                    }
                }
                return outtype;
            }
        }

        private class ActorSurrogator : ISerializationSurrogate
        {
            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                actRemoteActor remote = new actRemoteActor();
                remote.remoteTag = ((IActor)obj).Tag;
                info.SetType(typeof(actRemoteActor));
                info.AddValue("remoteTag", remote.remoteTag, typeof(actTag));
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                // Reset the property value using the GetValue method.
                typeof(actRemoteActor).GetProperty("remoteTag").SetValue(obj, info.GetValue("remoteTag", typeof(actTag)));
                return null; // ms bug here
            }
        }

        private class ActorSurrogatorSelector : SurrogateSelector 
        {
            public ActorSurrogatorSelector()
                : base()
            {
            }
            
            public override ISerializationSurrogate GetSurrogate(
    Type type,
    StreamingContext context,
    out ISurrogateSelector selector
)
            {
                

                if (type.IsSubclassOf(typeof(actActor)))
                {
                    Debug.WriteLine("push actor {0} to host directory", type);
                    selector = this;
                    return new ActorSurrogator();
                }
                else
                    return base.GetSurrogate(type, context, out selector);

            }
        }

        [TestMethod]
        public void TestSerializeActor()
        {
            //
            var tst1 = new Test1() { Name = "TestName1" };
            var tst2 = new Test1() { Name = "TestName2" };
            var lst = new List<IActor>();
            lst.Add(tst1);
            lst.Add(tst2);

            // serialize
            SerialObject so = new SerialObject();
            so.Data = lst;
            so.Tag = new actTag("test uri") ;
            NetDataContractSerializer dcs = new NetDataContractSerializer();
            dcs.SurrogateSelector = new ActorSurrogatorSelector();
            dcs.Binder = new ActorBinder();

            using (MemoryStream ms = new MemoryStream())
            {
                dcs.Serialize(ms, so);

                // display
                ms.Seek(0, SeekOrigin.Begin);
                StreamReader sr = new StreamReader(ms);
                while (!sr.EndOfStream)
                    Debug.Print(sr.ReadLine());

                // deserialize
                ms.Seek(0, SeekOrigin.Begin);
                Object obj = dcs.ReadObject(ms);
                SerialObject soread = (SerialObject)obj;

                // test
                var lst2 = (List<IActor>)soread.Data;

                Assert.AreEqual(2, lst2.Count);
                var l1 = (actRemoteActor)(lst2.First());
                var l2 = (actRemoteActor)(lst2.Last());

                Assert.AreEqual(so.Tag.Id, soread.Tag.Id);

                Assert.AreEqual(tst1.Tag.Id, l1.remoteTag.Id);
                Assert.AreEqual(tst2.Tag.Id, l2.remoteTag.Id);
            }
        }
    }
}
