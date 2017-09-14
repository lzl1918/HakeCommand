using Hake.Extension.StateMachine;
using HakeCommand.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Framework.Input.Internal
{
#if DEBUG
    public
#else
    internal
#endif
    sealed class Input : IInput
    {
        internal const char ESCAPE_CHAR = '`';

        public string Name { get; }
        public object[] Arguments { get; }
        public IReadOnlyDictionary<string, object> Options { get; }

        public bool ContainsError { get; }
        public string ErrorMessage { get; }


        internal Input(string name, object[] arguments, IReadOnlyDictionary<string, object> options)
        {
            Name = name;
            Arguments = arguments;
            Options = options;
            ContainsError = false;
            ErrorMessage = null;

        }
        internal Input(string error)
        {
            Name = "";
            Arguments = new object[0];
            Options = new Dictionary<string, object>();
            ErrorMessage = error;
            ContainsError = true;
        }

#if DEBUG
        public
#else
        internal
#endif
        static IInputCollection Parse(string input)
        {
            return inputParser.Parse(input);
        }
        private static InputParser inputParser = new InputParser();
    }
}
