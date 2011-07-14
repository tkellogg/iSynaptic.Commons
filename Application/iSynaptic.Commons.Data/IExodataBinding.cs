﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace iSynaptic.Commons.Data
{
    public interface IExodataBinding
    {
        bool Matches<TExodata, TContext, TSubject>(IExodataRequest<TExodata, TContext, TSubject> request);
        TExodata Resolve<TExodata, TContext, TSubject>(IExodataRequest<TExodata, TContext, TSubject> request);
    }
}
