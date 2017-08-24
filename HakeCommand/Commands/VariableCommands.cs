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
        private IHostInput hostInput;
        private IOutputEngine outputEngine;

        public VariableCommands(IVariableService variableService, IHostInput hostInput, IOutputEngine outputEngine)
        {
            this.variableService = variableService;
            this.hostInput = hostInput;
            this.outputEngine = outputEngine;
        }

        [Command("set")]
        public object SetVariable(string name, object value)
        {
            value = value ?? InputObject;
            try
            {
                if (name == null)
                {
                    outputEngine.WriteHint("variable name: ");
                    name = hostInput.ReadLine();
                }
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
                if (name == null)
                {
                    outputEngine.WriteHint("variable name: ");
                    name = hostInput.ReadLine();
                }
                return variableService.Get(name);
            }
            catch
            {
                throw;
            }
        }
    }
}
