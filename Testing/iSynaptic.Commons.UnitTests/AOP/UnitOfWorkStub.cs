﻿using System;
using System.Collections.Generic;
using System.Text;

using iSynaptic.Commons.AOP;

namespace iSynaptic.Commons.AOP
{
    public class UnitOfWorkStub : UnitOfWork<UnitOfWorkStub, object>
    {
        private Action<object> _ProcessHandler = null;

        public UnitOfWorkStub()
        {
        }

        public UnitOfWorkStub(Action<object> processHandler)
        {
            _ProcessHandler = processHandler;
        }

        protected override void Process(ref object item)
        {
            if (_ProcessHandler != null)
                _ProcessHandler(item);
        }

        public List<object> GetItems()
        {
            return Items;
        }
    }
}
