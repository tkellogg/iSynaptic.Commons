﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSynaptic.Commons.Data
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DescriptionAttribute : Attribute, IExodataAttribute<string, string>
    {
        private readonly string _Description;

        public DescriptionAttribute(string description)
        {
            _Description = description;
        }

        public bool ProvidesExodataFor<TExodata, TContext, TSubject>(IExodataRequest<TExodata, TContext, TSubject> request)
        {
            return request.Declaration == CommonExodata.Description;
        }

        public string Resolve<TContext, TSubject>(IExodataRequest<string, TContext, TSubject> request)
        {
            return _Description;
        }
    }
}
