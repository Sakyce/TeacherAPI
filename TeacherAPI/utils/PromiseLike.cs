using System;

namespace TeacherAPI.utils
{
    internal class PromiseLike<T>
    {
        private T result;
        private Exception exception;

        public PromiseLike()
        {

        }
        public PromiseLike<T> IfSuccess(Action<T> action)
        {
            if (result != null)
            {
                action.Invoke(result);
            }
            return this;
        }
        public PromiseLike<T> IfError(Action<Exception> action)
        {
            if (exception != null)
            {
                action.Invoke(exception);
            }
            return this;
        }

        public void Resolve(T result)
        {
            this.result = result;
        }
        public void Fail(Exception exception)
        {
            this.exception = exception;
        }
    }
}
