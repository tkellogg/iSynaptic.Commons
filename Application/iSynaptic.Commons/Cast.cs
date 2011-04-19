﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSynaptic.Commons
{
    public static class Cast<TSource, TDestination>
    {
        public static TDestination With(TSource source)
        {
            return IL.Cast<TSource, TDestination>.With(source);
        }
    }
}
