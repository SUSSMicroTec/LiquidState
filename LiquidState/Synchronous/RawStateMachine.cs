﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LiquidState.Common;
using LiquidState.Core;
using LiquidState.Synchronous.Core;

namespace LiquidState.Synchronous
{
    public abstract class RawStateMachineBase<TState, TTrigger> : AbstractStateMachineCore<TState, TTrigger>,
        IStateMachine<TState, TTrigger>
    {
        internal StateRepresentation<TState, TTrigger> CurrentStateRepresentation;
        internal Dictionary<TState, StateRepresentation<TState, TTrigger>> Representations;
        private readonly RawStateMachineDiagnostics<TState, TTrigger> diagnostics;

        protected RawStateMachineBase(TState initialState, Configuration<TState, TTrigger> configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);

            Representations = configuration.Representations;

            CurrentStateRepresentation = StateConfigurationHelper<TState, TTrigger>.FindStateRepresentation(
                initialState, Representations);

            if (CurrentStateRepresentation == null)
            {
                InvalidStateException<TState>.Throw(initialState);
            }

            diagnostics = new RawStateMachineDiagnostics<TState, TTrigger>(this);
        }

        public virtual void MoveToState(TState state, StateTransitionOption option = StateTransitionOption.Default)
        {
            if (!IsEnabled) return;
            ExecutionHelper.MoveToStateCore(state, option, this);
        }

        public virtual void Fire<TArgument>(ParameterizedTrigger<TTrigger, TArgument> parameterizedTrigger,
            TArgument argument)
        {
            if (!IsEnabled) return;
            ExecutionHelper.FireCore(parameterizedTrigger, argument, this);
        }

        public virtual void Fire(TTrigger trigger)
        {
            if (!IsEnabled) return;
            ExecutionHelper.FireCore(trigger, this);
        }

        public IStateMachineDiagnostics<TState, TTrigger> Diagnostics
        {
            get { return diagnostics; }
        }

        public override TState CurrentState
        {
            get { return CurrentStateRepresentation.State; }
        }
    }

    public class RawStateMachineDiagnostics<TState, TTrigger> : IStateMachineDiagnostics<TState, TTrigger>
    {
        private readonly RawStateMachineBase<TState, TTrigger> machine;
        public RawStateMachineDiagnostics(RawStateMachineBase<TState, TTrigger> machine)
        {
            this.machine = machine;
        }

        public bool CanHandleTrigger(TTrigger trigger, bool exactMatch = false)
        {
            return DiagnosticsHelper.CanHandleTrigger(trigger, machine, exactMatch);
        }

        public bool CanHandleTrigger(TTrigger trigger, Type argumentType)
        {
            return DiagnosticsHelper.CanHandleTrigger(trigger, machine, argumentType);
        }

        public bool CanHandleTrigger<TArgument>(TTrigger trigger)
        {
            return DiagnosticsHelper.CanHandleTrigger<TState, TTrigger, TArgument>(trigger, machine);
        }

        public IEnumerable<TTrigger> CurrentPermittedTriggers
        {
            get { return DiagnosticsHelper.EnumeratePermittedTriggers(machine); }
        }
    }

    public sealed class RawStateMachine<TState, TTrigger> : RawStateMachineBase<TState, TTrigger>
    {
        public RawStateMachine(TState initialState, Configuration<TState, TTrigger> configuration)
            : base(initialState, configuration)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(initialState != null);
        }
    }
}
