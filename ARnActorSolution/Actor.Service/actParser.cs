﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Actor.Base ;
using Actor.Server ;

namespace Actor.Service
{

    public class bhvReceiveLine : bhvBehavior<Tuple<IActor, string>>
    {
        public bhvReceiveLine() : base()
        {
            Pattern = t => t is Tuple<IActor,String> ;
            Apply = t =>
                {
                    IActor parser = new actStringParser();
                    parser.SendMessage(t);
                };
        }
    }

    public class actStringParser : BaseActor
    {
        public actStringParser()
            : base()
        {
            Become(new bhvBehavior<Tuple<IActor,string>>(
                t => 
                    {
                        char[] chr = {' '} ;
                        var stringtoparse = t.Item2.Trim().Split(chr) ;
                        foreach (string s in stringtoparse)
                        {
                            t.Item1.SendMessage(new Tuple<IActor,string>(this,s));
                        }
                    } 
                )) ;
        }
    }

    public class actParserServer : BaseActor
    {
        public actParserServer() : base()
        {
            Become(new bhvReceiveLine()) ;
        }
    }

    public class actParser : BaseActor
    {
        private List<String> fList = new List<String>();
        private actTag fParserServer ;
        public actParser()
            : base()
        {
            var collect = new bhvBehavior<Tuple<IActor,string>>(t =>
                {
                    fList.Add(t.Item2) ;
                    Console.WriteLine("parsed "+t.Item2);
                }
                );
            var connect = new bhvBehavior<actTag>(t =>
                {
                    fParserServer = t;
                }
                );
            var parse = new bhvBehavior<IEnumerable<string>>(t =>
                {
                    IActor aServer = null;
                    if (fParserServer == null)
                    {
                        aServer = new actParserServer();
                        fParserServer = aServer.Tag;
                    }
                    else
                    {
                        aServer = new actConnect(this, fParserServer.Uri, "ParserServer");
                    }
                    foreach (string s in t)
                    {
                        aServer.SendMessage(new Tuple<IActor, string>(this, s));
                    }
                }
            );
            Become(collect);
            AddBehavior(connect);
            AddBehavior(parse);
        }
    }
}
