using Unity.Collections;
using UnityEngine;

public class AIDirectorScript : MonoBehaviour
{
    [SerializeField] private bool CenterMassToggle = true;
    [SerializeField] private bool EqualizeSpeedToggle = true;
    [SerializeField] private bool PersonalSpaceToggle = true;

    private NativeArray<MobComponentData> allMobs;
    private NativeArray<CenterMassJobInput> centerMassInputs;
    private NativeArray<CenterMassJobOutput> centerMassOutputs;
    private NativeArray<EqualizeSpeedJobInput> equalizeSpeedInputs;
    private NativeArray<EqualizeSpeedJobOutput> equalizeSpeedOutputs;
    private NativeArray<PersonalSpaceJobInput> personalSpaceInputs;
    private NativeArray<PersonalSpaceJobOutput> personalSpaceOutputs;

}
