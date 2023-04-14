using UnityEngine;

namespace MisterGames.Interact.Objects {
    
    [CreateAssetMenu(fileName = nameof(InteractiveDrawerConfig), menuName = "MisterGames/Interact/" + nameof(InteractiveDrawerConfig))]
    public class InteractiveDrawerConfig : ScriptableObject{
        
        [Header("Motion: Speed")]
        [SerializeField] [Min(0f)] public float friction = 2f;
        [SerializeField] [Min(0f)] public float rebound = 0.2f;
        [SerializeField] [Min(0f)] public float minSpeed = 0.001f;
        [SerializeField] [Min(0f)] public float maxSpeed = 5f;

        [Header("Motion: Snapping")]
        [SerializeField] [Min(0f)] public float snapIfSpeedBelow = 0.1f;
        [SerializeField] [Min(0f)] public float snapSpeed = 0.3f;
        [SerializeField] [Range(0f, 1f)] public float snapToClosedAtProcess = 0.1f;
        [SerializeField] [Range(0f, 1f)] public float snapToOpenedAtProcess = 0.9f;
        
        [Header("Sounds: Volume")]
        [SerializeField] public float volumeBySpeedMultiplier = 1f;
        [SerializeField] public AnimationCurve volumeBySpeed = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        [Header("Sounds: Open Close")]
        [SerializeField] public bool enableOpenCloseSounds = true;
        [SerializeField] [Min(0.01f)] public float minOpenCloseDelayBetweenSounds = 0.5f;
        [SerializeField] [Min(0f)] public float openCloseSoundsVolumeMultiplier = 0.8f;
        [SerializeField] public AudioClip[] openSounds;
        [SerializeField] public AudioClip[] closeSounds;
        [SerializeField] public AudioClip[] finishOpenSounds;

        [Header("Sounds: Slides")]
        [SerializeField] public bool enableSlideSounds = true;
        [SerializeField] [Min(0.01f)] public float minSlideProcessDelta = 0.4f;
        [SerializeField] [Min(0.01f)] public float minDelayBetweenSlideSounds = 1f;
        [SerializeField] [Min(0.01f)] public float maxDelayBetweenSlideSounds = 2f;
        [SerializeField] [Min(0f)] public float slideSoundsVolumeMultiplier = 0.5f;
        [SerializeField] public AudioClip[] slideSounds;
        
        [Header("Sounds: Clicks")]
        [SerializeField] public bool enableClickSounds = true;
        [SerializeField] [Min(1)] public int clickersAmount = 10;
        [SerializeField] [Min(0.01f)] public float minDelayBetweenClickSounds = 0.2f;
        [SerializeField] [Min(0f)] public float clickSoundsVolumeMultiplier = 0.1f;
        [SerializeField] public AudioClip[] clickSounds;
        
    }
    
}