using QuadrantTree.Factories;
using QuadrantTree.Interfaces;

using System;
using System.Collections.Generic;

namespace QuadrantTree.Nodes;

/// <summary>
/// Represents a node in a generic Red-Black Tree used to store key-value pairs.
/// This class supports operations for managing the node's key, value, color, and child nodes.
/// </summary>
/// <typeparam name="TValue">The type of value associated with the node.</typeparam>
public class RedBlackTreeGenericNode<TValue>(int? key, TValue value) : IGenericRedBlackNode<int, TValue>
{
	private int? _key = key;
	private TValue _value = value;

	/// <summary>
	/// Gets the key associated with the node.
	/// </summary>
	public int GetKey() => _key == null ? default : _key.Value;

	/// <summary>
	/// Sets the key associated with the node to the specified value.
	/// </summary>
	public void SetKey(int value) => _key ??= value;

	/// <summary>
	/// Gets the value associated with the node.
	/// </summary>
	public TValue GetValue() => _value == null ? default : _value;

	/// <summary>
	/// Sets the value associated with the node to the specified value.
	/// </summary>
	public void SetValue(TValue value) => _value = value;

	/// <summary>
	/// Gets the type of the value associated with the node.
	/// </summary>
	public Type IsType => typeof(TValue);

	/// <summary>
	/// Gets or sets a value indicating whether the node is red in the Red-Black Tree.
	/// </summary>
	public bool IsRed { get; set; } = true;

	/// <summary>
	/// Checks if the node is empty (has no key).
	/// </summary>
	public bool IsEmpty() => _key == null;

	/// <summary>
	/// Gets or sets the parent node of the current node.
	/// </summary>
	public IGenericRedBlackNode<int, TValue> Parent { get; set; } = null;

	/// <summary>
	/// Gets or sets the left child node of the current node.
	/// </summary>
	public IGenericRedBlackNode<int, TValue> Left { get; set; } = null;

	/// <summary>
	/// Gets or sets the right child node of the current node.
	/// </summary>
	public IGenericRedBlackNode<int, TValue> Right { get; set; } = null;

	/// <summary>
	/// Initializes a new instance of the class using a key-value pair.
	/// </summary>
	/// <param name="data">The key-value pair to initialize the node with.</param>
	public RedBlackTreeGenericNode(KeyValuePair<int, TValue> data) : this(data.Key, data.Value) { }

	/// <summary>
	/// Gets an available child node from the node's left or right based on availability.
	/// </summary>
	/// <param name="node">An available child node, or null if no children are available.</param>
	public void GetAvailableNode(out RedBlackTreeGenericNode<TValue> node)
	{
		if (Right == null)
		{
			Right = GenericFactoryPool.Create(() => new RedBlackTreeGenericNode<TValue>(null, default));
			node = (RedBlackTreeGenericNode<TValue>)Right;
		}
		else if (Left == null)
		{
			Left = GenericFactoryPool.Create(() => new RedBlackTreeGenericNode<TValue>(null, default));
			node = (RedBlackTreeGenericNode<TValue>)Left;
		}
		else
		{
			node = null;
		}
	}

	/// <summary>
	/// Sets an available child node with the key and value from the provided node.
	/// </summary>
	/// <param name="newNode">The node to set as a child node if an empty slot is available.</param>
	/// <returns>The child node that was set, or null if no empty slots were available.</returns>
	public RedBlackTreeGenericNode<TValue> SetAvailableNode(RedBlackTreeGenericNode<TValue> newNode)
	{
		if (Right.IsEmpty())
		{
			Right.SetKey(newNode.GetKey());
			Right.SetValue(newNode.GetValue());
			return (RedBlackTreeGenericNode<TValue>)Right;
		}
		else if (Left.IsEmpty())
		{
			Left.SetKey(newNode.GetKey()); ;
			Left.SetValue(newNode.GetValue());
			return (RedBlackTreeGenericNode<TValue>)Left;
		}
		else
			return null;
	}

	/// <summary>
	/// Resets the internal state of the node to its default values, making it available for reuse.
	/// </summary>
	public void ResetState()
	{
		_key = default;
		_value = default;

		IsRed = true;

		Parent = null;
		Left = null;
		Right = null;
	}
}
