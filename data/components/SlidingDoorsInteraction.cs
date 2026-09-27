using System;
using System.Collections.Generic;
using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

/// <summary>Independently opens and closes pairs of sliding door leaves.</summary>
public partial class SlidingDoorsInteraction : Component
{
	public class DoorPair
	{
		[Parameter(Title = "Interaction Node")]
		public Node interactionNode;

		[Parameter(Title = "Left Leaf")]
		public Node leftLeaf;

		[Parameter(Title = "Right Leaf")]
		public Node rightLeaf;
	}

	private sealed class DoorRuntime
	{
		public DoorPair Pair;
		public EInteractable Interactable;
		public Action InteractionHandler;
		public Vec3 LeftClosedPosition;
		public Vec3 RightClosedPosition;
		public Vec3 LeftStartPosition;
		public Vec3 RightStartPosition;
		public Vec3 LeftTargetPosition;
		public Vec3 RightTargetPosition;
		public float Elapsed;
		public bool IsOpen;
		public bool IsMoving;
	}

	[Parameter(Title = "Door Pairs", Group = "Sliding Doors")]
	public List<DoorPair> doorPairs = new List<DoorPair>();

	[Parameter(Title = "Open Distance", Group = "Sliding Doors")]
	public float openDistance = 1.0f;

	[Parameter(Title = "Animation Seconds", Group = "Sliding Doors")]
	public float animationSeconds = 1.0f;

	private readonly List<DoorRuntime> runtimes = new List<DoorRuntime>();
	private GameFlowController gameFlow;
	private bool subscribedToGameFlow;

	private void Init()
	{
		if (doorPairs == null)
			return;

		for (int i = 0; i < doorPairs.Count; i++)
		{
			DoorPair pair = doorPairs[i];
			if (pair == null)
				continue;

			DoorRuntime runtime = new DoorRuntime
			{
				Pair = pair,
				LeftClosedPosition = pair.leftLeaf != null ? pair.leftLeaf.Position : Vec3.ZERO,
				RightClosedPosition = pair.rightLeaf != null ? pair.rightLeaf.Position : Vec3.ZERO
			};

			if (pair.interactionNode != null)
			{
				runtime.Interactable = ComponentSystem.GetComponentInChildren<EInteractable>(pair.interactionNode);
				if (runtime.Interactable == null)
					runtime.Interactable = ComponentSystem.GetComponentInParent<EInteractable>(pair.interactionNode);
			}

			if (runtime.Interactable != null)
			{
				runtime.InteractionHandler = () => ToggleDoor(runtime);
				runtime.Interactable.Interacted += runtime.InteractionHandler;
			}
			else
			{
				Log.Warning("SlidingDoorsInteraction: EInteractable was not found for door pair {0}.\n", i);
			}

			runtimes.Add(runtime);
		}
	}

	private void Update()
	{
		FindAndSubscribeToGameFlow();

		float duration = MathLib.Max(0.01f, animationSeconds);
		for (int i = 0; i < runtimes.Count; i++)
		{
			DoorRuntime runtime = runtimes[i];
			if (!runtime.IsMoving)
				continue;

			runtime.Elapsed += Game.IFps;
			float t = MathLib.Min(1.0f, runtime.Elapsed / duration);
			float eased = t * t * (3.0f - 2.0f * t);

			if (runtime.Pair.leftLeaf != null)
				runtime.Pair.leftLeaf.Position = runtime.LeftStartPosition
					+ (runtime.LeftTargetPosition - runtime.LeftStartPosition) * eased;
			if (runtime.Pair.rightLeaf != null)
				runtime.Pair.rightLeaf.Position = runtime.RightStartPosition
					+ (runtime.RightTargetPosition - runtime.RightStartPosition) * eased;

			if (t >= 1.0f)
				runtime.IsMoving = false;
		}
	}

	private void FindAndSubscribeToGameFlow()
	{
		if (gameFlow != null)
			return;

		gameFlow = ComponentSystem.FindComponentInWorld<GameFlowController>();
		if (gameFlow == null)
			return;

		gameFlow.ScenarioTwoStarted += CloseAllDoors;
		subscribedToGameFlow = true;
		if (gameFlow.CurrentPhase == GameFlowController.Phase.ScenarioTwo)
			CloseAllDoors();
	}

	private void CloseAllDoors()
	{
		for (int i = 0; i < runtimes.Count; i++)
		{
			DoorRuntime runtime = runtimes[i];
			runtime.IsOpen = false;
			runtime.IsMoving = false;
			runtime.Elapsed = 0.0f;

			if (runtime.Pair.leftLeaf != null)
				runtime.Pair.leftLeaf.Position = runtime.LeftClosedPosition;
			if (runtime.Pair.rightLeaf != null)
				runtime.Pair.rightLeaf.Position = runtime.RightClosedPosition;
		}
	}

	private void ToggleDoor(DoorRuntime runtime)
	{
		runtime.IsOpen = !runtime.IsOpen;
		runtime.Elapsed = 0.0f;
		runtime.IsMoving = true;

		float distance = runtime.IsOpen ? MathLib.Abs(openDistance) : 0.0f;
		runtime.LeftStartPosition = runtime.Pair.leftLeaf != null
			? runtime.Pair.leftLeaf.Position
			: runtime.LeftClosedPosition;
		runtime.RightStartPosition = runtime.Pair.rightLeaf != null
			? runtime.Pair.rightLeaf.Position
			: runtime.RightClosedPosition;
		runtime.LeftTargetPosition = runtime.LeftClosedPosition + new Vec3(-distance, 0.0f, 0.0f);
		runtime.RightTargetPosition = runtime.RightClosedPosition + new Vec3(distance, 0.0f, 0.0f);
	}

	private void Shutdown()
	{
		if (gameFlow != null && subscribedToGameFlow)
			gameFlow.ScenarioTwoStarted -= CloseAllDoors;

		for (int i = 0; i < runtimes.Count; i++)
		{
			DoorRuntime runtime = runtimes[i];
			if (runtime.Interactable != null && runtime.InteractionHandler != null)
				runtime.Interactable.Interacted -= runtime.InteractionHandler;
		}
		runtimes.Clear();
	}
}
