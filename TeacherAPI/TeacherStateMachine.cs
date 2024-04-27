namespace TeacherAPI
{
    public class TeacherState : NpcState
    {
        protected Teacher teacher;
        public TeacherState(Teacher teacher) : base(teacher)
        {
            this.teacher = teacher;
        }

        /// <summary>
        /// Avoid using this to add anger to your Teacher
        /// </summary>
        public virtual void NotebookCollected(int currentNotebooks, int maxNotebooks)
        {

        }

        /// <summary>
        /// Triggered when the player interact with a math machine, giving the good answer.
        /// <para>Avoid using this for raising anger</para>
        /// </summary>
        public virtual void GoodMathMachineAnswer()
        {

        }

        /// <summary>
        /// Triggered when the player interact with a math machine, giving the wrong answer.
        /// <para>Avoid using this for raising anger</para>
        /// </summary>
        public virtual void BadMathMachineAnswer()
        {

        }

        /// <summary>
        /// Triggered when the player exits the spawn.
        /// <para>Avoid using this for raising anger</para>
        /// </summary>
        public virtual void PlayerExitedSpawn()
        {

        }
    }
}
