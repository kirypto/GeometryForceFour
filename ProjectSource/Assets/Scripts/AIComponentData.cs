using UnityEngine;

public struct MobComponentData
{
    public Vector3 Position;
    public Vector3 Velocity;
}


public struct CenterMassJobOutput
{
    // target center of group
    public Vector3 GroupCenterMass;
}


public struct EqualizeSpeedJobOutput
{
    // match speed
    public Vector3 EqualizeSpeed;
}


public struct PersonalSpaceJobOutput
{
    // personal space
    public Vector3 PersonalSpace;
}
