using Unigine;

/// <summary>Enables a passenger after the free-roam timer and plays their local call.</summary>
public partial class PassengerCallInteraction : Component
{
	[ParameterFile(Filter = ".ogg|.oga|.wav|.mp3"), Parameter(Group = "Passenger Call")]
	public string callSoundPath = "";

	[Parameter(Group = "Passenger Call")]
	public float soundMinDistance = 1.0f;

	[Parameter(Group = "Passenger Call")]
	public float soundMaxDistance = 18.0f;

	[ParameterSlider(Min = 0.0f, Max = 1.0f, Group = "Passenger Call")]
	public float callSoundGain = 1.0f;

	private PassengerScenarioController scenario;
	private EInteractable interactable;
	private EInteractable subscribedInteractable;
	private SoundSource callSound;
	private bool subscribed, callStarted;

	private void Init()
	{
		interactable = ComponentSystem.GetComponentInChildren<EInteractable>(node);
		if (interactable != null)
			interactable.IsAvailable = false;
	}

	private void Update()
	{
		if (callSound != null && callSound.IsPlaying)
			callSound.WorldPosition = node.WorldPosition;

		if (interactable == null)
			interactable = ComponentSystem.GetComponentInChildren<EInteractable>(node);
		BindInteractable();
		if (interactable != null && callStarted && scenario != null)
			interactable.IsAvailable = scenario.CurrentStep == PassengerScenarioController.ScenarioStep.WaitingForPassenger;

		if (scenario == null)
		{
			scenario = ComponentSystem.FindComponentInWorld<PassengerScenarioController>();
			if (scenario != null && !subscribed)
			{
				scenario.PassengerCallRequested += OnPassengerCallRequested;
				scenario.StepChanged += OnStepChanged;
				subscribed = true;
				if (scenario.CurrentStep == PassengerScenarioController.ScenarioStep.WaitingForPassenger)
					OnPassengerCallRequested();
			}
		}
	}

	private void OnPassengerCallRequested()
	{
		if (callStarted)
			return;

		callStarted = true;
		PlayLocalCall();
		if (interactable != null)
			interactable.IsAvailable = true;
	}

	private void PlayLocalCall()
	{
		if (string.IsNullOrWhiteSpace(callSoundPath))
		{
			Log.Warning("PassengerCallInteraction: assign a passenger call audio file to callSoundPath.\n");
			return;
		}

		callSound = new SoundSource(callSoundPath);
		if (!callSound)
		{
			Log.Warning("PassengerCallInteraction: can't load call sound '{0}'.\n", callSoundPath);
			return;
		}

		callSound.WorldPosition = node.WorldPosition;
		callSound.MinDistance = MathLib.Max(0.0f, soundMinDistance);
		callSound.MaxDistance = MathLib.Max(callSound.MinDistance, soundMaxDistance);
		callSound.Gain = MathLib.Clamp(callSoundGain, 0.0f, 1.0f);
		callSound.Loop = 1;
		callSound.Play();
	}

	private void OnStepChanged(PassengerScenarioController.ScenarioStep step)
	{
		if (step != PassengerScenarioController.ScenarioStep.WaitingForPassenger && interactable != null)
			interactable.IsAvailable = false;
	}

	private void OnInteracted()
	{
		if (interactable == null || !interactable.IsAvailable || scenario == null)
			return;

		if (callSound != null)
			callSound.Stop();
		scenario.NotifyPassengerInteracted();
	}

	private void BindInteractable()
	{
		if (subscribedInteractable == interactable)
			return;
		if (subscribedInteractable != null)
			subscribedInteractable.Interacted -= OnInteracted;
		subscribedInteractable = interactable;
		if (subscribedInteractable != null)
			subscribedInteractable.Interacted += OnInteracted;
	}

	private void Shutdown()
	{
		if (scenario != null && subscribed)
		{
			scenario.PassengerCallRequested -= OnPassengerCallRequested;
			scenario.StepChanged -= OnStepChanged;
		}
		if (subscribedInteractable != null)
		{
			subscribedInteractable.Interacted -= OnInteracted;
			subscribedInteractable.IsAvailable = true;
		}
		if (callSound != null)
			callSound.DeleteLater();
	}
}
