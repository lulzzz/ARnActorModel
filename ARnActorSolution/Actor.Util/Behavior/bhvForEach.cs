﻿using Actor.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actor.Util
{


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "bhv")]
    public class bhvForEach<T> : bhvBehavior<Tuple<IEnumerable<T> , Action<T>>>
    {
        public bhvForEach() : base()
        {
            this.Pattern = t => { return true; };
            this.Apply = ForEach ;
        }

        private void ForEach(Tuple<IEnumerable<T> , Action<T>> msg)
        {
            foreach(T act in msg.Item1)
            {
                new BaseActor(new bhvDoForEach<T>()).SendMessage(Tuple.Create(act,msg.Item2)) ;
            }
        }
    }


        internal class bhvDoForEach<T> : bhvBehavior<Tuple<T,Action<T>>>
        {
            public bhvDoForEach()
            {
                this.Pattern = t => { return true ;} ;
                this.Apply = DoEach ;
            }
            private void DoEach(Tuple<T,Action<T>> msg)
            {
                msg.Item2(msg.Item1);
            }
        }
}

