using System.Collections.Generic;
using Unigine;

/// <summary>
/// Shows one complete passenger model at a time. Each emotion model contains
/// its own legacy skinned meshes and baked animation.
/// </summary>
public partial class PassengerEmotionAnimator : Component
{
	[Parameter(Group = "Passenger Models")]
	public Node initialPoseModel;
	[Parameter(Group = "Passenger Models")]
	public Node aggressiveModel;
	[Parameter(Group = "Passenger Models")]
	public Node confusedModel;
	[Parameter(Group = "Passenger Models")]
	public Node positiveNodModel;
	[Parameter(Group = "Passenger Models")]
	public Node carrierNodModel;

	private PetCarrierScenarioController scenario;
	private GameFlowController gameFlow;
	private bool subscribed;
	private bool subscribedToFlow;
	private bool passengerHiddenForScenarioTwo;
	private Node activeModel;
	private readonly Dictionary<Node, List<SurfaceMask>> modelSurfaceMasks = new Dictionary<Node, List<SurfaceMask>>();

	private sealed class SurfaceMask
	{
		public Unigine.Object Object;
		public int[] ViewportMasks;
		public int[] ShadowMasks;
	}

	private void Init()
	{
		ResolveModels();
		PrepareModels();
		ShowInitialPose();
	}

	private void Update()
	{
		FindAndSubscribeToScenario();
		FindAndSubscribeToGameFlow();
	}

	private void FindAndSubscribeToScenario()
	{
		if (scenario != null) return;

		scenario = ComponentSystem.FindComponentInWorld<PetCarrierScenarioController>();
		if (scenario == null) return;

		scenario.PassengerConversationChanged += OnConversationChanged;
		scenario.PassengerEmotionRequested += OnEmotionRequested;
		subscribed = true;

		OnConversationChanged(scenario.PassengerConversationActive);
	}

	private void FindAndSubscribeToGameFlow()
	{
		if (gameFlow != null) return;

		gameFlow = ComponentSystem.FindComponentInWorld<GameFlowController>();
		if (gameFlow == null) return;

		gameFlow.ScenarioTwoIntroductionRequested += OnScenarioTwoStarted;
		gameFlow.ScenarioTwoStarted += OnScenarioTwoStarted;
		subscribedToFlow = true;
		if (gameFlow.CurrentPhase == GameFlowController.Phase.ScenarioTwoIntroduction
			|| gameFlow.CurrentPhase == GameFlowController.Phase.ScenarioTwo
			|| gameFlow.CurrentPhase == GameFlowController.Phase.ScenarioTwoReview
			|| gameFlow.CurrentPhase == GameFlowController.Phase.RunSummary
			|| gameFlow.CurrentPhase == GameFlowController.Phase.Finished)
			OnScenarioTwoStarted();
	}

	private void OnScenarioTwoStarted()
	{
		passengerHiddenForScenarioTwo = true;
		HidePassenger();
		node.Enabled = false;
	}

	private void ResolveModels()
	{
		if (initialPoseModel == null) initialPoseModel = node.FindNode("Исходное положение", 1);
		if (aggressiveModel == null) aggressiveModel = node.FindNode("Aggressive1", 1);
		if (confusedModel == null) confusedModel = node.FindNode("Confused1", 1);
		if (positiveNodModel == null) positiveNodModel = node.FindNode("PositiveNod1", 1);
		if (carrierNodModel == null) carrierNodModel = node.FindNode("Кивок с сумкой1", 1);
	}

	private void PrepareModels()
	{
		PrepareModel(initialPoseModel);
		PrepareModel(aggressiveModel);
		PrepareModel(confusedModel);
		PrepareModel(positiveNodModel);
		PrepareModel(carrierNodModel);
	}

	private void PrepareModel(Node model)
	{
		if (model == null) return;
		model.Enabled = true;
		List<SurfaceMask> surfaces = new List<SurfaceMask>();
		CaptureSurfaceMasks(model, surfaces);
		modelSurfaceMasks[model] = surfaces;
	}

	private static void CaptureSurfaceMasks(Node current, List<SurfaceMask> surfaces)
	{
		if (current == null) return;

		Unigine.Object obj = current as Unigine.Object;
		if (obj != null)
		{
			int[] viewportMasks = new int[obj.NumSurfaces];
			int[] shadowMasks = new int[obj.NumSurfaces];
			for (int i = 0; i < obj.NumSurfaces; i++)
			{
				viewportMasks[i] = obj.GetViewportMask(i);
				shadowMasks[i] = obj.GetShadowMask(i);
			}
			surfaces.Add(new SurfaceMask
			{
				Object = obj,
				ViewportMasks = viewportMasks,
				ShadowMasks = shadowMasks
			});
		}

		for (int i = 0; i < current.NumChildren; i++)
			CaptureSurfaceMasks(current.GetChild(i), surfaces);
	}

	private void OnConversationChanged(bool active)
	{
		if (passengerHiddenForScenarioTwo)
		{
			HidePassenger();
			return;
		}
		if (scenario == null)
		{
			ShowInitialPose();
			return;
		}
		if (!active)
		{
			if (KeepDeliveredCarrierModel()) return;
			ShowInitialPose();
			return;
		}
		if (scenario.CurrentPassengerEmotion == PetCarrierScenarioController.PassengerEmotion.None)
		{
			ShowInitialPose();
			return;
		}

		PlayEmotion(scenario.CurrentPassengerEmotion);
	}

	private bool KeepDeliveredCarrierModel()
	{
		if (!scenario.DecisionHistory.Contains("CARRIER_DELIVERED")
			|| carrierNodModel == null)
			return false;

		if (activeModel != carrierNodModel)
		{
			ShowOnlyModel(carrierNodModel);
			RestartAnimationHierarchy(carrierNodModel);
		}
		return true;
	}

	private void OnEmotionRequested(PetCarrierScenarioController.PassengerEmotion emotion)
	{
		if (passengerHiddenForScenarioTwo) return;
		if (emotion == PetCarrierScenarioController.PassengerEmotion.None)
		{
			ShowInitialPose();
			return;
		}

		PlayEmotion(emotion);
	}

	private void PlayEmotion(PetCarrierScenarioController.PassengerEmotion emotion)
	{
		Node model = null;
		string modelName = emotion.ToString();
		switch (emotion)
		{
			case PetCarrierScenarioController.PassengerEmotion.Aggressive:
				model = aggressiveModel;
				modelName = "Aggressive1";
				break;
			case PetCarrierScenarioController.PassengerEmotion.Confused:
				model = confusedModel;
				modelName = "Confused1";
				break;
			case PetCarrierScenarioController.PassengerEmotion.PositiveNod:
				model = positiveNodModel;
				modelName = "PositiveNod1";
				break;
			case PetCarrierScenarioController.PassengerEmotion.CarrierNod:
				model = carrierNodModel;
				modelName = "Кивок с сумкой1";
				break;
		}

		if (!ShowAnimatedModel(model, modelName))
			ShowInitialPose();
	}

	private void ShowInitialPose()
	{
		if (initialPoseModel == null)
		{
			Log.Warning("PassengerEmotionAnimator: assign the 'Исходное положение' model in the component properties.\n");
			return;
		}

		ShowOnlyModel(initialPoseModel);
		ResetAnimationHierarchy(initialPoseModel);
	}

	private bool ShowAnimatedModel(Node model, string modelName)
	{
		if (model == null)
		{
			Log.Warning("PassengerEmotionAnimator: assign the '{0}' model in the component properties.\n", modelName);
			return false;
		}

		ShowOnlyModel(model);
		RestartAnimationHierarchy(model);
		return true;
	}

	private void ShowOnlyModel(Node model)
	{
		SetModelVisible(model, true);
		HideModelUnlessActive(initialPoseModel, model);
		HideModelUnlessActive(aggressiveModel, model);
		HideModelUnlessActive(confusedModel, model);
		HideModelUnlessActive(positiveNodModel, model);
		HideModelUnlessActive(carrierNodModel, model);
		activeModel = model;
	}

	private void HidePassenger()
	{
		HideModelUnlessActive(initialPoseModel, null);
		HideModelUnlessActive(aggressiveModel, null);
		HideModelUnlessActive(confusedModel, null);
		HideModelUnlessActive(positiveNodModel, null);
		HideModelUnlessActive(carrierNodModel, null);
		activeModel = null;
	}

	private void HideModelUnlessActive(Node model, Node modelToShow)
	{
		if (model == null || model == modelToShow) return;
		SetModelVisible(model, false);
		StopAnimationHierarchy(model);
	}

	private void SetModelVisible(Node model, bool visible)
	{
		if (model == null) return;
		List<SurfaceMask> surfaces;
		if (!modelSurfaceMasks.TryGetValue(model, out surfaces)) return;

		for (int i = 0; i < surfaces.Count; i++)
		{
			SurfaceMask surfaceMask = surfaces[i];
			for (int surface = 0; surface < surfaceMask.ViewportMasks.Length; surface++)
			{
				surfaceMask.Object.SetViewportMask(visible ? surfaceMask.ViewportMasks[surface] : 0, surface);
				surfaceMask.Object.SetShadowMask(visible ? surfaceMask.ShadowMasks[surface] : 0, surface);
			}
		}
	}

	private static void RestartAnimationHierarchy(Node current)
	{
		if (current == null) return;

		ObjectMeshSkinnedLegacy skinned = current as ObjectMeshSkinnedLegacy;
		if (skinned != null)
		{
			skinned.Loop = false;
			skinned.Time = 0.0f;
			skinned.Play();
		}

		for (int i = 0; i < current.NumChildren; i++)
			RestartAnimationHierarchy(current.GetChild(i));
	}

	private static void ResetAnimationHierarchy(Node current)
	{
		if (current == null) return;

		ObjectMeshSkinnedLegacy skinned = current as ObjectMeshSkinnedLegacy;
		if (skinned != null)
		{
			skinned.Time = 0.0f;
			skinned.Stop();
		}

		for (int i = 0; i < current.NumChildren; i++)
			ResetAnimationHierarchy(current.GetChild(i));
	}

	private static void StopAnimationHierarchy(Node current)
	{
		if (current == null) return;

		ObjectMeshSkinnedLegacy skinned = current as ObjectMeshSkinnedLegacy;
		if (skinned != null) skinned.Stop();

		for (int i = 0; i < current.NumChildren; i++)
			StopAnimationHierarchy(current.GetChild(i));
	}

	private void Shutdown()
	{
		if (initialPoseModel != null)
		{
			ShowOnlyModel(initialPoseModel);
			ResetAnimationHierarchy(initialPoseModel);
		}
		if (scenario != null && subscribed)
		{
			scenario.PassengerConversationChanged -= OnConversationChanged;
			scenario.PassengerEmotionRequested -= OnEmotionRequested;
		}
		if (gameFlow != null && subscribedToFlow)
		{
			gameFlow.ScenarioTwoIntroductionRequested -= OnScenarioTwoStarted;
			gameFlow.ScenarioTwoStarted -= OnScenarioTwoStarted;
		}
	}
}
