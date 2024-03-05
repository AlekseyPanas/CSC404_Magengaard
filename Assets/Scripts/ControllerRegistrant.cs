/*
Classes that provide logic for controllables should implement this interface
*/
using System;

public class ControllerRegistrant<T>
{
    private AControllable<T> controllable;
    public ControllerRegistrant (AControllable<T> controllable) {
        this.controllable = controllable;
    }
    public Action OnInterrupt = delegate {};

    public T GetSystem(){

    }
}
