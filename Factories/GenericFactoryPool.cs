using System;
using System.Collections.Concurrent;
using QuadrantTree.Interfaces;

namespace QuadrantTree.Factories;

/// <summary>
/// Provides a generic object pool for creating and recycling objects of various types.
/// Object pooling helps improve performance by reusing objects instead of repeatedly
/// creating and destroying them.
/// </summary>
public static class GenericFactoryPool
{
	private static readonly ConcurrentDictionary<Type, object> _pools = new();

	/// <summary>
	/// Checks the <seealso cref="ObjectPool{T}"/> or creates an object of the specified type using a factory pattern.
	/// If an <seealso cref="ObjectPool{T}"/> exists for the specified type, it will be used to retrieve
	/// a recycled object; otherwise, a new <seealso cref="ObjectPool{T}"/> will be created.
	/// <code> var myObject = GenericFactoryPool.Create(() => new object());</code>
	/// </summary>
	/// <typeparam name="T" >"The generic object type."</typeparam>
	/// <param name="factoryMethod">A function that creates and returns an object of type T. </param>
	/// <returns>The created or recycled object of type T.</returns>
	public static T Create<T>(Func<T> factoryMethod)
	{
		if (_pools.TryGetValue(typeof(T), out object pool) && pool is ObjectPool<T> objectPool)
		{
			return objectPool.GetObject(factoryMethod);
		}
		else
		{
			ObjectPool<T> newObjectPool = new();
			_pools.TryAdd(typeof(T), newObjectPool);
			return newObjectPool.GetObject(factoryMethod);
		}
	}

	/// <summary>
	/// Recycles an object of the specified type back into the object pool if a pool exists.
	/// If the object implements the `IRecyclable` interface, its state is reset before recycling.
	/// <code> GenericFactoryPool.Recycle(myObject);</code>
	/// </summary>
	/// <typeparam name="T">The type of object to recycle.</typeparam>
	/// <param name="item">The object to recycle.</param>
	public static void Recycle<T>(T item)
	{
		if (_pools.TryGetValue(typeof(T), out object pool) && pool is ObjectPool<T> objectPool)
		{
			objectPool.ReturnObject(item);
		}
	}


	public static void ClearPool<T>()
	{
		if (_pools.TryGetValue(typeof(T), out object pool) && pool is ObjectPool<T> objectPool)
		{
			objectPool.Clear();
		}
	}

	public static int GetPoolCount<T>()
	{
		if (_pools.TryGetValue(typeof(T), out object pool) && pool is ObjectPool<T> objectPool)
		{
			return objectPool.Count;
		}
		else
		{
			return 0;
		}
	}

	public static void SetPoolSize<T>(int size, Func<T> factoryMethod)
	{
		if (_pools.TryGetValue(typeof(T), out object pool) && pool is ObjectPool<T> objectPool)
		{
			objectPool.SetSize(size, factoryMethod);
		}
	}

	public static void Prepopulate<T>(int count, Func<T> factoryMethod)
	{
		if (_pools.TryGetValue(typeof(T), out object pool) && pool is ObjectPool<T> objectPool)
		{
			objectPool.Prepopulate(count, factoryMethod);
		}
	}



	/// <summary>
	/// An Ordered object pool of the specified type <typeparamref name="T"/>, used by the Generic Factory Pool to store object types.
	/// Not for outside use. 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	private sealed class ObjectPool<T>
	{
		private readonly ConcurrentQueue<T> _pool = new();
		private Action<T> _resetAction = null;

		/// <summary>
		/// Gets an object from the pool, either by reusing a recycled object or by creating a new one.
		/// </summary>
		/// <param name="factoryMethod">A function that creates and returns an object of type T.</param>
		/// <returns>An object of type T from the pool.</returns>
		internal T GetObject(Func<T> factoryMethod) 
			=> _pool.TryDequeue(out T pooledItem) ? pooledItem : factoryMethod();

		/// <summary>
		/// Returns an object back to the pool for potential reuse.
		/// If the object implements the `IRecyclable` interface, its state is reset before recycling.
		/// </summary>
		/// <param name="item">The object to return to the pool.</param>
		internal void ReturnObject(T item)
		{
			if (item is IRecyclable recyclable) recyclable.ResetState();

			_pool.Enqueue(item);
		}


		internal void Clear()
		{
			while (_pool.TryDequeue(out _)) { }
		}

		internal int Count => _pool.Count;

		internal void SetSize(int size, Func<T> factoryMethod)
		{
			while (_pool.Count > size && _pool.TryDequeue(out _)) { }

			while (_pool.Count < size)
			{
				_pool.Enqueue(factoryMethod());
			}
		}

		internal void Prepopulate(int count, Func<T> factoryMethod)
		{
			for (int i = 0; i < count; i++)
			{
				_pool.Enqueue(factoryMethod());
			}
		}

		internal void SetResetAction(Action<T> resetAction)
		{
			_resetAction = resetAction;
		}


	}

}
