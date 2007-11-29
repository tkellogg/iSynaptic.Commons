﻿using System;
using System.Collections.Generic;
using System.Text;

namespace iSynaptic.Commons
{
    public static class ActionExtensions
    {
        public static Action Curry<T1>(this Action<T1> f, T1 arg1)
        {
            return () => f(arg1);
        }

        public static Action Curry<T1, T2>(this Action<T1, T2> f, T1 arg1, T2 arg2)
        {
            return () => f(arg1, arg2);
        }

        public static Action<T2> Curry<T1, T2>(this Action<T1, T2> f, T1 arg1)
        {
            return t2 => f(arg1, t2);
        }

        public static Action Curry<T1, T2, T3>(this Action<T1, T2, T3> f, T1 arg1, T2 arg2, T3 arg3)
        {
            return () => f(arg1, arg2, arg3);
        }

        public static Action<T3> Curry<T1, T2, T3>(this Action<T1, T2, T3> f, T1 arg1, T2 arg2)
        {
            return t3 => f(arg1, arg2, t3);
        }

        public static Action<T2, T3> Curry<T1, T2, T3>(this Action<T1, T2, T3> f, T1 arg1)
        {
            return (t2, t3) => f(arg1, t2, t3);
        }

        public static Action Curry<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> f, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return () => f(arg1, arg2, arg3, arg4);
        }

        public static Action<T4> Curry<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> f, T1 arg1, T2 arg2, T3 arg3)
        {
            return t4 => f(arg1, arg2, arg3, t4);
        }

        public static Action<T3, T4> Curry<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> f, T1 arg1, T2 arg2)
        {
            return (t3, t4) => f(arg1, arg2, t3, t4);
        }

        public static Action<T2, T3, T4> Curry<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> f, T1 arg1)
        {
            return (t2, t3, t4) => f(arg1, t2, t3, t4);
        }
    }
}
