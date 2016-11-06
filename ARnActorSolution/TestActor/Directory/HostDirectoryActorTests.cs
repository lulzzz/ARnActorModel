﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Actor.Server;
using System;
using Actor.Util;
using System.Configuration;

namespace TestActor
{
    [TestClass()]
    public class HostDirectoryActorTests
    {

        [TestMethod()]
        public void GetStatTest()
        {
            var stat = HostDirectoryActor.GetInstance().GetStat();
            Assert.IsNotNull(stat);
            Assert.IsTrue(stat.Contains("Host entries"));
        }

        
        [TestMethod()]
        public void RegisterUnregisterTest()
        {
            TestLauncherActor.Test(() =>
            {
                ConfigurationManager.AppSettings["ListenerService"] = "MemoryListenerService";
                ConfigurationManager.AppSettings["SerializeService"] = "NetDataContractSerializeService";
                ActorServer.Start("localhost", 80, new HostRelayActor());
                var actor = new StateFullActor<string>();
                HostDirectoryActor.Register(actor);
                SerialObject so = new SerialObject(Tuple.Create(StateAction.Set,"Test"), actor.Tag);
                HostDirectoryActor.GetInstance().SendMessage(so);
                var result = actor.GetAsync(10000).Result;
                Assert.AreEqual(result, "Test");

                HostDirectoryActor.Unregister(actor);
                SerialObject so2 = new SerialObject(Tuple.Create(StateAction.Set, "Test2"), actor.Tag);
                HostDirectoryActor.GetInstance().SendMessage(so2);
                var result2 = actor.GetAsync(10000).Result;
                Assert.AreEqual("Test",result2,string.Format("Expected {0} Found {1}","Test",result2));
            });
        }
        
    }
}