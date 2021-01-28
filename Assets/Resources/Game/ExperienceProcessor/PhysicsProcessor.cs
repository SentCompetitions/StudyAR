using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Resources.Structs;
using UnityEngine;
using UnityEngine.Events;

namespace Resources.Game.ExperienceProcessor
{
    public class PhysicsProcessor : ExperienceProcessor
    {
        private float allResistance;
        private List<PhysicsElement> _elements;
        private List<PhysicsElement> _oldElements;
        private Battery source;

        public override string GetExperienceType()
        {
            return "physics";
        }

        void OnEnable()
        {
            _oldElements = new List<PhysicsElement>();
        }

        public override void OnSchemaUpdate()
        {
            Debug.Log("[CIRCUIT] Starting update...");
            StopAllCoroutines();
            StartCoroutine(UpdateSimulationCoroutine());
        }

        private IEnumerator UpdateSimulationCoroutine()
        {
            source = FindObjectsOfType<Battery>().First();
            _elements = new List<PhysicsElement>();
            _elements.Clear();

            StepInto(WirePort.GetWirePort(source.wirePorts, "+"));

            yield break;
        }

        private void StepInto(WirePort wirePort)
        {
            var sub = wirePort.element.GetComponent<PhysicsElement>();

            if (!(sub.Equals(source) && wirePort.type == "-"))
            {
                allResistance += sub.resistance;
                _elements.Add(sub);
                Debug.Log(wirePort.type);
                try
                {
                    if (string.IsNullOrEmpty(wirePort.way))
                        throw new NotFullCircuit("No way");
                    var wires = sub.GetWires(wirePort.way);
                    if (wires.Count > 0)
                        StepInto(wires.First().GetAnother(wirePort));
                    else
                        throw new NotFullCircuit("No wires");
                }
                catch (NotFullCircuit e)
                {
                    Debug.Log("[CIRCUIT] " + e);
                    allResistance = 0f;
                    SetElementState();
                }
            }
            else
            {
                SetElementState();
            }
        }

        private void SetElementState()
        {
            if (allResistance != 0f)
            {
                float apmerage = source.targetVoltage / allResistance;
                Debug.Log($"[CIRCUIT] Result resistance: {allResistance}");
                Debug.Log($"[CIRCUIT] Result amperage: {apmerage}");
                foreach (var e in _elements)
                {
                    e.amperage = apmerage;
                    e.voltage = source.targetVoltage;
                }
                _oldElements = new List<PhysicsElement>(_elements);

                Step step = NetworkGameManager.instance.experience.actions[NetworkGameManager.instance.experience.GetFirstUnCompleteStep()];
                if (step.action.StartsWith("SCHEME_MAKE_"))
                {
                    
                }
                GameManager.instance.localPlayer.onGameAction.Invoke("CIRCUIT_STATE_COMPLETE");
            }
            else
            {
                foreach (var e in _oldElements)
                {
                    e.amperage = 0f;
                    e.voltage = 0f;
                }
            }
        }
    }

    public class NotFullCircuit : Exception
    {
        public NotFullCircuit(string message) : base(message) { }
    }
}