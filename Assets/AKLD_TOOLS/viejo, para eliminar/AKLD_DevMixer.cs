using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR

public class AKLD_DevMixerGroup : CustomEditorBase

{
    [System.Serializable]
    public class AKLD_DevMixer
    {
        [Range(0, 100)]
        public float potentiometerValue = 80; // Inicializado en 80

        public AK.Wwise.RTPC rtpc;
        private float previousValue;

        public bool mute = false;

        public void UpdateMixer()
        {
            if (mute)
            {
                if (!Mathf.Approximately(previousValue, 0))
                {
                    previousValue = rtpc.GetGlobalValue();
                    rtpc.SetGlobalValue(0);
                }
            }
            else
            {
                if (!Mathf.Approximately(previousValue, potentiometerValue))
                {
                    rtpc.SetGlobalValue(potentiometerValue);
                    previousValue = potentiometerValue;
                }
            }
        }
    }

    public List<AKLD_DevMixer> devMixers = new List<AKLD_DevMixer>();

    public void AddDevMixer(AK.Wwise.RTPC rtpc)
    {
        var newDevMixer = new AKLD_DevMixer();
        newDevMixer.rtpc = rtpc;
        devMixers.Add(newDevMixer);
    }

    private void Update()
    {
        foreach (var devMixer in devMixers)
        {
            devMixer.UpdateMixer();
        }
    }
}

#endif
