//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2018, Alexandru Ciobanu (alex+git@ciobanu.org)
//
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in
//       the documentation and/or other materials provided with the distribution.
//     * Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
//  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
//  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//  LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace XtraLiteTemplates.Introspection
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.CSharp.RuntimeBinder;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;

    using JetBrains.Annotations;

    using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

    internal sealed class SimpleDynamicInvoker
    {
        private const int DynamicReservedArgumentCount = 2;
        private const int MaximumArgumentCount = 16 - DynamicReservedArgumentCount;

        private readonly IDictionary<string, Func<object, object>> _cachedGetDelegateDict =
            new Dictionary<string, Func<object, object>>();
        private readonly IDictionary<string, Func<object, object[], object>> _cachedFuncInvokeDelegateDict =
            new Dictionary<string, Func<object, object[], object>>();

        [NotNull]
        private static Func<object, object> CreateNewGetDelegate([NotNull] string property)
        {
            Debug.Assert(!string.IsNullOrEmpty(property), "property cannot be null or empty");

            var callSite = CallSite<Func<CallSite, object, object>>.Create(
                Binder.GetMember(
                    CSharpBinderFlags.None,
                    property,
                    typeof(SimpleDynamicInvoker),
                    new[]
                    {
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    }));

            return @object => callSite.Target(callSite, @object);
        }

        [NotNull]
        private static Func<object, object[], object> CreateNewFuncInvokeDelegate([NotNull] string method, int argCount)
        {
            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(method));
            }

            Debug.Assert(argCount >= 0 && argCount <= MaximumArgumentCount);

            var reqTypeList = new Type[argCount + DynamicReservedArgumentCount + 1];
            var argInfoList = new CSharpArgumentInfo[argCount + 1];

            reqTypeList[0] = typeof(CallSite);
            reqTypeList[1] = typeof(object);
            reqTypeList[2] = typeof(object);

            argInfoList[0] = CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null);

            for (var i = 0; i < argCount; i++)
            {
                reqTypeList[i + 3] = typeof(object);
                argInfoList[i + 1] = CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null);
            }

            var synthFuncType = Expression.GetFuncType(reqTypeList);
            var callSite = CallSite.Create(
                synthFuncType,
                Binder.InvokeMember(
                    CSharpBinderFlags.None,
                    method,
                    null,
                    typeof(SimpleDynamicInvoker),
                    argInfoList));

            var field = callSite
                .GetType()
                .GetField(nameof(CallSite<object>.Target));

            Debug.Assert(field != null);
            var @delegate = (Delegate)field.GetValue(callSite);
            Debug.Assert(@delegate != null);

            if (argCount == 0)
            {
                return (@object, args) =>
                {
                    Debug.Assert(args == null || args.Length == 0);
                    return @delegate.DynamicInvoke(callSite, @object);
                };
            }

            return (@object, args) =>
            {
                Debug.Assert(args != null && args.Length == argCount);
                var actualArgs = new object[argCount + 2];
                actualArgs[0] = callSite;
                actualArgs[1] = @object;
                Array.Copy(args, 0, actualArgs, actualArgs.Length - argCount, argCount);

                var ret = @delegate.DynamicInvoke(actualArgs);
                return ret;
            };
        }

        [CanBeNull]
        public object GetValue([CanBeNull] object @object, [NotNull] string member)
        {
            Expect.Identifier(nameof(member), member);

            if (@object == null)
            {
                return null;
            }

            if (!_cachedGetDelegateDict.TryGetValue(member, out var @delegate))
            {
                @delegate = CreateNewGetDelegate(member);
                _cachedGetDelegateDict.Add(member, @delegate);
            }

            Debug.Assert(@delegate != null, "@delegate is always expected to be set.");

            try
            {
                return @delegate(@object);
            }
            catch (TargetInvocationException targetInvokationError)
            {
                if (targetInvokationError.InnerException != null)
                {
                    if (targetInvokationError.InnerException is RuntimeBinderException)
                    {
                        return null;
                    }

                    throw targetInvokationError.InnerException;
                }
                throw;
            }
            catch (RuntimeBinderException)
            {
                return null;
            }
        }

        [CanBeNull]
        public object Invoke([CanBeNull] object @object, [NotNull] string member, [CanBeNull, ItemCanBeNull] object[] args = null)
        {
            Expect.Identifier(nameof(member), member);

            var argCount = args?.Length ?? 0;
            Expect.Between(nameof(args), argCount, 0, MaximumArgumentCount);

            if (@object == null)
            {
                return null;
            }

            var key = $"{member}.{argCount}";
            if (!_cachedFuncInvokeDelegateDict.TryGetValue(key, out var @delegate))
            {
                @delegate = CreateNewFuncInvokeDelegate(member, argCount);
                _cachedFuncInvokeDelegateDict.Add(key, @delegate);
            }
            Debug.Assert(@delegate != null, "@delegate is always expected to be set.");

            try
            {
                return @delegate(@object, args);
            }
            catch (TargetInvocationException targetInvokationError)
            {
                if (targetInvokationError.InnerException != null)
                {
                    if (targetInvokationError.InnerException is RuntimeBinderException)
                    {
                        return null;
                    }

                    throw targetInvokationError.InnerException;
                }
                throw;
            }
            catch (RuntimeBinderException)
            {
                return null;
            }
        }
    }
}