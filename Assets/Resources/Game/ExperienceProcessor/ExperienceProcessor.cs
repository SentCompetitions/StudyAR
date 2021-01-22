using UnityEngine;

namespace Resources.Game.ExperienceProcessor
{
    public abstract class ExperienceProcessor: MonoBehaviour
    {
        public abstract string GetExperienceType();

        public abstract void OnSchemaUpdate();
    }
}