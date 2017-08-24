using HakeCommand.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Services.VariableService
{
    public interface IVariableService : IDisposable
    {
        object Get(string name);
        object Set(string name, object value);
    }
    internal sealed class InternalVariableService : IVariableService
    {
        private Dictionary<string, object> variables;
        public InternalVariableService()
        {
            variables = new Dictionary<string, object>();
        }

        public object Get(string name)
        {
            if (name == null)
                throw new Exception("missing variable name");

            name = name.Trim().ToLower();
            object value;
            if (variables.TryGetValue(name, out value))
                return value;
            else
                throw new KeyNotFoundException($"variable undefined: {name}");
        }

        public object Set(string name, object value)
        {
            if (name == null)
                throw new Exception("missing variable name");

            name = name.Trim().ToLower();
            if (name.Length <= 0)
                throw new ArgumentException("illegal name: name cannot be empty");
            if (name[0] != '$')
                throw new ArgumentException("illegal name: name should start with $");
            variables[name] = value;
            return value;
        }

        public void Dispose()
        {
            variables = null;
        }
    }
}
