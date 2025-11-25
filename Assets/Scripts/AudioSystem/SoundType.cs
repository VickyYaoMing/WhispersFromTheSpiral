using UnityEngine;

namespace Assets.Scripts.AudioSystem
{
    public enum SoundType
    {
        None = 0,
        // --- UI ---
        UI_ClickButton,
        UI_MenuTheme, //Main theme main menu
        // UI_PauseOpen,
        // UI_PauseClose,

        // --- Doors ---
        SFX_DoorOpen,
        SFX_DoorTryOpen,

        // --- Ambience / Music ---
        Amb_Ominous1, //Music loop  
        Amb_Ominous2, //Music loop
        Amb_AmbientTheme, //main theme in game
        Amb_HeartbeatLoop,
        Amb_ClockTicking,
        SFX_ClockDong,
        Amb_WhisperVoices,
        Amb_RandomNoise,

        // --- Whips / Stingers / Instruments ---
        SFX_Piano_MenacingThumps,
        SFX_Piano_WhipPiano01,
        SFX_Piano_WhipPianoAttack, //single
        SFX_Piano_WhipPianoMutedHit, //Mutiple

        SFX_Strings_WhipFX1,
        SFX_Strings_WhipFX3,
        SFX_Strings_Harmonics,
        SFX_Violin_Creak,
        SFX_Violin_Gliss,
        SFX_Violin_Harmonic,
        SFX_DemoBreathing,
        SFX_MonsterRoar,

        // // --- Voice overs / events (if they are VO lines) ---
        // VO_FirstTest,
        // VO_PauseTest,
        // VO_WinningLanes,

    }
}
