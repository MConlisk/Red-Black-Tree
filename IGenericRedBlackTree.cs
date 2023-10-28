using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;

namespace QuadrantTree.Interfaces;

/// <summary>
/// Represents a generic Red-Black Tree data structure that allows efficient
/// insertion, removal, and retrieval of key-value pairs with integer keys.
/// This tree maintains a sorted order of keys for quick access to values.
/// </summary>
/// <typeparam name="TValue">The type of values stored in the tree.</typeparam>
public interface IGenericRedBlackTree<TValue> : IEnumerable, IRecyclable
{
	/// <summary>
	/// Gets a read-only collection of unique keys present in the tree.
	/// </summary>
	ReadOnlyCollection<int> Index { get; }

	/// <summary>
	/// Gets or sets the value associated with the specified key.
	/// </summary>
	/// <param name="key">The key to access or modify.</param>
	/// <returns>The value associated with the specified key.</returns>
	TValue this[int key] { get; set; }

	/// <summary>
	/// Retrieves the value associated with the specified key without modifying the tree.
	/// </summary>
	/// <param name="key">The key to retrieve the value for.</param>
	/// <returns>The value associated with the specified key, if found; otherwise, the default value of TValue.</returns>
	TValue GetValue(int key);

	/// <summary>
	/// Inserts a key-value pair into the Red-Black Tree.
	/// </summary>
	/// <param name="key">The key to insert.</param>
	/// <param name="value">The value associated with the key.</param>
	void Insert(int key, TValue value);

	/// <summary>
	/// Removes the key-value pair associated with the specified key from the tree.
	/// </summary>
	/// <param name="key">The key to remove.</param>
	void Remove(int key);

	/// <summary>
	/// Updates the value associated with the specified key in the tree.
	/// </summary>
	/// <param name="key">The key to update.</param>
	/// <param name="value">The new value to associate with the key.</param>
	void Update(int key, TValue value);

	/// <summary>
	/// Retrieves all key-value pairs in sorted order based on their keys.
	/// </summary>
	/// <returns>An enumerable collection of key-value pairs in ascending order of keys.</returns>
	IEnumerable<KeyValuePair<int, TValue>> GetAllInOrder();
}
