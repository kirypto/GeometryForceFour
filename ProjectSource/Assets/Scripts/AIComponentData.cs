using UnityEngine;

public struct MobComponentData
{
    public Vector3 Position;
    public Vector3 Velocity;
}
      

public struct CenterMassJobInput
{
    // target center of group
    public float GroupCenterMassScale;
}
public struct CenterMassJobOutput
{
    // target center of group
    public Vector3 GroupCenterMass;
}


public struct EqualizeSpeedJobInput
{
    // match speed
    public float EqualizeSpeedScale;
    public float EqualizeSpeedRadius;
}
public struct EqualizeSpeedJobOutput
{
    // match speed
    public Vector3 EqualizeSpeed;
}


public struct PersonalSpaceJobInput
{
    // personal space
    public float PersonalSpaceScale;
    public float PersonalSpaceRadius;
}
public struct PersonalSpaceJobOutput
{
    // personal space
    public Vector3 PersonalSpace;
}
