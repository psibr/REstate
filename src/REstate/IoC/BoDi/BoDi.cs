﻿/**************************************************************************************
 * 
 * BoDi: A very simple IoC container, easily embeddable also as a source code. 
 * 
 * BoDi was created to support SpecFlow (http://www.specflow.org) by Gaspar Nagy (http://gasparnagy.com/)
 * 
 * Project source & unit tests: http://github.com/gasparnagy/BoDi
 * License: Apache License 2.0
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 * 
 * Change history
 * 
 * vNext
 *   - Expose the ObjectContainer.RegisterFactoryAs in the IObjectContainer interface (by slawomir-brzezinski-at-interxion)
 *   - eliminate internal TypeHelper class
 *
 * v1.2
 *   - support for mapping of generic type definitions (by ibrahimbensalah)
 *   - object should be created in the parent container, if the registration was applied there
 *   - should be able to customize object creation with a container event (ObjectCreated)
 *   - should be able to register factory delegates
 *   - should be able to retrieve all named instance as a list with container.ResolveAll<T>()
 *   - should not allow resolving value types (structs)
 *   - should list registrations in container ToString()
 *   - should not dispose registered instances by default, disposal can be requested by the 'dispose: true' parameter
 *   - should be able to disable configuration file support (and the dependency on System.IrEstateConfiguration) with BODI_DISABLECONFIGFILESUPPORT compilation symbol
 *   - smaller code refactoring
 *   - improve resolution path handling
 * 
 * v1.1 - released with SpecFlow v1.9.0
 * 
 */

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
// ReSharper disable All

#if !BODI_LIMITEDRUNTIME
using System.Runtime.Serialization;
#endif

namespace REstate.IoC.BoDi
{
#if !BODI_LIMITEDRUNTIME
    [Serializable]
#endif
    internal class ObjectContainerException : Exception
    {
        public ObjectContainerException(string message, Type[] resolutionPath) : base(GetMessage(message, resolutionPath))
        {
        }

#if !BODI_LIMITEDRUNTIME
        protected ObjectContainerException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
#endif

        private static string GetMessage(string message, Type[] resolutionPath)
        {
            if (resolutionPath == null || resolutionPath.Length == 0)
                return message;

            return string.Format("{0} (resolution path: {1})", message, string.Join("->", resolutionPath.Select(t => t.FullName).ToArray()));
        }
    }

    internal interface IObjectContainer : IDisposable
    {
        /// <summary>
        /// Fired when a new object is created directly by the container. It is not invoked for resolving instance and factory registrations.
        /// </summary>
        event Action<object> ObjectCreated;

        /// <summary>
        /// Registers a type as the desired implementation type of an interface.
        /// </summary>
        /// <typeparam name="TType">Implementation type</typeparam>
        /// <typeparam name="TInterface">Interface will be resolved</typeparam>
        /// <param name="name">A identifier to register named instance, otherwise null.</param>
        /// <exception cref="ObjectContainerException">If there was already a resolve for the <typeparamref identifier="TInterface"/>.</exception>
        /// <remarks>
        ///     <para>Previous registrations can be overridden before the first resolution for the <typeparamref identifier="TInterface"/>.</para>
        /// </remarks>
        void RegisterTypeAs<TType, TInterface>(string name = null) where TType : class, TInterface;

        /// <summary>
        /// Registers an instance 
        /// </summary>
        /// <typeparam name="TInterface">Interface will be resolved</typeparam>
        /// <param name="instance">The instance implements the interface.</param>
        /// <param name="name">A identifier to register named instance, otherwise null.</param>
        /// <param name="dispose">Whether the instance should be disposed on container dispose, otherwise <c>false</c>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="instance"/> is null.</exception>
        /// <exception cref="ObjectContainerException">If there was already a resolve for the <typeparamref identifier="TInterface"/>.</exception>
        /// <remarks>
        ///     <para>Previous registrations can be overridden before the first resolution for the <typeparamref identifier="TInterface"/>.</para>
        ///     <para>The instance will be registered in the object pool, so if a <see cref="Resolve{T}()"/> (for another interface) would require an instance of the dynamic type of the <paramref name="instance"/>, the <paramref name="instance"/> will be returned.</para>
        /// </remarks>
        void RegisterInstanceAs<TInterface>(TInterface instance, string name = null, bool dispose = false) where TInterface : class;

        /// <summary>
        /// Registers an instance 
        /// </summary>
        /// <param name="instance">The instance implements the interface.</param>
        /// <param name="interfaceType">Interface will be resolved</param>
        /// <param name="name">A identifier to register named instance, otherwise null.</param>
        /// <param name="dispose">Whether the instance should be disposed on container dispose, otherwise <c>false</c>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="instance"/> is null.</exception>
        /// <exception cref="ObjectContainerException">If there was already a resolve for the <paramref name="interfaceType"/>.</exception>
        /// <remarks>
        ///     <para>Previous registrations can be overridden before the first resolution for the <paramref name="interfaceType"/>.</para>
        ///     <para>The instance will be registered in the object pool, so if a <see cref="Resolve{T}()"/> (for another interface) would require an instance of the dynamic type of the <paramref name="instance"/>, the <paramref name="instance"/> will be returned.</para>
        /// </remarks>
        void RegisterInstanceAs(object instance, Type interfaceType, string name = null, bool dispose = false);

        /// <summary>
        /// Registers an instance produced by <paramref name="factoryDelegate"/>. The delegate will be called only once and the instance it returned will be returned in each resolution.
        /// </summary>
        /// <typeparam name="TInterface">Interface to register as.</typeparam>
        /// <param name="factoryDelegate">The function to run to obtain the instance.</param>
        /// <param name="name">A identifier to resolve named instance, otherwise null.</param>
        void RegisterFactoryAs<TInterface>(Func<IObjectContainer, TInterface> factoryDelegate, string name = null);

        /// <summary>
        /// Resolves an implementation object for an interface or type.
        /// </summary>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <returns>An object implementing <typeparamref identifier="T"/>.</returns>
        /// <remarks>
        ///     <para>The container pools the objects, so if the interface is resolved twice or the same type is registered for multiple interfaces, a single instance is created and returned.</para>
        /// </remarks>
        T Resolve<T>();

        /// <summary>
        /// Resolves an implementation object for an interface or type.
        /// </summary>
        /// <param name="name">A identifier to resolve named instance, otherwise null.</param>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <returns>An object implementing <typeparamref identifier="T"/>.</returns>
        /// <remarks>
        ///     <para>The container pools the objects, so if the interface is resolved twice or the same type is registered for multiple interfaces, a single instance is created and returned.</para>
        /// </remarks>
        T Resolve<T>(string name);

        /// <summary>
        /// Resolves an implementation object for an interface or type.
        /// </summary>
        /// <param name="typeToResolve">The interface or type.</param>
        /// <param name="name">A identifier to resolve named instance, otherwise null.</param>
        /// <returns>An object implementing <paramref name="typeToResolve"/>.</returns>
        /// <remarks>
        ///     <para>The container pools the objects, so if the interface is resolved twice or the same type is registered for multiple interfaces, a single instance is created and returned.</para>
        /// </remarks>
        object Resolve(Type typeToResolve, string name = null);

        /// <summary>
        /// Resolves all implementations of an interface or type.
        /// </summary>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <returns>An object implementing <typeparamref identifier="T"/>.</returns>
        IEnumerable<T> ResolveAll<T>() where T : class;

        /// <summary>
        /// Determines whether the interface or type is registered.
        /// </summary>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <returns><c>true</c> if the interface or type is registered; otherwise <c>false</c>.</returns>
        bool IsRegistered<T>();

        /// <summary>
        /// Determines whether the interface or type is registered with the specified identifier.
        /// </summary>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <param name="name">The identifier.</param>
        /// <returns><c>true</c> if the interface or type is registered; otherwise <c>false</c>.</returns>
        bool IsRegistered<T>(string name);
    }

    internal interface IContainedInstance
    {
        IObjectContainer Container { get; }
    }

    internal class ObjectContainer : IObjectContainer
    {
        private const string REGISTERED_NAME_PARAMETER_NAME = "registeredName";

        /// <summary>
        /// A very simple immutable linked list of <see cref="Type"/>.
        /// </summary>
        private class ResolutionList
        {
            private readonly RegistrationKey currentRegistrationKey;
            private readonly Type currentResolvedType;
            private readonly ResolutionList nextNode;
            private bool IsLast { get { return nextNode == null; } }

            public ResolutionList()
            {
                Debug.Assert(IsLast);
            }

            private ResolutionList(RegistrationKey currentRegistrationKey, Type currentResolvedType, ResolutionList nextNode)
            {
                if (nextNode == null) throw new ArgumentNullException("nextNode");

                this.currentRegistrationKey = currentRegistrationKey;
                this.currentResolvedType = currentResolvedType;
                this.nextNode = nextNode;
            }

            public ResolutionList AddToEnd(RegistrationKey registrationKey, Type resolvedType)
            {
                return new ResolutionList(registrationKey, resolvedType, this);
            }

            public bool Contains(Type resolvedType)
            {
                if (resolvedType == null) throw new ArgumentNullException("resolvedType");
                return GetReverseEnumerable().Any(i => i.Value == resolvedType);
            }

            public bool Contains(RegistrationKey registrationKey)
            {
                return GetReverseEnumerable().Any(i => i.Key.Equals(registrationKey));
            }

            private IEnumerable<KeyValuePair<RegistrationKey, Type>> GetReverseEnumerable()
            {
                var node = this;
                while (!node.IsLast)
                {
                    yield return new KeyValuePair<RegistrationKey, Type>(node.currentRegistrationKey, node.currentResolvedType);
                    node = node.nextNode;
                }
            }

            public Type[] ToTypeList()
            {
                return GetReverseEnumerable().Select(i => i.Value ?? i.Key.Type).Reverse().ToArray();
            }

            public override string ToString()
            {
                return string.Join(",", GetReverseEnumerable().Select(n => string.Format("{0}:{1}", n.Key, n.Value)));
            }
        }

        private struct RegistrationKey
        {
            public readonly Type Type;
            public readonly string Name;

            public RegistrationKey(Type type, string name)
            {
                if (type == null) throw new ArgumentNullException("type");

                Type = type;
                Name = name;
            }

            private Type TypeGroup
            {
                get
                {
                    if (Type.GetTypeInfo().IsGenericType && !Type.GetTypeInfo().IsGenericTypeDefinition)
                        return Type.GetGenericTypeDefinition();
                    return Type;
                }
            }

            public override string ToString()
            {
                Debug.Assert(Type.FullName != null);
                if (Name == null)
                    return Type.FullName;

                return string.Format("{0}('{1}')", Type.FullName, Name);
            }

            bool Equals(RegistrationKey other)
            {
                var isInvertable = other.TypeGroup == Type || other.Type == TypeGroup || other.Type == Type;
                return isInvertable && String.Equals(other.Name, Name, StringComparison.CurrentCultureIgnoreCase);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (obj.GetType() != typeof(RegistrationKey)) return false;
                return Equals((RegistrationKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return TypeGroup.GetHashCode();
                }
            }
        }

#region Registration types

        private interface IRegistration
        {
            object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath);
        }

        private class TypeRegistration : IRegistration
        {
            private readonly Type implementationType;

            public TypeRegistration(Type implementationType)
            {
                this.implementationType = implementationType;
            }

            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                var typeToConstruct = GetTypeToConstruct(keyToResolve);

                var pooledObjectKey = new RegistrationKey(typeToConstruct, keyToResolve.Name);
                object obj = container.GetPooledObject(pooledObjectKey);

                if (obj == null)
                {
                    if (typeToConstruct.GetTypeInfo().IsInterface)
                        if (typeToConstruct.GetTypeInfo().IsGenericType &&
                            typeToConstruct.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                            obj = container.GetType().GetMethod(nameof(container.ResolveAll)).MakeGenericMethod(typeToConstruct.GenericTypeArguments.First()).Invoke(container, new object[0]);
                        else
                            throw new ObjectContainerException("Interface cannot be resolved: " + keyToResolve,
                                resolutionPath.ToTypeList());
                    else
                    {

                        obj = container.CreateObject(typeToConstruct, resolutionPath, keyToResolve);

                        container.objectPool.TryAdd(pooledObjectKey, obj);
                    }
                }

                return obj;
            }

            private Type GetTypeToConstruct(RegistrationKey keyToResolve)
            {
                var targetType = implementationType;
                if (targetType.GetTypeInfo().IsGenericTypeDefinition)
                {
                    var typeArgs = keyToResolve.Type.GetGenericArguments();
                    targetType = targetType.MakeGenericType(typeArgs);
                }
                return targetType;
            }

            public override string ToString()
            {
                return "Type: " + implementationType.FullName;
            }
        }

        private class InstanceRegistration : IRegistration
        {
            private readonly object instance;

            public InstanceRegistration(object instance)
            {
                this.instance = instance;
            }

            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                return instance;
            }

            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath,
                Type[] genericTypeArguments)
            {
                return Resolve(container, keyToResolve, resolutionPath);
            }

            public override string ToString()
            {
                string instanceText;
                try
                {
                    instanceText = instance.ToString();
                }
                catch (Exception ex)
                {
                    instanceText = ex.Message;
                }

                return "Instance: " + instanceText;
            }
        }

        private class FactoryRegistration : IRegistration
        {
            private readonly Delegate factoryDelegate;

            public FactoryRegistration(Delegate factoryDelegate)
            {
                this.factoryDelegate = factoryDelegate;
            }

            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                //TODO: store result object in pool?
                var obj = container.InvokeFactoryDelegate(factoryDelegate, resolutionPath, keyToResolve);
                return obj;
            }

            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath,
                Type[] genericTypeArguments)
            {
                return Resolve(container, keyToResolve, resolutionPath);
            }
        }

        private class NonDisposableWrapper
        {
            public object Object { get; private set; }

            public NonDisposableWrapper(object obj)
            {
                Object = obj;
            }
        }

        private class NamedInstanceDictionaryRegistration : IRegistration
        {
            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                var typeToResolve = keyToResolve.Type;
                Debug.Assert(typeToResolve.GetTypeInfo().IsGenericType && typeToResolve.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                var genericArguments = typeToResolve.GetGenericArguments();
                var keyType = genericArguments[0];
                var targetType = genericArguments[1];
                var result = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(genericArguments));

                foreach (var namedRegistration in container.registrations.Where(r => r.Key.Name != null && r.Key.Type == targetType).Select(r => r.Key))
                {
                    var convertedKey = ChangeType(namedRegistration.Name, keyType);
                    Debug.Assert(convertedKey != null);
                    result.Add(convertedKey, container.Resolve(namedRegistration.Type, namedRegistration.Name));
                }

                return result;
            }

            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath,
                Type[] genericTypeArguments)
            {
                return Resolve(container, keyToResolve, resolutionPath);
            }

            private object ChangeType(string name, Type keyType)
            {
                if (keyType.GetTypeInfo().IsEnum)
                    return Enum.Parse(keyType, name, true);

                Debug.Assert(keyType == typeof(string));
                return name;
            }
        }

#endregion

        private bool isDisposed = false;
        private readonly ObjectContainer baseContainer;
        private readonly ConcurrentDictionary<RegistrationKey, IRegistration> registrations = new ConcurrentDictionary<RegistrationKey, IRegistration>();
        private readonly ConcurrentDictionary<RegistrationKey, object> resolvedObjects = new ConcurrentDictionary<RegistrationKey, object>();
        private readonly ConcurrentDictionary<RegistrationKey, object> objectPool = new ConcurrentDictionary<RegistrationKey, object>();

        public event Action<object> ObjectCreated;

        public ObjectContainer(IObjectContainer baseContainer = null)
        {
            if (baseContainer != null && !(baseContainer is ObjectContainer))
                throw new ArgumentException("Base container must be an ObjectContainer", "baseContainer");

            this.baseContainer = (ObjectContainer)baseContainer;
            RegisterInstanceAs<IObjectContainer>(this);
        }

#region Registration

        public void RegisterTypeAs<TInterface>(Type implementationType, string name = null) where TInterface : class
        {
            Type interfaceType = typeof(TInterface);
            RegisterTypeAs(implementationType, interfaceType, name);
        }

        public void RegisterTypeAs<TType, TInterface>(string name = null) where TType : class, TInterface
        {
            Type interfaceType = typeof(TInterface);
            Type implementationType = typeof(TType);
            RegisterTypeAs(implementationType, interfaceType, name);
        }

        public void RegisterTypeAs(Type implementationType, Type interfaceType)
        {
            if (!IsValidTypeMapping(implementationType, interfaceType))
                throw new InvalidOperationException("type mapping is not valid");
            RegisterTypeAs(implementationType, interfaceType, null);
        }

        private bool IsValidTypeMapping(Type implementationType, Type interfaceType)
        {
            if (interfaceType.IsAssignableFrom(implementationType))
                return true;

            if (interfaceType.GetTypeInfo().IsGenericTypeDefinition && implementationType.GetTypeInfo().IsGenericTypeDefinition)
            {
                var baseTypes = GetBaseTypes(implementationType).ToArray();
                return baseTypes.Any(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == interfaceType);
            }

            return false;
        }

        private static IEnumerable<Type> GetBaseTypes(Type type)
        {
            if (type.GetTypeInfo().BaseType == null) return type.GetInterfaces();

            return Enumerable.Repeat(type.GetTypeInfo().BaseType, 1)
                .Concat(type.GetInterfaces())
                .Concat(type.GetInterfaces().SelectMany(GetBaseTypes))
                .Concat(GetBaseTypes(type.GetTypeInfo().BaseType));
        }


        private RegistrationKey CreateNamedInstanceDictionaryKey(Type targetType)
        {
            return new RegistrationKey(typeof(IDictionary<,>).MakeGenericType(typeof(string), targetType), null);
        }

        private void AddRegistration(RegistrationKey key, IRegistration registration)
        {
            registrations[key] = registration;

            if (key.Name != null)
            {
                var dictKey = CreateNamedInstanceDictionaryKey(key.Type);
                if (!registrations.ContainsKey(dictKey))
                {
                    registrations[dictKey] = new NamedInstanceDictionaryRegistration();
                }
            }
        }

        public void RegisterTypeAs(Type implementationType, Type interfaceType, string name)
        {
            var registrationKey = new RegistrationKey(interfaceType, name);
            AssertNotResolved(registrationKey);

            ClearRegistrations(registrationKey);

            AddRegistration(registrationKey, new TypeRegistration(implementationType));
        }

        public void RegisterInstanceAs(object instance, Type interfaceType, string name = null, bool dispose = false)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            var registrationKey = new RegistrationKey(interfaceType, name);
            AssertNotResolved(registrationKey);

            ClearRegistrations(registrationKey);
            AddRegistration(registrationKey, new InstanceRegistration(instance));
            objectPool[new RegistrationKey(instance.GetType(), name)] = GetPoolableInstance(instance, dispose);
        }

        private static object GetPoolableInstance(object instance, bool dispose)
        {
            return (instance is IDisposable) && !dispose ? new NonDisposableWrapper(instance) : instance;
        }

        public void RegisterInstanceAs<TInterface>(TInterface instance, string name = null, bool dispose = false) where TInterface : class
        {
            RegisterInstanceAs(instance, typeof(TInterface), name, dispose);
        }

        public void RegisterFactoryAs<TInterface>(Func<TInterface> factoryDelegate, string name = null)
        {
            RegisterFactoryAs(factoryDelegate, typeof(TInterface), name);
        }

        public void RegisterFactoryAs<TInterface>(Func<IObjectContainer, TInterface> factoryDelegate, string name = null)
        {
            RegisterFactoryAs(factoryDelegate, typeof(TInterface), name);
        }

        public void RegisterFactoryAs<TInterface>(Delegate factoryDelegate, string name = null)
        {
            RegisterFactoryAs(factoryDelegate, typeof(TInterface), name);
        }

        public void RegisterFactoryAs(Delegate factoryDelegate, Type interfaceType, string name = null)
        {
            if (factoryDelegate == null) throw new ArgumentNullException("factoryDelegate");
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");

            var registrationKey = new RegistrationKey(interfaceType, name);
            AssertNotResolved(registrationKey);

            ClearRegistrations(registrationKey);

            AddRegistration(registrationKey, new FactoryRegistration(factoryDelegate));
        }

        public bool IsRegistered<T>()
        {
            return this.IsRegistered<T>(null);
        }

        public bool IsRegistered<T>(string name)
        {
            Type typeToResolve = typeof(T);

            var keyToResolve = new RegistrationKey(typeToResolve, name);

            return registrations.ContainsKey(keyToResolve);
        }

        // ReSharper disable once UnusedParameter.Local
        private void AssertNotResolved(RegistrationKey interfaceType)
        {
            if (resolvedObjects.ContainsKey(interfaceType))
                throw new AlreadyResolvedException("An object has been resolved for this interface already.", null);
        }

        private void ClearRegistrations(RegistrationKey registrationKey)
        {
            registrations.TryRemove(registrationKey, out var _);
        }

#endregion

#region Resolve

        public T Resolve<T>()
        {
            return Resolve<T>(null);
        }

        public T Resolve<T>(string name)
        {
            Type typeToResolve = typeof(T);

            object resolvedObject = Resolve(typeToResolve, name);

            return (T)resolvedObject;
        }


        public object Resolve(Type typeToResolve, string name = null)
        {
            return Resolve(typeToResolve, new ResolutionList(), name);
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return registrations
                .Where(x => 
                    x.Key.Type == typeof(T) 
                    || typeof(T).IsConstructedGenericType 
                        && typeof(T).GetGenericTypeDefinition() == x.Key.Type)
                .Select(x => Resolve(x.Key.Type.GetTypeInfo().IsGenericTypeDefinition 
                    ? x.Key.Type.MakeGenericType(typeof(T).GenericTypeArguments) 
                    : x.Key.Type, x.Key.Name) as T);
        }

        private object Resolve(Type typeToResolve, ResolutionList resolutionPath, string name)
        {
            AssertNotDisposed();

            var keyToResolve = new RegistrationKey(typeToResolve, name);

            var resolvedObject = resolvedObjects.GetOrAdd(
                keyToResolve, 
                key => ResolveObject(key, resolutionPath));

            return resolvedObject;
        }

        private KeyValuePair<ObjectContainer, IRegistration>? GetRegistrationResult(RegistrationKey keyToResolve)
        {
            IRegistration registration;
            if (registrations.TryGetValue(keyToResolve, out registration))
            {
                return new KeyValuePair<ObjectContainer, IRegistration>(this, registration);
            }

            if (baseContainer != null)
                return baseContainer.GetRegistrationResult(keyToResolve);

            if (IsSpecialNamedInstanceDictionaryKey(keyToResolve))
            {
                var targetType = keyToResolve.Type.GetGenericArguments()[1];
                return GetRegistrationResult(CreateNamedInstanceDictionaryKey(targetType));
            }

            // if there was no named registration, we still return an empty dictionary
            if (IsDefaultNamedInstanceDictionaryKey(keyToResolve))
            {
                return new KeyValuePair<ObjectContainer, IRegistration>(this, new NamedInstanceDictionaryRegistration());
            }

            return null;
        }

        private bool IsDefaultNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
        {
            return IsNamedInstanceDictionaryKey(keyToResolve) &&
                   keyToResolve.Type.GetGenericArguments()[0] == typeof(string);
        }

        private bool IsSpecialNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
        {
            return IsNamedInstanceDictionaryKey(keyToResolve) &&
                   keyToResolve.Type.GetGenericArguments()[0].GetTypeInfo().IsEnum;
        }

        private bool IsNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
        {
            return keyToResolve.Name == null && keyToResolve.Type.GetTypeInfo().IsGenericType && keyToResolve.Type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
        }

        private object GetPooledObject(RegistrationKey pooledObjectKey)
        {
            object obj;
            if (GetObjectFromPool(pooledObjectKey, out obj))
                return obj;

            return null;
        }

        private bool GetObjectFromPool(RegistrationKey pooledObjectKey, out object obj)
        {
            if (!objectPool.TryGetValue(pooledObjectKey, out obj))
                return false;

            var nonDisposableWrapper = obj as NonDisposableWrapper;
            if (nonDisposableWrapper != null)
                obj = nonDisposableWrapper.Object;

            return true;
        }

        private object ResolveObject(RegistrationKey keyToResolve, ResolutionList resolutionPath)
        {
            if (keyToResolve.Type.GetTypeInfo().IsPrimitive || keyToResolve.Type == typeof(string) || keyToResolve.Type.GetTypeInfo().IsValueType)
                throw new ObjectContainerException("Primitive types or structs cannot be resolved: " + keyToResolve.Type.FullName, resolutionPath.ToTypeList());

            var registrationResult = GetRegistrationResult(keyToResolve) ??
                                     new KeyValuePair<ObjectContainer, IRegistration>(this, new TypeRegistration(keyToResolve.Type));

            var resolutionPathForResolve = registrationResult.Key == this ?
                resolutionPath : new ResolutionList();
            return registrationResult.Value.Resolve(registrationResult.Key, keyToResolve, resolutionPathForResolve);
        }

        private object CreateObject(Type type, ResolutionList resolutionPath, RegistrationKey keyToResolve)
        { 
            var ctors = type.GetConstructors();
            if (ctors.Length == 0)
                ctors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

            Debug.Assert(ctors.Length > 0, "Class must have a constructor!");

            int maxParamCount = ctors.Max(ctor => ctor.GetParameters().Length);
            var maxParamCountCtors = ctors.Where(ctor => ctor.GetParameters().Length == maxParamCount).ToArray();

            object obj;
            if (maxParamCountCtors.Length == 1)
            {
                ConstructorInfo ctor = maxParamCountCtors[0];
                if (resolutionPath.Contains(keyToResolve))
                    throw new ObjectContainerException("Circular dependency found! " + type.FullName, resolutionPath.ToTypeList());

                var args = ResolveArguments(ctor.GetParameters(), keyToResolve, resolutionPath.AddToEnd(keyToResolve, type));
                obj = ctor.Invoke(args);
            }
            else
            {
                throw new ObjectContainerException("Multiple public constructors with same maximum parameter count are not supported! " + type.FullName, resolutionPath.ToTypeList());
            }

            OnObjectCreated(obj);

            return obj;
        }

        protected virtual void OnObjectCreated(object obj)
        {
            var eventHandler = ObjectCreated;
            if (eventHandler != null)
                eventHandler(obj);
        }

        private object InvokeFactoryDelegate(Delegate factoryDelegate, ResolutionList resolutionPath, RegistrationKey keyToResolve)
        {
            if (resolutionPath.Contains(keyToResolve))
                throw new ObjectContainerException("Circular dependency found! " + factoryDelegate.ToString(), resolutionPath.ToTypeList());

            var args = ResolveArguments(factoryDelegate.GetMethodInfo().GetParameters(), keyToResolve, resolutionPath.AddToEnd(keyToResolve, null));
            return factoryDelegate.DynamicInvoke(args);
        }

        private object[] ResolveArguments(IEnumerable<ParameterInfo> parameters, RegistrationKey keyToResolve, ResolutionList resolutionPath)
        {
            return parameters.Select(p => IsRegisteredNameParameter(p) ? ResolveRegisteredName(keyToResolve) : Resolve(p.ParameterType, resolutionPath, null)).ToArray();
        }

        private object ResolveRegisteredName(RegistrationKey keyToResolve)
        {
            return keyToResolve.Name;
        }

        private bool IsRegisteredNameParameter(ParameterInfo parameterInfo)
        {
            return parameterInfo.ParameterType == typeof(string) &&
                   parameterInfo.Name.Equals(REGISTERED_NAME_PARAMETER_NAME);
        }

#endregion

        public override string ToString()
        {
            return string.Join(Environment.NewLine,
                registrations
                    .Where(r => !(r.Value is NamedInstanceDictionaryRegistration))
                    .Select(r => string.Format("{0} -> {1}", r.Key, (r.Key.Type == typeof(IObjectContainer) && r.Key.Name == null) ? "<self>" : r.Value.ToString())));
        }

        private void AssertNotDisposed()
        {
            if (isDisposed)
                throw new ObjectContainerException("Object container disposed", null);
        }

        public void Dispose()
        {
            isDisposed = true;

            foreach (var obj in objectPool.Values.OfType<IDisposable>().Where(o => !ReferenceEquals(o, this)))
                obj.Dispose();

            objectPool.Clear();
            registrations.Clear();
            resolvedObjects.Clear();
        }
    }

    internal class AlreadyResolvedException : ObjectContainerException
    {
        public AlreadyResolvedException(string message, Type[] resolutionPath) : base(message, resolutionPath)
        {
        }

        protected AlreadyResolvedException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}