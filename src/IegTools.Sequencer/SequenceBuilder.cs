﻿namespace IegTools.Sequencer;

using Microsoft.Extensions.Logging;
using FluentValidation;
using Handler;
using JetBrains.Annotations;
using Validation;

#nullable enable

/// <summary>
/// Provides methods to build a sequence.
/// </summary>
public class SequenceBuilder : ISequenceBuilder
{
    private readonly IValidator<SequenceConfiguration> _validator;

    private SequenceBuilder(IValidator<SequenceConfiguration> validator) =>
        _validator = validator;

    
    /// <inheritdoc />
    public SequenceConfiguration Configuration { get; init; } = new();



    /// <inheritdoc />
    public ISequenceBuilder ActivateDebugLogging(ILogger logger, EventId eventId)
    {
        Configuration.Logger  = logger;
        Configuration.EventId = eventId;
        return this;
    }



    /// <inheritdoc />
    public ISequence Build() => Build<Sequence>();


    /// <inheritdoc />
    public ISequence Build<TSequence>() where TSequence : ISequence, new()
    {
        AddDefaultValidators();

        if (!Configuration.DisableValidation)
            _validator?.ValidateAndThrow(Configuration);

        return new TSequence().SetConfiguration(Configuration);
    }



    /// <summary>
    /// Creates a new Sequence-Builder with an empty sequence.
    /// In some scenarios it is useful to have an empty sequence.
    /// Validation is disabled.
    /// </summary>
    public static ISequenceBuilder CreateEmpty(string fixedState = "Empty")
    {
        var builder = Create(new SequenceConfigurationValidator())
            .DisableValidation()
            .SetInitialState(fixedState);
        builder.Configuration.IsEmpty = true;
        return builder;
    }

    /// <summary>
    /// Creates a new Sequence-Builder for configuration in .NET 6 style.
    /// This is good for short crispy configs.
    /// </summary>
    public static ISequenceBuilder Create() =>
        Create(new SequenceConfigurationValidator());

    /// <summary>
    /// Creates a new Sequence-Builder for configuration in .NET 6 style.
    /// This is good for short crispy configs.
    /// </summary>
    public static ISequenceBuilder Create(IValidator<SequenceConfiguration> validator) =>
        new SequenceBuilder(validator);



    /// <summary>
    /// Configures the sequence in .NET 5 style.
    /// This is good for larger complex configs.
    /// </summary>
    /// <param name="configurationActions">The action.</param>
    public static ISequenceBuilder Configure(Action<ISequenceBuilder> configurationActions)
    {
        var sequenceBuilder = Create();
        configurationActions.Invoke(sequenceBuilder);
        return sequenceBuilder;
    }

    /// <summary>
    /// Configures the sequence in .NET 5 style.
    /// This is good for larger complex configs.
    /// </summary>
    /// <param name="validator">Custom validator</param>
    /// <param name="configurationActions">The action.</param>
    public static ISequenceBuilder Configure(IValidator<SequenceConfiguration> validator, Action<ISequenceBuilder> configurationActions)
    {
        var sequenceBuilder = Create(validator);
        configurationActions.Invoke(sequenceBuilder);
        return sequenceBuilder;
    }



    /// <inheritdoc />
    public ISequenceBuilder AddHandler<T>(T handler) where T: IHandler
    {
        Configuration.Handler.Add(handler);
        handler.Configuration = Configuration;
        return this;
    }


    /// <inheritdoc />
    public ISequenceBuilder AddValidator<T>() where T : IHandlerValidator, new()
    {
        Configuration.Validators.Add(new T());
        return this;
    }



    /// <inheritdoc />
    public ISequenceBuilder DisableValidation()
    {
        Configuration.DisableValidation = true;
        return this;
    }

    /// <inheritdoc />
    public ISequenceBuilder DisableValidationForStates(params string[] states)
    {
        Configuration.DisableValidationForStates = states;
        return this;
    }

    /// <inheritdoc />
    public ISequenceBuilder SetInitialState(string initialState)
    {
        Configuration.InitialState = initialState;
        return this;
    }

    
    private void AddDefaultValidators()
    {
        AddValidator<InitialStateValidator>();
        AddValidator<ForceStateValidator>();
        AddValidator<StateTransitionValidator>();
        AddValidator<AnyStateTransitionValidator>();
        AddValidator<ContainsStateTransitionValidator>();
    }
}