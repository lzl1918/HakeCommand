using HakeCommand.Framework;
using HakeCommand.Framework.Input;
using HakeCommand.Framework.Services.OutputEngine;
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

        [Statement("Create a new variable, or change the existing variable")]
        [Command("set")]
        public object SetVariable(string name, object value)
        {
            value = value ?? InputObject;
            try
            {
                if (name == null)
                    name = ReadLine("variable name");
                return variableService.Set(name, value);
            }
            catch (Exception ex)
            {
                SetException(ex);
                return null;
            }
        }

        [Statement("Retrive the value of a variable")]
        [Command("get")]
        public object GetVariable(string name)
        {
            try
            {
                if (name == null)
                    name = ReadLine("variable name");
                return variableService.Get(name);
            }
            catch (Exception ex)
            {
                SetException(ex);
                return null;
            }
        }
    }
}
