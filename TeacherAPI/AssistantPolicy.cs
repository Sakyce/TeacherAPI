using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeacherAPI
{
    public enum PossibleAssistantAllowType
    {
        /// <summary>
        /// Only teachers of the specified name can assist your teacher.
        /// </summary>
        Allow,
        /// <summary>
        /// All the teachers of the specified name can't assist your teacher. In most cases you should use this.
        /// </summary>
        Deny
    }

    /// <summary>
    /// Defines the allowed assistants of your teacher, how much and which conditions.
    /// If you don't want assistants for your teacher, you should set an empty AssistantPolicy and set it to Allow.
    /// </summary>
    public class AssistantPolicy
    {
        internal PossibleAssistantAllowType allowType;
        internal List<string> assistantsList = new List<string>();
        internal float probability = 1f;
        internal int maxAssistants = 1;

        public AssistantPolicy(PossibleAssistantAllowType allowType)
        {
            this.allowType = allowType;
            probability = 1f;
        }
        /// <summary>
        /// Not used yet.
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public AssistantPolicy MaxAssistants(int max)
        {
            maxAssistants = max;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="probability">Probability that the teacher will select</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If the probability is lower than 0 or higher than 1</exception>
        public AssistantPolicy Probability(float probability)
        {
            this.probability = probability;
            if (probability > 1f || probability < 0) throw new ArgumentException("Probability can't be more than 1 or less than 0!");
            return this;
        }
        /// <summary>
        /// Adds assistants to the list by their names.
        /// </summary>
        /// <param name="characterNames"></param>
        /// <returns></returns>
        public AssistantPolicy AddAssistants(params string[] characterNames)
        {
            foreach (var name in characterNames)
            {
                assistantsList.Add(name);
            }
            return this;
        }

        internal bool CheckAssistant(Teacher assistant)
        {
            if (allowType == PossibleAssistantAllowType.Allow)
            {
                return assistantsList.Contains(EnumExtensions.GetExtendedName<Character>((int) assistant.Character));
            } 
            else if (allowType == PossibleAssistantAllowType.Deny)
            {
                return !assistantsList.Contains(EnumExtensions.GetExtendedName<Character>((int)assistant.Character));
            }
            return true;
        }
    }
}
