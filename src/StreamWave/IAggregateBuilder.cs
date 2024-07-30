﻿namespace StreamWave;

/// <summary>
/// Interface for building an aggregate with specified configurations.
/// </summary>
/// <typeparam name="TState">The type of the aggregate state.</typeparam>
/// <typeparam name="TId">The type of the identifier used for the aggregate.</typeparam>
public interface IAggregateBuilder<TState, TId>
{
    /// <summary>
    /// Configures the builder with a set of events to be applied to the aggregate.
    /// </summary>
    /// <param name="events">An array of events to be applied to the aggregate.</param>
    /// <returns>The current instance of <see cref="IAggregateBuilder{TState, TId}"/>.</returns>
    IAggregateBuilder<TState, TId> WithEvents(Event[] events);

    /// <summary>
    /// Configures the builder with a loader function for loading the event stream.
    /// </summary>
    /// <param name="loader">A function that provides the loader delegate, using the service provider.</param>
    /// <returns>The current instance of <see cref="IAggregateBuilder{TState, TId}"/>.</returns>
    IAggregateBuilder<TState, TId> WithLoader(Func<IServiceProvider, LoadEventStreamDelegate<TId>> loader);

    /// <summary>
    /// Configures the builder with a saver function for saving the aggregate.
    /// </summary>
    /// <param name="saver">A function that provides the saver delegate, using the service provider.</param>
    /// <returns>The current instance of <see cref="IAggregateBuilder{TState, TId}"/>.</returns>
    IAggregateBuilder<TState, TId> WithSaver(Func<IServiceProvider, SaveAggregateDelegate<TState, TId>> saver);

    /// <summary>
    /// Configures the builder with an applier function for applying events to the aggregate's state.
    /// </summary>
    /// <param name="applier">A function that provides the applier delegate, using the service provider.</param>
    /// <returns>The current instance of <see cref="IAggregateBuilder{TState, TId}"/>.</returns>
    IAggregateBuilder<TState, TId> WithApplier(Func<IServiceProvider, ApplyEventDelegate<TState>> applier);

    /// <summary>
    /// Configures the builder with an applier function for applying specific events to the aggregate's state.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to be applied.</typeparam>
    /// <param name="applier">A function that applies the specific event type to the state.</param>
    /// <returns>The current instance of <see cref="IAggregateBuilder{TState, TId}"/>.</returns>
    IAggregateBuilder<TState, TId> WithApplier<TEvent>(Func<TState, TEvent, Task<TState>> applier)
        where TEvent : Event;

    /// <summary>
    /// Configures the builder with a validator function for validating the aggregate's state.
    /// </summary>
    /// <param name="validator">A function that provides the validator delegate, using the service provider.</param>
    /// <returns>The current instance of <see cref="IAggregateBuilder{TState, TId}"/>.</returns>
    IAggregateBuilder<TState, TId> WithValidator(Func<IServiceProvider, ValidateStateDelegate<TState>> validator);

    /// <summary>
    /// Configures the builder with a validation rule and message.
    /// </summary>
    /// <param name="rule">A function that represents the validation rule to be applied to the state.</param>
    /// <param name="message">The validation message to be returned if the rule fails.</param>
    /// <returns>The current instance of <see cref="IAggregateBuilder{TState, TId}"/>.</returns>
    IAggregateBuilder<TState, TId> WithValidator(Func<TState, bool> rule, string message);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    IAggregate<TState, TId> Build(IServiceProvider serviceProvider);
}
