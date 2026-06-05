using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Expost.RuleReconstruction
{
    public sealed class RunController
    {
        private readonly MonoBehaviour coroutineOwner;
        private readonly PuzzleSession session;

        private Coroutine runRoutine;

        public RunController(MonoBehaviour coroutineOwner, PuzzleSession session)
        {
            this.coroutineOwner = coroutineOwner;
            this.session = session;
        }

        public bool IsRunning { get; private set; }
        public bool ShowResult { get; private set; }
        public bool ShowMismatch { get; private set; }
        public bool ShowResultBanner { get; private set; }
        public BoardState DisplayBoard { get; private set; }
        public HashSet<GridPosition> ActiveAffectedCells { get; private set; } = new();

        public void ShowTarget()
        {
            StopRunRoutine();
            session.ResetSimulation();
            ActiveAffectedCells.Clear();
            DisplayBoard = session.CurrentStage.TargetBoard;
            ShowResult = false;
            ShowMismatch = false;
            ShowResultBanner = false;
        }

        public void ResetAttemptState()
        {
            StopRunRoutine();
            session.ResetSimulation();
            ActiveAffectedCells.Clear();
            ShowResultBanner = false;
        }

        public void StartRun()
        {
            StopRunRoutine();
            runRoutine = coroutineOwner.StartCoroutine(RunSimulation());
        }

        private IEnumerator RunSimulation()
        {
            IsRunning = true;
            ShowResult = true;
            ShowMismatch = false;
            ShowResultBanner = false;
            session.ResetSimulation();
            ActiveAffectedCells.Clear();
            DisplayBoard = session.ResultBoard;

            yield return new WaitForSeconds(0.35f);

            while (!session.IsComplete)
            {
                session.SetActiveSource(session.AppliedSourceCount);
                ActiveAffectedCells = session.GetAffectedCells(session.ActiveSourceIndex);
                DisplayBoard = session.ResultBoard;
                yield return new WaitForSeconds(0.25f);

                session.ApplyNextSource();
                DisplayBoard = session.ResultBoard;
                yield return new WaitForSeconds(0.45f);
            }

            session.ClearActiveSource();
            ActiveAffectedCells.Clear();

            if (!session.ValidationResult.IsClear)
            {
                ShowMismatch = true;
                ShowResultBanner = true;
                yield return new WaitForSeconds(1.1f);
                ShowResultBanner = false;
            }
            else
            {
                session.MarkCurrentStageCleared();
                ShowResultBanner = true;
                yield return new WaitForSeconds(1.1f);
                ShowResultBanner = false;
            }

            IsRunning = false;
            runRoutine = null;
        }

        private void StopRunRoutine()
        {
            if (runRoutine == null)
            {
                IsRunning = false;
                return;
            }

            coroutineOwner.StopCoroutine(runRoutine);
            runRoutine = null;
            IsRunning = false;
        }
    }
}
