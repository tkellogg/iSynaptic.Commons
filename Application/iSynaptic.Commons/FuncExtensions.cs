﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace iSynaptic.Commons
{
    public static partial class FuncExtensions
    {
        public static Action ToAction<TRet>(this Func<TRet> self)
        {
            Guard.NotNull(self, "self");
            return () => self();
        }

        public static Func<T, bool> And<T>(this Func<T, bool> self, Func<T, bool> right)
        {
            Guard.NotNull(self, "self");
            Guard.NotNull(right, "right");

            return input => self(input) && right(input);
        }

        public static Func<T, bool> Or<T>(this Func<T, bool> self, Func<T, bool> right)
        {
            Guard.NotNull(self, "self");
            Guard.NotNull(right, "right");
            
            return input => self(input) || right(input);
        }

        public static Func<T, bool> XOr<T>(this Func<T, bool> self, Func<T, bool> right)
        {
            Guard.NotNull(self, "self");
            Guard.NotNull(right, "right");

            return input => self(input) ^ right(input);
        }

        public static IComparer<T> ToComparer<T>(this Func<T, T, int> self)
        {
            Guard.NotNull(self, "self");
            return new FuncComparer<T>(self);
        }

        public static Func<TResult> Memoize<TResult>(this Func<TResult> self)
        {
            return Memoize(self, false);
        }

        public static Func<TResult> Memoize<TResult>(this Func<TResult> self, bool threadSafe)
        {
            Guard.NotNull(self, "self");
            
            TResult result = default(TResult);
            Exception exception = null;
            bool executed = false;

            Func<TResult> memoizedFunc = () =>
            {
                if (!executed)
                {
                    try
                    {
                        result = self();
                    }
                    catch(Exception ex)
                    {
                        exception = ex;
                        throw;
                    }
                    finally
                    {
                        executed = true;
                    }
                }

                if (exception != null)
                    exception.Rethrow();

                return result;
            };

            return threadSafe
                       ? memoizedFunc.Synchronize(() => !executed)
                       : memoizedFunc;
        }

        public static Func<TResult> Synchronize<TResult>(this Func<TResult> self)
        {
            return self.Synchronize(() => true);
        }

        public static Func<TResult> Synchronize<TResult>(this Func<TResult> self, Func<bool> needsSynchronizationPredicate)
        {
            Guard.NotNull(self, "self");
            Guard.NotNull(needsSynchronizationPredicate, "needsSynchronizationPredicate");

            var lockObject = new object();

            return () =>
            {
                if(needsSynchronizationPredicate())
                {
                    lock (lockObject)
                    {
                        return self();
                    }
                }

                return self();
            };
        }

        private class FuncComparer<T> : IComparer<T>
        {
            private readonly Func<T, T, int> _Strategy;

            public FuncComparer(Func<T, T, int> strategy)
            {
                _Strategy = strategy;
            }

            public int Compare(T x, T y)
            {
                return _Strategy(x, y);
            }
        }

    }
}
