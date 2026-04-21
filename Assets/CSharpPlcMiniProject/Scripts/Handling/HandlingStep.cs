namespace CSharpPlcMiniProject.Handling
{
    public enum HandlingStep
    {
        WaitingForCan,
        MoveToPick,
        CloseGripper,
        LiftCan,
        MoveToPlace,
        LowerToPlace,
        OpenGripper,
        CommandLiftAfterPlace,
        LiftAfterPlace,
        ReturnToWaitPosition
    }
}
