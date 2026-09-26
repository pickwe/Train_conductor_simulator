using System;
using Unigine;

/// <summary>Marks a node as a target that can be used with the E key.</summary>
public partial class EInteractable : Component
{
	[ShowInEditor, Parameter(Group = "Interaction")]
	public bool isAvailable = true;

	public event Action Interacted;
	public Node InteractionNode => node;

	public bool IsAvailable
	{
		get { return isAvailable; }
		set { isAvailable = value; }
	}

	public void Interact()
	{
		if (isAvailable)
			Interacted?.Invoke();
	}
}
