﻿using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Actor.Base;

namespace TestActor
{
    /// <summary>
    /// Description résumée pour QueueFactoryTest
    /// </summary>
    [TestClass]
    public class QueueFactoryTest
    {
        [TestMethod]
        public void LockFreeCastingTest()
        {
            var factory = new QueueFactory<string>
            {
                Style = QueueStyle.LockFree
            };
            var lockFree = factory.GetQueue();
            Assert.IsNotNull(lockFree);
            Assert.IsTrue(lockFree is LockFreeQueue<string>);
            lockFree.Add("Test1");
            lockFree.Add("Test2");
            Assert.AreEqual(2, lockFree.Count());
            var result = lockFree.TryTake(out string s);
            Assert.IsTrue(result);
            Assert.AreEqual("Test1",s) ;
        }

        [TestMethod]
        public void LockingCastingTest()
        {
            var factory = new QueueFactory<string>
            {
                Style = QueueStyle.Locking
            };
            var locking = factory.GetQueue();
            Assert.IsNotNull(locking);
            Assert.IsTrue(locking is LockQueue<string>);
            locking.Add("Test1");
            locking.Add("Test2");
            Assert.AreEqual(2, locking.Count());
            var result = locking.TryTake(out string s);
            Assert.IsTrue(result);
            Assert.AreEqual("Test1", s);
        }

        [TestMethod]
        public void RingQueueCastingTest()
        {
            var factory = new QueueFactory<string>
            {
                Style = QueueStyle.Ring
            };
            IMessageQueue<string> messageQueue = factory.GetQueue();
            Assert.IsNotNull(messageQueue);
            Assert.IsTrue(messageQueue is RingQueue<string>);
            messageQueue.Add("Test1");
            messageQueue.Add("Test2");
            Assert.AreEqual(2, messageQueue.Count());
            var result = messageQueue.TryTake(out string s);
            Assert.IsTrue(result);
            Assert.AreEqual("Test1", s);
        }
    }
}
