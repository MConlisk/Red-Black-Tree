using System;

namespace QuadrantTree.Interfaces;

/// <summary>
/// Represents an interface for objects that encapsulate key-value data pairs.
/// Objects implementing this interface expose methods to access and modify
/// their internal key and value components, as well as a property to identify
/// their underlying data type.
/// </summary>
/// <typeparam name="TKey">The type of the key component in the data pair.</typeparam>
/// <typeparam name="TValue">The type of the value component in the data pair.</typeparam>
public interface IData<TKey, TValue>
{
	/// <summary>
	/// Gets the <see cref="Type"/> of the underlying data type, allowing
	/// identification of the data type represented by the object.
	/// </summary>
	Type IsType { get; }

	/// <summary>
	/// Retrieves the key component of the data pair.
	/// </summary>
	/// <returns>The key component of the data pair.</returns>
	TKey GetKey();

	/// <summary>
	/// Sets the key component of the data pair to the specified value.
	/// </summary>
	/// <param name="value">The new value to set as the key component.</param>
	void SetKey(TKey value);

	/// <summary>
	/// Retrieves the value component of the data pair.
	/// </summary>
	/// <returns>The value component of the data pair.</returns>
	TValue GetValue();

	/// <summary>
	/// Sets the value component of the data pair to the specified value.
	/// </summary>
	/// <param name="value">The new value to set as the value component.</param>
	void SetValue(TValue value);
}
