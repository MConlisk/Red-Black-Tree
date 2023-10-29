using System;

namespace QuadrantTree.Interfaces;

/// <summary>
/// Represents an interface for a node in a Red-Black tree, which is a type of self-balancing binary search tree.
/// It maintains balance within the tree by using node colors (red and black) and enforcing specific rules.
/// </summary>
/// <typeparam name="TKey">The type of the key associated with the node.</typeparam>
/// <typeparam name="TValue">The type of the value associated with the node.</typeparam>
public interface IGenericRedBlackNode<TKey, TValue> : IData<TKey, TValue>, IRecyclable
{
	/// <summary>
	/// Gets or sets a value indicating whether this node is red.
	/// The color of a node is used to enforce certain balancing rules within the tree.
	/// The color red signifies that the node has a connection with a neighboring node, 
	/// and it is used to maintain balance in the tree.
	/// </summary>
	bool IsRed { get; set; } // Indicates the node's state

	/// <summary>
	/// Gets a value indicating whether this node has any values or if it's Empty.
	/// This is used when determining if the node is a leaf or not.
	/// </summary>
	bool IsEmpty();

	/// <summary>
	/// Gets or sets the parent node of the current node.
	/// The parent node is the immediate ancestor of the current node in the tree structure.
	/// </summary>
	IGenericRedBlackNode<TKey, TValue> Parent { get; set; } // Reflection to the parent Node

	/// <summary>
	/// Gets or sets the left child node of the current node.
	/// The left child of a node is either a red or black node that is positioned to the left of its parent node.
	/// </summary>
	IGenericRedBlackNode<TKey, TValue> Left { get; set; } // Left Node used in Balancing

	/// <summary>
	/// Gets or sets the right child node of the current node.
	/// The right child of a node is either a red or black node that is positioned to the right of its parent node.
	/// </summary>
	IGenericRedBlackNode<TKey, TValue> Right { get; set; } // Right Node used in Balancing
}
