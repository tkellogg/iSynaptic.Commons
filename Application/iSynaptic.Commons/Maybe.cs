﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iSynaptic.Commons
{
    public struct Maybe<T> : IMaybe<T>, IEquatable<Maybe<T>>, IEquatable<T>
    {
        private struct MaybeResult
        {
            public T Value;
            public bool HasValue;
            public Exception Exception;
        }

        public static readonly Maybe<T> NoValue = new Maybe<T>();
        public static readonly Maybe<T> Default = new Maybe<T>(default(T));

        private readonly Func<MaybeResult> _Computation;

        public Maybe(Func<T> computation) : this()
        {
            Guard.NotNull(computation, "computation");
            _Computation = Default.Bind(x => computation()).Computation;
        }

        public Maybe(T value) : this()
        {
            _Computation = () => new MaybeResult {Value = value, HasValue = true};
        }

        public Maybe(Exception exception)
        {
            Guard.NotNull(exception, "exception");
            _Computation = () => new MaybeResult {Exception = exception};
        }

        private Maybe(Func<MaybeResult> computation) : this()
        {
            Guard.NotNull(computation, "computation");
            _Computation = computation;
        }

        public static Maybe<T> Unsafe(Func<Maybe<T>> unsafeComputation)
        {
            Guard.NotNull(unsafeComputation, "unsafeComputation");

            Func<MaybeResult> finalComputation =
                () => unsafeComputation().Computation();

            return new Maybe<T>(finalComputation.Memoize());
        }

        private Func<MaybeResult> Computation
        {
            get { return _Computation ?? (() => new MaybeResult()); }
        }

        public T Value
        {
            get
            {
                var result = Computation();

                if(result.Exception != null)
                    result.Exception.ThrowPreservingCallStack();

                if(result.HasValue != true)
                    throw new InvalidOperationException("No value can be provided.");

                return result.Value;
            }
        }

        public bool HasValue { get { return Computation().HasValue; } }
        public Exception Exception { get { return Computation().Exception; } }

        public bool Equals(T other)
        {
            return Equals(new Maybe<T>(other));
        }

        public bool Equals(Maybe<T> other)
        {
            return Equals(other, EqualityComparer<T>.Default);
        }

        public bool Equals(Maybe<T> other, IEqualityComparer<T> comparer)
        {
            Guard.NotNull(comparer, "comparer");

            if (Exception != null)
                return other.Exception != null && other.Exception == Exception;

            if (other.Exception != null)
                return false;

            if (!HasValue)
                return !other.HasValue;

            if (!other.HasValue)
                return false;

            return comparer.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (obj is Maybe<T>)
                return Equals((Maybe<T>) obj);

            return false;
        }

        public override int GetHashCode()
        {
            if (Exception != null)
                return Exception.GetHashCode();

            if (HasValue != true)
                return -1;

            if (Value == null)
                return 0;

            return Value.GetHashCode();
        }

        public static bool operator ==(Maybe<T> left, Maybe<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Maybe<T> left, Maybe<T> right)
        {
            return !(left == right);
        }

        public static bool operator ==(Maybe<T> left, T right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Maybe<T> left, T right)
        {
            return !(left == right);
        }

        public static bool operator ==(T left, Maybe<T> right)
        {
            return right.Equals(left);
        }

        public static bool operator !=(T left, Maybe<T> right)
        {
            return !(left == right);
        }

        public Maybe<TResult> Bind<TResult>(Func<T, TResult> func)
        {
            Guard.NotNull(func, "func");
            return Bind(x => new Maybe<TResult>(func(x)));
        }

        public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> func)
        {
            Guard.NotNull(func, "func");

            var computation = Computation;

            Func<Maybe<TResult>.MaybeResult> boundComputation = () =>
            {
                var result = computation();

                if (result.Exception != null)
                    return new Maybe<TResult>.MaybeResult { Exception = result.Exception };

                if (result.HasValue != true)
                    return new Maybe<TResult>.MaybeResult();

                try
                {
                    return func(result.Value).Computation();
                }
                catch (Exception ex)
                {
                    return new Maybe<TResult>.MaybeResult { Exception = ex };
                }
            };

            return new Maybe<TResult>(boundComputation.Memoize());
        }

        public static implicit operator Maybe<T>(T value)
        {
            return new Maybe<T>(value);
        }

        public static explicit operator T(Maybe<T> value)
        {
            return value.Value;
        }
    }

    public static class Maybe
    {
        public static Maybe<T> Value<T>(T value)
        {
            return new Maybe<T>(value);
        }

        public static Maybe<T> Value<T>(Func<T> computation)
        {
            Guard.NotNull(computation, "computation");
            return new Maybe<T>(computation);
        }

        public static Maybe<T> NotNull<T>(T value) where T: class
        {
            return Value(value).NotNull();
        }

        public static Maybe<T> NotNull<T>(Func<T> value) where T : class
        {
            return Value(value).NotNull();
        }

        public static Maybe<T?> NotNull<T>(T? value) where T : struct
        {
            return Value(value).NotNull();
        }

        public static Maybe<T?> NotNull<T>(Func<T?> value) where T : struct
        {
            return Value(value).NotNull();
        }

        public static Maybe<T> Using<T, TResource>(TResource resource, Func<TResource, Maybe<T>> selector) where TResource : IDisposable
        {
            Guard.NotNull(resource, "resource");
            Guard.NotNull(selector, "selector");

            return Using(() => resource, selector);
        }

        public static Maybe<T> Using<T, TResource>(Func<TResource> computation, Func<TResource, Maybe<T>> selector) where TResource : IDisposable
        {
            Guard.NotNull(computation, "computation");
            Guard.NotNull(selector, "selector");
            
            return Maybe<TResource>.Default
                .Using(x => computation(), selector);
        }

        public static Maybe<TResult> Using<T, TResource, TResult>(this Maybe<T> self, Func<T, TResource> resourceSelector, Func<TResource, Maybe<TResult>> selector) where TResource : IDisposable
        {
            Guard.NotNull(resourceSelector, "resourceSelector");
            Guard.NotNull(selector, "selector");

            return self.Select(x =>
            {
                using (var resource = resourceSelector(x))
                    return selector(resource);
            });
        }

        public static Maybe<TResult> Select<T, TResult>(this Maybe<T> self, Func<T, TResult> selector)
        {
            Guard.NotNull(selector, "selector");
            return self.Bind(selector);
        }

        public static Maybe<TResult> Select<T, TResult>(this Maybe<T> self, Func<T, Maybe<TResult>> selector)
        {
            Guard.NotNull(selector, "selector");
            return self.Bind(selector);
        }

        public static Maybe<T> NotNull<T>(this Maybe<T> self) where T : class
        {
            return self.NotNull(x => x);
        }

        public static Maybe<T?> NotNull<T>(this Maybe<T?> self) where T : struct
        {
            return self.NotNull(x => x);
        }

        public static Maybe<T> NotNull<T, TResult>(this Maybe<T> self, Func<T, TResult> selector) where TResult : class
        {
            Guard.NotNull(selector, "selector");
            return self.Where(x => selector(x) != null);
        }

        public static Maybe<T> NotNull<T, TResult>(this Maybe<T> self, Func<T, TResult?> selector) where TResult : struct
        {
            Guard.NotNull(selector, "selector");
            return self.Where(x => selector(x).HasValue);
        }

        public static Maybe<TResult> Coalesce<T, TResult>(this Maybe<T> self, Func<T, TResult> selector) where TResult : class
        {
            Guard.NotNull(selector, "selector");
            return self.Coalesce(selector, () => Maybe<TResult>.NoValue);
        }

        public static Maybe<TResult?> Coalesce<T, TResult>(this Maybe<T> self, Func<T, TResult?> selector) where TResult : struct
        {
            Guard.NotNull(selector, "selector");
            return self.Coalesce(selector, () => Maybe<TResult?>.NoValue);
        }

        public static Maybe<TResult> Coalesce<T, TResult>(this Maybe<T> self, Func<T, TResult> selector, Func<Maybe<TResult>> valueIfNullFactory) where TResult : class
        {
            Guard.NotNull(selector, "selector");
            Guard.NotNull(valueIfNullFactory, "valueIfNullFactory");

            return self.Select(selector)
                .When(x => x.HasValue && x.Value == null, x => valueIfNullFactory());
        }

        public static Maybe<TResult?> Coalesce<T, TResult>(this Maybe<T> self, Func<T, TResult?> selector, Func<Maybe<TResult?>> valueIfNullFactory) where TResult : struct 
        {
            Guard.NotNull(selector, "selector");
            Guard.NotNull(valueIfNullFactory, "valueIfNullFactory");

            return self.Select(selector)
                .When(x => x.HasValue && x.Value.HasValue != true, x => valueIfNullFactory());
        }

        public static Maybe<T> Where<T>(this Maybe<T> self, Func<T, bool> predicate)
        {
            Guard.NotNull(predicate, "predicate");
            return self.Bind(x => predicate(x) ? x : Maybe<T>.NoValue);
        }

        public static Maybe<T> Unless<T>(this Maybe<T> self, Func<T, bool> predicate)
        {
            Guard.NotNull(predicate, "predicate");
            return self.Where(x => !predicate(x));
        }

        public static T Return<T>(this Maybe<T> self)
        {
            return self.Return(default(T));
        }

        public static T Return<T>(this Maybe<T> self, T @default)
        {
            return self.Return(() => @default);
        }

        public static T Return<T>(this Maybe<T> self, Func<T> @default)
        {
            return self
                .ThrowOnException()
                .OnNoValue(@default)
                .Value;
        }

        public static Maybe<T> Do<T>(this Maybe<T> self, Action<T> action)
        {
            Guard.NotNull(action, "action");
            return self.Bind(x =>
            {
                action(x);
                return x;
            });
        }

        public static Maybe<T> Assign<T>(this Maybe<T> self, ref T target)
        {
            if (self.HasValue)
                target = self.Value;

            return self;
        }

        public static Maybe<T> OnNoValue<T>(this Maybe<T> self, T value)
        {
            return self.OnNoValue(() => value);
        }

        public static Maybe<T> OnNoValue<T>(this Maybe<T> self, Func<T> valueFactory)
        {
            Guard.NotNull(valueFactory, "valueFactory");
            return self.OnNoValue(() => Value(valueFactory));
        }

        public static Maybe<T> OnNoValue<T>(this Maybe<T> self, Action action)
        {
            Guard.NotNull(action, "action");
            return self.OnNoValue(() => { action(); return Maybe<T>.NoValue; });
        }

        public static Maybe<T> OnNoValue<T>(this Maybe<T> self, Func<Maybe<T>> valueFactory)
        {
            Guard.NotNull(valueFactory, "valueFactory");
            return self.When(x => x.Exception == null && x.HasValue != true, x => valueFactory());
        }

        public static Maybe<T> OnException<T>(this Maybe<T> self, T value)
        {
            return self.OnException(() => value);
        }

        public static Maybe<T> OnException<T>(this Maybe<T> self, Func<T> valueFactory)
        {
            return self.OnException(x => Value(valueFactory()));
        }

        public static Maybe<T> OnException<T>(this Maybe<T> self, Action<Exception> handler)
        {
            Guard.NotNull(handler, "handler");
            return self.OnException(x => { handler(x); return new Maybe<T>(x); });
        }

        public static Maybe<T> OnException<T>(this Maybe<T> self, Func<Exception, Maybe<T>> handler)
        {
            Guard.NotNull(handler, "handler");
            return self.When(x => x.Exception != null, x => handler(x.Exception));
        }

        public static Maybe<T> ThrowOnException<T>(this Maybe<T> self)
        {
            return self.ThrowOnException(typeof(Exception));
        }

        public static Maybe<T> ThrowOnException<T>(this Maybe<T> self, Type exceptionType)
        {
            Guard.NotNull(exceptionType, "exceptionType");
            return self.ThrowOnException(x => exceptionType.IsAssignableFrom(x.GetType()));
        }

        public static Maybe<T> ThrowOnException<T>(this Maybe<T> self, Func<Exception, bool> predicate)
        {
            Guard.NotNull(predicate, "predicate");

            Func<Maybe<T>> boundComputation = () =>
            {
                if (self.Exception != null && predicate(self.Exception))
                    self.Exception.ThrowPreservingCallStack();

                return self;
            };

            return Maybe<T>.Unsafe(boundComputation);
        }

        public static Maybe<T> ThrowOnNoValue<T>(this Maybe<T> self, Exception exception)
        {
            Guard.NotNull(exception, "exception");
            return self.ThrowOnNoValue(() => exception);
        }

        public static Maybe<T> ThrowOnNoValue<T>(this Maybe<T> self, Func<Exception> exceptionSelector)
        {
            Guard.NotNull(exceptionSelector, "exceptionSelector");
            return self
                .When(x => x.Exception == null && !x.HasValue, x => { throw exceptionSelector(); })
                .ThrowOnException();
        }

        public static Maybe<T> With<T, TSelected>(this Maybe<T> self, Func<T, TSelected> selector, Action<TSelected> action)
        {
            Guard.NotNull(selector, "selector");
            Guard.NotNull(action, "action");

            return With(self, x => Value(selector(x)), action);
        }

        public static Maybe<T> With<T, TSelected>(this Maybe<T> self, Func<T, Maybe<TSelected>> selector, Action<TSelected> action)
        {
            Guard.NotNull(selector, "selector");
            Guard.NotNull(action, "action");

            return self.Bind(x =>
            {
                selector(x)
                    .Do(action)
                    .ThrowOnException()
                    .Run();

                return x;
            });
        }

        public static Maybe<T> When<T>(this Maybe<T> self, Func<Maybe<T>, bool> predicate, Func<Maybe<T>, Maybe<T>> computation)
        {
            Guard.NotNull(predicate, "predicate");
            Guard.NotNull(computation, "computation");

            return Value(self)
                    .Select(predicate)
                    .Select(x => x ? computation(self) : self);
        }

        public static Maybe<T> When<T>(this Maybe<T> self, T value, Func<T, Maybe<T>> computation)
        {
            Guard.NotNull(computation, "computation");
            return self.When(x => x.Equals(value), x => x.Bind(computation));
        }

        public static Maybe<T> When<T>(this Maybe<T> self, T value, Action<T> action)
        {
            Guard.NotNull(action, "action");
            return self.When(value, x => { action(x); return x; });
        }

        public static Maybe<T> When<T>(this Maybe<T> self, Func<Maybe<T>, bool> predicate, Maybe<T> result)
        {
            Guard.NotNull(predicate, "predicate");
            return self.When(predicate, x => result);
        }

        public static Maybe<T> When<T>(this Maybe<T> self, Func<Maybe<T>, bool> predicate, Action<Maybe<T>> action)
        {
            Guard.NotNull(predicate, "predicate");
            Guard.NotNull(action, "action");
            return self.When(predicate, x => { action(x); return x; });
        }

        public static Maybe<T> When<T>(this Maybe<T> self, Maybe<T> value, Maybe<T> result)
        {
            return self.When(x => x.Equals(value), result);
        }

        public static Maybe<T> Run<T>(this Maybe<T> self)
        {
            return self.HasValue ? self : self;
        }

        public static Maybe<T> RunAsync<T>(this Maybe<T> self, CancellationToken cancellationToken = default(CancellationToken), TaskCreationOptions taskCreationOptions = TaskCreationOptions.None, TaskScheduler taskScheduler = default(TaskScheduler))
        {
            var task = Task.Factory.StartNew(() => self.Run(), cancellationToken, taskCreationOptions, taskScheduler ?? TaskScheduler.Default);
            return Maybe<T>.Default.Bind(x => task.Result);
        }

        public static Maybe<T> Synchronize<T>(this Maybe<T> self)
        {
            Func<Maybe<T>> synchronizedComputation = () => self.Exception != null ? self : self;
            synchronizedComputation = synchronizedComputation.Synchronize();

            return Value(synchronizedComputation)
                .Select(x => x);
        }

        public static Maybe<TResult> Cast<T, TResult>(this Maybe<T> self)
        {
            return self.Select(x => (TResult)(object)x);
        }

        public static Maybe<TResult> OfType<T, TResult>(this Maybe<T> self)
        {
            return self
                .Where(x => x is TResult)
                .Select(x => (TResult)(object)x);
        }

        public static Maybe<T> Or<T>(this Maybe<T> self, Maybe<T> other)
        {
            return self.OnNoValue(() => other);
        }

        public static Maybe<TResult> Join<T, U, TResult>(this Maybe<T> self, Maybe<U> other, Func<T, U, TResult> selector)
        {
            return self.Select(t => other.Select(r => selector(t, r)));
        }

        public static Maybe<Tuple<T, U>> Join<T, U>(this Maybe<T> self, Maybe<U> other)
        {
            return self.Join(other, Tuple.Create);
        }

        public static T? ToNullable<T>(this Maybe<T> self) where T : struct
        {
            return self.Select(x => (T?) x)
                .OnNoValue(() => (T?)null)
                .Return();
        }
    }
}
