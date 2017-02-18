﻿using System;
using System.Collections.Generic;
using System.Text;
using Actor.Base;
using System.Linq;
using System.Diagnostics;
using System.Globalization;

namespace Actor.Util
{

    public class PeerBehaviors<K, V> : Behaviors, INodeBehavior<K, V>
    {
        internal Dictionary<K, V> Nodes = new Dictionary<K, V>();
        internal Dictionary<HashKey, IPeerActor<K,V>> Peers = new Dictionary<HashKey, IPeerActor<K,V>>();
        private HashKey fCurrentPeer;
        internal HashKey CurrentPeer
        {
            get
            {
                if (fCurrentPeer == null)
                {
                    fCurrentPeer = HashKey.ComputeHash(LinkedActor.Tag.Key());
                }
                return fCurrentPeer;
            }
        }
        public PeerBehaviors() : base()
        {
            BecomeBehavior(new PeerStoreNode<K, V>())
                .AddBehavior(new PeerGetNode<K, V>())
                .AddBehavior(new PeerFindPeer<K, V>())
                .AddBehavior(new PeerNewPeer<K, V>())
                .AddBehavior(new PeerDeleteNode<K, V>())
                .AddBehavior(new Behavior<IFuture<HashKey>>(f => f.SendMessage(this.CurrentPeer)))
                .AddBehavior(new Behavior<IFuture<IEnumerable<K>>>())// AskKeys();
                .AddBehavior(new Behavior<IFuture<IEnumerable<Tuple<HashKey, IActor>>>>()); // AskNodes();
        }

        public void DeleteNode(K k)
        {
            LinkedActor.SendMessage("PeerDeleteNode", k);
        }

        public void GetNode(K k, IActor actor)
        {
            LinkedActor.SendMessage("PeerGetNode", k, actor);
        }

        public IFuture<HashKey> GetHashKey()
        {
            var future = new Future<HashKey>();
            LinkedActor.SendMessage(future);
            return future;
        }

        public void StoreNode(K k, V v)
        {
            LinkedActor.SendMessage("PeerStoreNode", k, v);
        }

    }

    public class PeerDeleteNode<K, V> : Behavior<string, K>
    {
        public PeerDeleteNode() : base()
        {
            this.Pattern = (s, k) => s == "PeerDeleteNode";
            this.Apply = (s, k) => (LinkedTo as PeerBehaviors<K, V>).Nodes.Remove(k);
        }
    }

    public class PeerStoreNode<K, V> : Behavior<string, K, V>
    {
        public PeerStoreNode() : base()
        {
            this.Pattern = (s, k, v) => s == "PeerStoreNode";
            this.Apply = (s, k, v) =>
            {
                (LinkedTo as PeerBehaviors<K, V>).Nodes[k] = v;
                Debug.WriteLine(string.Format("New node in : {0}", (LinkedTo as PeerBehaviors<K, V>).CurrentPeer.ToString()), "PeerBehavior");
            };
        }
    }

    public class PeerGetNode<K, V> : Behavior<string, K, IActor>
    {
        public PeerGetNode() : base()
        {
            this.Pattern = (s, k, i) => s == "PeerGetNode";
            this.Apply = (s, k, i) => i.SendMessage((LinkedTo as PeerBehaviors<K, V>).Nodes[k]);
        }
    }

    public class PeerNewPeer<K, V> : Behavior<string, IPeerActor<K,V>, HashKey>
    {
        public PeerNewPeer() : base()
        {
            this.Pattern = (s, i, h) => s == "PeerNewPeer";
            this.Apply = (s, i, h) =>
            {
                Debug.WriteLine(String.Format(CultureInfo.InvariantCulture,"New peer : {0}", h.ToString()),"PeerBehavior");
                (LinkedTo as PeerBehaviors<K, V>).Peers[h] = i;
            };
        }
    }

    public class PeerFindPeer<K, V> : Behavior<string, K, IFuture<IPeerActor<K, V>>>
    {
        public PeerFindPeer() : base()
        {
            this.Pattern = (s, k, i) => s == "PeerFindPeer";
            this.Apply = (s, k, i) =>
            {
                var key = HashKey.ComputeHash(k.ToString());
                var current = (LinkedTo as PeerBehaviors<K, V>).CurrentPeer;
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,"Search Key {0} in {1}", key.ToString(), current.ToString()) , "PeerBehavior");
                if (key.CompareTo(current) <= 0)
                {
                    // Store here
                    i.SendMessage(LinkedActor as IPeerActor<K,V>);
                }
                else
                {
                    // find best peer
                    var nextPeer = (LinkedTo as PeerBehaviors<K, V>).Peers
                      .Where(n => n.Key.CompareTo(current) > 0).OrderBy(n => n.Key).FirstOrDefault();
                    if (nextPeer.Key != null)
                    {
                        nextPeer.Value.SendMessage(s, k, i);
                    }
                    else
                    {
                        i.SendMessage(LinkedActor as IPeerActor<K, V>);
                    }

                }
            };
        }
    }

    public interface IPeerBehavior<K,V>
    {
        void FindPeer(K k, IFuture<IPeerActor<K,V>> actor);
        void NewPeer(IPeerActor<K,V> actor, HashKey hash);
    }

    public interface INodeBehavior<K, V>
    {
        void StoreNode(K k, V v);
        void GetNode(K k, IActor actor);
        void DeleteNode(K k);
        IFuture<HashKey> GetHashKey();
    }

    public interface IAgentBehavior<K>
    {
        IFuture<IEnumerable<K>> AskKeys();
        IFuture<IEnumerable<Tuple<HashKey,IActor>>> AskPeers();
    }

    public interface IPeerActor<K,V> : IActor, IPeerBehavior<K,V>, IAgentBehavior<K>, INodeBehavior<K, V>
    { }

    public class PeerActor<K, V> : BaseActor, IPeerActor<K,V>, INodeBehavior<K, V>
    {
        private INodeBehavior<K, V> fNodeBehaviorService;
        public PeerActor() : base()
        {
            var bhv = new PeerBehaviors<K, V>();
            fNodeBehaviorService = bhv;
            Become(bhv);
        }

        public void FindPeer(K k, IFuture<IPeerActor<K,V>> actor)
        {
            this.SendMessage("PeerFindPeer", k, actor);
        }
        public void NewPeer(IPeerActor<K,V> actor, HashKey hash)
        {
            this.SendMessage("PeerNewPeer", actor, hash);
        }


        public IFuture<IEnumerable<K>> AskKeys()
        {
            var future = new Future<IEnumerable<K>>();
            this.SendMessage("AgentAskKeys",future);
            return future;
        }

        public IFuture<IEnumerable<Tuple<HashKey, IActor>>> AskPeers()
        {
            var future = new Future<IEnumerable<Tuple<HashKey, IActor>>>();
            this.SendMessage("AgentAskNodes",future);
            return future;
        }

        public void StoreNode(K k, V v)
        {
            fNodeBehaviorService.StoreNode(k, v);
        }

        public void GetNode(K k, IActor actor)
        {
            fNodeBehaviorService.GetNode(k, actor);
        }

        public void DeleteNode(K k)
        {
            fNodeBehaviorService.DeleteNode(k);
        }

        public IFuture<HashKey> GetHashKey()
        {
            return fNodeBehaviorService.GetHashKey();
        }
        // kv behaviors
        // store kv
        // searchK
        // delete kv
        // nodelist
        // receive new node
        // check for node existence

        // using node list order by key
        // k xor on known value >= current hash ==> goto next node
        // else store here

        // new node => add to list
        // agent find abnormal key (not in 3 middle value)
        // go to next node with key
    }

}