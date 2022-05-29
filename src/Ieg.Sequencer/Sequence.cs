﻿namespace Ieg.Sequencer;

using System;
using System.Diagnostics;
using System.Linq;

public class Sequence : ISequence
{
    private readonly SequenceConfiguration _configuration;


    public Sequence(SequenceConfiguration configuration)
    {
        _configuration = configuration;
        CurrentState   = configuration.InitialState;
    }


    /// <inheritdoc />
    public string CurrentState { get; private set; }

    public Stopwatch Stopwatch { get; } = new();


    /// <inheritdoc />
    public ISequence SetState(string state)
    {
        CurrentState = state;
        return this;
    }
        
    /// <inheritdoc />
    public ISequence SetState(string state, Func<bool> constraint)
    {
        if (constraint.Invoke()) CurrentState = state;
        return this;
    }

    /// <inheritdoc />
    public virtual ISequence Run()
    {
        if (ExecuteForceStateDescriptor(GetForceStateDescriptor())) return this;
            
        ExecuteStateTransitionDescriptors();
        return this;
    }

      
    private ForceStateDescriptor GetForceStateDescriptor() =>
        _configuration.Descriptors.OfType<ForceStateDescriptor>()?.LastOrDefault();

    private bool ExecuteForceStateDescriptor(ForceStateDescriptor forceState)
    {
        var complied = forceState?.Constraint?.Invoke() ?? false;
        if (complied) CurrentState = forceState.State;

        return complied;
    }

    private void ExecuteStateTransitionDescriptors() =>
        _configuration.Descriptors.OfType<StateTransitionDescriptor>().ToList()
            .ForEach(ExecuteStateTransitionDescriptor);

    private void ExecuteStateTransitionDescriptor(StateTransitionDescriptor state)
    {
        if (state.ValidateTransition(CurrentState))
        {
            SetState(state.NextState);
            state.Action?.Invoke();
        }
    }
}