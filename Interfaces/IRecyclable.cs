namespace QuadrantTree.Interfaces;

/// <summary>
/// Represents an interface for objects that can be recycled by resetting their internal state.
/// Objects implementing this interface should provide a method to reset their state to a
/// predefined initial condition, allowing them to be reused efficiently.
/// </summary>
public interface IRecyclable
{
	/// <summary>
	/// Resets the internal state of the object to its initial condition, making it
	/// available for reuse without the need for creating a new instance.
	/// </summary>
	void ResetState();
}
