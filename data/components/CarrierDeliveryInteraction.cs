using Unigine;

#if UNIGINE_DOUBLE
using Vec3 = Unigine.dvec3;
#else
using Vec3 = Unigine.vec3;
#endif

/// <summary>Handles picking up the carrier and handing it to the passenger with E.</summary>
public partial class CarrierDeliveryInteraction : Component
{
	[ShowInEditor, Parameter(Group = "Carrier Delivery")]
	public Node passengerNode;

	[ParameterSlider(Min = 0.2f, Max = 2.0f, Group = "Carrier Delivery")]
	public float pickupAnimationSeconds = 0.8f;

	private PetCarrierScenarioController scenario;
	private EInteractable carrierInteractable;
	private EInteractable passengerInteractable;
	private EInteractable subscribedCarrier;
	private EInteractable subscribedPassenger;
	private bool pickupAnimating, carrierCollected, availabilityApplied;
	private Vec3 pickupStartPosition;
	private float pickupElapsed;

	private void Update()
	{
		if (scenario == null)
			scenario = ComponentSystem.FindComponentInWorld<PetCarrierScenarioController>();
		if (carrierInteractable == null)
			carrierInteractable = ComponentSystem.GetComponentInParent<EInteractable>(node);
		if (passengerNode != null && passengerInteractable == null)
			passengerInteractable = ComponentSystem.GetComponentInParent<EInteractable>(passengerNode);

		BindInteractions();
		if (scenario == null) return;
		if (!availabilityApplied && scenario.IsStarted)
		{
			availabilityApplied = true;
			if (!scenario.CarrierAvailable)
			{
				if (carrierInteractable != null) carrierInteractable.IsAvailable = false;
				node.Enabled = false;
				return;
			}
			node.Enabled = true;
		}
		if (pickupAnimating)
		{
			UpdatePickupAnimation();
			return;
		}

		if (carrierInteractable != null)
			carrierInteractable.IsAvailable = scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingCarrierPickup;
		if (passengerInteractable != null)
		{
			PetCarrierScenarioController.Stage stage = scenario.CurrentStage;
			passengerInteractable.IsAvailable = stage == PetCarrierScenarioController.Stage.AwaitingPassengerInteraction
				|| stage == PetCarrierScenarioController.Stage.AwaitingPassengerHandoff;
		}
	}

	private void UpdatePickupAnimation()
	{
		if (Game.Player == null)
			return;

		pickupElapsed += Game.IFps;
		float duration = MathLib.Max(0.2f, pickupAnimationSeconds);
		float t = MathLib.Min(1.0f, pickupElapsed / duration);
		float eased = t * t * (3.0f - 2.0f * t);
		Node player = Game.Player;
		Vec3 forward = player.GetWorldDirection(MathLib.AXIS.NZ);
		Vec3 right = player.GetWorldDirection(MathLib.AXIS.X);
		Vec3 down = player.GetWorldDirection(MathLib.AXIS.NY);
		Vec3 target = player.WorldPosition + forward * 0.55f + right * 0.35f + down * 0.45f;
		node.WorldPosition = pickupStartPosition + (target - pickupStartPosition) * eased;

		if (t < 1.0f) return;

		pickupAnimating = false;
		carrierCollected = true;
		node.Enabled = false;
		if (carrierInteractable != null) carrierInteractable.IsAvailable = false;
		scenario.NotifyCarrierPickedUp();
		// The carrier node is disabled above, so its Update can no longer
		// enable the passenger interaction for the handoff stage.
		if (passengerInteractable != null)
			passengerInteractable.IsAvailable = scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingPassengerHandoff;
		else
			Log.Warning("CarrierDeliveryInteraction: assign passengerNode so the passenger can be interacted with after pickup.\n");
	}

	private void BindInteractions()
	{
		if (subscribedCarrier != carrierInteractable)
		{
			if (subscribedCarrier != null) subscribedCarrier.Interacted -= OnCarrierInteracted;
			subscribedCarrier = carrierInteractable;
			if (subscribedCarrier != null) subscribedCarrier.Interacted += OnCarrierInteracted;
		}
		if (subscribedPassenger != passengerInteractable)
		{
			if (subscribedPassenger != null) subscribedPassenger.Interacted -= OnPassengerInteracted;
			subscribedPassenger = passengerInteractable;
			if (subscribedPassenger != null) subscribedPassenger.Interacted += OnPassengerInteracted;
		}
	}

	private void OnCarrierInteracted()
	{
		if (scenario == null || scenario.CurrentStage != PetCarrierScenarioController.Stage.AwaitingCarrierPickup)
			return;
		if (pickupAnimating || carrierCollected) return;

		pickupStartPosition = node.WorldPosition;
		pickupElapsed = 0.0f;
		pickupAnimating = true;
		if (carrierInteractable != null) carrierInteractable.IsAvailable = false;
	}

	private void OnPassengerInteracted()
	{
		if (scenario == null || passengerInteractable == null || !passengerInteractable.IsAvailable)
			return;

		scenario.NotifyPassengerInteraction();
	}

	private void Shutdown()
	{
		if (subscribedCarrier != null) subscribedCarrier.Interacted -= OnCarrierInteracted;
		if (subscribedPassenger != null) subscribedPassenger.Interacted -= OnPassengerInteracted;
	}
}
