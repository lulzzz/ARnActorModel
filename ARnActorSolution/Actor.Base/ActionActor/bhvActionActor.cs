﻿/*****************************************************************************
		               ARnActor Actor Model Library .Net
     
	 Copyright (C) {2015}  {ARn/SyndARn} 
 
 
     This program is free software; you can redistribute it and/or modify 
     it under the terms of the GNU General Public License as published by 
     the Free Software Foundation; either version 2 of the License, or 
     (at your option) any later version. 
 
 
     This program is distributed in the hope that it will be useful, 
     but WITHOUT ANY WARRANTY; without even the implied warranty of 
     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
     GNU General Public License for more details. 
 
 
     You should have received a copy of the GNU General Public License along 
     with this program; if not, write to the Free Software Foundation, Inc., 
     51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA. 
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Actor.Base ;

namespace Actor.Base
{

    /// <summary>
    /// bhvAction
    ///     this behavior allows to pass an action as behavior to an actor
    ///     Most frequent use : public method to send an async action to the same actor
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "bhv")]
    public class bhvAction : bhvBehavior<Action>
    {
        public bhvAction()
            : base()
        {
            Pattern = t => { return t is Action ;} ;
            Apply = t => t.Invoke() ;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "bhv")]
    public class bhvAction<T> : bhvBehavior<Tuple<Action<T>, T>>
    {
        public bhvAction()
            : base()
        {
            Pattern = t => { return t is Tuple<Action<T>, T> ; };
            Apply = t => { t.Item1.Invoke(t.Item2); };
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "bhv")]
    public class bhvAction<T1,T2> : bhvBehavior<Tuple<Action<T1,T2>, T1,T2>>
    {
        public bhvAction()
            : base()
        {
            Pattern = t => { return t is Tuple<Action<T1,T2>, T1,T2>; };
            Apply = t => { t.Item1.Invoke(t.Item2,t.Item3); };
        }
    }

    /// <summary>
    /// actActionActor
    ///     Action actor are a facility : they provide template to send method as message within an actor
    ///     e.g. SendMessageTo(() => {do something},anActor) ;
    /// </summary>
    public class ActionActor : actActor
    {
        public ActionActor()
            : base()
        {
            Become(new bhvAction());
        }
        public void SendAction(Action anAction)
        {
            SendMessage(anAction);
        }
    }

    /// <summary>
    /// actActionActor
    ///   Action actor with the added type parameter if needed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "act")]
    public class actAction<T> : actActor
    {
        public actAction()
            : base()
        {
            Behaviors bhvs = new Behaviors();
            bhvs.AddBehavior(new bhvAction());
            bhvs.AddBehavior(new bhvAction<T>());
            BecomeMany(bhvs);
        }

        public void SendAction(Action anAction)
        {
            SendMessage(anAction);
        }

        public void SendAction(Action<T> anAction, T aT)
        {
            SendMessage(Tuple.Create(anAction, aT));
        }

    }

}
