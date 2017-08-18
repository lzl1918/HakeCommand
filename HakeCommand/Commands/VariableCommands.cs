using HakeCommand.Framework;
using HakeCommand.Framework.Services.VariableService;
using System;
using System.Collections.Generic;
using System.Text;

namespace HakeCommand.Commands
{
    public sealed class VariableCommands : CommandSet
    {
        private IVariableService variableService;

        public VariableCommands(IVariableService variableService)
        {
            this.variableService = variableService;
        }

        [Command("set")]
        public object SetVariable(string name, object value)
        {
            value = value ?? InputObject;
            try
            {
                return variableService.Set(name, value);
            }
            catch
            {
                throw;
            }
        }

        [Command("get")]
        public object GetVariable(string name)
        {
            try
            {
                return variableService.Get(name);
            }
            catch
            {
                throw;
            }
        }
    }
}
