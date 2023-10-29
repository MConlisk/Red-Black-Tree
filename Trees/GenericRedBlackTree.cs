using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Collections;
using QuadrantTree.Interfaces;
using QuadrantTree.Nodes;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.ObjectModel;
using QuadrantTree.Factories;

namespace QuadrantTree.Trees;

/// <summary>
/// This is a Red-Black Tree with a Generic value type.
/// additionally, an integer type as a key 
/// </summary>
/// <typeparam name="TValue">The Generic _value Type</typeparam>
/// <param name="maxSize">The Maximum size this tree is allowed to grow.</param>
public sealed class GenericRedBlackTree<TValue>(int? maxSize) : IGenericRedBlackTree<TValue>
{
	/// <summary>
	/// Private fields for the tree.
	/// </summary>
	private HashSet<int> _index = GenericFactoryPool.Create(() => new HashSet<int>());
	private int? _maxSize = maxSize == default ? default : maxSize < 0 ? 0 : maxSize;
	private RedBlackTreeGenericNode<TValue> _root;
	private int _traversal;

	/// <summary>
	/// Sets the Traversing method for traversing through to tree.
	/// 0 for In-Order Traversal, 1 for Pre-Order Traversal, 2 for Level-Order Traversal
	/// </summary>
	/// <param name="traversal"></param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public void SetTraversal(int traversal) => _traversal = !(traversal < 3) && !(traversal >= 0)
			? throw new ArgumentOutOfRangeException(nameof(traversal), $"Expected 0, 1 or 2 for the traversal parameter, but received {traversal} instead.")
			: traversal;

	/// <summary>
	/// Initializes a new instance of the <see cref="GenericRedBlackTree{TValue}"/> class.
	/// </summary>
	/// <param name="maxSize">An optional maximum size for the tree. If null, the tree has no maximum size limit.</param>
	public GenericRedBlackTree() : this(default) { }

	/// <summary>
	/// Gets the set of keys in the Red-Black Tree.
	/// </summary>
	public ReadOnlyCollection<int> Index => new(_index.ToList());

	/// <summary>
	/// Gets or sets the maximum size of the tree. If set to null, there is no maximum size limit.
	/// </summary>
	public int MaxSize { get => _maxSize.Value; set { _maxSize ??= value; } }

	/// <summary>
	/// Checks if the Red-Black Tree contains a specific key.
	/// </summary>
	/// <param name="id">The key to check for.</param>
	/// <returns>True if the key is present in the tree; otherwise, false.</returns>
	public bool Contains(int id) => _index.Contains(id);

	/// <summary>
	/// Gets the number of elements in the Red-Black Tree.
	/// </summary>
	public int Count { get => _index.Count; }

	/// <summary>
	/// Inserts a new key-value pair into the Red-Black Tree.
	/// </summary>
	/// <param name="key">The key to insert.</param>
	/// <param name="value">The value associated with the key.</param>
	public void Insert(int key, TValue value)
	{
		if (_index.Add(key))
		{
			var newNode = GenericFactoryPool.Create(() => new RedBlackTreeGenericNode<TValue>(key, value));

			if (_root == null)
			{
				_root = newNode;
				_root.IsRed = false;
			}
			else if (_root.Left == null)
			{
				_root.Left = newNode;
			}
			else if (_root.Right == null)
			{
				_root.Right = newNode;
			}
			else
			{
				InsertLeaf(_root, newNode);
			}
			return;
		}
		throw new DuplicateNameException(key.ToString());
	}

	/// <summary>
	/// Inserts a new leaf node into the Red-Black Tree.
	/// </summary>
	/// <param name="current">The current node to start the insertion from.</param>
	/// <param name="newNode">The new node to insert as a leaf.</param>
	private static void InsertLeaf(RedBlackTreeGenericNode<TValue> current, RedBlackTreeGenericNode<TValue> newNode)
	{
		ArgumentNullException.ThrowIfNull(current);
		ArgumentNullException.ThrowIfNull(newNode);

		var queue = GenericFactoryPool.Create(() => new ConcurrentQueue<RedBlackTreeGenericNode<TValue>>());
		queue.Enqueue(current);

		while (queue.TryDequeue(out RedBlackTreeGenericNode<TValue> parent))
		{
			if (parent.Left == null)
			{
				parent.Left = newNode;
				newNode.Parent = parent;
				break;
			}
			else if (parent.Right == null)
			{
				parent.Right = newNode;
				newNode.Parent = parent;
				break;
			}
			else
			{
				queue.Enqueue((RedBlackTreeGenericNode<TValue>)parent.Left);
				queue.Enqueue((RedBlackTreeGenericNode<TValue>)parent.Right);
				continue;
			}
		}
		GenericFactoryPool.Recycle(queue);
	}

	/// <summary>
	/// Removes a key-value pair from the Red-Black Tree.
	/// </summary>
	/// <param name="key">The key to remove.</param>
	public void Remove(int key)
	{
		var node = GenericFactoryPool.Create(() => FindNodeContainingKey(_root, key));
		if (node == null)
			return;
		if (_index.Remove(key))
		{
			RedBlackTreeGenericNode<TValue> replacementNode = node.Left == null || node.Right == null ? node : Successor(ref node);
			RedBlackTreeGenericNode<TValue> child = (RedBlackTreeGenericNode<TValue>)(replacementNode.Left ?? replacementNode.Right);

			if (child != null)
				child.Parent = replacementNode.Parent;

			if (replacementNode.Parent == null)
				_root = child;
			else if (replacementNode == replacementNode.Parent.Left)
				replacementNode.Parent.Left = child;
			else
				replacementNode.Parent.Right = child;

			if (replacementNode != node)
				node.SetValue(replacementNode.GetValue());

			if (!replacementNode.IsRed)
				FixRemove(ref child, (RedBlackTreeGenericNode<TValue>)replacementNode.Parent);

			// Recycle of the node after all operations are complete
			GenericFactoryPool.Recycle(node);
		}
	}

	/// <summary>
	/// Updates the value associated with a key in the Red-Black Tree.
	/// </summary>
	/// <param name="key">The key to update.</param>
	/// <param name="value">The new value to associate with the key.</param>
	public void Update(int key, TValue value)
	{
		Remove(key);
		Insert(key, value);
	}

	/// <summary>
	/// Gets the value associated with a specific key in the Red-Black Tree.
	/// </summary>
	/// <param name="key">The key to look up.</param>
	/// <returns>The value associated with the key.</returns>
	public TValue GetValue(int key)
	{
		if (_index.Contains(key))
		{
			var node = FindNodeContainingKey(_root, key);
			return node != null ? node.GetValue() : throw new KeyNotFoundException(key.ToString());
		}
		throw new KeyNotFoundException(key.ToString());
	}	

	/// <summary>
	/// Finds a node in the Red-Black Tree that contains a specific key.
	/// </summary>
	/// <param name="current">The current node to start the search from.</param>
	/// <param name="key">The key to search for.</param>
	/// <returns>The node containing the key, or null if not found.
	private static RedBlackTreeGenericNode<TValue> FindNodeContainingKey(RedBlackTreeGenericNode<TValue> current, int key)
	{
		RedBlackTreeGenericNode<TValue> result = null;  // Initialize the result node

		while (current != null)
		{
			int compareResult = key.CompareTo(current.GetKey());
			if (compareResult == 0)
			{
				result = current;  // Update the result if key is found
				break;
			}
			current = (RedBlackTreeGenericNode<TValue>)(compareResult < 0 ? current.Left : current.Right);
		}
		return result;  // Return the result node, if found, or null
	}

	/// <summary>
	/// Finds the minimum node in a given Red-Black Tree.
	/// </summary>
	/// <param name="node">The root node of the tree.</param>
	/// <returns>The minimum node in the tree.
	private static RedBlackTreeGenericNode<TValue> Minimum(RedBlackTreeGenericNode<TValue> node)
	{
		while (node.Left != null)
			node = (RedBlackTreeGenericNode<TValue>)node.Left;
		return node;
	}

	/// <summary>
	/// Finds the successor node in a given Red-Black Tree.
	/// </summary>
	/// <param name="node">The node to find the successor for.
	/// <returns>The successor node of the given node.
	private static RedBlackTreeGenericNode<TValue> Successor(ref RedBlackTreeGenericNode<TValue> node)
	{
		if (node.Right != null)
			return Minimum((RedBlackTreeGenericNode<TValue>)node.Right);

		var parent = node.Parent;
		while (parent != null && node == parent.Right)
		{
			node = (RedBlackTreeGenericNode<TValue>)parent;
			parent = parent.Parent;
		}
		return (RedBlackTreeGenericNode<TValue>)parent;
	}

	/// <summary>
	/// Fixes the tree structure after an insert operation.
	/// </summary+---
	/// <param name="node">The newly inserted node.
	private void FixInsert(ref RedBlackTreeGenericNode<TValue> node)
	{
		while (node != null && node.Parent != null && node.Parent.IsRed)
		{
			if (node.Parent == node.Parent.Parent.Left)
			{
				var uncle = node.Parent.Parent.Right;

				if (uncle != null && uncle.IsRed)
				{
					// Case 1: Recoloring
					node.Parent.IsRed = false;
					uncle.IsRed = false;
					if (node.Parent.Parent != null)
					{
						node.Parent.Parent.IsRed = true;
						node = (RedBlackTreeGenericNode<TValue>)node.Parent.Parent;
					}
				}
				else
				{
					if (node == node.Parent.Right)
					{
						// Case 2: Left rotation
						node = (RedBlackTreeGenericNode<TValue>)node.Parent;
						RotateLeft(node);
					}

					// Case 3: Right rotation
					node.Parent.IsRed = false;
					if (node.Parent.Parent != null)
					{
						node.Parent.Parent.IsRed = true;
						RotateRight((RedBlackTreeGenericNode<TValue>)node.Parent.Parent);
					}
				}
			}
			else
			{
				var uncle = node.Parent.Parent.Left;

				if (uncle != null && uncle.IsRed)
				{
					// Case 1: Recoloring
					node.Parent.IsRed = false;
					uncle.IsRed = false;
					if (node.Parent.Parent != null)
					{
						node.Parent.Parent.IsRed = true;
						node = (RedBlackTreeGenericNode<TValue>)node.Parent.Parent;
					}
				}
				else
				{
					if (node == node.Parent.Left)
					{
						// Case 2: Right rotation
						node = (RedBlackTreeGenericNode<TValue>)node.Parent;
						RotateRight(node);
					}

					// Case 3: Left rotation
					node.Parent.IsRed = false;
					if (node.Parent.Parent != null)
					{
						node.Parent.Parent.IsRed = true;
						RotateLeft((RedBlackTreeGenericNode<TValue>)node.Parent.Parent);
					}
				}
			}
		}
		_root.IsRed = false;
	}

	/// <summary>
	/// Fixes the tree structure after a remove operation.
	/// </summary>
	/// <param name="xNode">The node to fix.
	/// <param name="parent">The parent of the node to fix.
	private void FixRemove(ref RedBlackTreeGenericNode<TValue> xNode, RedBlackTreeGenericNode<TValue> parent)
	{

		while (xNode != _root && (xNode == null || !xNode.IsRed))
		{
			if (xNode == parent.Left)
			{
				RedBlackTreeGenericNode<TValue> sibling = (RedBlackTreeGenericNode<TValue>)parent.Right;

				if (sibling.IsRed)
				{
					sibling.IsRed = false;
					parent.IsRed = true;
					RotateLeft(parent);
					sibling = (RedBlackTreeGenericNode<TValue>)parent.Right;
				}

				if ((sibling.Left == null || !sibling.Left.IsRed) &&
					(sibling.Right == null || !sibling.Right.IsRed))
				{
					sibling.IsRed = true;
					xNode = parent;
					parent = (RedBlackTreeGenericNode<TValue>)xNode.Parent;
				}
				else
				{
					if (sibling.Right == null || !sibling.Right.IsRed)
					{
						sibling.Left.IsRed = false;
						sibling.IsRed = true;
						RotateRight(sibling);
						sibling = (RedBlackTreeGenericNode<TValue>)parent.Right;
					}

					sibling.IsRed = parent.IsRed;
					parent.IsRed = false;
					if (sibling.Right != null)
						sibling.Right.IsRed = false;
					RotateLeft(parent);
					xNode = _root;
				}
			}
			else
			{
				RedBlackTreeGenericNode<TValue> sibling = (RedBlackTreeGenericNode<TValue>)parent.Left;

				if (sibling.IsRed)
				{
					sibling.IsRed = false;
					parent.IsRed = true;
					RotateRight(parent);
					sibling = (RedBlackTreeGenericNode<TValue>)parent.Left;
				}

				if ((sibling.Left == null || !sibling.Left.IsRed) &&
					(sibling.Right == null || !sibling.Right.IsRed))
				{
					sibling.IsRed = true;
					xNode = parent;
					parent = (RedBlackTreeGenericNode<TValue>)xNode.Parent;
				}
				else
				{
					if (sibling.Left == null || !sibling.Left.IsRed)
					{
						sibling.Right.IsRed = false;
						sibling.IsRed = true;
						RotateLeft(sibling);
						sibling = (RedBlackTreeGenericNode<TValue>)parent.Left;
					}

					sibling.IsRed = parent.IsRed;
					parent.IsRed = false;
					if (sibling.Left != null)
						sibling.Left.IsRed = false;
					RotateRight(parent);
					xNode = _root;
				}
			}
		}

		if (xNode != null)
			xNode.IsRed = false;
	}

	/// <summary>
	/// Rotates the tree to the left, preserving the Red-Black Tree properties.
	/// </summary>
	/// <param name="xNode">The node to rotate.
	private void RotateLeft(RedBlackTreeGenericNode<TValue> xNode)
	{
		RedBlackTreeGenericNode<TValue> yNode = (RedBlackTreeGenericNode<TValue>)xNode.Right;
		xNode.Right = yNode.Left;

		if (yNode.Left != null)
			yNode.Left.Parent = xNode;

		yNode.Parent = xNode.Parent;

		if (xNode.Parent == null)
			_root = yNode;
		else if (xNode == xNode.Parent.Left)
			xNode.Parent.Left = yNode;
		else
			xNode.Parent.Right = yNode;

		yNode.Left = xNode;
		xNode.Parent = yNode;
	}

	/// <summary>
	/// Rotates the tree to the right, preserving the Red-Black Tree properties.
	/// </summary>
	/// <param name="yNode">The node to rotate.
	private void RotateRight(RedBlackTreeGenericNode<TValue> yNode)
	{
		RedBlackTreeGenericNode<TValue> xNode = (RedBlackTreeGenericNode<TValue>)yNode.Left;
		yNode.Left = xNode.Right;

		if (xNode.Right != null)
			xNode.Right.Parent = yNode;

		xNode.Parent = yNode.Parent;

		if (yNode.Parent == null)
			_root = xNode;
		else if (yNode == yNode.Parent.Left)
			yNode.Parent.Left = xNode;
		else
			yNode.Parent.Right = xNode;

		xNode.Right = yNode;
		yNode.Parent = xNode;
	}

	/// <summary>
	/// Gets or sets the value associated with the specified key in the Red-Black Tree.
	/// </summary>
	/// <param name="key">The key to access or modify.</param>
	/// <returns>The value associated with the key.</returns>
	public TValue this[int key]
	{
		get
		{
			var node = FindNodeContainingKey(_root, key);
			return node != null ? node.GetValue() : throw new KeyNotFoundException(key.ToString());
		}
		set
		{
			var node = FindNodeContainingKey(_root, key);
			if (node.GetKey() == key)
			{
				node.SetValue(value);
			}
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through the elements of the Red-Black Tree in an in-order traversal.
	/// </summary>
	/// <returns>An enumerator that can be used to iterate through the elements of the Red-Black Tree in an in-order traversal.</returns>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public IEnumerator<KeyValuePair<int, TValue>> GetEnumerator()
	{
		switch (_traversal)
		{
			case 0: // In Order Traversal
				_traversal = 0;
				foreach (var KeyValueItem in InOrderTraversal(_root))
				{
					yield return KeyValueItem;
				}
				break;

			case 1: // Pre-Order Traversal
				_traversal = 1;
				foreach (var KeyValueItem in PreOrderTraversal(_root))
				{
					yield return KeyValueItem;
				}
				break;

			case 2: // Level Order Traversal
			default:
				_traversal = 2;
				foreach (var KeyValueItem in LevelOrderTraversal(_root))
				{
					yield return KeyValueItem;
				}
				break;

		}
	}

	/// <summary>
	/// Performs an in-order traversal of the Red-Black Tree, yielding key-value pairs.
	/// </summary>
	/// <param name="node">The root node of the tree to traverse.</param>
	/// <returns>An enumerable of key-value pairs in in-order traversal order.</returns>
	private static IEnumerable<KeyValuePair<int, TValue>> InOrderTraversal(RedBlackTreeGenericNode<TValue> node)
	{
		if (node == null)
			yield break;

		foreach (var leftNode in InOrderTraversal((RedBlackTreeGenericNode<TValue>)node.Left))
		{
			yield return new(leftNode.Key, leftNode.Value);
		}

		yield return new KeyValuePair<int, TValue>(node.GetKey(), node.GetValue());

		foreach (var rightNode in InOrderTraversal((RedBlackTreeGenericNode<TValue>)node.Right))
		{
			yield return new(rightNode.Key, rightNode.Value);
		}
	}

	/// <summary>
	/// Performs a pre-order traversal of the Red-Black Tree, yielding key-value pairs.
	/// </summary>
	/// <param name="node">The root node of the tree to traverse.</param>
	/// <returns>An enumerable of key-value pairs in pre-order traversal order.</returns>
	private static IEnumerable<KeyValuePair<int, TValue>> PreOrderTraversal(RedBlackTreeGenericNode<TValue> node)
	{
		if (node == null)
			yield break;

		yield return new KeyValuePair<int, TValue>(node.GetKey(), node.GetValue());

		foreach (var leftNode in InOrderTraversal((RedBlackTreeGenericNode<TValue>)node.Left))
		{
			yield return new(leftNode.Key, leftNode.Value);
		}

		foreach (var rightNode in InOrderTraversal((RedBlackTreeGenericNode<TValue>)node.Right))
		{
			yield return new(rightNode.Key, rightNode.Value);
		}
	}

	/// <summary>
	/// Performs a level-order traversal of the Red-Black Tree, yielding key-value pairs.
	/// </summary>
	/// <param name="node">The root node of the tree to traverse.</param>
	/// <returns>An enumerable of key-value pairs in level-order traversal order.</returns>
	private static IEnumerable<KeyValuePair<int, TValue>> LevelOrderTraversal(RedBlackTreeGenericNode<TValue> node)
	{
		var queue = GenericFactoryPool.Create(() => new ConcurrentQueue<RedBlackTreeGenericNode<TValue>>());
		queue.Enqueue(node);

		while (queue.TryDequeue(out RedBlackTreeGenericNode<TValue> currentNode))
		{
			yield return new KeyValuePair<int, TValue>(node.GetKey(), node.GetValue());

			if (currentNode.Left != null) queue.Enqueue((RedBlackTreeGenericNode<TValue>)currentNode.Left);
			if (currentNode.Right != null) queue.Enqueue((RedBlackTreeGenericNode<TValue>)currentNode.Right);
		}
		GenericFactoryPool.Recycle(queue);
	}

	/// <summary>
	/// Returns all elements of the Red-Black Tree in in-order traversal order.
	/// </summary>
	/// <returns>An enumerable of all key-value pairs in in-order traversal order.</returns>
	public IEnumerable<KeyValuePair<int, TValue>> GetAllInOrder()
	{
		foreach (var item in InOrderTraversal(_root))
		{
			yield return item;
		}
	}

	/// <summary>
	/// Returns all elements of the Red-Black Tree in pre-order traversal order.
	/// </summary>
	/// <returns>An enumerable of all key-value pairs in pre-order traversal order.</returns>
	public IEnumerable<KeyValuePair<int, TValue>> GetAllPreOrder()
	{
		foreach (var item in PreOrderTraversal(_root))
		{
			yield return item;
		}
	}

	/// <summary>
	/// Returns all elements of the Red-Black Tree in level-order traversal order.
	/// </summary>
	/// <returns>An enumerable of all key-value pairs in level-order traversal order.</returns>
	public IEnumerable<KeyValuePair<int, TValue>> GetAllLevelOrder()
	{
		foreach (var item in LevelOrderTraversal(_root))
		{
			yield return item;
		}
	}

	/// <summary>
	/// Resets the state of the Red-Black Tree, effectively clearing it and resetting any configuration options.
	/// </summary>
	public void ResetState()
	{
		GenericFactoryPool.Recycle(_index);
		GenericFactoryPool.Recycle(_root);
		_index.Clear();
		
		_index = GenericFactoryPool.Create(() => new HashSet<int>());
		_maxSize = null;

	}
}
