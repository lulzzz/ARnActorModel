﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Actor.Base;
using System.Collections.Concurrent;

namespace Actor.Server
{
    public class ActorFactory : IActorFactory
    {
        public IActor CastNewActor(string actorAddress)
        {
            var actor = new BaseActor(); // and force tag
            return actor;
        }
    }

    public interface IActorFactory
    {
        IActor CastNewActor(string actorAddress);
    }

    public class ActorAddressFactory
    {
        public static long HashAddress(string anAddress)
        {
            CheckArg.Address(anAddress);
            var hash = anAddress.GetHashCode();
            return hash;
        }

        private readonly IActorFactory fActorFactory;
        private readonly ConcurrentDictionary<string, IActor> fDico = new ConcurrentDictionary<string, IActor>() ;

        public ActorAddressFactory(IActorFactory actorFactory)
        {
            fActorFactory = actorFactory;
        }

        public IActor GetActor(string actorAddress)
        {
            return fDico[actorAddress];
        }

        public void CreateActorAddress(string actorAddress)
        {
            fDico[actorAddress] = fActorFactory.CastNewActor(actorAddress);
        }
    }
}
