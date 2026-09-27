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

	[ParameterFile(Filter = ".png|.dds|.jpg|.tga"), Parameter(Group = "Carrier Delivery")]
	public string carrierIconPath = "";

	[ParameterSlider(Min = 0.2f, Max = 2.0f, Group = "Carrier Delivery")]
	public float pickupAnimationSeconds = 0.8f;

	private PetCarrierScenarioController scenario;
	private EInteractable carrierInteractable;
	private EInteractable passengerInteractable;
	private EInteractable subscribedCarrier;
	private EInteractable subscribedPassenger;
	private Gui gui;
	private WidgetSprite inventoryPanel, inventoryIcon, fallbackBagBody, fallbackBagHandle;
	private bool pickupAnimating, carrierCollected, availabilityApplied;
	private Vec3 pickupStartPosition;
	private float pickupElapsed;

	private void Init()
	{
		gui = Gui.GetCurrent();
		inventoryPanel = new WidgetSprite(gui, "")
		{
			Color = new vec4(0.02f, 0.035f, 0.05f, 0.9f),
			Hidden = true
		};
		gui.AddChild(inventoryPanel, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		if (!string.IsNullOrWhiteSpace(carrierIconPath))
		{
			inventoryIcon = new WidgetSprite(gui, carrierIconPath) { Hidden = true };
			gui.AddChild(inventoryIcon, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		}
		else
		{
			fallbackBagHandle = new WidgetSprite(gui, "")
			{
				Color = new vec4(0.85f, 0.62f, 0.32f, 1.0f),
				Hidden = true
			};
			gui.AddChild(fallbackBagHandle, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
			fallbackBagBody = new WidgetSprite(gui, "")
			{
				Color = new vec4(0.55f, 0.3f, 0.12f, 1.0f),
				Hidden = true
			};
			gui.AddChild(fallbackBagBody, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		}
	}

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
		inventoryPanel.Hidden = false;
		if (inventoryIcon != null) inventoryIcon.Hidden = false;
		if (fallbackBagBody != null)
		{
			fallbackBagBody.Hidden = false;
			fallbackBagHandle.Hidden = false;
		}
		LayoutInventory();
		scenario.NotifyCarrierPickedUp();
		// The carrier node is disabled above, so its Update can no longer
		// enable the passenger interaction for the handoff stage.
		if (passengerInteractable != null)
			passengerInteractable.IsAvailable = scenario.CurrentStage == PetCarrierScenarioController.Stage.AwaitingPassengerHandoff;
		else
			Log.Warning("CarrierDeliveryInteraction: assign passengerNode so the passenger can be interacted with after pickup.\n");
	}

	private void LayoutInventory()
	{
		if (gui == null || inventoryPanel == null) return;
		const int width = 76;
		const int height = 76;
		int left = gui.Width - width - 24;
		int top = 24;
		inventoryPanel.SetPosition(left, top);
		inventoryPanel.Width = width;
		inventoryPanel.Height = height;
		if (inventoryIcon != null)
		{
			inventoryIcon.SetPosition(left + 8, top + 8);
			inventoryIcon.Width = 60;
			inventoryIcon.Height = 60;
		}
		else if (fallbackBagBody != null)
		{
			fallbackBagHandle.SetPosition(left + 28, top + 14);
			fallbackBagHandle.Width = 20;
			fallbackBagHandle.Height = 10;
			fallbackBagBody.SetPosition(left + 17, top + 23);
			fallbackBagBody.Width = 42;
			fallbackBagBody.Height = 40;
		}
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
		inventoryPanel?.DeleteLater();
		inventoryIcon?.DeleteLater();
		fallbackBagBody?.DeleteLater();
		fallbackBagHandle?.DeleteLater();
	}
}
